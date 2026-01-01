using Carter;
using Microsoft.JSInterop.Infrastructure;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Infrastructure;
using DotNetEnv;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

Env.Load();
builder.Configuration.AddEnvironmentVariables();




// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();



builder.Services.Scan(scan => scan.FromAssembliesOf(typeof(Program))
    .AddClasses(classes => classes
        .AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
    .AsImplementedInterfaces()
        .WithScopedLifetime()

    .AddClasses(classes => classes.
        AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
    .AsImplementedInterfaces()
        .WithScopedLifetime()

    .AddClasses(classes => classes
        .AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
    .AsImplementedInterfaces()
        .WithScopedLifetime());

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCarter();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});



var app = builder.Build();

// Global API prefix

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();

var api = app.MapGroup("/api/v1");
api.MapCarter();


app.Run();


