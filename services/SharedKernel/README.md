# SharedKernel

Cross-cutting library shared by WebApi (mirrors NexSchedBackend SharedKernel).

```
SharedKernel/
├── DTOs/                 # TenantContext, shared request/response types
├── Entities/             # Base entities, platform & tenant entities
├── Enums/                # TenantStatus, UserType, roles
├── Settings/             # AppSettings, JwtSettings, DatabaseSettings
├── Services/             # EmailService, etc.
└── Utilities/
    ├── Exceptions/       # Domain exceptions
    ├── Extensions/       # HttpContext / TenantContext extensions
    ├── Helpers/          # JwtHelper, DbHelper (SQL Server)
    └── Middlewares/      # GlobalException, TenantContext
```

Register via `Startup.cs` → `InjectGlobalConfigurations()` (to be added).
