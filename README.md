# HttpClient vs IHttpClientFactory in .NET

## Overview

In .NET applications, making HTTP requests to external APIs is commonly done using:

- `HttpClient`
- `IHttpClientFactory`

Understanding the difference between these two is important for building scalable and production-ready applications.

This project demonstrates:

- What `HttpClient` is
- Problems caused by improper `HttpClient` usage
- What `IHttpClientFactory` is
- How `IHttpClientFactory` solves those problems
- Recommended modern practices in ASP.NET Core

---

# What is HttpClient?

`HttpClient` is a class provided by .NET for sending HTTP requests and receiving HTTP responses from external resources such as REST APIs.

Namespace:

```csharp
System.Net.Http
```

Example:

```csharp
HttpClient client = new HttpClient();

HttpResponseMessage response =
    await client.GetAsync("https://api.example.com");
```

---

# Features of HttpClient

- Supports GET, POST, PUT, DELETE, PATCH requests
- Supports asynchronous programming
- Handles request headers and authentication
- Supports JSON and REST APIs
- Used for communicating with external services

---

# Traditional Usage of HttpClient

Many developers initially use `HttpClient` like this:

```csharp
using var client = new HttpClient();

var response = await client.GetAsync(url);
```

or inside loops:

```csharp
for (int i = 0; i < 1000; i++)
{
    using var client = new HttpClient();

    await client.GetAsync(url);
}
```

Although this appears correct, it creates serious production problems.

---

# Problems with HttpClient

## 1. Socket Exhaustion

### What Happens Internally?

`HttpClient` internally uses TCP sockets to communicate over the network.

When an `HttpClient` instance is disposed, the underlying TCP connection is **not immediately released** by the operating system.

Instead, it enters a state called:

```text
TIME_WAIT
```

During this time, the socket remains reserved for some time before being fully released.

---

## Problem Scenario

If multiple `HttpClient` instances are created repeatedly:

```csharp
new HttpClient()
new HttpClient()
new HttpClient()
```

many TCP connections are also created.

Over time:

- Available sockets become exhausted
- Connections remain in `TIME_WAIT`
- New requests fail

This results in errors such as:

```text
SocketException:
Only one usage of each socket address is normally permitted
```

or

```text
No buffer space available
```

This issue is known as:

# Socket Exhaustion

---

# Why Reusing HttpClient Helps

`HttpClient` is designed to be:

```text
Reusable and long-lived
```

Reusing the same instance allows:

- Connection pooling
- TCP connection reuse
- Better performance
- Reduced latency
- Lower resource consumption

Example:

```csharp
private static readonly HttpClient _client =
    new HttpClient();
```

This approach solves socket exhaustion.

---

# Another Problem: DNS Stale Records

Using a singleton/static `HttpClient` introduces another issue.

Suppose an API hostname changes IP address dynamically:

```text
api.myservice.com
```

This is common in:

- Kubernetes
- Cloud environments
- Load balancers
- Microservices

A long-lived `HttpClient` may continue using an old cached IP address.

This can cause:

- Requests failing
- Traffic going to dead servers
- DNS updates not being respected

This is known as:

# DNS Stale Connection Problem

---

# What is IHttpClientFactory?

`IHttpClientFactory` was introduced in ASP.NET Core 2.1 to solve problems related to improper `HttpClient` lifecycle management.

It is a factory abstraction used to create and manage `HttpClient` instances efficiently.

---

# Benefits of IHttpClientFactory

- Prevents socket exhaustion
- Handles DNS refresh correctly
- Manages connection pooling
- Integrates with Dependency Injection
- Supports named and typed clients
- Supports resiliency with Polly
- Centralized HTTP client configuration

---

# How IHttpClientFactory Solves the Problem

Instead of creating and disposing sockets repeatedly, `IHttpClientFactory` internally manages:

```text
HttpMessageHandler
```

and reuses handlers safely.

Important concept:

```text
HttpClient instances are lightweight
Handlers are expensive
```

The factory pools handlers and rotates them periodically to avoid stale DNS issues.

---

# Registering IHttpClientFactory

In `Program.cs`:

```csharp
builder.Services.AddHttpClient();
```

---

# Basic Usage

```csharp
public class UserService
{
    private readonly HttpClient _httpClient;

    public UserService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    public async Task<string> GetUsersAsync()
    {
        return await _httpClient.GetStringAsync(
            "https://jsonplaceholder.typicode.com/users");
    }
}
```

---

# Typed Client Example

## Registration

```csharp
builder.Services.AddHttpClient<UserService>();
```

## Service

```csharp
public class UserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}
```

Typed clients are the recommended approach for large applications.

---

# Named Client Example

## Registration

```csharp
builder.Services.AddHttpClient("GitHub", client =>
{
    client.BaseAddress =
        new Uri("https://api.github.com/");
});
```

## Usage

```csharp
var client = factory.CreateClient("GitHub");
```

Useful when multiple APIs are used.

---

# Comparison: HttpClient vs IHttpClientFactory

| Feature | HttpClient | IHttpClientFactory |
|---|---|---|
| Sends HTTP requests | ✅ | ❌ |
| Lifecycle management | ❌ | ✅ |
| Prevents socket exhaustion | ❌ | ✅ |
| Handles DNS refresh | ❌ | ✅ |
| Dependency Injection support | ❌ | ✅ |
| Connection pooling | Manual | Automatic |
| Named clients | ❌ | ✅ |
| Typed clients | ❌ | ✅ |
| Polly integration | ❌ | ✅ |
| Recommended for production | ⚠️ | ✅ |

---

# Recommended Best Practice

## Avoid This

```csharp
using var client = new HttpClient();
```

inside controllers, loops, or per-request operations.

---

## Recommended

```csharp
builder.Services.AddHttpClient();
```

or

```csharp
builder.Services.AddHttpClient<MyService>();
```

---

# Modern Recommendation

For ASP.NET Core applications:

- Use `IHttpClientFactory`
- Use Typed Clients for clean architecture
- Avoid creating `HttpClient` manually per request

---

# Key Takeaway

The problem is **not** with `HttpClient` itself.

The real problem is:

```text
Improper lifecycle management of HttpClient
```

`IHttpClientFactory` solves this by:

- Reusing handlers
- Managing socket lifetimes
- Refreshing DNS automatically
- Providing centralized configuration

---

# Conclusion

`HttpClient` is the core API used for making HTTP requests in .NET.

However, improper usage can lead to:

- Socket exhaustion
- DNS stale connections
- Performance issues

`IHttpClientFactory` provides a modern, scalable, and production-ready solution for managing HTTP connections efficiently in ASP.NET Core applications.

It is the recommended approach for all modern .NET web applications.
