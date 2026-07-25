Controllers live under `Features/{Module}/` — not in this folder.

Each module follows:

```
Features/{Module}/
├── {Module}Controller.cs
├── {Module}Service.cs
├── DTOs.cs
└── Infrastructure/
    ├── I{Module}Repository.cs
    └── {Module}Repository.cs
```
