namespace Castle.Windsor.Facilities.AspNetCore.Resolvers;

internal static class GenericTypeExtensions
{
    public static bool MatchesType(this Type type, Type otherType)
    {
        var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        var genericOtherType = otherType.IsGenericType ? otherType.GetGenericTypeDefinition() : otherType;
        return genericType == genericOtherType;
    }
}