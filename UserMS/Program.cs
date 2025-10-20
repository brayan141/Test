using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserMS.Infrastructure.Extension;
using UserMS.Middleware;
using UserMS.Service;

var builder = WebApplication.CreateBuilder(args);

// Secret key debe ser igual al de IdentityMS
var secretKey = "G8r#qBzL5uM2!eWp9@YtNvZcF3$hRxKd";
var issuer = "IdentityMS";
var audience = "UserMS";

// Servicios base
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddDbContext(builder.Configuration);
builder.Services.AddAutoMapper();
builder.Services.AddAddScopedServices();
builder.Services.AddTransientServices();
builder.Services.AddSetting(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerOpenAPI();
builder.Services.AddMediatorCQRS();
builder.Services.AddVersioning();

// JSON y controladores
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", cors =>
    {
        var env = builder.Environment;

        if (env.IsDevelopment())
        {
            cors.WithOrigins("http://localhost:5047", "http://localhost:4200") // API Gateway
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .AllowCredentials();
        }
        else
        {
            cors.WithOrigins("https://api.softnova.com.co:5010", "http://localhost:4200") // <-- dominio de producci n del API Gateway
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
        }
    });
});

// Autenticaci n JWT con clave sim trica
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        };
    });

// Autorizaci n por scope (opcional)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserMS", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "UserMS");
    });
});

var app = builder.Build();

// Swagger
app.UseSwagger(options =>
        options.RouteTemplate = "swagger/{documentName}/swagger.json")
    .UseSwaggerUI(c =>
    {
        c.DocumentTitle = "User MS API v1.0";
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User MS API v1.0");
        c.DisplayRequestDuration();
        c.DefaultModelExpandDepth(-1);
        c.EnableDeepLinking();
        c.InjectStylesheet("assets/swagger-ui.css");
    });

// Middlewares
app.UseStaticFiles();
// descomentariar cuando tengamos el certificado de producci n
//app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/health");

app.Run();