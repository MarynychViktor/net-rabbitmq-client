## Rabbitmq client
#### AMQP 0-9-1 protocol

Main goal of this project is to create basic implementation of amqp-0-9-1 protocol (aka rabbitmq client) for learning
purposes

### Usage example
```c#
var connectionFactory = new ConnectionFactory();
var connection = await connectionFactory.CreateConnectionAsync(options =>
{
    options.Host = "localhost";
    options.Port = 5672;
    options.Vhost = "foo";
    options.User = "bar";
    options.Password = "secret";
});

var channel = await connection.CreateChannelAsync();
var exchangeName = "foo-exchange";
var routingKey = "foo-bar-key";

var queueName = await channel.QueueDeclare();
await channel.ExchangeDeclare(exchangeName);
await channel.QueueBind(queueName, exchangeName, routingKey);

await channel.BasicConsume(queueName, async (envelope) =>
{
    Console.WriteLine($"Received message {Encoding.Default.GetString(envelope.Payload.Content)}");
    await channel.BasicAck(envelope);
});

await channel.BasicPublishAsync(exchangeName, routingKey, new Message("Hello from app!"u8.ToArray()));
```
### Roadmap

- [x] Handshake with AMQP server
- [ ] Connection: implemented methods - Open, Start, Tune, Close, todo: ...
- [x] Channel: Open, Close, Flow
- [ ] Queue: implemented methods - Declare, Bind, todo: ...
- [x] Exchange: implemented methods - Declare, Delete
- [ ] Basic: implemented methods - Publish, Consume, Ack, todo: ...

