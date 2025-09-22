using NoteApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Bind DatabaseSettings from appsettings.json or environment variables
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<NoteService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Notes}/{action=Index}/{id?}");

Console.WriteLine("✅ NoteApp running on http://0.0.0.0:5050");

app.Run("http://0.0.0.0:5050");
