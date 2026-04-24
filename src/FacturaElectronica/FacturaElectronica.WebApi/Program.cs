using System.Reflection;
using Asp.Versioning;
using FacturaElectronica.Aplicacion.Abstracciones;
using FacturaElectronica.Aplicacion.Extensions;
using FacturaElectronica.Dominio.Extensions;
using FacturaElectronica.Infraestructura.Adapters.Inbound.Jobs.Extensions;
using FacturaElectronica.Infraestructura.Adapters.Inbound.Messaging.Extensions;
using FacturaElectronica.Infraestructura.Adapters.Outbound.Persistence.Extensions;
using FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.Extensions;
using FacturaElectronica.Infraestructura.Adapters.Outbound.Web.Hacienda.Extensions;
using FacturaElectronica.WebApi.Extensions;
using FacturaElectronica.WebApi.Middleware;
using Serilog;
using SmartCore.Telemetry;
// using Microservices.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// builder.AddServiceDefaults();

builder.Host.UseSerilog((context, loggerConfig) =>
{
    var appName = context.HostingEnvironment.ApplicationName;
    loggerConfig.Enrich.WithProperty("ApplicationName", appName)
        .ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddSmartCoreTelemetry(options =>
{
    options.ServiceName    = "factura-electronica-service";
    options.Version        = "1.0.0";
    options.Environment    = builder.Environment.EnvironmentName;
    options.OtlpEndpoint   = builder.Configuration["Telemetry:OtlpEndpoint"] ?? "http://localhost:4317";
    options.EnableMassTransit = true;
    options.SamplerRatio   = 1.0;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .ServicioDominio(builder.Configuration)
    .AddApplicationLayer()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration)
    .AddServiciosHacienda(builder.Configuration)
    .AddFacturaElectronicaStorage(builder.Configuration)
    .AddJobsLayer()
    .AddFacturacionElectronicaMassTransit(builder.Configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

builder.Services.AddApiVersionExtension();

builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

var app = builder.Build();

//app.UseHttpLogging();

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

var versionsGroup = app.MapGroup("api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet);
app.MapEndpoints(versionsGroup);

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();
}

app.MapHealthChecks("/api/health");

app.UseMiddleware<TenantMiddleware>();

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.Run();