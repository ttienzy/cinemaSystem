namespace Gateway.API.Configuration;

public sealed class GatewayRuntimeOptions
{
    public string? OcelotProfile { get; set; }

    public GatewayCorsOptions Cors { get; set; } = new();

    public GatewayCacheOptions Cache { get; set; } = new();

    public GatewayServiceDiscoveryOptions ServiceDiscovery { get; set; } = new();
}

public sealed class GatewayCorsOptions
{
    public string[] AllowedOrigins { get; set; } = [];
}

public sealed class GatewayCacheOptions
{
    public GatewayRedisCacheOptions Redis { get; set; } = new();
}

public sealed class GatewayRedisCacheOptions
{
    public bool Enabled { get; set; } = true;

    public string ConnectionString { get; set; } = "127.0.0.1:6379";

    public int Database { get; set; }

    public bool EnableKeyspaceNotifications { get; set; }

    public int MaxRetries { get; set; } = 100;

    public int RetryTimeoutMs { get; set; } = 50;
}

public sealed class GatewayServiceDiscoveryOptions
{
    public bool Enabled { get; set; }
}
