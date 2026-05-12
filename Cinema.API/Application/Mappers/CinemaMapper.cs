using CinemaEntity = Cinema.API.Domain.Entities.Cinema;

namespace Cinema.API.Application.Mappers;

public static class CinemaMapper
{
    public static CinemaDto CinemaMapToDto(this CinemaEntity cinema)
    {
        return new CinemaDto
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Address = cinema.Address,
            City = cinema.City,
            Status = cinema.GetStatus(),
            TotalHalls = cinema.GetTotalHalls(),
            TotalSeats = cinema.GetTotalSeats(),
            CreatedAt = cinema.CreatedAt
        };
    }

    public static CinemaDetailDto CinemaMapToDetailDto(this CinemaEntity cinema)
    {
        return new CinemaDetailDto
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Address = cinema.Address,
            City = cinema.City,
            Status = cinema.GetStatus(),
            TotalHalls = cinema.GetTotalHalls(),
            TotalSeats = cinema.GetTotalSeats(),
            CreatedAt = cinema.CreatedAt,
            CinemaHalls = cinema.CinemaHalls
                .Select(cinemaHall => cinemaHall.CinemaHallMapToDto())
                .ToList()
        };
    }

    public static CinemaAdminOverviewDto CinemaMapToAdminOverviewDto(this CinemaEntity cinema)
    {
        return new CinemaAdminOverviewDto
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Address = cinema.Address,
            City = cinema.City,
            Status = cinema.GetStatus(),
            TotalHalls = cinema.GetTotalHalls(),
            TotalSeats = cinema.GetTotalSeats(),
            CreatedAt = cinema.CreatedAt,
            CinemaHalls = cinema.CinemaHalls
                .OrderBy(cinemaHall => cinemaHall.Name)
                .Select(cinemaHall => cinemaHall.CinemaHallMapToDto())
                .ToList()
        };
    }

    public static CinemaHallDto CinemaHallMapToDto(this CinemaHall cinemaHall)
    {
        return new CinemaHallDto
        {
            Id = cinemaHall.Id,
            CinemaId = cinemaHall.CinemaId,
            Name = cinemaHall.Name,
            TotalSeats = cinemaHall.TotalSeats,
            SeatMapConfigured = cinemaHall.HasConfiguredSeatMap(),
            CreatedAt = cinemaHall.CreatedAt
        };
    }

    public static CinemaHallDetailDto CinemaHallMapToDetailDto(this CinemaHall cinemaHall)
    {
        var hallDto = cinemaHall.CinemaHallMapToDto();

        return new CinemaHallDetailDto
        {
            Id = hallDto.Id,
            CinemaId = hallDto.CinemaId,
            Name = hallDto.Name,
            TotalSeats = hallDto.TotalSeats,
            SeatMapConfigured = hallDto.SeatMapConfigured,
            CreatedAt = hallDto.CreatedAt,
            CinemaName = cinemaHall.Cinema?.Name ?? string.Empty,
            Seats = cinemaHall.Seats
                .Select(seat => seat.SeatMapToDto())
                .ToList()
        };
    }

    public static SeatDto SeatMapToDto(this Seat seat)
    {
        return new SeatDto
        {
            Id = seat.Id,
            CinemaHallId = seat.CinemaHallId,
            Row = seat.Row,
            Number = seat.Number,
            DisplayName = seat.GetDisplayName()
        };
    }
}
