using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StartStop.Data;
using StartStop.Services; // <- adiciona o namespace do EmailService
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();

// Banco de dados
builder.Services.AddDbContext<StartStopContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Serviço de e-mail (injeção de dependência)
builder.Services.AddSingleton<EmailService>(sp =>
{
    var config = builder.Configuration.GetSection("SmtpSettings");
    return new EmailService(
        config["Server"],              // servidor SMTP
        int.Parse(config["Port"]),     // porta
        config["Username"],            // usuário
        config["Password"],            // senha
        config["SenderEmail"],         // e-mail remetente
        config["SenderName"]           // nome do remetente
    );
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Tratamento de erro 403 → redireciona para página de acesso negado
app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode == 403)
    {
        response.Redirect("/Login/AcessoNegado");
    }
    return Task.CompletedTask;
});

// Rotas padrão
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

   



app.Run();

