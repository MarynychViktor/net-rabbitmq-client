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
        { "bit", "byte" },
        { "octet", "byte" },
        { "short", "short" },
        { "long", "int" },
        { "longlong", "long" },
        { "shortstr", "string" },
        { "longstr", "string" },
        { "timestamp", "timestamp" },
        { "table", "Dictionary<string, object>" },
    };
    private static readonly Dictionary<string, string> DomainToWriterMap = new()
    {
        { "class-id", "Short" },
        { "consumer-tag", "ShortStr" },
        { "delivery-tag", "Int" },
        { "exchange-name", "ShortStr" },
        { "method-id", "Short" },
        { "no-ack", "Bit" },
        { "no-local", "Bit" },
        { "no-wait", "Bit" },
        { "path", "ShortStr" },
        { "peer-properties", "FieldTable" },
        { "queue-name", "ShortStr" },
        { "redelivered", "Bit" },
        { "message-count", "Int" },
        { "reply-code", "Short" },
        { "reply-text", "ShortStr" },
        { "bit", "Bit" },
        { "octet", "Bit" },
        { "short", "Short" },
        { "long", "Int" },
        { "longlong", "Long" },
        { "shortstr", "ShortStr" },
        { "longstr", "LongStr" },
        { "timestamp", "timestamp" },
        { "table", "FieldTable" },
    };

    public static string GetDomainWriterMethod(string domain)
    {
        return $"Write{StrFormatUtils.ToPascalCase(DomainToWriterMap[domain])}";
    }
    
    public static string GetDomainReaderMethod(string domain)
    {
        return $"Read{StrFormatUtils.ToPascalCase(DomainToWriterMap[domain])}";
    }
}