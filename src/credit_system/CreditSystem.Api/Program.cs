using CreditSystem.Api.EndPoints;
using CreditSystem.Application;
using CreditSystem.Domain;
using CreditSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddBusiness(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Credit System API", Version = "v1" });
});

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

app.UseHttpsRedirection();



app.Run();