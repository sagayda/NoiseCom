using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using NoiseCom.Noise.Generators;
using NoiseCom.Noise.Hash;

namespace NoiseCom.Serialization;

public static class ModelTypeRegistry
{
    private static readonly Dictionary<string, Type> _aliasToType = [];
    private static readonly Dictionary<Type, string> _typeToAlias = [];

    public static bool Initialized { get; private set; }

    public static void Initialize()
    {
        if (Initialized)
            return;

        foreach (
            var type in ReflectionHelper.EnumerateTypes(assembly =>
                assembly.FullName?.StartsWith("System") == false
                && assembly.FullName?.StartsWith("Microsoft") == false
            )
        )
        {
            var attr = type.GetCustomAttribute<ModelTypeAttribute>(false);
            if (attr != null)
            {
                if (_aliasToType.ContainsKey(attr.Alias))
                    throw new InvalidOperationException();

                _aliasToType[attr.Alias] = type;
                _typeToAlias[type] = attr.Alias;
            }
        }

        Initialized = true;
    }

    public static List<Type> GetDefinedTypes()
    {
        Initialize();

        return [.. _aliasToType.Values];
    }

    public static Type GetTypeByAlias(string alias, bool allowUndefined = true)
    {
        Initialize();

        if (_aliasToType.TryGetValue(alias, out var type))
            return type;

        if (allowUndefined == false)
            throw new KeyNotFoundException($"Type with model alias '{alias}' is not registered");

        var foundType = FindTypeByAlias(alias);

        return foundType
            ?? throw new KeyNotFoundException($"Type with model alias '{alias}' can not be found");
    }

    public static bool TryGetTypeByAlias(
        string alias,
        [NotNullWhen(true)] out Type? type,
        bool allowUndefined = true
    )
    {
        Initialize();

        if (allowUndefined == false)
            return _aliasToType.TryGetValue(alias, out type);

        type = FindTypeByAlias(alias);

        return type is not null;
    }

    public static string GetAliasByType(Type type, bool allowUndefined = true)
    {
        Initialize();

        if (_typeToAlias.TryGetValue(type, out var alias))
            return alias;

        if (allowUndefined == false)
            throw new KeyNotFoundException($"Type '{type.Name}' is not marked with [ModelType].");

        return GetAlias(type);
    }

    public static bool TryGetAliasByType(
        Type type,
        [NotNullWhen(true)] out string? alias,
        bool allowUndefined = true
    )
    {
        Initialize();

        if (allowUndefined == false)
            return _typeToAlias.TryGetValue(type, out alias);

        alias = GetAlias(type);
        return true;
    }

    public static List<Type> GetDefinedGenerators()
    {
        Initialize();

        return ReflectionHelper.FindImplementations(typeof(INoise<,>), _aliasToType.Values);
    }

    public static List<Type> GetDefinedGenerators(Type hashType, Type dimensionType)
    {
        Initialize();

        return ReflectionHelper.FindImplementations(
            typeof(INoise<,>).MakeGenericType([hashType, dimensionType]),
            _aliasToType.Values
        );
    }

    public static List<Type> GetDefinedHashes()
    {
        Initialize();

        return ReflectionHelper.FindImplementations(typeof(IHash<>), _aliasToType.Values);
    }

    public static List<Type> GetDefinedDimensions()
    {
        Initialize();

        return ReflectionHelper.FindImplementations(
            typeof(IDimensionalPoint<>),
            _aliasToType.Values
        );
    }

    private static string GetAlias(Type forType)
    {
        string cleanName = forType.Name;

        int backtickIndex = cleanName.IndexOf('`');
        if (backtickIndex > 0)
            cleanName = cleanName[..backtickIndex];

        return cleanName;
    }

    private static Type? FindTypeByAlias(string targetAlias)
    {
        var matchedTypes = new List<Type>();

        foreach (var type in ReflectionHelper.EnumerateTypes())
        {
            var alias = GetAlias(type);

            if (string.Equals(alias, targetAlias, StringComparison.OrdinalIgnoreCase))
                matchedTypes.Add(type);
        }

        if (matchedTypes.Count == 0)
            return null;

        if (matchedTypes.Count > 1)
        {
            var typeNames = string.Join(", ", matchedTypes.Select(t => t.FullName));
            throw new AmbiguousMatchException(
                $"Found multiple types with alias '{targetAlias}': {typeNames}"
            );
        }

        return matchedTypes[0];
    }
}
