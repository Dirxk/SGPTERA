using Microsoft.AspNetCore.Authentication.Negotiate;
using TeracromDatabase;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de servicios
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(540);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation(); // Solo para desarrollo

// Configuraci�n de DapperContext
builder.Services.AddScoped<DapperContext>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new DapperContext(configuration);
});

// Configuraci�n de autenticaci�n y autorizaci�n
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

// Configuraci�n del pipeline de middlewares
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
