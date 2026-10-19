using System.Globalization;
using GLMS_Web.ApiServices;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Allows services to access Session through HttpContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

// JWT handler attaches token to outgoing API requests
builder.Services.AddTransient<JwtTokenHandler>();

// API service layer for frontend
builder.Services.AddHttpClient<ContractApiService>()
    .AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<ClientApiService>()
    .AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<ServiceRequestApiService>()
    .AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<AuthApiService>();

var cultureInfo = new CultureInfo("en-ZA");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(cultureInfo);
    options.SupportedCultures = new[] { cultureInfo };
    options.SupportedUICultures = new[] { cultureInfo };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

var app = builder.Build();
app.UseRequestLocalization();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
