namespace Shared.Models.DataModels.EquipmentDtos
{
    /// <summary>Request tạo mới thiết bị.</summary>
    public record EquipmentRequest(
        Guid CinemaId,
        Guid? ScreenId,
        string EquipmentType,
        DateTime PurchaseDate,
        string Status = "working"
    );

    /// <summary>Response thông tin thiết bị.</summary>
    public record EquipmentResponse(
        Guid Id,
        Guid CinemaId,
        Guid? ScreenId,
        string? ScreenName,
        string EquipmentType,
        DateTime PurchaseDate,
        string Status
    );

    /// <summary>Request tạo log bảo trì.</summary>
    public record MaintenanceLogRequest(
        Guid EquipmentId,
        DateTime MaintenanceDate,
        decimal? Cost,
        string IssuesFound,
        bool IsCompleted = false
    );

    /// <summary>Response log bảo trì.</summary>
    public record MaintenanceLogResponse(
        Guid Id,
        Guid EquipmentId,
        DateTime MaintenanceDate,
        decimal? Cost,
        string IssuesFound,
        bool IsCompleted
    );
}
