/* 
*    BrowserStorageService.cs
*    3/14/2026
*    ======================================
*    - Initial creation
*    ======================================
*    Service Layer for browser storage access
*   
*/

using Microsoft.JSInterop;
using System.Text.Json;

namespace NextSteps.Client
{
    public class BrowserStorageService
    {
        private readonly IJSRuntime _js;

        public BrowserStorageService(IJSRuntime js)
        {
            _js = js;
        }

        // Store something in local storage, identifiable by a key
        public async Task SetInStorage<T>(string key, T value)
        {
            await _js.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value));
        }

        // Get something from local storage by its key
        public async Task<T?> GetFromStorage<T>(string key)
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", key);
            return json == null ? default : JsonSerializer.Deserialize<T>(json);
        }

        // Remove something from local storage by its key
        public async Task RemoveFromStorage(string key)
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }
}
