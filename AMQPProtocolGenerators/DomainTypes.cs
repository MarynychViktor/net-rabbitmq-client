namespace AMQPProtocolGenerators;

public static class DomainTypes
{
    public static readonly Dictionary<string, string> DomainToInternalTypeMap = new()
    {
        { "class-id", "short" },
        { "consumer-tag", "string" },
        { "delivery-tag", "int" },
        { "exchange-name", "string" },
        { "method-id", "short" },
        { "no-ack", "byte" },
        { "no-local", "byte" },
        { "no-wait", "byte" },
        { "path", "string" },
        { "peer-properties", "Dictionary<string, object>" },
        { "queue-name", "string" },
        { "redelivered", "byte" },
        { "message-count", "int" },
        { "reply-code", "short" },
        { "reply-text", "string" },
        { "bit", "bool" },
        { "octet", "byte" },
        { "short", "short" },
        { "long", "int" },
        { "longlong", "long" },
        { "shortstr", "string" },
        { "longstr", "string" },
        { "timestamp", "timestamp" },
        { "table", "Dictionary<string, object>" },
    };
    private static readonly Dictionary<string, string> DomainToVisitorMap = new()
    {
        { "class-id", "short" },
        { "consumer-tag", "shortstr" },
        { "delivery-tag", "int" },
        { "exchange-name", "shortstr" },
        { "method-id", "short" },
        { "no-ack", "bit" },
        { "no-local", "bit" },
        { "no-wait", "bit" },
        { "path", "shortstr" },
        { "peer-properties", "table" },
        { "queue-name", "shortstr" },
        { "redelivered", "bit" },
        { "message-count", "int" },
        { "reply-code", "short" },
        { "reply-text", "shortstr" },
        { "bit", "bit" },
        { "octet", "bit" },
        { "short", "short" },
        { "long", "int" },
        { "longlong", "long" },
        { "shortstr", "shortstr" },
        { "longstr", "longstr" },
        { "timestamp", "timestamp" },
        { "table", "table" },
    };

    public static string GetDomainVisitor(string domain)
    {
        return DomainUtils.ToPascalCase(DomainToVisitorMap[domain]);
    }
}