using Tikhole.Website.Components;

namespace Tikhole.Website
{
    public class Tikhole
    {
        public static void Main(string[] args)
        {
            _ = Task.Run(() => Engine.Tikhole.Main());
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            var app = builder.Build();
            if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");
            app.UseStaticFiles();
            app.UseAntiforgery();
            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
            app.Run();
        }
    }
}