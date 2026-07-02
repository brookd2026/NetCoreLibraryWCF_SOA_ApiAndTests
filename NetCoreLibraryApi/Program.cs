using BookLibrary_WCFService;
using BookLibrary_WCFService.Models;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LibraryConnection")));

// 1. Register CoreWCF services
builder.Services.AddServiceModelServices();

// 2. REQUIRED: Register metadata services
builder.Services.AddServiceModelMetadata();

// 3. Register your actual service implementation as a singleton or transient
builder.Services.AddTransient<LibraryService>();

var app = builder.Build();

// 4. REQUIRED: Configure the metadata behavior to allow HTTP/HTTPS GET requests
var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
serviceMetadataBehavior.HttpGetEnabled = true;
// serviceMetadataBehavior.HttpsGetEnabled = true; // Uncomment if you use HTTPS

app.UseServiceModel(serviceBuilder =>
{
    // 5. Add your service and map it to an endpoint
    serviceBuilder.AddService<LibraryService>();

    serviceBuilder.AddServiceEndpoint<LibraryService, ILibraryService>(
        new CoreWCF.BasicHttpBinding(),
        "/Services/LibraryService.svc"
    );
});

app.Run();
