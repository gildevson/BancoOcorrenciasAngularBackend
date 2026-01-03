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
// 3. CORS - FRONTEND (Hostinger) + Local
// ===============================================
const string CorsPolicy = "Front";
builder.Services.AddCors(options => {
    options.AddPolicy(CorsPolicy, policy => {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://bancoocorrencia.com",
                "https://www.bancoocorrencia.com"
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
        // ⚠️ JWT em header NÃO precisa AllowCredentials().
        // Se você usar cookies/sessão, aí sim habilita:
        // .AllowCredentials();
    });
});

// ===============================================
// 4. INJEÇÃO DE DEPENDÊNCIA
// ===============================================
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<PermissaoRepository>();
builder.Services.AddScoped<NoticiaRepository>();
builder.Services.AddScoped<BancoRepository>();
builder.Services.AddScoped<ResetSenhaRepository>();
builder.Services.AddScoped<OcorrenciasMotivosRepository>();
builder.Services.AddScoped<BancosRepository>();

builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<PasswordResetService>();

// ===============================================
// 5. CLIENTES HTTP
// ===============================================
builder.Services.AddMemoryCache();

// ✅ BRAPI (ajustado para bater com seu appsettings: BrapiSettings)
var brapiBase = (builder.Configuration["BrapiSettings:BaseUrl"] ?? "https://brapi.dev/api/")
    .TrimEnd('/') + "/";

var brapiToken = builder.Configuration["BrapiSettings:ApiKey"]; // sua key

builder.Services.AddHttpClient("brapi", c => {
    c.BaseAddress = new Uri(brapiBase);
    c.Timeout = TimeSpan.FromSeconds(30);

    // Se sua BRAPI usa header "Authorization: Bearer <token>", mantém assim:
    if (!string.IsNullOrWhiteSpace(brapiToken))
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", brapiToken);
});

// ✅ AwesomeAPI (moedas)
builder.Services.AddHttpClient("awesomeapi", c => {
    c.BaseAddress = new Uri("https://economia.awesomeapi.com.br/");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// (Opcional) client genérico
builder.Services.AddHttpClient();

// ===============================================
// 6. AUTENTICAÇÃO JWT
// ===============================================
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new Exception("JWT:Key não encontrada. Configure Jwt__Key no ambiente (Koyeb).");

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
                if (context.Exception is SecurityTokenExpiredException)
                    context.Response.Headers.Append("Token-Expired", "true");

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ===============================================
// 7. SWAGGER
// ===============================================
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RemessaSegura API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT desta forma: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ===============================================
// 8. MIDDLEWARES
// ===============================================

// ✅ Importantíssimo atrás de proxy (Koyeb)
app.UseForwardedHeaders();

// ✅ Swagger liberado em produção para testes (depois você pode restringir)
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RemessaSegura API v1");
    c.RoutePrefix = "swagger";
});

app.UseStaticFiles();

// ⚠️ Se der loop/erro no Koyeb, comente a linha abaixo.
// Geralmente funciona bem com ForwardedHeaders.
app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
