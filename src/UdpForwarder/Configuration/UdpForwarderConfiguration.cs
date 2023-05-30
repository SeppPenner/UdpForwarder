// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UdpForwarderConfiguration.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class containing the UDP forwarder configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace UdpForwarder.Configuration;

/// <summary>
/// A class containing the UDP forwarder configuration.
/// </summary>
public sealed class UdpForwarderConfiguration
{
    /// <summary>
    /// Gets or sets the listen port.
    /// </summary>
    public int ListenPort { get; set; }

    /// <summary>
    /// Gets or sets the forward port.
    /// </summary>
    public int ForwardPort { get; set; }

    /// <summary>
    /// The forward host.
    /// </summary>
    public string ForwardHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets the forward endpoint.
    /// </summary>
    [JsonIgnore]
    public IPEndPoint? ForwardEndpoint { get; private set; }

    /// <summary>
    /// Gets or sets the timeout seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets the timeout.
    /// </summary>
    [JsonIgnore]
    public TimeSpan Timeout => TimeSpan.FromSeconds(this.TimeoutSeconds);

    /// <summary>
    /// Gets or sets the service delay in milliseconds.
    /// </summary>
    public int ServiceDelayInMilliSeconds { get; set; } = 3000;

    /// <summary>
    /// Gets or sets the heartbeat interval in milliseconds.
    /// </summary>
    public int HeartbeatIntervalInMilliSeconds { get; set; } = 30000;

    /// <summary>
    /// Gets a value indicating whether the configuration is valid or not.
    /// </summary>
    /// <returns>A <see cref="bool"/> value indicating whether the configuration is valid or not.</returns>
    public bool IsValid()
    {
        if (!this.ListenPort.IsPortValid())
        {
            throw new ConfigurationException("The listen port is invalid.");
        }

        if (!this.ForwardPort.IsPortValid())
        {
            throw new ConfigurationException("The forward port is invalid.");
        }

        if (string.IsNullOrWhiteSpace(this.ForwardHost))
        {
            throw new ConfigurationException("The forward host is invalid.");
        }

        if (this.ServiceDelayInMilliSeconds <= 0)
        {
            throw new ConfigurationException("The service delay is invalid.");
        }

        if (this.HeartbeatIntervalInMilliSeconds <= 0)
        {
            throw new ConfigurationException("The heartbeat interval is invalid.");
        }

        if (!IPAddress.TryParse(this.ForwardHost, out var ipAddress))
        {
            throw new ConfigurationException("The forward host is invalid.");
        }

        if (this.TimeoutSeconds < 0)
        {
            this.TimeoutSeconds = 30;
        }

        this.ForwardEndpoint = new(ipAddress, this.ForwardPort);

        return true;
    }
}
