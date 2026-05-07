using Flightr.Data;
using Flightr.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Flightr API",
        Version = "v1",
        Description = "API for managing flight logs and pilot profiles"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter your JWT token in the format: Bearer {your token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ExternalApi", policy =>
    {
        if (corsOrigins.Length == 0 || corsOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(corsOrigins);
        }

        policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .AllowAnyHeader();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "server=localhost;port=3306;database=flightr;user=flightr;password=flightr_dev";

builder.Services.AddDbContext<FlightrDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is missing from configuration.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<FlightrDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

// Email sender (uses Smtp settings if configured). If Smtp:Host is not set, the implementation will log and skip sending.
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Flightr API v1");
        options.RoutePrefix = "swagger";
    });

    app.UseCors("ExternalApi");
}
else
{
    app.UseHttpsRedirection();

    app.UseCors("ExternalApi");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
