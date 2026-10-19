using System.Text;
using System.Text.Json.Serialization;
using GLMS_API.Factories;
using GLMS_API.Models;
using GLMS_API.Observers;
using GLMS_API.Repositories;
using GLMS_API.Services;
using GLMS_API.Strategies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<GlmsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();

// Services
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<ContractService>();
builder.Services.AddScoped<ServiceRequestService>();
builder.Services.AddScoped<CurrencyService>();
builder.Services.AddScoped<ServiceRequestSubject>(sp =>
{
    var notifications = new List<string>();
    var subject = new ServiceRequestSubject(notifications);
    subject.Attach(new GeneralNotifier(notifications));
    return subject;
});

// Patterns
builder.Services.AddScoped<IContractFactory, ContractFactory>();
builder.Services.AddScoped<IPriceStrategy, UsdToZarStrategy>();
builder.Services.AddScoped<PricingContext>();


builder.Services.AddHttpClient();


var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }