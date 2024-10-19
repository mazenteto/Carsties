using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Service;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitDB(WebApplication app)
    {
        await DB.InitAsync("SearchDb",MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDBConniction")));

        await DB.Index<Item>()
            .Key(x=>x.Make,KeyType.Text)
            .Key(x=>x.Model,KeyType.Text)
            .Key(x=>x.Color,KeyType.Text)
            .CreateAsync();
        var count = await DB.CountAsync<Item>();
       using var scope = app.Services.CreateScope();
        var httpClient =scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
        var items =await httpClient.GetItemsForSearchDB();
        System.Console.WriteLine(items.Count+" Returned from Auction Service");
        if(items.Count>0)
        await DB.SaveAsync(items);
    }

}
