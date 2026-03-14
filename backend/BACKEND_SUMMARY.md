# Backend - Legal Document Comparator

**.NET 8.0 backend** with **Clean Architecture** for comparing legal documents using AI.

---

## Project Structure

**Solution:** `LegalDocumentComparator.sln`  
**Shared build config:** `Directory.Build.props` - `net8.0`, `LangVersion: latest`, `Nullable: enable`, `ImplicitUsings: enable`

| Project | SDK | Role |
|---|---|---|
| `LegalDocumentComparator.Domain` | `Microsoft.NET.Sdk` | Entities, enums, value objects, exceptions |
| `LegalDocumentComparator.Application` | `Microsoft.NET.Sdk` | Use cases, interfaces, DTOs |
| `LegalDocumentComparator.Infrastructure` | `Microsoft.NET.Sdk` | EF Core, repositories, external services |
| `LegalDocumentComparator.WebApi` | `Microsoft.NET.Sdk.Web` | Controllers, middleware, startup |
| `LegalDocumentComparator.UnitTests` | `Microsoft.NET.Sdk` | xUnit test project |

**Reference graph:** Domain <- Application <- Infrastructure <- WebApi; UnitTests -> Domain + Application

---

## Architecture Overview

The backend follows Clean Architecture with strict dependency rules:

- **Domain** — pure C# entities with factory methods and invariant guards. No framework dependencies.
- **Application** — use cases (CQRS-lite commands/queries), repository interfaces, service interfaces, DTOs. No infrastructure dependencies.
- **Infrastructure** — EF Core + SQLite, PDF extraction (PdfPig), text diff (DiffPlex), Groq Cloud AI, BCrypt, JWT. Implements all Application interfaces.
- **WebApi** — ASP.NET Core controllers, JWT middleware, exception → RFC 7807 Problem Details mapping, Swagger, CORS.

Authentication is JWT Bearer. Roles: `User` (owns documents) and `Manager` (sees all documents, all comparisons).

Documents are stored as files on disk. Each document can have multiple versions. Comparing two versions triggers PDF text extraction, diff analysis, and an AI-generated legal summary — all cached in SQLite.

---

## Technologies

| Category | Library | Version |
|---|---|---|
| Framework | .NET / ASP.NET Core | 8.0 |
| Database | SQLite + EF Core | 8.0 |
| PDF Processing | PdfPig | 0.1.8 |
| Text Diff | DiffPlex | 1.7.2 |
| AI / LLM | Groq Cloud API | (via HttpClient) |
| Password Hashing | BCrypt.Net-Next | 4.0.3 |
| JWT | System.IdentityModel.Tokens.Jwt | 7.3.1 |
| JWT Middleware | Microsoft.AspNetCore.Authentication.JwtBearer | 8.0 |
| API Docs | Swashbuckle.AspNetCore | 6.5.0 |
| Tests | xUnit + Moq + FluentAssertions | 2.6.6 / 4.20.70 / 6.12.0 |

All libraries are open source or free!

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/v1/auth/login` | - | Login, returns JWT |
| POST | `/api/v1/auth/register` | - | Register new user |
| GET | `/api/v1/documents` | Bearer | List documents (+ `?includeAllUsers` for managers) |
| POST | `/api/v1/documents/upload` | Bearer | Upload PDF (`multipart/form-data`, 100 MB max) |
| GET | `/api/v1/documents/versions/{id}/file` | Bearer | Download a version PDF |
| POST | `/api/v1/comparisons/compare` | Bearer | Compare two versions with AI |

---

## Running Locally

```bash
cd backend/src/LegalDocumentComparator.WebApi
dotnet run

# Swagger UI available at https://localhost:5001/swagger
```

Default seed credentials:
- **Manager:** `admin@legal.com` / `Admin123!`
- **User:** `user@legal.com` / `User123!`

---

## Tests

```bash
cd backend
dotnet test
```

126 tests — Domain entities (7 files) + Application handlers (6 files). No Infrastructure tests.
