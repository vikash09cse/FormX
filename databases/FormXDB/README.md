# FormXDB — SQL Server migrations

DbUp-based schema, seed, and stored procedures for FormX.

```
FormXDB/
├── Schema/Tables/          # Core + forms/roles/projects tables
├── Schema/Seed/            # Platform admin seed
├── Migrations/             # Template seed / incremental scripts
└── Programmability/
    └── Procedures/         # Auth, platform, users, document templates
```

## Phase 1 form schema tables

| Table | Purpose |
|-------|---------|
| `roles` | Tenant custom roles (`isleader`) |
| `user_roles` | User ↔ role |
| `projects` | Tenant project master |
| `user_scopes` | Leader visibility by project |
| `forms` | Tenant forms |
| `form_groups` | Groups within a form |
| `form_fields` | Dynamic fields (parent, email notify, regex preset) |
| `form_field_options` | Dropdown/radio/checkbox options |
| `form_field_parent_options` | Many parent option values for conditional show |
| `validation_regex_presets` | Predefined regex formats (seeded) |
| `form_roles` | Which roles may fill a form |
| `form_submissions` | Submitted responses |
| `form_submission_values` | Field values per submission |

```bash
dotnet run --project databases/FormXDB
```
