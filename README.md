<p align="center">
  <img height="140" src="https://raw.githubusercontent.com/dotnetcore/CAP/master/docs/content/img/logo.svg?sanitize=true" />
</p>

# Savorboard.CAP.InMemoryMessageQueue

A lightweight in-memory message queue transport for [CAP](https://github.com/dotnetcore/CAP).

Designed ONLY for local development, demos and automated tests. Do not use in production.

[![Build status](https://ci.appveyor.com/api/projects/status/txg29kmg0o6u4c2j?svg=true)](https://ci.appveyor.com/project/yang-xiaodong/savorboard-cap-inmemorymessagequeue)
[![NuGet](https://img.shields.io/nuget/v/Savorboard.CAP.InMemoryMessageQueue.svg)](https://www.nuget.org/packages/Savorboard.CAP.InMemoryMessageQueue)
![License](https://img.shields.io/badge/license-MIT-blue.svg)

---
## Install
```bash
dotnet add package Savorboard.CAP.InMemoryMessageQueue
```

---
## Features
- Zero external dependencies
- Works entirely in-memory (lifetime = process lifetime)
- Publish / subscribe via standard CAP attributes
- Fast startup, simple to configure
- Great for unit / integration tests
- .NET 8 compatible

---
## Limitations
Not persistent. Not cross-process. Not clustered. Not durable. Not for performance benchmarking. Not for production.
If the process stops, messages are gone.

---
## Quick Start
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCap(options =>
{
    options.DefaultGroup = "demo.group";
    options.UseInMemoryMessageQueue();
    // You can still configure a persistent storage component if needed.
});

var app = builder.Build();

app.MapGet("/publish", async (ICapPublisher cap) =>
{
    await cap.PublishAsync("demo.topic", new { Id = Guid.NewGuid(), Time = DateTimeOffset.UtcNow });
    return "Published";
});

app.Run();
```
Subscriber example:
```csharp
public class DemoSubscriber
{
    [CapSubscribe("demo.topic")]
    public void Handle(dynamic payload)
    {
        Console.WriteLine($"Received: {payload.Id} at {payload.Time}");
    }
}
```

---
## Configuration
Currently no extra tunable options:
```csharp
options.UseInMemoryMessageQueue();
```

---
## Sample
See `samples/InMemorySample` for a minimal runnable example.

---
## Testing
```bash
dotnet test
```

---
## Contributing
Issues and PRs welcome. Keep changes small and focused.

---
## License
MIT

---
## Summary
Use for development & tests only. Switch to a real queue for production workloads.
