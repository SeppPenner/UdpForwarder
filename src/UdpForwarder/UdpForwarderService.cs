// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UdpForwarderService.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The UDP forwarder service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace UdpForwarder;

/// <seealso cref="BackgroundService"/>
/// <inheritdoc cref="BackgroundService"/>
/// <summary>
/// The UDP forwarder service.
/// </summary>
internal sealed class UdpForwarderService : BackgroundService
{
    /// <summary>
    /// The stopwatch for the application lifetime.
    /// </summary>
    private readonly Stopwatch uptimeStopWatch = Stopwatch.StartNew();

    /// <summary>
    /// Gets or sets the last heartbeat timestamp.
    /// </summary>
    private DateTimeOffset LastHeartbeatAt { get; set; }

    /// <summary>
    /// Gets or sets the logger.
    /// </summary>
    private ILogger Logger { get; set; } = Log.Logger;

    /// <summary>
    /// Gets or sets the service configuration.
    /// </summary>
    private UdpForwarderConfiguration ServiceConfiguration { get; set; }

    /// <summary>
    /// The UDP forwarder.
    /// </summary>
    private readonly UdpForwarder udpForwarder;

    /// <summary>
    /// Initializes a new instance of the <see cref="UdpForwarderService"/> class.
    /// </summary>
    /// <param name="configuration">The UDP forwarder configuration.</param>
    public UdpForwarderService(UdpForwarderConfiguration configuration)
    {
        // Load the configuration.
        this.ServiceConfiguration = configuration;

        // Create the logger.
        this.Logger = LoggerConfig.GetLoggerConfiguration(nameof(UdpForwarderService))
            .WriteTo.Sink((ILogEventSink)Log.Logger)
            .CreateLogger();

        // Create the UDP forwarder.
        this.udpForwarder = new UdpForwarder(configuration, this.Logger);
    }

    /// <inheritdoc cref="BackgroundService"/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        this.Logger.Information("Starting UDP forwarder service");
        _ = Task.Run(async () =>
        {
            await this.udpForwarder.StartForwarding(cancellationToken);
        }, cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    /// <inheritdoc cref="BackgroundService"/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.Logger.Information("Stopping UDP forwarder service");
        this.udpForwarder.StopForwarding(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    /// <inheritdoc cref="BackgroundService"/>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this.Logger.Information("Executing UDP forwarder service");

        while (!cancellationToken.IsCancellationRequested)
        {
            // Run the heartbeat and log some memory information.
            this.LogMemoryInformation();
            await Task.Delay(this.ServiceConfiguration.ServiceDelayInMilliSeconds, cancellationToken);
        }
    }

    /// <summary>
    /// Logs the memory information.
    /// </summary>
    private void LogMemoryInformation()
    {
        var totalMemory = GC.GetTotalMemory(false);
        var memoryInfo = GC.GetGCMemoryInfo();
        var totalMemoryFormatted = SystemGlobals.GetValueWithUnitByteSize(totalMemory);
        var heapSizeFormatted = SystemGlobals.GetValueWithUnitByteSize(memoryInfo.HeapSizeBytes);
        var memoryLoadFormatted = SystemGlobals.GetValueWithUnitByteSize(memoryInfo.MemoryLoadBytes);
        this.Logger.Information(
            "Heartbeat for service {ServiceName}: Total {Total}, heap size: {HeapSize}, memory load: {MemoryLoad}, uptime {Uptime}",
            Program.ServiceName, totalMemoryFormatted, heapSizeFormatted, memoryLoadFormatted, this.uptimeStopWatch.Elapsed);
    }
}
