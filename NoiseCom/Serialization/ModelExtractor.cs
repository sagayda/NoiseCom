using System.Reflection;
using NoiseCom.Noise.Generators;
using NoiseCom.Noise.Hash;

namespace NoiseCom.Serialization;

public static class ModelExtractor
{
    [Flags]
    public enum ModelParameterFlags
    {
        None = 0b0000_0000,
        WasInlined = 0b0000_0001,
        Reference = 0b0000_0010,
        TypeReference = 0b0000_0100,

        ObjectIsEnum = 0b0000_1000,
        ObjectIsGeneric = 0b0001_0000,
        ObjectIsGenericParameter = 0b0010_0000,
    }

    public record ModelParameterDefinition(
        string Name,
        Type ModelType,
        Type ObjectType,
        ModelParameterFlags Flags = ModelParameterFlags.None
    );

    private class ModelExtractionContext
    {
        public Dictionary<object, int> ObjectIdMap { get; } = [];
        public List<NoiseModel> Models { get; } = [];

        public int CurrentId { get; set; }

        public void AddModel(object obj, NoiseModel model)
        {
            ObjectIdMap.Add(obj, CurrentId);
            Models.Add(model);
            CurrentId++;
        }
    }

    private class ObjectExtractionContext(Type hashType, Type dimensionType)
    {
        public Type HashType { get; set; } = hashType;
        public Type PointType { get; set; } = dimensionType;

        public Dictionary<string, object> Parameters { get; set; } = [];
        public Dictionary<string, int> Links { get; set; } = [];

        public Dictionary<int, object> IdObjectMap { get; } = [];
    }

    public static List<ModelParameterDefinition> ExtractModelParameters(
        Type fromType,
        bool isInlined = false
    )
    {
        List<ModelParameterDefinition> definitions = [];

        var targetType = fromType;

        var defaultFlags = isInlined ? ModelParameterFlags.WasInlined : ModelParameterFlags.None;

        static ModelParameterFlags GetObjectFlags(Type objectType)
        {
            var flags = ModelParameterFlags.None;

            if (objectType.IsEnum)
                flags |= ModelParameterFlags.ObjectIsEnum;

            if (objectType.IsGenericType)
                flags |= ModelParameterFlags.ObjectIsGeneric;

            if (objectType.IsGenericParameter)
                flags |= ModelParameterFlags.ObjectIsGenericParameter;

            return flags;
        }

        foreach (var member in GetTypeDataMembers(targetType))
        {
            if (member.IsDefined(typeof(ModelReferenceAttribute), true))
            {
                var memberType = GetMemberType(member) ?? throw new InvalidOperationException();

                var flags =
                    defaultFlags | ModelParameterFlags.Reference | GetObjectFlags(memberType);

                definitions.Add(
                    new(
                        member.Name,
                        typeof(int), // the id type
                        memberType,
                        flags
                    )
                );
                continue;
            }

            if (member.IsDefined(typeof(ModelTypeReferenceAttribute), true))
            {
                var memberType = GetMemberType(member) ?? throw new InvalidOperationException();

                var flags =
                    defaultFlags | ModelParameterFlags.TypeReference | GetObjectFlags(memberType);

                definitions.Add(new(member.Name, typeof(string), memberType, flags));

                continue;
            }

            if (member.IsDefined(typeof(ModelInlineAttribute), true))
            {
                var memberType = GetMemberType(member);

                // HACK: maybe throw exception?
                if (memberType is null)
                    continue;

                var inlinedDefinitions = ExtractModelParameters(memberType, true);
                definitions.AddRange(inlinedDefinitions);

                continue;
            }

            if (member.IsDefined(typeof(ModelPropertyAttribute), true))
            {
                var memberType = GetMemberType(member) ?? throw new InvalidOperationException();

                var flags = defaultFlags | GetObjectFlags(memberType);

                definitions.Add(new(member.Name, memberType, memberType, flags));
            }
        }

        return definitions;
    }

