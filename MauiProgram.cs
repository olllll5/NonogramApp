using Microsoft.Extensions.Logging;
using NonogramApp.Services;

namespace NonogramApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // --- НАСТРОЙКА SUPABASE ---
        var url = "https://xphcflocpgrxthgkumxz.supabase.co";
        var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InhwaGNmbG9jcGdyeHRoZ2t1bXh6Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzA2MzEzODMsImV4cCI6MjA4NjIwNzM4M30.WRbZUycgSDBEIGjNpnoJk2CRGiAIzYXFyrDAnGhNxOs";

        builder.Services.AddSingleton(new Supabase.Client(url, key, new Supabase.SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true
        }));

        builder.Services.AddSingleton<DatabaseService>();

        // Регистрация страниц
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LevelsPage>();
        builder.Services.AddTransient<GamePage>();
        builder.Services.AddTransient<ProfilePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
