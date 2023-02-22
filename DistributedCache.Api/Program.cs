using DistributedCache.Api.Models;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Caching
builder.Services.AddScoped<IDistributedCacheService, DistributedCacheService>();
builder.Services.AddStackExchangeRedisCache(
    options => options.Configuration = builder.Configuration.GetConnectionString("Redis"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/cache", async (IDistributedCache cache) =>
{
    const string cacheKey = "TestCacheKey";
    var cachedItem = await cache.GetStringAsync(cacheKey);
    if (cachedItem != null)
    {
        return Results.Ok($"FROM CACHE: {cachedItem}");
    }

    var response = $"Response time: {DateTime.Now}";
    await cache.SetStringAsync(cacheKey, response);

    return Results.Ok(response);
});

app.MapGet("api/objectcache", async (IDistributedCacheService cacheService) =>
{
    const string cacheKey = "TestObjectCacheKey";
    var cachedItem = await cacheService.GetAsync<Response?>(cacheKey);
    if (cachedItem != null)
    {
        return Results.Ok($"FROM CACHE: Time: {cachedItem.DateTime} Guid: {cachedItem.Guid}");
    }

    var response = new Response(Guid.NewGuid(), DateTime.Now);
    await cacheService.SetAsync(cacheKey, response);

    return Results.Ok($"Time: {response.DateTime} Guid: {response.Guid}");
});

app.Run();
