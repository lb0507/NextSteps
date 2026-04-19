using Azure.Identity;
using DataLibrary.ServiceLayer.BlobService;
using DataLibrary.ServiceLayer.ContactService;
using DataLibrary.ServiceLayer.EventService;
using DataLibrary.ServiceLayer.FuneralService;
using DataLibrary.ServiceLayer.MapService;
using DataLibrary.ServiceLayer.NoteService;
using DataLibrary.ServiceLayer.TaskService;
using DataLibrary.ServiceLayer.UserService;
using NextSteps.Client;
using NextSteps.Components;
using Syncfusion.Blazor;

namespace NextSteps
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add the key vault
            builder.Configuration.AddAzureKeyVault(
                new Uri(builder.Configuration["KeyVault:VaultUri"]),
                new DefaultAzureCredential()
            );

            // Add services to the container
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddBlazorBootstrap();

            // Get the Syncfusion license key from Azure Key Vault and register it
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["SyncfusionLicense"]);
            builder.Services.AddSyncfusionBlazor();

            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
            builder.Services.AddScoped<BrowserStorageService>();
            builder.Services.AddScoped<IUserService, UserService>();// add User API services
            builder.Services.AddScoped<IFuneralService, FuneralService>();// add Funeral API services
            builder.Services.AddScoped<ITaskService, TaskService>();// add Task API services
            builder.Services.AddScoped<INoteService, NoteService>();// add Note API services
            builder.Services.AddScoped<IContactService, ContactService>();// add Contact API services
            builder.Services.AddScoped<IEventService, EventService>();// add Event API services
            builder.Services.AddScoped<IMapService, MapService>();// add Map API services
            builder.Services.AddScoped<IBlobService, BlobService>();// add Blob API services

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }
    }
}
