/* 
*    IMapService.cs
*    4/4/2026
*    ======================================
*    - Initial creation
*    ======================================
*    Interface for calls to the Azure Maps APIs
*   
*/

namespace DataLibrary.ServiceLayer.MapService
{
    public interface IMapService
    {
        // Search an input area for businesses that offer a specified type of service
        Task<List<Dictionary<string, string>>?> SearchForService(string category, string location, double radiusInMiles, int limit, string? brand = null);

        // Get the latitude and longitude of a passed in location string
        Task<(double, double)?> GetCoords(string location);
    }
}
