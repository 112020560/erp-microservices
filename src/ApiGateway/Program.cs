using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// ── YARP ─────────────────────────────────────────────────────────────────────
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ── Auth (Keycloak JWT) ───────────────────────────────────────────────────────
var keycloak = builder.Configuration.GetSection("Keycloak");
var authEnabled = keycloak.GetValue<bool>("Enabled");

if (authEnabled)
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // Authority = Keycloak realm URL
            // e.g.  http://localhost:8082/auth/realms/erp
            options.Authority = keycloak["Authority"];
            options.Audience  = keycloak["Audience"];

            options.RequireHttpsMetadata = false; // dev only

            options.TokenValidationParameters.ValidateIssuer   = true;
            options.TokenValidationParameters.ValidateAudience = true;
        });

    builder.Services.AddAuthorization();
}

// ── CORS (ajusta origins cuando tengas el front) ──────────────────────────────
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin()
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();

if (authEnabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapReverseProxy();

app.Run();
