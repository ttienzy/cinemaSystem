namespace Cinema.Shared.Models;

public sealed class ConsulOptions
{
    public const string SectionName = "Consul";

    public bool Enabled { get; set; }

    public string AgentAddress { get; set; } = "http://localhost:8500";

    public string ServiceName { get; set; } = string.Empty;

    public string? ServiceId { get; set; }

    public string? PublicHost { get; set; }

    public string HealthCheckPath { get; set; } = "/health";

    public string[] Tags { get; set; } = [];

    public bool PreferHttps { get; set; }
}