    public static NoiseCompositeModel ExtractModel<THash, TPoint>(INoise<THash, TPoint> fromNoise)
        where THash : IHash8<THash>
        where TPoint : struct, IDimensionalPoint<TPoint>
    {
        var context = new ModelExtractionContext();

        NoiseModel rootModel = ExtractModel(fromNoise, context);

        var hashAlias = ModelTypeRegistry.GetAliasByType(typeof(THash));
        var dimensionAlias = ModelTypeRegistry.GetAliasByType(typeof(TPoint));

        return new NoiseCompositeModel(hashAlias, dimensionAlias, rootModel.Id)
        {
            Models = context.Models,
        };
    }

    public static INoise<THash, TPoint> ExtractNoise<THash, TPoint>(NoiseCompositeModel fromModel)
        where THash : IHash8<THash>
        where TPoint : struct, IDimensionalPoint<TPoint>
    {
        return ExtractNoise<THash, TPoint, INoise<THash, TPoint>>(fromModel);
    }

    public static TNoise ExtractNoise<THash, TPoint, TNoise>(NoiseCompositeModel fromModel)
        where THash : IHash8<THash>
        where TPoint : struct, IDimensionalPoint<TPoint>
        where TNoise : INoise<THash, TPoint>
    {
        var context = new ObjectExtractionContext(typeof(THash), typeof(TPoint));

        var sortedModels = SortByDependencies(
            fromModel.Models,
            node => node.Id,
            node => node.Links.Values
        );

        foreach (var model in sortedModels)
            ExtractObject(model, context);

        var rootNoise = context.IdObjectMap[fromModel.RootModelId];

        return (TNoise)rootNoise;
    }

    private static object ExtractObject(NoiseModel fromModel, ObjectExtractionContext context)
    {
        var objType = ModelTypeRegistry.GetTypeByAlias(fromModel.TypeAlias);

        context.Parameters = fromModel.Parameters;
        context.Links = fromModel.Links;

        var obj = BuildObject(objType, context);

        context.IdObjectMap.Add(fromModel.Id, obj);

        return obj;
    }

    private static object BuildObject(Type objectType, ObjectExtractionContext context)
    {
        var dataMembers = GetTypeDataMembers(objectType);
        Dictionary<MemberInfo, object> dataMemberValueMap = [];

        // firstly, search for ready-to-load parameters
        foreach (
            var member in dataMembers.Where(member =>
                member.IsDefined(typeof(ModelPropertyAttribute))
            )
        )
        {
            if (context.Parameters.TryGetValue(member.Name, out object? value))
                dataMemberValueMap.Add(member, value);
            else
                throw new InvalidOperationException(); // TODO: default values handling
        }

        // link with already parsed models
        foreach (
            var member in dataMembers.Where(member =>
                member.IsDefined(typeof(ModelReferenceAttribute))
            )
        )
        {
            if (context.Links.TryGetValue(member.Name, out int id) == false)
                throw new InvalidOperationException();

            if (context.IdObjectMap.TryGetValue(id, out var obj) == false)
                throw new InvalidOperationException();

            dataMemberValueMap.Add(member, obj);
        }

        // create objects for ModelTypeReferenceAttribute properties
        foreach (
            var member in dataMembers.Where(member =>
                member.IsDefined(typeof(ModelTypeReferenceAttribute))
            )
        )
        {
            if (context.Parameters.TryGetValue(member.Name, out var value) == false)
                throw new InvalidOperationException(member.Name);

            if (value is not string typeDef)
                throw new InvalidOperationException($"{value.GetType().Name}");

            var memberType = ModelTypeRegistry.GetTypeByAlias(typeDef);

            memberType = ResolveGenericType(
                memberType,
                context.HashType,
                context.PointType,
                null, // we should not associate TypeReference model members with any other data members
                null
            );

            var instance = Activator.CreateInstance(memberType);

            if (instance is null)
                throw new InvalidOperationException();

            dataMemberValueMap.Add(member, instance);
        }

