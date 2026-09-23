using BE_Mobile.Services;
using BE_Mobile.Security;
using Microsoft.AspNetCore.Authentication;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Lắng nghe trên tất cả card mạng (0.0.0.0) để thiết bị thật (iPhone, Android) có thể kết nối qua Wi-Fi
builder.WebHost.UseUrls("http://0.0.0.0:5113");

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddBackendServices();
builder.Services.AddDataProtection();
builder.Services.AddSingleton<AdminTokenService>();
builder.Services.AddAuthentication(AdminTokenService.Scheme)
    .AddScheme<AuthenticationSchemeOptions, AdminAuthenticationHandler>(AdminTokenService.Scheme, _ => { });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("admin-login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddCors(options => options.AddPolicy("AdminWeb", policy =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    if (origins.Length > 0) policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(AdminSwagger.Configure);

var app = builder.Build();

if (args.Contains("--bootstrap-admin", StringComparer.Ordinal))
{
    await AdminBootstrap.RunAsync(app.Services, app.Configuration);
    await app.DisposeAsync();
    return;
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AdminWeb");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
