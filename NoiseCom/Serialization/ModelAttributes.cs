namespace NoiseCom.Serialization;

// generics
[AttributeUsage(AttributeTargets.GenericParameter)]
public class ModelHashAttribute : Attribute;

[AttributeUsage(AttributeTargets.GenericParameter)]
public class ModelDimensionAttribute : Attribute;

// construction
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
public class ModelConstructorAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ModelInjectConstructorArgumentAttribute(string parameterName) : Attribute
{
    public string ParameterName { get; } = parameterName;
}

// serialization
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ModelPropertyAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ModelReferenceAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ModelInlineAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ModelTypeReferenceAttribute : Attribute;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct,
    AllowMultiple = false,
    Inherited = false
)]
public class ModelTypeAttribute(string modelTypeAlias) : Attribute
{
    public string Alias { get; } = modelTypeAlias;
}
