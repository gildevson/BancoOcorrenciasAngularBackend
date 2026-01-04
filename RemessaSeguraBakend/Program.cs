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
    // No Koyeb/Cloud enviamos redes conhecidas como limpas para evitar bloqueios de IP
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
// 3. CORS - CONFIGURAÇÃO RESILIENTE
// ===============================================
const string CorsPolicy = "Front";
builder.Services.AddCors(options => {
    options.AddPolicy(CorsPolicy, policy => {
        policy.SetIsOriginAllowed(origin => {
            // Permite localhost e seus domínios oficiais
            var host = new Uri(origin).Host;
            return host == "localhost" ||
                   host == "bancoocorrencia.com" ||
                   host.EndsWith(".bancoocorrencia.com") ||
                   host.EndsWith(".koyeb.app");
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // Necessário se usar cookies ou SignalR futuramente
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

var brapiBase = (builder.Configuration["BrapiSettings:BaseUrl"] ?? "https://brapi.dev/api/").TrimEnd('/') + "/";
var brapiToken = builder.Configuration["BrapiSettings:ApiKey"];

builder.Services.AddHttpClient("brapi", c => {
    c.BaseAddress = new Uri(brapiBase);
    c.Timeout = TimeSpan.FromSeconds(30);
    if (!string.IsNullOrWhiteSpace(brapiToken))
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", brapiToken);
});

builder.Services.AddHttpClient("awesomeapi", c => {
    c.BaseAddress = new Uri("https://economia.awesomeapi.com.br/");
    c.Timeout = TimeSpan.FromSeconds(30);
});

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
        Description = "Bearer {seu_token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ===============================================
// 8. MIDDLEWARES PIPELINE (ORDEM CRÍTICA)
// ===============================================

// 1. Deve ser o primeiro para o .NET entender o protocolo HTTPS do Koyeb
app.UseForwardedHeaders();

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RemessaSegura API v1");
    c.RoutePrefix = "swagger";
});

app.UseStaticFiles();

// 2. Comente HttpsRedirection se o CORS continuar falhando no Koyeb 
// (muitas vezes o proxy do Koyeb já resolve isso e o redirecionamento quebra o CORS)
// app.UseHttpsRedirection(); 

// 3. CORS deve vir ANTES de Authentication e Authorization
app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();