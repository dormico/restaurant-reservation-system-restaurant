using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Restaurant
{
  public class RestaurantById
  {    
    [FunctionName("GetRestaurantById")]
    public static async Task<IActionResult> GetRestaurant(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", 
        Route = "RestaurantItems/{id}")] HttpRequest req,
        [CosmosDB(
            databaseName: "Restaurants",
            collectionName: "RestaurantItems",
            ConnectionStringSetting = "CosmosDBConnectionString",
            Id = "{id}",
            PartitionKey = "{id}")] RestaurantItem restaurantItem,
        ILogger log)
    {
      log.LogInformation("C# HTTP trigger function processed a request.");

      if (restaurantItem == null)
      {
        log.LogInformation($"Restaurant not found");
        return new NotFoundResult();
      }
      else
      {
        log.LogInformation($"Found RestaurantItem, Name: {restaurantItem.Name}");
        return new OkObjectResult(restaurantItem);
      }
    }

    [FunctionName("GetMenuByRestaurantId")]
    public static async Task<IActionResult> GetMenu(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", 
        Route = "RestaurantItems/{partitionKey}/{id}/menu")] HttpRequest req,
        [CosmosDB(
            databaseName: "Restaurants",
            collectionName: "RestaurantItems",
            ConnectionStringSetting = "CosmosDBConnectionString",
            Id = "{id}",
            PartitionKey = "{partitionKey}")] RestaurantItem restaurantItem,
        ILogger log)
    {
      log.LogInformation("C# HTTP trigger function processed a request.");

      if (restaurantItem == null)
      {
        log.LogInformation($"Restaurant not found");
        return new NotFoundResult();
      }
      else
      {
        log.LogInformation($"Found Menu of Restaurant: {restaurantItem.Menu}");
        return new OkObjectResult(restaurantItem.Menu);
      }
    }
  }
}
