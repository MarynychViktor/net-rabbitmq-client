# RabbitMQ client
The client was built around AMQP 0-9-1 protocol and provides all necessary methods to work with the RabbitMQ server.

## Usage
### 1. Set-up connection and channel
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
var channel = await connection.CreateChannelAsync();
```
### 2. Declare exchange, queue and bind them
```c#
var exchangeName = "my-exchange";
var routingKey = "some-key";

var queueName = await channel.QueueDeclare("my-queue", durable: true);
await channel.ExchangeDeclare(exchangeName);

await channel.QueueBind(queueName, exchangeName, routingKey);
```
### 3. Publish message
```c#
await channel.BasicPublishAsync(exchangeName, routingKey, new Message("Hello from app!"u8.ToArray()));
```
### 4. Consume message
#### Using async consumer
```c#
await channel.BasicConsume(queueName, async (message) =>
{
  Console.WriteLine($"Message: {Encoding.Default.GetString(message.Payload.Content)}");
  await channel.BasicAck(message);
});
```
#### Manual consuming
```c#
var message = await channel.BasicGet(queueName);

if (message != null) {
    Console.WriteLine($"Message: {Encoding.Default.GetString(message.Payload.Content)}");
    await channel.BasicAck(message);
}
```

## Currently implemented features
**1. Connection** ✅
> TODO: add support for advanced connection options

**2. Channel** ✅

**3. Exchange** ✅

**4. Queue** ✅

**5. Basic** ✅

**6. Tx**

Not implemented


