using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

builder.WebHost.ConfigureKestrel(options =>
{
    if (isDocker)
    {
        // Only bind HTTP inside Docker
        options.ListenAnyIP(8080);
    }
    else
    {
        // Bind both HTTP and HTTPS for local dev
        options.ListenAnyIP(8080);
        options.ListenAnyIP(8081, listenOptions =>
        {
            listenOptions.UseHttps(); // This uses the dev certificate
        });
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductContext>();
    context.Database.Migrate();

    //if (!context.Products.Any())
    //{
    //    context.Products.AddRange(
    //        new Product { Name = "Coca-Cola", Description="Coco-cola desc", Price = 10 },
    //        new Product { Name = "Fanta", Description="Fanta desc", Price = 10 },
    //        new Product { Name = "Bebsi", Description="Bebsi desc", Price = 5 }
    //        );

    //    context.SaveChanges();
    //}

}

app.MapControllers();

app.Run();
