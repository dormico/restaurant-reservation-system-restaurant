using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Restaurant;

public static class AddRestaurantOrchestration
{
  [FunctionName("AddRestaurantOrchestration")]
  public static async Task<string> RunOrchestrator(
      [OrchestrationTrigger] IDurableOrchestrationContext context,
      ILogger log)
  {
    log.LogInformation("AddRestaurantOrchestration function processed a request.");

    RestaurantItem adat = context.GetInput<RestaurantItem>();

    var outputs = new List<string>();

    var RId = await context.CallActivityAsync<string>("AddRestaurantActivity", adat);
    var added = await context.CallHttpAsync(HttpMethod.Post, new System.Uri("https://orders-func-app.azurewebsites.net/api/addorderrestaurant?rid=" + RId));
    log.LogInformation($"AddRestaurantOrchestration ended.");

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
    RestaurantItem data = await req.Content.ReadAsAsync<RestaurantItem>();

    // Function input comes from the request content.
    string instanceId = await starter.StartNewAsync("AddRestaurantOrchestration", data);

    log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

    return starter.CreateCheckStatusResponse(req, instanceId);
  }
}