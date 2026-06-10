var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ADD THIS
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Your service
builder.Services.AddSingleton<OpenAIService>();

var app = builder.Build();

// ADD THIS (IMPORTANT)
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();