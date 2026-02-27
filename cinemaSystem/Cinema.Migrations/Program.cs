using System;
using Cinema.Migrations;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// Register DbContexts thông qua Aspire (connection string tự inject)
builder.AddSqlServerDbContext<BookingContext>("BookingDb");
builder.AddSqlServerDbContext<AppIdentityContext>("IdentityDb");

// Register Identity cho Seed Data (cần UserManager, RoleManager)
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppIdentityContext>();

// Register Worker
builder.Services.AddHostedService<MigrationWorker>();

var app = builder.Build();
app.Run();
