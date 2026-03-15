using DataLibrary.ServiceLayer.FuneralService;
using DataLibrary.ServiceLayer.NoteService;
using DataLibrary.ServiceLayer.TaskService;
using DataLibrary.ServiceLayer.UserService;
using NextSteps.Client;
using NextSteps.Components;

namespace NextSteps
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddBlazorBootstrap();

            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
            builder.Services.AddScoped<BrowserStorageService>();
            builder.Services.AddScoped<IUserService, UserService>();// add User API services
            builder.Services.AddScoped<IFuneralService, FuneralService>();// add Funeral API services
            builder.Services.AddScoped<ITaskService, TaskService>();// add Task API services
            builder.Services.AddScoped<INoteService, NoteService>();// add Note API services

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
