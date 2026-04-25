# 🌍 IP Blocker API

A **.NET 8 Web API** that manages blocked countries and validates IP addresses using third-party geolocation. Built with clean architecture principles, thread-safe in-memory storage, and full Swagger documentation.

> **Live Demo:** [http://ip-blocker.runasp.net](http://ip-blocker.runasp.net)

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Design Decisions](#design-decisions)

---

## Overview

This API allows you to:
- Maintain a list of **permanently blocked countries**
- Set **temporary blocks** that auto-expire after a configurable duration
- **Geolocate any IP address** using the [ipapi.co](https://ipapi.co) API
- **Automatically detect** if an incoming request comes from a blocked country
- **Log every access attempt** with IP, country, timestamp, and user agent

All data is stored **in-memory** using thread-safe collections — no database required.

---

## Features

| Feature | Details |
|---|---|
| Permanent country blocking | Add / remove countries from a persistent block list |
| Temporary blocking | Block a country for 1–1440 minutes with auto-expiry |
| IP geolocation | Resolve any IP to country, city, ISP via ipapi.co |
| Caller detection | Automatically detect the calling IP from `HttpContext` |
| Access logging | Log every check-block attempt with full metadata |
| Pagination | All list endpoints support `page` and `pageSize` |
| Search / filter | Filter blocked countries by code or name |
| Background cleanup | Hosted service removes expired blocks every 5 minutes |
| Swagger UI | Full interactive API documentation at the root URL |

---

## Tech Stack

- **Framework:** ASP.NET Core 8
- **Language:** C# 12
- **Storage:** `ConcurrentDictionary` + `ConcurrentQueue` (in-memory, thread-safe)
- **HTTP Client:** `IHttpClientFactory` with named client
- **Geolocation API:** [ipapi.co](https://ipapi.co) (free tier, no key required)
- **Documentation:** Swagger / Swashbuckle
- **Background Jobs:** `BackgroundService` (hosted service)
- **JSON:** Newtonsoft.Json

---

## Project Structure

```
IpBlockerApi/
├── Controllers/
│   ├── CountriesController.cs     # Endpoints 1, 2, 3, 7
│   ├── IpController.cs            # Endpoints 4, 5
│   └── LogsController.cs          # Endpoint 6
├── Services/
│   ├── IGeolocationService.cs
│   ├── GeolocationService.cs      # Calls ipapi.co via HttpClient
│   ├── ICountryBlockService.cs
│   ├── CountryBlockService.cs     # Core blocking business logic
│   ├── ILogService.cs
│   └── LogService.cs
├── Repositories/
│   ├── IBlockedCountryRepository.cs
│   ├── InMemoryBlockedCountryRepository.cs   # ConcurrentDictionary
│   ├── ITemporalBlockRepository.cs
│   ├── InMemoryTemporalBlockRepository.cs    # ConcurrentDictionary
│   ├── ILogRepository.cs
│   └── InMemoryLogRepository.cs              # ConcurrentQueue
├── Models/
│   ├── BlockedCountry.cs
│   ├── TemporalBlock.cs
│   ├── BlockAttemptLog.cs
│   ├── GeoLocationResult.cs
│   └── DTOs/
│       └── Requests.cs            # Request/response models + PagedResponse<T>
├── BackgroundServices/
│   └── TemporalBlockCleanupService.cs  # Runs every 5 minutes
├── appsettings.json
└── Program.cs                     # DI registration + middleware pipeline
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Internet connection (for ipapi.co calls)

### Run locally

```bash
# 1. Clone the repository
git clone https://github.com/A-Gaffer/IpBlockerApi.git
cd IpBlockerApi

# 2. (Optional) Add your ipapi.co key in appsettings.json
#    Leave empty to use the free unauthenticated tier (1000 req/day)

# 3. Run
dotnet run

# 4. Open Swagger
# https://localhost:7173
```

### Configure the geolocation API

In `appsettings.json`:

```json
"GeoLocation": {
  "BaseUrl": "https://ipapi.co",
  "ApiKey": ""
}
```

The API key is **optional** — ipapi.co works without one up to 1,000 requests/day. To get a free key, sign up at [ipapi.co](https://ipapi.co).

> ⚠️ Never commit your API key to GitHub. Use `appsettings.Development.json` for local secrets and add it to `.gitignore`.

---

## API Endpoints

### Countries

#### `POST /api/countries/block`
Block a country permanently.

**Request body:**
```json
{
  "countryCode": "US"
}
```

**Responses:**
| Code | Meaning |
|---|---|
| 200 | Country blocked successfully |
| 409 | Country is already blocked |
| 400 | Invalid country code format |

---

#### `DELETE /api/countries/block/{countryCode}`
Remove a country from the permanent block list.

**Example:** `DELETE /api/countries/block/US`

**Responses:**
| Code | Meaning |
|---|---|
| 200 | Country unblocked |
| 404 | Country was not blocked |

---

#### `GET /api/countries/blocked`
Get all permanently blocked countries with pagination and search.

**Query parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `page` | int | 1 | Page number |
| `pageSize` | int | 10 | Items per page (max 100) |
| `search` | string | — | Filter by country code or name |

**Example:** `GET /api/countries/blocked?page=1&pageSize=5&search=eg`

**Response:**
```json
{
  "page": 1,
  "pageSize": 5,
  "totalCount": 1,
  "totalPages": 1,
  "data": [
    {
      "countryCode": "EG",
      "countryName": "Egypt",
      "blockedAt": "2026-04-25T00:00:00Z"
    }
  ]
}
```

---

#### `POST /api/countries/temporal-block`
Block a country for a limited duration. Auto-unblocked after expiry.

**Request body:**
```json
{
  "countryCode": "TR",
  "durationMinutes": 120
}
```

**Validation:**
- `durationMinutes` must be between **1 and 1440** (24 hours)
- Country code must be a valid 2-letter ISO code
- Returns **409 Conflict** if already temporarily blocked

---

### IP

#### `GET /api/ip/lookup?ipAddress={ip}`
Resolve an IP address to its country and ISP details.

- If `ipAddress` is omitted, the **caller's own IP** is used automatically.

**Example:** `GET /api/ip/lookup?ipAddress=8.8.8.8`

**Response:**
```json
{
  "ip": "8.8.8.8",
  "countryCode": "US",
  "countryName": "United States",
  "city": "Mountain View",
  "isp": "Google LLC"
}
```

---

#### `GET /api/ip/check-block`
Automatically detects the caller's IP, resolves its country, checks if it's blocked, and logs the attempt.

**Response:**
```json
{
  "ip": "154.183.67.232",
  "countryCode": "EG",
  "countryName": "Egypt",
  "isBlocked": false,
  "message": "Access allowed — country 'EG' is not blocked."
}
```

---

### Logs

#### `GET /api/logs/blocked-attempts`
Return a paginated list of all check-block attempts.

**Query parameters:**

| Parameter | Type | Default |
|---|---|---|
| `page` | int | 1 |
| `pageSize` | int | 10 |

**Response:**
```json
{
  "page": 1,
  "pageSize": 10,
  "totalCount": 3,
  "totalPages": 1,
  "data": [
    {
      "ipAddress": "154.183.67.232",
      "timestamp": "2026-04-25T00:13:00Z",
      "countryCode": "EG",
      "isBlocked": false,
      "userAgent": "Mozilla/5.0 ..."
    }
  ]
}
```

---

## Design Decisions

### Why in-memory instead of a database?

This project intentionally avoids a database to demonstrate proficiency with thread-safe in-memory data structures. `ConcurrentDictionary` is used for O(1) lookups on blocked countries, and `ConcurrentQueue` for append-only log storage. The tradeoff is that data resets on restart — acceptable for this use case.

### Why Singleton for repositories?

Repositories must outlive individual HTTP requests to preserve data across the app's lifetime. Registering them as `Singleton` ensures a single shared instance exists for the entire process, making the in-memory collections act as a true shared store.

### Why IHttpClientFactory instead of new HttpClient()?

Creating `new HttpClient()` per request exhausts the socket pool under load. `IHttpClientFactory` manages connection pooling and lifetime correctly, and allows named clients with pre-configured timeouts and headers.

### Why BackgroundService for cleanup?

`BackgroundService` runs on a separate thread from the HTTP pipeline, so the 5-minute cleanup loop never blocks or delays incoming requests. It uses `IServiceProvider` to resolve the repository from the DI container safely.

---

## Author

**Ahmed Gaffer**
- GitHub: [@A-Gaffer](https://github.com/A-Gaffer)
- LinkedIn: [ahmed-gaffer](https://linkedin.com/in/ahmed-gaffer)
