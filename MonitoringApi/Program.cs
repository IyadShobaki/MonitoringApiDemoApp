using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MonitoringApi.HealthChecks;
using WatchDog;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
   .AddCheck<RandomHealthCheck>("Site Health Check")
   .AddCheck<RandomHealthCheck>("Database Health Check"); // same for demo only

builder.Services.AddWatchDogServices();

builder.Services.AddHealthChecksUI(opts =>
{
   // The UI will point to this url 
   // We can add multiple end points
   // In microservices env we can have 1 client (web server) to
   // check and monitor the health of all APIs from 1 location
   opts.AddHealthCheckEndpoint("api", "/health");
   opts.SetEvaluationTimeInSeconds(5);  //5 sec for demo only. Maybe every 10 min in prod
   opts.SetMinimumSecondsBetweenFailureNotifications(10); //10 sec for demo only. Maybe every 5 min 

}).AddInMemoryStorage(); // Store these data in memory for this demo but it can be stored in the DB

var app = builder.Build();

app.UseWatchDogExceptionLogger(); // It logs any unhandled exceptions if we miss any


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
   ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse // to help us formt our health checks ti give us more info
});
app.MapHealthChecksUI();

app.UseWatchDog(opts =>
{
   opts.WatchPageUsername = app.Configuration.GetValue<string>("WatchDog:UserName");
   opts.WatchPagePassword = app.Configuration.GetValue<string>("WatchDog:Password");
   opts.Blacklist = "health"; // To not monitor this endpoint
});

app.Run();
