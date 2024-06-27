using System.Threading.Channels;
using AMQPClient.Protocol;
using AMQPClient.Protocol.Methods.Connection;
using Microsoft.Extensions.Logging;

namespace AMQPClient;

public class SystemChannel(Channel<object> trxChannel, IAmqpFrameSender frameSender, InternalConnection connection)
    : ChannelBase(frameSender, 0)
{
    private ChannelReader<object> RxChannel => trxChannel.Reader;
    private ILogger<SystemChannel> Logger { get; } = DefaultLoggerFactory.CreateLogger<SystemChannel>();
    private CancellationTokenSource _listenerCancellationSource;

    public async Task CloseConnection()
    {
        var method = new ConnectionClose();
        await CallMethodAsync<ConnectionCloseOk>(method, checkForClosed: false);
        await _listenerCancellationSource.CancelAsync();
        await connection.CloseAsync();
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
                            if (frame.Method.IsAsyncResponse())
                            {
                                if (!SyncMethodHandles.TryDequeue(out var result))
                                    throw new Exception("No task completion source found");

                                result.SetResult(new MethodResult(frame.Method));
                                break;
                            }

                            switch (frame.Method)
                            {
                                case ConnectionClose closeMethod:
                                    Logger.LogCritical("Closing connection\n {code}, {text} ", closeMethod.ReplyCode, closeMethod.ReplyText);
                                    if (SyncMethodHandles.TryDequeue(out var result))
                                    {
                                        result.SetResult(new MethodResult(null, closeMethod.ReplyCode, closeMethod.ReplyText));
                                    }

                                    await CallMethodAsync(new ConnectionCloseOk());
                                    await connection.CloseAsync();
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