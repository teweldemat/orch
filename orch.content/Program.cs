using Microsoft.EntityFrameworkCore;
using orch.content;
using orch.core.ef;

var builder = WebApplication.CreateBuilder(args);

ContentServerConfig Config = builder.Configuration.GetSection("ContentServerConfig").Get<ContentServerConfig>();
// Add services to the container.
string conStr = builder.Configuration.GetConnectionString("pgcon");

builder.Services.AddDbContext<ContentDb>(options => options.UseNpgsql(conStr));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
