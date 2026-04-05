/* 
*    IMapService.cs
*    4/4/2026
*    ======================================
*    - Initial creation
*    ======================================
*    Interface for calls to the Azure Maps APIs
*   
*/

using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace DataLibrary.ServiceLayer.MapService;
public class MapService(IConfiguration _config) : IMapService
{
    // Search an input area for businesses that offer a specified type of service
    public async Task<List<Dictionary<string, string>>?> SearchForService(string category, string location, double radiusInMiles, int limit, string? brandSet = null)
    {
        try
        {
            // Get the longitude and latitude of the input location
            var position = await GetCoords(location);

            // Convert from miles to meters
            double radiusInMeters = radiusInMiles * 1609.344;

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_config["MapsUri"] ?? string.Empty);
                httpClient.DefaultRequestHeaders.Add("Subscription-Key", _config["MapsSubscriptionKey"]);

                // Use HttpClient to call the Azure Maps Search API for point-of-interest search
                HttpResponseMessage response = await httpClient.GetAsync(
                    $"https://atlas.microsoft.com/search/poi/json" +
                    $"?api-version=1.0" +
                    $"&query={Uri.EscapeDataString(category)}" +
                    $"&lat={position.Value.Item1}" +
                    $"&lon={position.Value.Item2}" +
                    $"&radius={radiusInMeters}" +
                    $"&limit={limit}"
                    );

                if (response.IsSuccessStatusCode)
                {
                    // Read the result as a string
                    var responseResult = await response.Content.ReadAsStringAsync();

                    // Parse the result into a Json Object
                    var jsonResponse = JObject.Parse(responseResult);

                    List<Dictionary<string, string>> results = new();

                    // Parse Json Object response into a list of dictionaries to return
                    foreach (var result in jsonResponse["results"])
                    {
                        // Create a new dictionary to hold the name, phone number, and address of the business
                        Dictionary<string, string> entry = new();
                        entry.Add("Name", result["poi"]?["name"]?.ToString() ?? string.Empty);
                        entry.Add("Phone", result["poi"]?["phone"]?.ToString() ?? string.Empty);
                        entry.Add("Location", result["address"]?["freeformAddress"]?.ToString() ?? string.Empty);

                        // Add the dictionary to the list of results
                        results.Add(entry);
                    }

                    return results;
                }
                else
                    Console.WriteLine($"Error: {response.ReasonPhrase}");
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Request error: " + e.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }

        return null;
    }

    // Get the latitude and longitude of a passed in location string
    public async Task<(double, double)?> GetCoords(string location)
    {
        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_config["MapsUri"] ?? string.Empty);
                httpClient.DefaultRequestHeaders.Add("Subscription-Key", _config["MapsSubscriptionKey"]);

                // Use HttpClient to call the Azure Maps Search API for fuzzy location search
                HttpResponseMessage response = await httpClient.GetAsync(
                        $"https://atlas.microsoft.com/search/fuzzy/json" +
                        $"?api-version=1.0" +
                        $"&query={Uri.EscapeDataString(location)}" +
                        $"&limit=1" 
                    );

                if (response.IsSuccessStatusCode)
                {
                    // Read the result as a string
                    var result = await response.Content.ReadAsStringAsync();

                    // Parse the result into a Json Object
                    var jsonResponse = JObject.Parse(result);

                    // Get the latitude and longitude
                    var lat = jsonResponse["results"]?[0]?["position"]?["lat"];
                    var lon = jsonResponse["results"]?[0]?["position"]?["lon"];

                    // Return the coordinates as doubles
                    return ((double)lat, (double)lon);
                }
                else
                    Console.WriteLine($"Error: {response.ReasonPhrase}");
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Request error: " + e.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }

        return null;
    }
}