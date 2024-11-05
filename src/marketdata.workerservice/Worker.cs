using marketdata.domain;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace marketdata.workerservice;

public class Worker(ILogger<Worker> logger, 
                    CancellationTokenSource sharedCts, 
                    IMarketSocket socket,
                    IMessageHandler messageHandler) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly CancellationTokenSource _sharedCts = sharedCts;
    private readonly IMarketSocket _socket = socket;
    private readonly IMessageHandler _messageHandler = messageHandler;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => _logger.LogDebug("Stopping WebSocket listener."));

        try
        {
            _socket.MessageReceived += async (sender, message) => await _messageHandler.Process(message);
            
            await _socket.Connect(stoppingToken);

            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _sharedCts.Token);

            await _socket.Listen(linkedTokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred: {Message}", ex.Message);
        }
    }

}
