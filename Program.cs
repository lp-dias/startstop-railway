using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StartStop.Data;
using StartStop.Services; // EmailService
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();

// 🔹 Conexão MySQL (Railway ou local)
var host = Environment.GetEnvironmentVariable("MYSQLHOST") ?? "localhost";
var portDb = Environment.GetEnvironmentVariable("MYSQLPORT") ?? "3306";
var user = Environment.GetEnvironmentVariable("MYSQLUSER") ?? "root";
var password = Environment.GetEnvironmentVariable("MYSQLPASSWORD") ?? "alemanci"; // senha local
var database = Environment.GetEnvironmentVariable("MYSQLDATABASE") ?? "startstop";

var connectionString = $"Server={host};Port={portDb};Database={database};Uid={user};Pwd={password};";

builder.Services.AddDbContext<StartStopContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Autenticação por cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/AcessoNegado";
    });

builder.Services.AddAuthorization();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
});

// Serviço de e-mail
builder.Services.AddSingleton<EmailService>(sp =>
{
    var config = builder.Configuration.GetSection("SmtpSettings");
    return new EmailService(
        config["Server"],
        int.Parse(config["Port"]),
        config["Username"],
        config["Password"],
        config["SenderEmail"],
        config["SenderName"]
    );
});

var app = builder.Build();

// 🔹 Aplica migrations automaticamente
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StartStopContext>();
    db.Database.Migrate();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Tratamento de erro 403
app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode == 403)
    {
        response.Redirect("/Login/AcessoNegado");
    }
    return Task.CompletedTask;
});

// 🔹 Rota inicial
app.MapGet("/", () => "API StartStop rodando no Railway!");

// Rotas padrão MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 🔹 Ajuste da porta do Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

app.Run();



