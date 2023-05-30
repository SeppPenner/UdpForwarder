// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UdpForwarder.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The UDP forwarder class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace UdpForwarder;

/// <summary>
/// The UDP forwarder class.
/// </summary>
internal sealed class UdpForwarder
{
    /// <summary>
    /// The UDP forwarder configuration.
    /// </summary>
    private readonly UdpForwarderConfiguration udpForwarderConfiguration;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// The UDP connection retry policy.
    /// </summary>
    private static readonly RetryPolicy RetryPolicy = Policy
              .Handle<Exception>()
              .WaitAndRetryForever(
                sleepDurationProvider: retryAttempt =>
                {
                    if (retryAttempt < 5)
                    {
                        var timeToWait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                        Log.Logger.Error("Waiting {Seconds} seconds, request failed", timeToWait.TotalSeconds);
                        return timeToWait;
                    }
                    else
                    {
                        var timeToWait = TimeSpan.FromSeconds(10);
                        Log.Logger.Error("Waiting {Seconds} seconds, request failed", timeToWait.TotalSeconds);
                        return timeToWait;
                    }
                },
                onRetry: (ex, _) =>
                {
                    Log.Logger.Error(ex, "An error occured: {Message}", ex.Message);
                });

    /// <summary>
    /// The cancellation token source.
    /// </summary>
    private CancellationTokenSource cancellationTokenSource = new();

    /// <summary>
    /// The listening UDP client.
    /// </summary>
    private UdpClient? listenClient;

    /// <summary>
    /// The forwarding UDP client.
    /// </summary>
    private UdpClient? forwardClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UdpForwarder"/> class.
    /// </summary>
    /// <param name="udpForwarderConfiguration">The UDP forwarder configuration.</param>
    /// <param name="logger">The logger.</param>
    public UdpForwarder(UdpForwarderConfiguration udpForwarderConfiguration, ILogger logger)
    {
        this.udpForwarderConfiguration = udpForwarderConfiguration;
        this.logger = logger;
    }

    /// <summary>
    /// Starts the UDP forwarding.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task StartForwarding(CancellationToken cancellationToken)
    {
        this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        this.logger.Information("Starting UDP forwarding.");

        try
        {
            while (true)
            {
                // Check if the clients are connected.
                while (this.listenClient is null || this.forwardClient is null)
                {
                    // Clients are not yet connected, try to reconnect with Polly.
                    await RetryPolicy.Execute(this.Reconnect);
                }

                // Get data in receive task.
                var receiveTask = this.listenClient.ReceiveAsync();

                // Wait for data or timeout.
                var completedTask = await Task.WhenAny(receiveTask, Task.Delay(this.udpForwarderConfiguration.Timeout, this.cancellationTokenSource.Token));

                if (completedTask == receiveTask)
                {
                    // Data was received.
                    var receivedResult = await receiveTask;
                    var receivedData = receivedResult.Buffer;

                    // Forward data.
                    await this.forwardClient.SendAsync(receivedData, receivedData.Length, this.udpForwarderConfiguration.ForwardEndpoint);

                    this.logger.Information("Forwarded {Bytes} bytes", receivedData.Length);

                    // Reset the timeout.
                    this.cancellationTokenSource.CancelAfter(this.udpForwarderConfiguration.Timeout);
                }
                else
                {
                    // Timeout reached, reconnect.
                    this.logger.Information("Reached timeout, reconnecting now");
                    await this.Reconnect();
                }
            }
        }
        catch (SocketException ex)
        {
#pragma warning disable Serilog004 // Constant MessageTemplate verifier
            this.logger.Error(ex, ex.Message);
#pragma warning restore Serilog004 // Constant MessageTemplate verifier
        }
        finally
        {
            // Close connections.
            await this.CloseConnections();
        }
    }

    /// <summary>
    /// Stops the UDP forwarding.
    /// </summary>
    public void StopForwarding(CancellationToken cancellationToken)
    {
        this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        this.cancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// Reconnects the UDP clients.
    /// </summary>
    private async Task Reconnect()
    {
        await this.CloseConnections();
        this.Connect();
    }

    /// <summary>
    /// Closes the UDP connections.
    /// </summary>
    private async Task CloseConnections()
    {
        this.cancellationTokenSource?.Cancel();

        // Dispose the listen client.
        this.listenClient?.Close();
        this.listenClient?.Dispose();
        this.listenClient = null;

        // Dispose the forward client.
        this.forwardClient?.Close();
        this.forwardClient?.Dispose();
        this.forwardClient = null;

        // Wait for closed connections.
        await Task.Delay(100);
    }

    /// <summary>
    /// Connects the UDP clients.
    /// </summary>
    private void Connect()
    {
        this.listenClient = new UdpClient(this.udpForwarderConfiguration.ListenPort);
        this.forwardClient = new UdpClient();

        this.logger.Information("Clients connected, forwarding from {ListenPort} to {ForwardEndpoint}",
            this.udpForwarderConfiguration.ListenPort,
            this.udpForwarderConfiguration.ForwardEndpoint);
    }
}