        // build inlined objects recursively
        foreach (
            var member in dataMembers.Where(member =>
                member.IsDefined(typeof(ModelInlineAttribute))
            )
        )
        {
            var memberType = GetMemberType(member);

            if (memberType is null)
                throw new InvalidOperationException();

            var instance = BuildObject(memberType, context);

            dataMemberValueMap.Add(member, instance);
        }

        // handle generic types
        objectType = ResolveGenericType(
            objectType,
            context.HashType,
            context.PointType,
            member => dataMemberValueMap[member].GetType(),
            dataMembers
        );

        // find constructor and map its arguments
        object[]? constructorArgs = null;
        List<MemberInfo> unsatisfiedDataMembers = [.. dataMemberValueMap.Keys];

        ConstructorInfo? constructor = objectType
            .GetConstructors()
            .FirstOrDefault(ctor => ctor.IsDefined(typeof(ModelConstructorAttribute), false));

        if (constructor is not null)
        {
            var parameters = constructor.GetParameters();
            constructorArgs = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var ctorParameter = parameters[i];

                // search by attributes
                var suitableMember = unsatisfiedDataMembers.FirstOrDefault(member =>
                    member
                        .GetCustomAttribute<ModelInjectConstructorArgumentAttribute>()
                        ?.ParameterName == ctorParameter.Name
                );

                // if not found, search by name
                suitableMember ??= unsatisfiedDataMembers.FirstOrDefault(member =>
                    StringComparer.OrdinalIgnoreCase.Equals(member.Name, ctorParameter.Name)
                );

                if (suitableMember is null)
                    throw new InvalidOperationException();

                constructorArgs[i] = dataMemberValueMap[suitableMember];
                unsatisfiedDataMembers.Remove(suitableMember);
            }
        }
        else
        {
            constructor = objectType.GetConstructor(Type.EmptyTypes);
        }

        // create object via ctor or via activator in the case of structs
        object? resultInstance = null;
        if (constructor is not null)
            resultInstance = constructor.Invoke(constructorArgs);
        else if (objectType.IsValueType)
            resultInstance = Activator.CreateInstance(objectType);

        if (resultInstance is null)
            throw new InvalidOperationException();

        // set properties that left
        foreach (var member in unsatisfiedDataMembers)
        {
            // we must get updated MemberInfos after the generic type was resolved
            var updatedMember = objectType.GetMember(member.Name).First();

            SetMemberValue(updatedMember, resultInstance, dataMemberValueMap[member]);
        }

