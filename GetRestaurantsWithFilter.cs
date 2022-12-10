using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents.Client;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Linq;

#nullable enable

namespace Restaurant
{
  public static class GetRestaurantsWithFilter
  {
    [FunctionName("GetRestaurantsWithFilter")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        [CosmosDB(
            databaseName: "Restaurants",
            collectionName: "RestaurantItems",
            ConnectionStringSetting = "CosmosDBConnectionString")] DocumentClient client,
        ILogger log)
    {
      log.LogInformation("GetRestaurantsWithFilter trigger function processed a request.");

      bool? takeaway = null;
      int? minprice = null;
      int? maxprice = null;
      int? open = null;
      string? style = null;
      int? rating = null;

      style = req.Query["style"];
      minprice = getIntValue(req, "minprice");
      maxprice = getIntValue(req, "maxprice");
      rating = getIntValue(req, "rating");
      if (req.Query["takeaway"] == "true") takeaway = true;
      else if (req.Query["takeaway"] == "false") takeaway = false;
      string op = req.Query["openat"];
      try
      {
        var openat = op.Split(":", 2, StringSplitOptions.RemoveEmptyEntries);
        log.LogInformation("Open at: " + openat[0] + " H " + openat[1] + " M");
        open = Int32.Parse(openat[0]) * 60 + Int32.Parse(openat[1]);
      }
      catch { open = -1; }

      Uri collectionUri = UriFactory.CreateDocumentCollectionUri("Restaurants", "RestaurantItems");
      var itemQuery = client.CreateDocumentQuery<RestaurantItem>(collectionUri);
      IDocumentQuery<RestaurantItem> query = client.CreateDocumentQuery<RestaurantItem>(collectionUri)
            .AsDocumentQuery();

      var rs = new List<RestaurantItem>();

      while (query.HasMoreResults)
      {
        foreach (RestaurantItem result in await query.ExecuteNextAsync())
        {
          var pass = true;
          if (takeaway != null) { if (result.Takeaway != (bool)takeaway) pass = false; }
          if (minprice >= 1 && minprice <= 5) { if (result.Pricing < minprice) pass = false; }
          if (maxprice >= 1 && maxprice <= 5) { if (result.Pricing >= maxprice) pass = false; }
          if (open >= 0 && open <= 24 * 60)
          {
            if (result.OpeningH * 60 + result.OpeningM > open
          || result.ClosingH * 60 + result.ClosingM < open) pass = false;
          }
          if (style != null && style != "") { if (result.Style != style) pass = false; }
          if (rating >= 0 && rating <= 5) { if (result.Rating < rating) pass = false; }
          if (pass) { rs.Add(result); }
        }
      }
      return new OkObjectResult(new { Restaurants = rs });
    }

    private static int getIntValue(HttpRequest req, string variableName)
    {
      int value;
      try { value = Int32.Parse(req.Query[variableName]); }
      catch { value = -1; }
      return value;
    }
  }
}
