# FormX

Multi-tenant platform for tenant onboarding, user management, and document templates.

Single **WebApi** serves both Angular apps directly (no API Gateway).

## Repository layout

```
src/
├── FormX-Backend.sln           # .NET 10 backend solution
├── SuperAdmin/                 # Angular — Platform Admin
├── TenantLogin/                # Angular — Tenant portal
├── databases/
│   └── FormXDB/                # SQL Server migrations (DbUp)
└── services/
    ├── SharedKernel/           # JWT, middleware, helpers
    └── WebApi/                 # REST API (Features/*)
```

## Modules

### Super Admin (port 4200)

| Module | Route |
|--------|-------|
| Tenant Management | `/tenants` |
| Platform User Management | `/platform-users` |
| Template Management | `/document-templates` |
| Login | `/login` |

### Tenant Portal (port 4201)

| Module | Route |
|--------|-------|
| Login | `/login` |
| Tenant User Management | `/users` |
| Templates | `/document-templates` |

### API

| Module | Route prefix |
|--------|--------------|
| Auth | `/api/auth` |
| Platform Admin | `/api/platform` |
| Global Document Templates | `/api/platform/document-templates` |
| Tenant Users | `/api/users` |
| Tenant Document Templates | `/api/document-templates` |

## Quick start

```bash
# From FormX/src
dotnet restore FormX-Backend.sln
dotnet build FormX-Backend.sln

# Create / migrate database (configure connection string first)
dotnet run --project databases/FormXDB

# API
dotnet run --project services/WebApi

# Super Admin
cd SuperAdmin && npm install && ng serve --port 4200

# Tenant Portal
cd TenantLogin && npm install && ng serve --port 4201
```

Dev API base: `https://localhost:7150/api`

## Default credentials (after FormXDB migration)

| User | Email | Password |
|------|-------|----------|
| Platform Admin | `admin@formx.com` | `Admin@123` |

## Auth endpoints

- `POST /api/auth/platform-login`
- `POST /api/auth/tenant-login`
- `GET /api/auth/tenant-by-subdomain/{subdomain}`
- `POST /api/auth/refresh-token`
- `POST /api/auth/logout`
