using System.Threading.Channels;
using AMQPClient.Protocol;
using Microsoft.Extensions.Logging;

namespace AMQPClient;

internal class SystemChannel(Channel<object> trxChannel, IAmqpFrameSender frameSender, ConnectionImpl connectionImpl)
    : ChannelBase(frameSender, 0)
{
    private ChannelReader<object> RxChannel => trxChannel.Reader;
    private ILogger<SystemChannel> Logger { get; } = DefaultLoggerFactory.CreateLogger<SystemChannel>();
    private CancellationTokenSource _listenerCancellationSource;

    public async Task CloseConnectionAsync()
    {
        var method = new Protocol.Classes.Connection.Close()
        {
            ReplyCode = 320,
            ReplyText = "Closed by peer"
        };
        await CallMethodAsync<Protocol.Classes.Connection.CloseOk>(method, checkForClosed: false);
        await _listenerCancellationSource.CancelAsync();
    }

    internal void StartListener()
    {
        _listenerCancellationSource = new CancellationTokenSource();
        var cancellationToken = _listenerCancellationSource.Token;
        Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Logger.LogDebug("Stopping listener loop...");
                        break;
                    }

                    Logger.LogDebug("Waiting for next frame....");
                    var nextFrame = await RxChannel.ReadAsync(cancellationToken);

                    switch (nextFrame)
                    {
                        case AmqpMethodFrame frame:
                            if (frame.Method.IsAsyncResponse)
                            {
                                if (!SyncMethodHandles.TryDequeue(out var result))
                                    throw new Exception("No task completion source found");

                                result.SetResult(new MethodResult(frame));
                                break;
                            }

                            switch (frame.Method)
                            {
                                case Protocol.Classes.Connection.Close closeMethod:
                                    Logger.LogCritical("Closing connection\n {code}, {text} ", closeMethod.ReplyCode, closeMethod.ReplyText);
                                    var result = new MethodResult(null, closeMethod.ReplyCode, closeMethod.ReplyText);
                                    
                                    if (SyncMethodHandles.TryDequeue(out var source))
                                    {
                                        source.SetResult(result);
                                    }

                                    LastErrorResult = result;
                                    State = ChannelState.Failed;
                                    await CallMethodAsync(new Protocol.Classes.Connection.CloseOk());
                                    await connectionImpl.CloseAsync();
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                            break;
                        default:
                            throw new Exception("Unknown frame");
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                Logger.LogWarning("Frame loop exited");
            } catch (Exception e)
            {
                Logger.LogError("StartListener loop failed with {msg}", e.Message);
                throw;
            }

            return Task.CompletedTask;
        }, cancellationToken);
    }
}