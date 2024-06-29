namespace AMQPProtocolGenerators;

public static class DomainUtils
{
    public static string ToPascalCase(string domain)
    {
        return string.Join(null, domain.Split("-").Select(str => Char.ToUpper(str[0]) + str[1..]));
    }

    public static string ToCamelCase(string domain)
    {
        return string.Join(null, domain.Split("-").Select((str, i) => i > 0  ? Char.ToUpper(str[0]) + str[1..] : str));
    }
}