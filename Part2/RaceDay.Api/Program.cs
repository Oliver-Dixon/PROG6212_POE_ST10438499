var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger: lists every endpoint so it can be tested in the browser
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
