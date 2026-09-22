using BE_Mobile.Services;

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

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
