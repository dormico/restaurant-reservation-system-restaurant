using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;

namespace Restaurant
{
  public static class GetAllRestaurants
  {
    static readonly HttpClient client = new HttpClient();

    [FunctionName("GetAllRestaurants")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = null)]HttpRequest req,
        [CosmosDB(
                databaseName: "Restaurants",
                collectionName: "RestaurantItems",
                ConnectionStringSetting = "CosmosDBConnectionString")] DocumentClient client,
        ILogger log)
    {
      log.LogInformation("C# HTTP trigger function processed a request.");

      Uri collectionUri = UriFactory.CreateDocumentCollectionUri("Restaurants", "RestaurantItems");

      var itemQuery = client.CreateDocumentQuery<RestaurantItem>(collectionUri);

      IDocumentQuery<RestaurantItem> query = client.CreateDocumentQuery<RestaurantItem>(collectionUri)
          .AsDocumentQuery();

      var rs = new List<RestaurantItem>();

      while (query.HasMoreResults)
      {
        foreach (RestaurantItem result in await query.ExecuteNextAsync())
        {
          log.LogInformation(result.Name);
          rs.Add(result);
        }
      }
      return new OkObjectResult(new { Restaurants = rs });
    }
  }
}