## Basic usage

### JSON configuration (Adjust this to your needs)
```json
{
    "AllowedHosts": "*",
    "UdpForwarderService": {
        "ServiceDelayInMilliSeconds": 30000,
        "HeartbeatIntervalInMilliSeconds": 30000,
        "ListenPort": 1700,
        "ForwardPort": 1800,
        "ForwardHost": "192.168.2.205",
        "TimeoutSeconds": 30
    }
}
```
