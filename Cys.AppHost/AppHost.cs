using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("cinema-postgres-data")
    .WithPgWeb();

var redis = builder.AddRedis("redis")
    .WithRedisInsight();

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var identityDb = postgres.AddDatabase("identitydb");
var cinemaDb = postgres.AddDatabase("cinemadb");
var movieDb = postgres.AddDatabase("moviedb");
var bookingDb = postgres.AddDatabase("bookingdb");
var paymentDb = postgres.AddDatabase("paymentdb");

// Shared JWT configuration: every service that issues OR validates tokens must agree on key + issuer + audience.
var jwtKey = builder.AddParameter("jwt-key", secret: true);
var jwtIssuer = builder.AddParameter("jwt-issuer");
var jwtAudience = builder.AddParameter("jwt-audience");

// Cinema shared configuration: shared Cloudinary credentials for storing movie posters and cinema images.
var cloudinaryCloudName = builder.AddParameter("cloudinary-cloud-name");
var cloudinaryApiKey = builder.AddParameter("cloudinary-api-key", secret: true);
var cloudinaryApiSecret = builder.AddParameter("cloudinary-api-secret", secret: true);

//Payment shared configuration: shared Sepay credentials for processing payments.
var sepayMerchantId = builder.AddParameter("sepay-merchant-id");
var sepayApiKey = builder.AddParameter("sepay-secret-key", secret: true);

var identity = builder.AddProject<Projects.Identity_API>("identity")
    .WithReference(identityDb)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WaitFor(identityDb);

var cinema = builder.AddProject<Projects.Cinema_API>("cinema")
    .WithReference(cinemaDb)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WaitFor(cinemaDb);

var movie = builder.AddProject<Projects.Movie_API>("movie")
    .WithReference(movieDb)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Cloudinary__CloudName", cloudinaryCloudName)
    .WithEnvironment("Cloudinary__ApiKey", cloudinaryApiKey)
    .WithEnvironment("Cloudinary__ApiSecret", cloudinaryApiSecret)
    .WaitFor(movieDb);

var payment = builder.AddProject<Payment_API>("payment")
    .WithReference(paymentDb)
    .WithReference(rabbitmq)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Sepay__Merchant__Id", sepayMerchantId)
    .WithEnvironment("Sepay__Secret__Key", sepayApiKey)
    .WaitFor(paymentDb)
    .WaitFor(rabbitmq);

var booking = builder.AddProject<Projects.Booking_API>("booking")
    .WithReference(bookingDb)
    .WithReference(cinema)
    .WithReference(movie)
    .WithReference(payment)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WaitFor(bookingDb)
    .WaitFor(redis)
    .WaitFor(rabbitmq);

var gateway = builder.AddProject<Projects.Gateway>("gateway")
    .WithReference(identity)
    .WithReference(cinema)
    .WithReference(movie)
    .WithReference(booking)
    .WithReference(payment)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WaitFor(identity)
    .WaitFor(cinema)
    .WaitFor(movie)
    .WaitFor(booking)
    .WaitFor(payment);

movie
    .WithReference(cinema)
    .WithReference(booking);

var ngrokDomain = builder.Configuration["Ngrok:Domain"];
var ngrokGatewayArgs = string.IsNullOrWhiteSpace(ngrokDomain)
    ? new object[] { "http", gateway.GetEndpoint("https") }
    : new object[] { "http", gateway.GetEndpoint("https"), "--domain", ngrokDomain };

builder.AddExecutable("ngrok-gateway", "ngrok", ".", ngrokGatewayArgs)
    .WaitFor(gateway);




builder.Build().Run();
