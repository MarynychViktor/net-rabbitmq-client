## Rabbitmq client
#### AMQP 0-9-1 protocol

Main goal of this project is to create basic implementation of amqp-0-9-1 protocol (aka rabbitmq client) for learning
purposes

### Usage example
**1. Connection** ✅

Connection creation with basic set of options 
> TODO: add support for auth mechanism selection, TLS, advanced connections options
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

//....
```
**2. Channel** ✅
```c#
//....
var channel = await connection.CreateChannelAsync();

// Do something with channel

await channel.Close();
```
**3. Exchange and Queue**
  1. **Exchange** ✅
  2. **Queue** ❌
  - [x] Implemented methods
    - Declare
    - Bind

  - [ ] TODO:
    - Unbind
    - Purge
    - Delete
```c#
//....
var exchangeName = "foo-exchange";
var routingKey = "foo-bar-key";

var queueName = await channel.QueueDeclare();

await channel.ExchangeDeclare(exchangeName);

await channel.QueueBind(queueName, exchangeName, routingKey);
//....
```
**4. Basic** ❌
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

```c#
//...
await channel.BasicConsume(queueName, async (message) =>
{
    Console.WriteLine($"Received message {Encoding.Default.GetString(message.Payload.Content)}");
    await channel.BasicAck(message);
});

await channel.BasicPublishAsync(exchangeName, routingKey, new Message("Hello from app!"u8.ToArray()));
```
**5. Tx** ❌

Not implemented


### Roadmap

- [x] Handshake with AMQP server
- [ ] Connection
- [x] Channel
- [ ] Queue
- [x] Exchange
- [ ] Basic

