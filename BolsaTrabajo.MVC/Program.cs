using BolsaTrabajo.MVC.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Registro de servicios en el contenedor de dependencias
builder.Services.AddControllersWithViews();

// ---> CLIENTE HTTP PARA CONSUMIR LA API (el token se agrega con TokenHandler)
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenHandler>();
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
}).AddHttpMessageHandler<TokenHandler>();

// ---> AUTENTICACION CON COOKIE: guarda los datos del usuario y el token obtenido de Identity
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);   // Misma duración que el token de la API
        options.SlidingExpiration = false;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configuración del pipeline de peticiones HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS por defecto dura 30 días; ajustar para producción (https://aka.ms/aspnetcore-hsts)
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
