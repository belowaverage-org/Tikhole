using Tikhole.Website.Components;

namespace Tikhole.Website
{
    public class Tikhole
    {
        public static WebApplication? WebApplication;
        public static void Main(string[] args)
        {
            _ = Task.Run(() => new Engine.Tikhole());
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            WebApplication = builder.Build();
            if (!WebApplication.Environment.IsDevelopment()) WebApplication.UseExceptionHandler("/Error");
            WebApplication.UseStaticFiles();
            WebApplication.UseAntiforgery();
            WebApplication.MapRazorComponents<App>().AddInteractiveServerRenderMode();
            WebApplication.Run();
        }
        public static void StopTikhole()
        {
            Engine.Tikhole.Listener?.Client.Close();
        }
        public static void RestartTikhole()
        {
            Engine.Tikhole.Listener?.Client.Close();
            _ = Task.Run(() => new Engine.Tikhole());
        }
    }
}