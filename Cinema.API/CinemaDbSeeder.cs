using Cinema.API.Data;
using Cinema.API.Entities;
using Microsoft.EntityFrameworkCore;
using CinemaEntity = Cinema.API.Entities.Cinema;

namespace Cinema.API;

public static class CinemaDbSeeder
{
    public static async Task SeedAsync(CinemaDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        if (await context.Cinemas.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var seedCinema in GetSeedData())
        {
            var cinema = await context.Cinemas
                .Include(c => c.CinemaHalls)
                .ThenInclude(h => h.Seats)
                .FirstOrDefaultAsync(c => c.Name == seedCinema.Name, cancellationToken);

            if (cinema is null)
            {
                cinema = CinemaEntity.Create(seedCinema.Name, seedCinema.Address, seedCinema.City);
                context.Cinemas.Add(cinema);
                await context.SaveChangesAsync(cancellationToken);
            }

            await EnsureHallsAsync(context, cinema, seedCinema.Halls, cancellationToken);
        }
    }

    private static async Task EnsureHallsAsync(
        CinemaDbContext context,
        CinemaEntity cinema,
        IReadOnlyCollection<SeedHall> seedHalls,
        CancellationToken cancellationToken)
    {
        foreach (var seedHall in seedHalls)
        {
            var hall = cinema.CinemaHalls.FirstOrDefault(h => h.Name == seedHall.Name);
            if (hall is null)
            {
                hall = CinemaHall.Create(cinema.Id, seedHall.Name);
                context.CinemaHalls.Add(hall);
                cinema.CinemaHalls.Add(hall);
                await context.SaveChangesAsync(cancellationToken);
            }

            await EnsureSeatsAsync(context, hall, seedHall.Rows, seedHall.SeatsPerRow, cancellationToken);
        }
    }

    private static async Task EnsureSeatsAsync(
        CinemaDbContext context,
        CinemaHall hall,
        IReadOnlyCollection<string> rows,
        int seatsPerRow,
        CancellationToken cancellationToken)
    {
        var existingSeats = hall.Seats
            .Select(seat => (seat.Row, seat.Number))
            .ToHashSet();

        var seatsToAdd = new List<Seat>();

        foreach (var row in rows)
        {
            for (var number = 1; number <= seatsPerRow; number++)
            {
                if (existingSeats.Contains((row, number)))
                {
                    continue;
                }

                seatsToAdd.Add(Seat.Create(hall.Id, row, number));
            }
        }

        if (seatsToAdd.Count == 0)
        {
            return;
        }

        await context.Seats.AddRangeAsync(seatsToAdd, cancellationToken);
        hall.IncreaseTotalSeats(seatsToAdd.Count);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyCollection<SeedCinema> GetSeedData()
    {
        return
        [
            new SeedCinema(
                "Galaxy Nguyen Du",
                "116 Nguyen Du, District 1",
                "Ho Chi Minh City",
                [
                    new SeedHall("Hall 1", ["A", "B", "C", "D"], 10),
                    new SeedHall("Hall 2", ["A", "B", "C"], 8)
                ]),
            new SeedCinema(
                "CGV Vincom Ba Trieu",
                "191 Ba Trieu, Hai Ba Trung",
                "Ha Noi",
                [
                    new SeedHall("Hall 1", ["A", "B", "C", "D", "E"], 10),
                    new SeedHall("Hall 2", ["A", "B", "C"], 8)
                ]),
            new SeedCinema(
                "Lotte Cinema Da Nang",
                "6 Nai Nam, Hai Chau",
                "Da Nang",
                [
                    new SeedHall("Hall 1", ["A", "B", "C", "D"], 9),
                    new SeedHall("Hall 2", ["A", "B", "C"], 8)
                ])
        ];
    }

    private sealed record SeedCinema(
        string Name,
        string Address,
        string City,
        IReadOnlyCollection<SeedHall> Halls);

    private sealed record SeedHall(
        string Name,
        IReadOnlyCollection<string> Rows,
        int SeatsPerRow);
}
