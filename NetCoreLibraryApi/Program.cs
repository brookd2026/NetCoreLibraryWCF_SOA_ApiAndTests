using BookLibrary_WCFService;
using BookLibrary_WCFService.Models;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add this line to make HttpContext injectible:
builder.Services.AddHttpContextAccessor();

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

    // Configure a BasicHttpBinding optimized for large streaming data
    var streamingBinding = new CoreWCF.BasicHttpBinding
    {
        // StreamedResponse allows streaming from Server -> Client chunk-by-chunk
        TransferMode = CoreWCF.TransferMode.StreamedResponse,

        // Increase the maximum size allowed to pass (e.g., 500 MB)
        MaxReceivedMessageSize = 400000000,

        // Give large files adequate time to transfer over slower network speeds
        SendTimeout = System.TimeSpan.FromMinutes(15),
        ReceiveTimeout = System.TimeSpan.FromMinutes(15)
    };

    serviceBuilder.AddServiceEndpoint<LibraryService, ILibraryService>(
        new CoreWCF.BasicHttpBinding(), //streamBinding
        "/Services/LibraryService.svc"
    );
});

app.Run();
