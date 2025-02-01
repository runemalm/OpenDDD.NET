using Bookstore.Domain.Model.Ports;
using Bookstore.Infrastructure.Adapters.Console;
using Bookstore.Infrastructure.Persistence.EfCore;
using OpenDDD.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add OpenDDD services
builder.Services.AddOpenDDD<BookstoreDbContext>(builder.Configuration, 
    options =>  
    {  
        options.UseEfCore()
               .UseSQLite("DataSource=Infrastructure/Persistence/EfCore/Bookstore.db;Cache=Shared")
               .UseInMemoryMessaging()
               .SetEventListenerGroup("Bookstore")
               .SetEventTopicTemplates(
                   "Bookstore.Domain.{EventName}",
                   "Bookstore.Interchange.{EventName}"
                )
               .EnableAutoRegistration();
    },
    services =>
    {
        // Add port adapters
        services.AddTransient<IEmailPort, ConsoleEmailAdapter>();
    }
);

// Add Controller Services
builder.Services.AddControllers();

// Build the application
var app = builder.Build();

// Use OpenDDD Middleware
app.UseOpenDDD();

// Use Swagger Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// Use HTTP->HTTPS Redirection Middleware
app.UseHttpsRedirection();

// Use CORS Middleware
app.UseCors("AllowAll");

// Map Controller Actions to Endpoints
app.MapControllers();

// Run the application
app.Run();
