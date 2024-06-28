## RabbitMQ client
The client was built around AMQP 0-9-1 protocol and provides all necessary methods to work with the RabbitMQ server.

### Usage example
#### Create connection
```c#
var connectionFactory = new ConnectionFactory();
var connection = await connectionFactory.CreateConnectionAsync(options =>
{
    options.Host = "cow.rmq2.cloudamqp.com"; // "localhost";
    options.Port = 5672;
    options.Vhost = "foo";
    options.User = "bar";
    options.Password = "secret";
});
```
#### Create channel
```c#
var channel = await connection.CreateChannelAsync();

// Do something with channel

await channel.Close();
```
#### Declare exchange and bind queue
```c#
var exchangeName = "foo-exchange";
var routingKey = "foo-bar-key";

var queueName = await channel.QueueDeclare();

await channel.ExchangeDeclare(exchangeName);

await channel.QueueBind(queueName, exchangeName, routingKey);
```
#### Publish message
```c#
await channel.BasicPublishAsync(exchangeName, routingKey, new Message("Hello from app!"u8.ToArray()));
```
#### Consume message
```c#
await channel.BasicConsume(queueName, async (message) =>
{
Console.WriteLine($"Received message {Encoding.Default.GetString(message.Payload.Content)}");
await channel.BasicAck(message);
});
```

### Implemented features
**1. Connection** ✅
Connection creation with basic set of options 
> TODO: add support for auth mechanism selection, TLS, advanced connections options

**2. Channel** ✅

**3. Exchange** ✅

**4. Queue** ✅

**5. Basic** ❌

Some advanced methods not implemented
- [x] Implemented methods
  - Consume
  - Publish
  - Deliver
  - Ack
  - Reject

- [ ] TODO:
  - Qos
  - Nack
  - Return
  - Recover

**6. Tx** - Not implemented


