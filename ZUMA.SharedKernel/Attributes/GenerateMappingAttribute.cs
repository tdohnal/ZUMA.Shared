[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateMappingAttribute : Attribute
{
    public Type TargetType { get; }
    public GenerateMappingAttribute(Type targetType)
        => TargetType = targetType;
}