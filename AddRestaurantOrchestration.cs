using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Restaurant;

public static class AddRestaurantOrchestration
{
  [FunctionName("AddRestaurantOrchestration")]
  public static async Task<string> RunOrchestrator(
      [OrchestrationTrigger] IDurableOrchestrationContext context,
      ILogger log)
  {
    log.LogInformation("AddRestaurantOrchestration function processed a request.");

    Restaurant data = context.GetInput<Restaurant>();

    var RId = await context.CallActivityAsync<string>("AddRestaurantActivity", data.RestaurantItem);
    var order = await context.CallHttpAsync(HttpMethod.Post, new System.Uri("https://orders-func-app.azurewebsites.net/api/addorderrestaurant?rid=" + RId));
    var userData = new {
      restaurant = RId,
      username = data.RestaurantItem.Name,
      email = data.RestaurantItem.Email,
      password = data.Password
    };
    string userJson = JsonConvert.SerializeObject(userData);
    var user = await context.CallHttpAsync(HttpMethod.Post, new System.Uri("https://auth-func-app.azurewebsites.net/api/adduser"), userJson);

    log.LogInformation($"AddRestaurantOrchestration ended.");
    log.LogInformation("Restaurant response: " + RId);
    log.LogInformation("Order response: " + order.StatusCode + " " + order.Content);
    log.LogInformation("User response: " + user.StatusCode + " " + user.Content);
    return RId;
  }

  [FunctionName("AddRestaurantActivity")]
  public static async Task<string> RunActivity(
      [ActivityTrigger] RestaurantItem rData,
      [CosmosDB(
        databaseName: "Restaurants",
        collectionName: "RestaurantItems",
        ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<dynamic> documentsOut,
      ILogger log)
  {
    log.LogInformation("AddRestaurant activity function processed a request.");

    // name can not be data (throws error)
    RestaurantItem newRestaurant = rData;    

    // create a random ID
    var RId = System.Guid.NewGuid().ToString();
    newRestaurant.id = RId;
    newRestaurant.partitionKey = RId;

    // Add a JSON document to the output container.
    var dataWithId = Newtonsoft.Json.JsonConvert.SerializeObject(newRestaurant);
    Console.WriteLine("Sending JSON data to Cosmos: " + dataWithId);
    await documentsOut.AddAsync(newRestaurant);

    return RId;
  }

  [FunctionName("AddRestaurantOrchestration_HttpStart")]
  public static async Task<HttpResponseMessage> HttpStart(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
      [DurableClient] IDurableOrchestrationClient starter,
      ILogger log)
  {
    Restaurant data = await req.Content.ReadAsAsync<Restaurant>();

    // Function input comes from the request content.
    string instanceId = await starter.StartNewAsync("AddRestaurantOrchestration", data);

    log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

    return starter.CreateCheckStatusResponse(req, instanceId);
  }
}