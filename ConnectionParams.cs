namespace AMQPClient;

public class ConnectionParams
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Vhost { get; set; } = "";
    public string User { get; set; } = "root";
    public string Password { get; set; } = "";
}