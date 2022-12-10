using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Restaurant
{
  public static class AddRestaurant
  {
    [FunctionName("AddRestaurant")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [CosmosDB(
        databaseName: "Restaurants",
        collectionName: "RestaurantItems",
        ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<dynamic> documentsOut,
        ILogger log)
    {
      log.LogInformation("AddRestaurant trigger function processed a request.");

      string requestBody = String.Empty;
      using (StreamReader streamReader = new StreamReader(req.Body))
      {
        requestBody = await streamReader.ReadToEndAsync();
      }
      dynamic data = JsonConvert.DeserializeObject<RestaurantItem>(requestBody);

      RestaurantItem newRestaurant = data;

      // create a random ID
      var RId = System.Guid.NewGuid().ToString();
      newRestaurant.id = RId;
      newRestaurant.partitionKey = RId;

      // Add a JSON document to the output container.
      var dataWithId = JsonConvert.SerializeObject(newRestaurant);
      Console.WriteLine("Sending JSON data to Cosmos: " + dataWithId);
      await documentsOut.AddAsync(newRestaurant);

      var responseMessage = JsonConvert.SerializeObject(RId);

      return new OkObjectResult(responseMessage);
    }
  }
}