        return resultInstance;
    }

    private static NoiseModel ExtractModel(object fromObject, ModelExtractionContext context)
    {
        var type = fromObject.GetType();
        var typeAlias = ModelTypeRegistry.GetAliasByType(type);

        var model = new NoiseModel(context.CurrentId, typeAlias);

        context.AddModel(fromObject, model);

        FillModel(fromObject, model, context);

        return model;
    }

    private static void FillModel(object fromObject, NoiseModel model, ModelExtractionContext state)
    {
        var type = fromObject.GetType();

        foreach (var member in GetTypeDataMembers(type))
        {
            if (member.IsDefined(typeof(ModelReferenceAttribute), true))
            {
                var value = GetMemberValue(member, fromObject);

                if (value is null)
                    continue;

                if (state.ObjectIdMap.TryGetValue(value, out int foundId))
                {
                    model.Links.Add(member.Name, foundId);
                }
                else
                {
                    model.Links.Add(member.Name, state.CurrentId);
                    ExtractModel(value, state);
                }

                continue;
            }

            if (member.IsDefined(typeof(ModelTypeReferenceAttribute), true))
            {
                var memberType = GetMemberType(member);

                if (memberType is null)
                    throw new InvalidOperationException();

                var typeAlias = ModelTypeRegistry.GetAliasByType(memberType);

                model.Parameters.Add(member.Name, typeAlias);

                continue;
            }

            if (member.IsDefined(typeof(ModelInlineAttribute), true))
            {
                var toInline = GetMemberValue(member, fromObject);

                if (toInline is null)
                    continue;

                FillModel(toInline, model, state);

                continue;
            }

            if (member.IsDefined(typeof(ModelPropertyAttribute), true))
            {
                var value = GetMemberValue(member, fromObject);

                if (value is not null)
                    model.Parameters.Add(member.Name, value);
            }
        }
    }

    private static Type ResolveGenericType(
        Type targetType,
        Type hashType,
        Type dimensionType,
        Func<MemberInfo, Type>? resolveByMember,
        IEnumerable<MemberInfo>? relatedMembers
    )
    {
        if (targetType.IsGenericTypeDefinition == false)
            return targetType;

        Type ResolveArgument(Type genericArg)
        {
            if (genericArg.IsDefined(typeof(ModelHashAttribute)))
                return hashType;

            if (genericArg.IsDefined(typeof(ModelDimensionAttribute)))
                return dimensionType;

            if (resolveByMember is null)
                throw new InvalidOperationException();

            // search for other types in data members
            var relatedMember = relatedMembers?.FirstOrDefault(member =>
                GetMemberType(member) == genericArg
                && (
                    member.IsDefined(typeof(ModelTypeReferenceAttribute))
                    || member.IsDefined(typeof(ModelReferenceAttribute))
                )
            );

            if (relatedMember is not null)
                return resolveByMember(relatedMember);

            throw new InvalidOperationException();
        }

        var resolved = targetType.GetGenericArguments().Select(ResolveArgument).ToArray();

        return targetType.MakeGenericType(resolved);
    }

    // TODO: throw exception?
    private static Type? GetMemberType(MemberInfo member)
    {
        return member switch
        {
            PropertyInfo property => property.PropertyType,
            FieldInfo field => field.FieldType,
            _ => null,
        };
    }

    private static object? GetMemberValue(MemberInfo member, object owner)
    {
        return member switch
        {
            PropertyInfo property => property.GetValue(owner),
            FieldInfo field => field.GetValue(owner),
            _ => null,
        };
    }

    private static void SetMemberValue(MemberInfo member, object owner, object? value)
    {
        switch (member)
        {
            case PropertyInfo property:
                if (property.CanWrite == false)
                    throw new InvalidOperationException();

                property.SetValue(owner, value);
                break;
            case FieldInfo field:
                Console.WriteLine(member.Name);
                field.SetValue(owner, value);
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    private static List<MemberInfo> GetTypeDataMembers(Type fromType)
    {
        return
        [
            .. fromType
                .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(member =>
                    member.MemberType == MemberTypes.Field
                    || member.MemberType == MemberTypes.Property
                ),
        ];
    }

    private static List<T> SortByDependencies<T>(
        IEnumerable<T> source,
        Func<T, int> getId,
        Func<T, IEnumerable<int>> getDependencyIds
    )
    {
        var result = new List<T>();
        var visited = new HashSet<int>();
        var visiting = new HashSet<int>();

        var map = source.ToDictionary(getId);

        foreach (var item in source)
            Visit(getId(item), map, getDependencyIds, visited, visiting, result);

        return result;
    }

    private static void Visit<T>(
        int currentId,
        Dictionary<int, T> map,
        Func<T, IEnumerable<int>> getDependencyIds,
        HashSet<int> visited,
        HashSet<int> visiting,
        List<T> result
    )
    {
        if (visited.Contains(currentId))
            return;

        if (visiting.Contains(currentId))
            throw new InvalidOperationException(
                $"Cycle dependency at the model with id {currentId}"
            );

        if (map.TryGetValue(currentId, out var item) == false)
            throw new InvalidOperationException($"Missing dependency with id {currentId}");

        visiting.Add(currentId);

        foreach (var depId in getDependencyIds(item))
            Visit(depId, map, getDependencyIds, visited, visiting, result);

        visiting.Remove(currentId);
        visited.Add(currentId);

        result.Add(item);
    }
}
