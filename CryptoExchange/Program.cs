using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

//Token bucket rate limiter
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 5;//allow only 5 requests
        options.Window = TimeSpan.FromSeconds(10);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;//do not queue requests, reject immediately

    });

    options.AddSlidingWindowLimiter(policyName: "sliding", options =>
    {
        options.PermitLimit = 5;//allow 5 requests
        options.Window = TimeSpan.FromSeconds(15);//per 15seconds
        options.SegmentsPerWindow = 3;//split into 3 segments 5s each
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });

    options.AddTokenBucketLimiter(policyName: "token_bucket", options =>
    {
        options.TokenLimit = 10; // Bucket size: max 10 tokens allowed in the bucket
        options.ReplenishmentPeriod = TimeSpan.FromSeconds(10); //How often we add tokens
        options.TokensPerPeriod = 2; //How many tokens we add each period
        options.QueueProcessingOrder= QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });

    options.AddPolicy("tiered_policy", httpContext =>
    {
        var tier = httpContext.Request.Query["tier"].ToString();
        bool isGold = tier.Equals("gold", StringComparison.OrdinalIgnoreCase);

        if (isGold)
        {
            return RateLimitPartition.GetFixedWindowLimiter(partitionKey: "gold_member", factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(10)
            });
        }
        else
        {
            var userIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";
            return RateLimitPartition.GetFixedWindowLimiter(partitionKey: userIp, factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(10)
            });
        }
    });
});


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
