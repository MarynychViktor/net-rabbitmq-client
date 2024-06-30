# RabbitMQ client
The client was built around AMQP 0-9-1 protocol and provides all necessary methods to work with the RabbitMQ server.
> [!NOTE]  
> The main idea behind this project is to dive into the protocol and write a client from scratch that could read/write binary chunks of data, interpret them according to the AMQP-0-9-1 protocol rules, and communicate with a real RabbitMQ server.

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

var queueName = await channel.QueueDeclareAsync("my-queue", durable: true);
await channel.ExchangeDeclareAsync(exchangeName);

await channel.QueueBindAsync(queueName, exchangeName, routingKey);
```
### 3. Publish message
```c#
await channel.BasicPublishAsync(exchangeName, routingKey, new Message("Hello from app!"u8.ToArray()));
```
### 4. Consume message
#### Using async consumer
```c#
var consumerId = await channel.BasicConsumeAsync(queueName, async (message) =>
{
  Console.WriteLine($"Message: {Encoding.Default.GetString(message.Payload.Content)}");
  await channel.BasicAck(message);
});
```
#### Manual consuming
```c#
var message = await channel.BasicGetAsync(queueName);

if (message != null) {
    Console.WriteLine($"Message: {Encoding.Default.GetString(message.Payload.Content)}");
    await channel.BasicAckAsync(message);
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


