using AspNetCoreElasticsearchAuditTrail;

var builder = WebApplication.CreateBuilder(args);

var indexPerMonth = false;
var amountOfPreviousIndicesUsedInAlias = 3;

builder.Services.AddAuditTrail<CustomAuditTrailLog>(options =>
    options.UseSettings(indexPerMonth,
        amountOfPreviousIndicesUsedInAlias,
        builder.Configuration["ElasticsearchUserName"],
        builder.Configuration["ElasearchPassword"],
        builder.Configuration["ElasearchUrl"])
);

builder.Services.AddControllersWithViews();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
