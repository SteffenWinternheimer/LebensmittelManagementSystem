using Microsoft.AspNetCore.Hosting;
using LMS.Services;
using System.Net;
using LMS.Logic;
using LMS;
using Blazored.Modal;

var builder = WebApplication.CreateBuilder();

string ipAddress = null;
var host = Dns.GetHostEntry(Dns.GetHostName());
foreach (var ip in host.AddressList)
{
    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    {
        ipAddress = ip.ToString();
    }
}
string url = "http://" + ipAddress + ":5003";
builder.WebHost.UseUrls(url);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddBlazoredModal();
var app = builder.Build();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

ProductService.InitializeLists();
Statistics.LoadStatistics();


app.Run();