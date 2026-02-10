

## Dating App (Angular 20 + ASP.NET Core)

This was a project that was based on the Udemy course: `Build an app with ASPNET Core and Angular from scratch` - After completion, I added and improved the security features as well as did changes to the look of the client application.


Live demo: https://da-10-2025.azurewebsites.net

*Minor Note: This is being run the F1 plan on azure and may reach its usage limit*



### Tech Stack Used
- Client: Angular 20, Tailwind CSS, DaisyUI, SignalR client
- API: ASP.NET Core, EF Core, ASP.NET Identity, JWT auth, SignalR
- Data: SQL Server (local/azure)

### Key Features
- User registration/login with role-based access
- Member discovery with filters and pagination
- Likes system with list views
- Real-time presence and messaging (SignalR)
- Admin role management

## Architecture & Security Summary

### Overall Structure
- API (ASP.NET Core): Controllers, Services, Data, Entities, DTOs, SignalR, Middleware
- Client (Angular 20): Core services/interceptors/guards, feature modules, shared UI

### Auth & Token Flow
- ASP.NET Identity for user management and hashing
- Access token (JWT) issued on login/register
- Refresh token stored in HttpOnly cookie
- Refresh token rotation with family reuse detection

### Security Features Implemented
- JWT issuer/audience validation with short-lived access tokens (20 minutes)
- Refresh token hashing at rest + reuse revocation (family-based rotation)
- Authorization enforced on likes/messages endpoints
- Safe member ID retrieval to avoid exception loops
- SignalR auth uses latest in-memory token
- Client refresh scheduling from JWT exp + single-flight refresh
- 401 auto-refresh and retry in the client

### Notable Hardening Changes
- Refresh tokens moved to dedicated entity with indexes and metadata
- Reuse detection revokes the entire token family
- Admin member profile seeding to prevent missing-member errors

## Run Locally (Fresh Clone)

### Prereqs
- .NET SDK 9.x
- Node.js 20+
- Angular CLI 20
- SQL Server (local or Docker)

### 1) Clone
```bash
git clone <this>
cd Dotnet9-DatingApp
```

### 2) Start SQL Server
If you already have SQL Server, skip this. (I would still recommend docker using the docker-compose.yml)

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Password@2" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

### 3) Configure API Settings
Default dev settings are in API/appsettings.Development.json.

Make sure:
- ConnectionStrings:DefaultConnection points to your SQL Server
- TokenKey is >= 64 chars
- Jwt:Issuer and Jwt:Audience are set

### 4) Run the API
```bash
cd API
dotnet restore
dotnet ef database update
dotnet run
```

API runs at https://localhost:5001 (see API/Properties/launchSettings.json).

If HTTPS fails, create and trust a local dev certificate:
```bash
dotnet dev-certs https --trust
```

### 5) Run the Client
```bash
cd ../client
npm install
npm start
```

Client runs at https://localhost:4200.

### 6) Login
Seeded accounts are created on first run (see API/Data/Seed.cs).

- Admin: admin@test.com / Pa$$w0rd
- Sample members use the same password
