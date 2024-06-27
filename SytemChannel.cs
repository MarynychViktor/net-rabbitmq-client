using System.Collections.Concurrent;
using System.Threading.Channels;
using AMQPClient.Protocol;
using AMQPClient.Protocol.Methods;
using AMQPClient.Protocol.Methods.Basic;
using AMQPClient.Protocol.Methods.Channels;
using AMQPClient.Protocol.Methods.Exchanges;
using AMQPClient.Protocol.Methods.Queues;
using AMQPClient.Protocol.Types;
using Microsoft.Extensions.Logging;

namespace AMQPClient;

public class SystemChannel(Channel<object> trxChannel, IAmqpFrameSender frameSender)
    : ChannelBase(frameSender, 0)
{
    private readonly Dictionary<string, Action<AmqpEnvelope>> _consumersByTags = new();
    private ChannelReader<object> RxChannel => trxChannel.Reader;
    private ILogger<ChannelImpl> Logger { get; } = DefaultLoggerFactory.CreateLogger<ChannelImpl>();
    private CancellationTokenSource _listenerCancellationSource;

    public async Task Close()
    {
        var method = new ChannelCloseMethod();
        IsClosed = true;
        await CallMethodAsync<ChannelCloseOkMethod>(ChannelId, method, checkForClosed: false);
        await _listenerCancellationSource.CancelAsync();
    }


    internal void StartListener()
    {
        _listenerCancellationSource = new CancellationTokenSource();
        var cancellationToken = _listenerCancellationSource.Token;
        Task.Run(async () =>
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.LogInformation("Stopping listener loop...");
                    break;
                }

                Logger.LogDebug("Waiting for next frame....");
                var nextFrame = await RxChannel.ReadAsync(cancellationToken);

                switch (nextFrame)
                {
                    case AmqpMethodFrame frame:
                        Logger.LogDebug("AmqpMethod frame received with type {type}", frame.Method);
                        if (frame.Method.IsAsyncResponse())
                        {
                            if (!SyncMethodHandles[ChannelId]
                                    .TryDequeue(out var result))
                                throw new Exception("No task completion source found");

                            result.SetResult(frame);
                        }

                        if (frame.Method.HasBody())
                        {
                            // TODO: remove envolope class
                            var envelopePayload = new AmqpEnvelopePayload(
                                frame.Properties,
                                frame.Body
                            );

                            var envelope = new AmqpEnvelope(frame.Method, envelopePayload);
                            if (envelope.Method is BasicDeliver method)
                                _consumersByTags[method.ConsumerTag].Invoke(envelope);
                        }

                        break;
                    default:
                        throw new Exception("Unknown frame");
                }
            }

            return Task.CompletedTask;
        }, cancellationToken);
    }
}