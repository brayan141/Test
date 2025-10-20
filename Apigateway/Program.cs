using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Quitar al instalar certificado
//builder.Services.AddSingleton<DelegatingHandler, BypassSslValidationHandler>();

// Configuración autenticación JWT Bearer
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "IdentityMS",

            ValidateAudience = true,
            ValidAudiences = new[] { "UserMS", "LocationMS", "InventoryMS" }, // O usa ValidAudiences si tienes varios

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("G8r#qBzL5uM2!eWp9@YtNvZcF3$hRxKd")
            ),

            NameClaimType = "sub",
            RoleClaimType = "scope"
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Falló autenticación:");
                Console.WriteLine(context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validado");
                var identity = context.Principal.Identity as ClaimsIdentity;
                var scopeClaims = identity?.FindAll("scope").Select(c => c.Value).ToArray();

                if (scopeClaims != null && scopeClaims.Length > 0)
                {
                    Console.WriteLine("Scopes encontrados: " + string.Join(", ", scopeClaims));
                }
                else
                {
                    Console.WriteLine("Claim 'scope' no existe");
                }

                return Task.CompletedTask;
            }
        };
    });

// Evita el mapeo automático del claim "scope"
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("scope");

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", cors =>
    {
        cors.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// Cargar configuración Ocelot desde archivo
builder.Configuration.AddJsonFile("configuration.json", optional: false, reloadOnChange: true);

// Añadir servicios Ocelot
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// Middleware para debug de requests
app.Use(async (context, next) =>
{
    Console.WriteLine("Request Path: " + context.Request.Path);
    Console.WriteLine("Authorization Header: " + context.Request.Headers["Authorization"]);
    await next();
});

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();

app.Run();
