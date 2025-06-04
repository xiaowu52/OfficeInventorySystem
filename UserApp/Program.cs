using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using UserApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Redis配置
var redisConnectionString = "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConnectionString));

// 添加Redis分布式缓存
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "UserApp_";
});

// 添加Redis服务
builder.Services.AddSingleton<RedisService>();
builder.Services.AddHostedService<RedisPubSubService>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

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


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");
    endpoints.MapControllers(); // 添加这行支持属性路由
});

app.Run();

app.MapGet("/debug/routes", context => {
    var endpoints = context.RequestServices
        .GetRequiredService<IEnumerable<EndpointDataSource>>()
        .SelectMany(source => source.Endpoints);
    return context.Response.WriteAsync(string.Join("\n", endpoints));
});