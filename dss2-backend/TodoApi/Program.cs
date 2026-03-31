using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Data;
using TodoApi.Services;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────
// 1. CONTROLLERS + JSON OPTIONS
// ─────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings ("low" not 0)
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
    });

// ─────────────────────────────────────────
// 2. SWAGGER
// ─────────────────────────────────────────
builder.Services.AddOpenApi();
// ─────────────────────────────────────────
// 3. DATABASE
// ─────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connString = builder.Configuration
                            .GetConnectionString("DefaultConnection");
    options.UseNpgsql(connString);
});

// ─────────────────────────────────────────
// 4. JWT AUTHENTICATION
// ─────────────────────────────────────────

// Prevent ASP.NET from remapping JWT claim names
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer            = true,
            ValidateAudience          = true,
            ValidateIssuerSigningKey  = true,
            ValidateLifetime          = true,
            ValidIssuer               = jwt["Issuer"],
            ValidAudience             = jwt["Audience"],
            IssuerSigningKey          = new SymmetricSecurityKey(key),
            ClockSkew                 = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// ─────────────────────────────────────────
// 5. CORS
// ─────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // React dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ─────────────────────────────────────────
// 6. HTTP CONTEXT ACCESSOR
// ─────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ─────────────────────────────────────────
// 7. SERVICES
// ─────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITodoService, TodoService>();

var app = builder.Build();

// ─────────────────────────────────────────
// 8. MIDDLEWARE PIPELINE
// ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");

// ORDER MATTERS — Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();