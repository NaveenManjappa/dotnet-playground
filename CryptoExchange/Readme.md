# CryptoExchange API

A comprehensive ASP.NET Core 10.0 educational project demonstrating advanced rate-limiting strategies and traffic management patterns. This project explores multiple approaches to controlling API traffic, protecting backend resources, and implementing tiered service models.

## 📋 Overview

The CryptoExchange API serves as a learning platform for understanding rate-limiting algorithms and their practical applications. Rather than focusing on cryptocurrency data, the emphasis is on the sophisticated rate-limiting mechanisms that protect modern APIs from abuse and resource exhaustion while enabling flexible service tier management.

## 🎓 Core Concepts Explored

- **Rate-Limiting Algorithms**: Implementation of five distinct rate-limiting strategies
- **Traffic Control Patterns**: Different approaches to managing request flow
- **Resource Protection**: Strategies for preventing server overload
- **Tiered Service Models**: Implementation of subscription-based access control
- **Concurrency Management**: Limiting simultaneous request processing

## 🏗️ Architecture

### Technology Stack

| Component | Version |
|-----------|---------|
| **.NET Framework** | 10.0 |
| **ASP.NET Core** | 10.0 |
| **Language** | C# 13 (nullable reference types, implicit usings) |

### Project Organization

```
CryptoExchange/
├── Controllers/              # Three controllers demonstrating different rate-limiting policies
│   ├── MarketController.cs
│   ├── NewsController.cs
│   └── SlidingController.cs
├── Program.cs               # Rate limiter configuration and middleware setup
└── Configuration files      # Application settings and launch profiles
```

## 🛡️ Rate-Limiting Strategies Deep Dive

Rate limiting is a fundamental technique in API design that controls the number of requests a client can make within a specified time period. This project explores five different algorithms, each with distinct trade-offs suitable for different scenarios.

### 1. Fixed Window Algorithm

**Concept**: Time is divided into fixed, non-overlapping intervals (windows). Each window allows a fixed number of requests.

```
Window 1 (0-10s): [□□□□□]  5 requests allowed
Window 2 (10-20s): [□□□□□]  5 requests allowed
Window 3 (20-30s): [□□□□□]  5 requests allowed
```

**Configuration in this project**:
- Permit Limit: 5 requests
- Window Duration: 10 seconds
- Queue Limit: 0 (no queueing, immediate rejection)

**Trade-offs**:
- ✅ **Simple**: Easy to understand and implement
- ✅ **Memory Efficient**: Requires minimal state tracking
- ❌ **Burst Risk**: Allows all permits to be consumed at window boundaries

**Use Case**: Simple APIs with predictable traffic patterns where burst handling isn't critical.

---

### 2. Sliding Window Algorithm

**Concept**: Maintains a continuous window of requests that "slides" forward. More granular than fixed windows by dividing time into segments.

```
Time: 0----5----10----15----20
      [===|===|===]          ← Window slides continuously
      Segment 1: 0-5s (5 reqs)
      Segment 2: 5-10s (5 reqs)
      Segment 3: 10-15s (5 reqs)
      Always: Latest 15 seconds (all 3 segments)
```

**Configuration in this project** (Applied to `SlidingController`):
- Permit Limit: 5 requests per 15-second window
- Segments Per Window: 3 (dividing 15s into three 5s segments)
- Queue Limit: 0

**Trade-offs**:
- ✅ **Smooth Traffic**: Prevents burst issues at window boundaries
- ✅ **Fair Distribution**: More uniform request distribution
- ❌ **Complex State**: Requires tracking multiple segments
- ❌ **Higher Memory**: More overhead than fixed window

**Use Case**: APIs requiring smoother traffic control without sudden rejections at time boundaries.

---

### 3. Token Bucket Algorithm

**Concept**: Tokens are added to a "bucket" at a fixed rate. Each request consumes one token. When the bucket is full, new tokens are discarded. This allows controlled bursts.

