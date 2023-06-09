#pragma warning disable IDE0065 // Die using-Anweisung wurde falsch platziert.
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Net;
global using System.Net.Sockets;
global using System.Reflection;
global using System.Text.Json.Serialization;

global using Polly;
global using Polly.Retry;

global using UdpForwarder.Configuration;
global using UdpForwarder.Constants;
global using UdpForwarder.Exceptions;
global using UdpForwarder.Extensions;

global using Serilog;
global using Serilog.Core;
global using Serilog.Events;
global using Serilog.Exceptions;

global using ILogger = Serilog.ILogger;
#pragma warning restore IDE0065 // Die using-Anweisung wurde falsch platziert.