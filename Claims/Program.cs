using Application.Interfaces;
using Application.Services;
using Claims.Auditing;
using Infrastructure.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
);


builder.Services.AddSingleton<CosmosClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var configurationSection = configuration.GetSection("CosmosDb");

    var account = configurationSection.GetSection("Account")?.Value;
    var key = configurationSection.GetSection("Key")?.Value;

    return new CosmosClient(account, key);
});


builder.Services.AddScoped<IClaimCosmosDbService>(provider =>
    new ClaimCosmosDbService(
        provider.GetRequiredService<CosmosClient>(),
        "ClaimDb",
        "Claim"));

builder.Services.AddScoped<ICoverCosmosDbService>(provider =>
    new CoverCosmosDbService(
        provider.GetRequiredService<CosmosClient>(),
        "ClaimDb",
        "Cover"));
builder.Services.AddDbContext<AuditContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IQueueStorageService, QueueStorageService>();
builder.Services.AddScoped<IClaimService, ClaimService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuditContext>();
    context.Database.Migrate();
}
app.Run();

public partial class Program { }