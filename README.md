# WorkJournal API

A production-minded ASP.NET Core Web API for managing work items, designed to demonstrate disciplined engineering practices across local development, CI/CD, and Azure deployment.

---

## Overview

WorkJournal is a lightweight system for tracking work items with support for:

- creating and retrieving work items
- prioritization and completion tracking
- validation and consistent error handling
- deterministic query behavior

This project emphasizes **correct system design** over feature breadth.

---

## Architecture Highlights

### Application Design
- ASP.NET Core minimal API
- Vertical slice organization (Endpoints, Validation, Contracts)
- Clean separation of concerns

### Data Layer
- SQL Server across all environments
- Entity Framework Core
- Explicit migration workflow (no runtime auto-migrations)

### API Behavior
- Problem Details for consistent error responses
- Input validation
- Deterministic ordering:
    - `CreatedAtUtc DESC`
    - `Id DESC` (tie-breaker)

---

## Environments

| Environment | Database | Auth | Notes |
|------------|--------|------|------|
| Local | Docker SQL Server | SQL auth | Port `14333` |
| CI | SQL Server container | SQL auth | Port `1433` |
| Azure (Dev) | Azure SQL | Managed Identity | No secrets |

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker

---

### Run locally

Start SQL Server:

```bash
docker run -e "ACCEPT_EULA=Y" \
           -e "MSSQL_SA_PASSWORD=WorkJ0urnal42" \
           -p 14333:1433 \
           --name workjournal-sql \
           -d mcr.microsoft.com/mssql/server:2022-latest