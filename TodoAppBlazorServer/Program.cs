using Microsoft.AspNetCore.Hosting;
using LMS.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

string ipAddress = null;
var host = Dns.GetHostEntry(Dns.GetHostName());
foreach (var ip in host.AddressList)
{
    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    {
        ipAddress = ip.ToString();
    }
}
builder.WebHost.UseUrls("http://"+ipAddress+":5003", "http://localhost:5001");

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<ITodoService, TodoService>();

var app = builder.Build();

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

app.Run();