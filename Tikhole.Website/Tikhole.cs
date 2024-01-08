using Tikhole.Website.Components;

namespace Tikhole.Website
{
    public class Tikhole
    {
        public static WebApplication? WebApplication;
        public static Engine.Tikhole Engine = new();
        public static void Main(string[] args)
        {
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
            Engine.Dispose();   
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive | GCCollectionMode.Forced, true);
        }
        public static void RestartTikhole()
        {
            StopTikhole();
            Engine = new Engine.Tikhole();
        }
    }
}