```
Time  Bucket State           Request
0s    [●●●●●●●●●●]         Request 1: ✓ (remove ●)
0.5s  [●●●●●●●●●]          Request 2: ✓ (remove ●)
1s    +2 tokens added       [●●●●●●●●●●●] (10 max)
2s    +2 tokens added       [●●●●●●●●●●●] (capped at 10)
2.5s                        Request 3-10: ✓ (remove ●●●●●●●●)
3s    [empty]               Request 11: ✗ (no tokens)
3.5s  +2 tokens added       [●●]
3.6s                        Request 11: ✓ (remove ●)
```

**Configuration in this project** (Applied to `NewsController`):
- Bucket Size: 10 tokens (maximum burst capacity)
- Replenishment Rate: 2 tokens every 10 seconds
- Queue Limit: 0

**Trade-offs**:
- ✅ **Burst Friendly**: Allows controlled bursts of traffic
- ✅ **Fair to Fast Clients**: Rewards timely requests
- ✅ **Smooth Rate**: Token replenishment creates steady flow
- ❌ **Moderate Complexity**: More state than fixed window

**Use Case**: APIs where occasional traffic spikes are acceptable (e.g., news feeds, social media).

---

### 4. Tiered Policy (Custom Partitioning)

**Concept**: Different rate limits apply to different client categories, determined by runtime conditions. This enables subscription-based service models.

**How It Works**:
```
Request arrives with query parameter: tier=gold

┌─ Is tier "gold"? ─┐
│                   │
Yes                 No
│                   │
✓ Gold Tier        ✓ Standard Tier
100 req/10s        5 req/10s (per IP)
```

**Configuration in this project** (Applied to `MarketController`):
- Gold Tier: 100 requests per 10 seconds (premium users)
- Standard Tier: 5 requests per 10 seconds (free users, differentiated by IP address)
- Partition Key: Query parameter `tier` for gold; IP address for standard

**Trade-offs**:
- ✅ **Revenue Model**: Enables monetization through tiered access
- ✅ **Flexible**: Conditions can be any request property
- ✅ **Fair**: Each partition gets its own limit pool
- ❌ **Partition Overhead**: More complex logic

**Use Case**: SaaS platforms, public APIs with premium tiers, usage-based pricing models.

---

### 5. Concurrency Limiter

**Concept**: Limits the number of requests being processed simultaneously, rather than the rate at which they arrive. Critical for resource-intensive operations.

```
Time    Processing Queue         Action
0s      [Request 1] [Request 2]  Both executing (2 max)
0.1s    [Request 1] [Request 2]  
        [Queued: Req 3, Req 4]   Can queue 2 more
0.5s    [Request 1] ✓ DONE       → Start Request 3
        [Request 2] [Request 3]  Processing continues
2.5s    [Request 2] ✓ DONE       → Start Request 4
        [Request 3] [Request 4]  
5s      [Request 3] ✓ DONE       → No more queued
        [Request 4]
```

**Configuration in this project** (Applied to `MarketController.GetHeavyReport()`):
- Concurrent Permits: 2 (maximum simultaneous requests)
- Queue Limit: 2 (allows 2 more to wait)
- Total Capacity: 4 (2 processing + 2 queued)

**Trade-offs**:
- ✅ **Resource Protection**: Prevents CPU/memory exhaustion
- ✅ **Predictable Load**: Server never overwhelmed
- ✅ **Fair Queuing**: FIFO ensures fairness
- ❌ **Latency**: Queued requests experience wait time
- ❌ **Not Rate Limiting**: Doesn't restrict total requests, just simultaneous processing

**Use Case**: Heavy computations, database queries, file processing, or any resource-constrained operation.

---

## 📡 Rate-Limiting in Action

All rate limiters are configured in `Program.cs` and registered as middleware at application startup:

```csharp
app.UseRateLimiter();  // Middleware intercepts all requests
```

### Request Flow

```
Request arrives
    ↓
[Rate Limiter Middleware]
    ↓
Determine applicable policy
    ↓
Acquire permit/token
    ↓
✓ Success: Pass to controller
✗ Failure: Return 429 Too Many Requests
```

### Error Response

When a rate limit is exceeded, clients receive:

- **Status Code**: 429 Too Many Requests
- **Retry-After Header**: Indicates when to retry (in seconds)
- **Body Message**: "Too many requests. Please try again later in {seconds}"

This standard HTTP response allows clients to implement intelligent retry logic.

