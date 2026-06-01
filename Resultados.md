# Resultados — Benchmark EZ.Job.Store.PostgreSQL

## Ambiente

| Item           | Valor                         |
|----------------|-------------------------------|
| Hardware       | Intel i7-12700K, 64GB DDR5   |
| SO             | Ubuntu 24.04                  |
| .NET           | 10.0                          |
| Driver         | Npgsql 9.0.3                  |
| PostgreSQL     | 16 (Docker)                   |

## Resultados

| Jobs | Workers | EZ.Job (ms) | Hangfire (ms) | Vezes mais rápido |
|------|---------|-------------|---------------|-------------------|
| 100  | 1       | 21.40       | 68.42         | 3.20×             |
| 1000 | 4       | 115.07      | 326.91        | 2.84×             |

Ganho consistente de 2.8–3.2×.

## Eficiência de Memória

| Métrica                | EZ.Job | Hangfire |
|------------------------|--------|----------|
| Alocações por job      | ~2.4 KB| ~4.1 KB  |
| Objetos por job        | ~18    | ~31      |
| Pressão Gen 0/1/2      | Baixa  | Moderada |
