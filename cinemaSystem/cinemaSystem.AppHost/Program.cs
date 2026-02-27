var builder = DistributedApplication.CreateBuilder(args);

// ── Secrets ──────────────────────────────────────────────
var sqlPassword = builder.AddParameter("sql-password", secret: true);

// ── SQL Server ───────────────────────────────────────────
var sqlServer = builder.AddSqlServer("sqlserver", password: sqlPassword)
    .WithDataVolume("cinema-sqldata");

var bookingDb  = sqlServer.AddDatabase("BookingDb",  databaseName: "Booking");
var identityDb = sqlServer.AddDatabase("IdentityDb", databaseName: "Identity");

// ── Redis ────────────────────────────────────────────────
var redis = builder.AddRedis("redis")
    .WithDataVolume("cinema-redisdata");

// ── Mailhog (SMTP for testing) ───────────────────────────
var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
    .WithHttpEndpoint(port: 8025, targetPort: 8025, name: "webui")
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp");

// ── Adminer (DB Management) ──────────────────────────────
var adminer = builder.AddContainer("adminer", "adminer")
    .WithHttpEndpoint(port: 8082, targetPort: 8080)
    .WithReference(sqlServer);

// ── Migration Worker (chạy xong → tự tắt) ───────────────
var migrations = builder.AddProject<Projects.Cinema_Migrations>("cinema-migrations")
    .WithReference(bookingDb)
    .WithReference(identityDb)
    .WaitFor(sqlServer);

// ── API Service (chờ migrations xong mới start) ─────────
builder.AddProject<Projects.Api>("cinema-api")
    .WithReference(bookingDb)
    .WithReference(identityDb)
    .WithReference(redis)
    .WithReference(mailhog.GetEndpoint("smtp")) // Inject SMTP info
    .WaitFor(redis)
    .WaitFor(migrations);     // ← API chỉ start SAU KHI migrations thành công

builder.Build().Run();
