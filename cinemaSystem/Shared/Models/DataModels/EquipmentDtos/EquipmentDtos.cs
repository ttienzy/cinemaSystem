namespace Shared.Models.DataModels.EquipmentDtos
{
    /// <summary>Request to create new equipment.</summary>
    public record EquipmentRequest(
        Guid CinemaId,
        Guid? ScreenId,
        string EquipmentType,
        DateTime PurchaseDate,
        string Status = "working"
    );

    /// <summary>Response containing equipment information.</summary>
    public record EquipmentResponse(
        Guid Id,
        Guid CinemaId,
        Guid? ScreenId,
        string? ScreenName,
        string EquipmentType,
        DateTime PurchaseDate,
        string Status
    );

    /// <summary>Request to create maintenance log.</summary>
    public record MaintenanceLogRequest(
        Guid EquipmentId,
        DateTime MaintenanceDate,
        decimal? Cost,
        string IssuesFound,
        bool IsCompleted = false
    );

    /// <summary>Response for maintenance log.</summary>
    public record MaintenanceLogResponse(
        Guid Id,
        Guid EquipmentId,
        DateTime MaintenanceDate,
        decimal? Cost,
        string IssuesFound,
        bool IsCompleted
    );
}