## 🔄 Comparison of Rate-Limiting Strategies

| Aspect | Fixed Window | Sliding Window | Token Bucket | Tiered Policy | Concurrency |
|--------|--------------|---|---|---|---|
| **Implementation Complexity** | Simple | Medium | Medium | Medium | Simple |
| **Memory Overhead** | Low | Medium | Low | Medium | Low |
| **Burst Handling** | Poor | Good | Excellent | Good | N/A |
| **Boundary Effects** | Yes | No | No | No | N/A |
| **Fairness** | Fair | Very Fair | Fair | Depends on tier | Very Fair |
| **Best For** | Simple APIs | Smooth traffic | Flexible apps | SaaS models | Heavy ops |

---

## 📚 Practical Considerations

### When to Use Fixed Window
- Simple internal APIs with predictable patterns
- Minimal computational resources available
- Clear business hour vs off-hour patterns

### When to Use Sliding Window
- Public APIs with mixed traffic patterns
- Need smooth request distribution
- Want to eliminate boundary-time bursts

### When to Use Token Bucket
- APIs serving consumer applications (social media, news)
- Acceptable to allow brief traffic spikes
- Want flexibility for burst capacity
- Popular choice for industry standards (AWS, Azure)

### When to Use Tiered Policy
- Multi-tenant platforms (SaaS)
- Subscription-based business models
- Different service levels for different user types
- IP-based or authentication-based differentiation needed

### When to Use Concurrency Limiting
- Protecting database connections
- Managing CPU-intensive operations
- File processing or heavy computations
- Database query limiting

### Combining Strategies
Many production systems use multiple limiters together:
- Token bucket for global rate limits
- Concurrency limiter for expensive operations
- Tiered policy for customer segmentation

## 🚀 Implementation Architecture

### ASP.NET Core Rate Limiter Middleware

The project uses ASP.NET Core's built-in rate-limiting middleware, configured in `Program.cs`:

```csharp
builder.Services.AddRateLimiter(options =>
{
    // Configure rejection status code and handler
    // Define multiple rate-limiting policies
    // Apply custom partition logic
});

// Later in the pipeline
app.UseRateLimiter();
```

### Policy Application Patterns

**Controller-level**: All requests to the controller use this policy
```csharp
[EnableRateLimiting("token_bucket")]
public class NewsController : ControllerBase
```

**Action-level**: Override with different policy for specific endpoint
```csharp
[EnableRateLimiting("concurrency_policy")]
public async Task<IActionResult> GetHeavyReport()
```

### Controllers in This Project

- **MarketController**: Demonstrates tiered policies and concurrency limiting
- **NewsController**: Uses token bucket algorithm
- **SlidingController**: Uses sliding window algorithm

Each controller illustrates rate-limiting concepts through different endpoints.

## 🎯 Learning Outcomes

By studying this project, you'll understand:

1. **Fixed Window Rate Limiting**: How simple time-based bucketing works and its limitations
2. **Sliding Window Algorithm**: How to achieve smooth traffic distribution
3. **Token Bucket Algorithm**: How to balance average rates with burst capacity
4. **Dynamic Partitioning**: How to implement tiered service levels and multi-tenancy
5. **Concurrency Control**: How to protect against resource exhaustion
6. **ASP.NET Core Middleware**: How to integrate custom policies into the request pipeline
7. **API Design Patterns**: Best practices for protecting and scaling public APIs

## 📚 Further Reading

- [RFC 6585: Status Code 429](https://tools.ietf.org/html/rfc6585#section-4) - HTTP specification for rate limiting
- [Token Bucket Algorithm](https://en.wikipedia.org/wiki/Token_bucket) - Wikipedia overview
- [Leaky Bucket Algorithm](https://en.wikipedia.org/wiki/Leaky_bucket) - Related algorithm
- [Rate Limiting Strategies](https://cloud.google.com/solutions/rate-limiting-strategies-techniques) - Google Cloud guide
- [ASP.NET Core Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit) - Microsoft documentation

---

**Purpose**: Educational exploration of rate-limiting concepts and algorithms  
**Framework**: .NET 10.0 with ASP.NET Core  
**Last Updated**: November 18, 2025
