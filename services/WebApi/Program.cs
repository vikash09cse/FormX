using SharedKernel;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddPolicy("AllowAngularApps", p =>
    p.WithOrigins("http://localhost:4200", "https://localhost:4200",
                  "http://localhost:4201", "https://localhost:4201")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

builder.InjectGlobalConfigurations(typeof(WebApiStartup).Assembly);

var app = builder.Build();
app.UseGlobalConfigurations();
app.Run();
