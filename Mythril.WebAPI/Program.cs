var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
options.AddDefaultPolicy(policy => policy.WithOrigins("http://localhost:5078") // your Blazor client URL
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
