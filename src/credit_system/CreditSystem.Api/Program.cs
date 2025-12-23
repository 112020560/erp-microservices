using CreditSystem.Api.EndPoints;
using CreditSystem.Api.Infrastructure;
using CreditSystem.Application;
using CreditSystem.Domain;
using CreditSystem.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddBusiness(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Credit System API", Version = "v1" });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

// Endpoints
app.MapLoanContractEndpoints();
app.MapAdminEndpoints();
app.MapDelinquentLoansEndpoints();

app.UseHttpsRedirection();



app.Run();