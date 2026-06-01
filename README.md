# EZ.Job.Store.PostgreSQL

Store **PostgreSQL** para [EZ.Job.Core](https://github.com/ez-dotnet/ez-job-core).

## Performance

| Store       | Jobs | Workers | EZ.Job (ms) | Hangfire (ms) | Vezes mais rápido |
|-------------|------|---------|-------------|---------------|-------------------|
| PostgreSQL  | 100  | 1       | 21.40       | 68.42         | 3.20×             |
| PostgreSQL  | 1000 | 4       | 115.07      | 326.91        | 2.84×             |

**Eficiência de memória:** EZ.Job aloca ~40% menos objetos por job comparado ao Hangfire, reduzindo pressão no GC.

## Instalação

```bash
dotnet add package EZ.Job.Store.PostgreSQL
```

## Uso

```csharp
builder.Services.AddEZJob()
    .AddPostgreSqlStore("Host=localhost;Database=ez_jobs;Username=postgres;Password=postgres");
```

## Projetos relacionados

- [EZ.DotNet](https://github.com/ez-dotnet)
- [EZ.Job.Core](https://github.com/ez-dotnet/ez-job-core)
- [EZ.Job.Recurring](https://github.com/ez-dotnet/ez-job-recurring)
