using QareebChat;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependenceyInjection(builder.Configuration);

builder.Host.UseSerilog((context,configration)=>configration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
