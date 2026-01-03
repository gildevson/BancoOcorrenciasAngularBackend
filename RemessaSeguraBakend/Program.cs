using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RemessaSeguraBakend.Data;
using RemessaSeguraBakend.Repositories;
using RemessaSeguraBakend.Services;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===============================================
// 0. FORWARDED HEADERS (Koyeb / Proxy HTTPS)
// ===============================================
builder.Services.Configure<ForwardedHeadersOptions>(options => {
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ===============================================
// 1. CONFIGURAÇÃO DE CONTROLLERS
// ===============================================
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

// ===============================================
// 2. INFRAESTRUTURA E DATABASE
// ===============================================
builder.Services.AddSingleton<DbConnectionFactory>();

// ===============================================
// 3. CORS - CONFIGURAÇÃO COMPLETA
// ===============================================
const string CorsPolicy = "RemessaSeguraPolicy";

builder.Services.AddCors(options => {
    options.AddPolicy(CorsPolicy, policy => {
        // Ambiente de desenvolvimento
        if (builder.Environment.IsDevelopment()) {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        // Ambiente de produção
        else {
            policy.WithOrigins(
                    // Frontend em produção (Hostinger)
                    "https://bancoocorrencia.com",
                    "https://www.bancoocorrencia.com",

                    // API Backend (Koyeb) - para requisições internas
                    "https://exceptional-melita-gildevson-sistemas-1fffc163.koyeb.app",

                    // Local development
                    "http://localhost:4200",
                    "http://localhost:5000",
                    "https://localhost:5001"
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithExposedHeaders("Token-Expired", "Authorization");
        }
    });
});

// ===============================================
// 4. INJEÇÃO DE DEPENDÊNCIA - REPOSITORIES
// ===============================================
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<PermissaoRepository>();
builder.Services.AddScoped<NoticiaRepository>();
builder.Services.AddScoped<BancoRepository>();
builder.Services.AddScoped<ResetSenhaRepository>();
builder.Services.AddScoped<OcorrenciasMotivosRepository>();
builder.Services.AddScoped<BancosRepository>();

// ===============================================
// 5. INJEÇÃO DE DEPENDÊNCIA - SERVICES
// ===============================================
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<PasswordResetService>();

// ===============================================
// 6. CLIENTES HTTP
// ===============================================
builder.Services.AddMemoryCache();

// BRAPI - API de Ações
var brapiBase = (builder.Configuration["BrapiSettings:BaseUrl"] ?? "https://brapi.dev/api/").TrimEnd('/') + "/";
var brapiToken = builder.Configuration["BrapiSettings:ApiKey"];

builder.Services.AddHttpClient("brapi", c => {
    c.BaseAddress = new Uri(brapiBase);
    c.Timeout = TimeSpan.FromSeconds(30);
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    if (!string.IsNullOrWhiteSpace(brapiToken)) {
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", brapiToken);
    }
});

// AwesomeAPI - Cotações de Moedas
builder.Services.AddHttpClient("awesomeapi", c => {
    c.BaseAddress = new Uri("https://economia.awesomeapi.com.br/");
    c.Timeout = TimeSpan.FromSeconds(30);
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// Cliente HTTP genérico
builder.Services.AddHttpClient();

// ===============================================
// 7. AUTENTICAÇÃO JWT
// ===============================================
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey)) {
    throw new InvalidOperationException(
        "JWT:Key não configurada. " +
        "Configure a variável de ambiente 'Jwt__Key' no Koyeb."
    );
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents {
            OnAuthenticationFailed = context => {
                if (context.Exception is SecurityTokenExpiredException) {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context => {
                // Log para debug em caso de falha na autenticação
                if (builder.Environment.IsDevelopment()) {
                    Console.WriteLine($"OnChallenge error: {context.Error}, {context.ErrorDescription}");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ===============================================
// 8. SWAGGER / OpenAPI
// ===============================================
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "RemessaSegura API",
        Version = "v1",
        Description = "API para gestão de remessas seguras e notícias financeiras",
        Contact = new OpenApiContact {
            Name = "Suporte RemessaSegura",
            Email = "suporte@bancoocorrencia.com"
        }
    });

    // Configuração de segurança JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// ===============================================
// BUILD DA APLICAÇÃO
// ===============================================
var app = builder.Build();

// ===============================================
// 9. MIDDLEWARES PIPELINE
// ===============================================

// Forwarded Headers - DEVE SER O PRIMEIRO
app.UseForwardedHeaders();

// Swagger - Disponível em todos os ambientes
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RemessaSegura API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "RemessaSegura API Documentation";
});

// Static Files
app.UseStaticFiles();

// HTTPS Redirection
// Comentar se der problema no Koyeb
app.UseHttpsRedirection();

// CORS - ANTES de Authentication
app.UseCors(CorsPolicy);

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

// Health Check endpoint
app.MapGet("/health", () => Results.Ok(new {
    status = "healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}))
.AllowAnonymous();

// Root endpoint
app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

// ===============================================
// EXECUÇÃO
// ===============================================
try {
    app.Logger.LogInformation("🚀 Iniciando RemessaSegura API...");
    app.Logger.LogInformation("📍 Ambiente: {Environment}", app.Environment.EnvironmentName);
    app.Logger.LogInformation("🌐 CORS configurado para: {Policy}", CorsPolicy);

    app.Run();
} catch (Exception ex) {
    app.Logger.LogCritical(ex, "❌ Erro fatal ao iniciar a aplicação");
    throw;
}