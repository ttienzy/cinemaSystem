namespace Cinema.API.Client.Client;

public interface ICinemaApiClient
{
    Task<ApiResponse<PaginatedResponse<CinemaDto>>> GetCinemasAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CinemaDetailDto>> GetCinemaByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PaginatedResponse<CinemaAdminOverviewDto>>> GetCinemaAdminOverviewAsync(
        string? search = null,
        string? city = null,
        string? status = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CinemaAdminSummaryDto>> GetCinemaAdminSummaryAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CinemaDto>> CreateCinemaAsync(
        CreateCinemaRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CinemaDto>> UpdateCinemaAsync(
        Guid id,
        CreateCinemaRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteCinemaAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<CinemaHallDto>>> GetHallsByCinemaIdAsync(
        Guid cinemaId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CinemaHallDetailDto>> GetHallByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<CinemaHallDto>>> LookupHallsAsync(
        CinemaHallLookupRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<SeatDto>>> GetHallSeatsAsync(
        Guid hallId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CinemaHallDto>> CreateHallAsync(
        CreateCinemaHallRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CinemaHallDto>> UpdateHallAsync(
        Guid id,
        UpdateCinemaHallRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteHallAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<SeatDto>>> GetSeatsByHallIdAsync(
        Guid hallId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<SeatDto>> GetSeatByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<SeatDto>> CreateSeatAsync(
        CreateSeatRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<SeatDto>>> BulkCreateSeatsAsync(
        BulkCreateSeatsRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<SeatDto>> UpdateSeatAsync(
        Guid id,
        UpdateSeatRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteSeatAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> BulkDeleteSeatsAsync(
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default);
}
