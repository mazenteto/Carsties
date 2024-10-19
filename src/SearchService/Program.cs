

using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async() =>
{
   try
{
    await DbInitializer.InitDB(app);
}
catch (System.Exception e)
{
    
    System.Console.WriteLine(e);
} 
});


app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
=> HttpPolicyExtensions
.HandleTransientHttpError()
.OrResult(msg=>msg.StatusCode==System.Net.HttpStatusCode.NotFound)
.WaitAndRetryForeverAsync(_=>TimeSpan.FromSeconds(5));