using FastEndpoints;
using Focuswave.AuthService.Domain.Users;
using Focuswave.AuthService.Infrastructure.Vault;
using Focuswave.AuthService.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Focuswave.AuthService API", Version = "v1" });
});

var vaultOptions = builder.Configuration.GetSection("Vault").Get<VaultOptions>();

var vaultService =
    new VaultService(vaultOptions ?? throw new ApplicationException("Vault configuration is null"))
    ?? throw new ApplicationException("Cannot create vault service");

var connectionString = vaultService
    .GetSecretAsync("dev/auth-service", "AuthConnection")
    .Run()
    .AsTask()
    .Result.IfFail(err => throw new ApplicationException($"Ошибка: {err}"))
    .IfNone(() => throw new ApplicationException("Cant get connection string"));

builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(connectionString));

builder
    .Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        // другие опции
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder
    .Services.AddIdentityServer()
    .AddAspNetIdentity<User>()
    .AddInMemoryClients(Config.Clients)
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryApiResources(Config.ApiResources)
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddDeveloperSigningCredential(); // для dev-целей

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseFastEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Focuswave.AuthService API v1");
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthorization();

app.MapControllers();

app.Run();
