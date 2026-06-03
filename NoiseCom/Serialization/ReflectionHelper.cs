using System.Reflection;

namespace NoiseCom.Serialization;

public static class ReflectionHelper
{
    private class AssignPair(Type from, Type to)
    {
        public Type From = from;
        public Type To = to;

        public override bool Equals(object? obj)
        {
            if (obj is not AssignPair other)
                return false;

            return From.Equals(other.From) && To.Equals(other.To);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(From, To);
        }
    }

    public static IEnumerable<Type> EnumerateTypes(Func<Assembly, bool> assemblySelector)
    {
        return AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(assemblySelector)
            .SelectMany(
                (assembly) =>
                {
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
#pragma warning disable CS8601 // Possible null reference assignment.
                        types = [.. e.Types.Where(t => t != null)];
#pragma warning restore CS8601 // Possible null reference assignment.
                    }

                    return types;
                }
            );
    }

    public static IEnumerable<Type> EnumerateTypes()
    {
        return AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(
                (assembly) =>
                {
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
#pragma warning disable CS8601 // Possible null reference assignment.
                        types = [.. e.Types.Where(t => t != null)];
#pragma warning restore CS8601 // Possible null reference assignment.
                    }

                    return types;
                }
            );
    }

    public static bool ImplementsInterface(Type targetType, Type targetInterface)
    {
        if (targetInterface.IsInterface == false)
            throw new ArgumentException();

        if (targetType.IsGenericParameter)
            throw new ArgumentException();

        bool ignoreGenericParameters =
            targetInterface.IsGenericType && targetInterface.IsGenericTypeDefinition;

        foreach (var implementedInterface in targetType.GetInterfaces())
        {
            if (ignoreGenericParameters)
            {
                if (IsGenericTypeDefinitionEqual(implementedInterface, targetInterface))
                    return true;
            }
            else
            {
                if (IsAssignable(implementedInterface, targetInterface))
                    return true;
            }
        }

        return false;
    }

    public static List<Type> FindImplementations(Type ofInterface, IEnumerable<Type> among)
    {
        if (ofInterface.IsInterface == false)
            throw new ArgumentException();

        var implementations = new List<Type>();

        foreach (var type in among)
        {
            if (type.IsInterface || type.IsAbstract)
                continue;

            if (ImplementsInterface(type, ofInterface))
                implementations.Add(type);
        }

        return implementations;
    }

    public static List<Type> FindImplementations(Type ofInterface)
    {
        return FindImplementations(ofInterface, EnumerateTypes());
    }

    public static List<Type> FindAssignableTypes(Type to, IEnumerable<Type> among)
    {
        if (to.IsGenericParameter)
            return [.. among.Where(type => SatisfyGenericParameter(type, to))];
        else
            return [.. among.Where(type => IsAssignable(type, to))];
    }

    public static List<Type> FindAssignableTypes(Type to)
    {
        return FindAssignableTypes(to, EnumerateTypes());
    }

    public static bool IsGenericTypeDefinitionEqual(Type first, Type second)
    {
        Type firstBase = first.IsGenericType ? first.GetGenericTypeDefinition() : first;
        Type secondBase = second.IsGenericType ? second.GetGenericTypeDefinition() : second;

        return firstBase == secondBase;
    }

    public static bool IsAssignable(Type from, Type to)
    {
        return IsAssignable(from, to, null);
    }

    public static bool IsGenericParameterBroaderOrEqual(Type target, Type other)
    {
        return IsGenericParameterBroaderOrEqual(target, other, null);
    }

    public static bool SatisfyGenericParameter(Type type, Type genericParameter)
    {
        return SatisfyGenericParameter(type, genericParameter, null);
    }

    private static bool IsAssignable(Type from, Type to, HashSet<AssignPair>? callStack = null)
    {
        callStack ??= [];

        // if we have already called this method with the same pair, there is a cycle, probably CRTP
        if (callStack.Add(new(from, to)) == false)
            return true; // HACK: returning true works for now, but needs more testing

        if (from == to)
            return true;

        Type? fromBase = null;
        foreach (var implementedInterface in from.GetInterfaces())
        {
            if (implementedInterface == to)
                return true;

            if (IsGenericTypeDefinitionEqual(implementedInterface, to))
            {
                fromBase = implementedInterface; // WARN: if class implements save interface several times with different generic parameters, this this can cause the right interface to be skipped
                break;
            }
        }

        Type? current = from;
        while (fromBase is null && current is not null)
        {
            if (current == to)
                return true;

            if (IsGenericTypeDefinitionEqual(current, to))
                fromBase = current;

            current = current.BaseType;
        }

        if (fromBase is null)
            return false;

        var fromParameters = fromBase.GetGenericArguments();
        var toParameters = to.GetGenericArguments();

        if (fromParameters.Length != toParameters.Length)
            return false;

        for (int i = 0; i < toParameters.Length; i++)
        {
            var fromParam = fromParameters[i];
            var toParam = toParameters[i];

            //compare constraints
            if (fromParam.Equals(toParam))
                continue;

            bool isValid = (fromParam.IsGenericParameter, toParam.IsGenericParameter) switch
            {
                (true, true) => IsGenericParameterBroaderOrEqual(toParam, fromParam, callStack),

                // WARN: this could lead to inappropriate behaviour
                (true, false) => SatisfyGenericParameter(toParam, fromParam, callStack),

                (false, true) => SatisfyGenericParameter(fromParam, toParam, callStack),

                (false, false) => IsAssignable(fromParam, toParam, callStack),
            };

            if (isValid == false)
                return false;
        }

        return true;
    }

    private static bool SatisfyGenericParameter(
        Type type,
        Type genericParameter,
        HashSet<AssignPair>? stack = null
    )
    {
        if (type.IsGenericParameter || genericParameter.IsGenericParameter == false)
            throw new InvalidOperationException();

        var attributes = genericParameter.GenericParameterAttributes;

        if (
            attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint)
            && type.IsValueType
        )
        {
            return false;
        }

        if (
            attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)
            && (type.IsValueType == false || Nullable.GetUnderlyingType(type) is not null)
        )
        {
            return false;
        }

        if (
            attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)
            && type.IsValueType == false
            && type.GetConstructor(Type.EmptyTypes) is null
        )
        {
            return false;
        }

        foreach (var constraint in genericParameter.GetGenericParameterConstraints())
            if (IsAssignable(type, constraint, stack) == false)
                return false;

        return true;
    }

    private static bool IsGenericParameterBroaderOrEqual(
        Type target,
        Type other,
        HashSet<AssignPair>? stack = null
    )
    {
        if (target.IsGenericParameter == false || other.IsGenericParameter == false)
            throw new InvalidOperationException();

        var targetAttributes = target.GenericParameterAttributes;
        var otherAttributes = other.GenericParameterAttributes;

        if (
            targetAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint)
            && otherAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) == false
        )
        {
            return false;
        }

        if (
            targetAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)
            && otherAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)
                == false
        )
        {
            return false;
        }

        if (
            targetAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)
            && otherAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)
                == false
        )
        {
            return false;
        }

        var targetConstraints = target.GetGenericParameterConstraints();
        var otherConstraints = other.GetGenericParameterConstraints();

        foreach (var targetConstraint in targetConstraints)
        {
            bool isSatisfied = false;

            foreach (var otherConstraint in otherConstraints)
            {
                if (IsAssignable(targetConstraint, otherConstraint, stack))
                {
                    isSatisfied = true;
                    break;
                }
            }

            if (isSatisfied == false)
                return false;
        }

        return true;
    }
}
