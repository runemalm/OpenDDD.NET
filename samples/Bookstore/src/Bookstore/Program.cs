using OpenDDD.API.Extensions;
using Bookstore.Domain.Model.Ports;
using Bookstore.Infrastructure.Adapters.Console;
using Bookstore.Infrastructure.Persistence.EfCore;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add OpenDDD services
// builder.Services.AddOpenDDD<BookstoreDbContext>(builder.Configuration, 
builder.Services.AddOpenDDD(builder.Configuration,
    options =>  
    {  
        options.UseInMemoryDatabase()
               .UseInMemoryMessaging()
               .SetEventListenerGroup("Bookstore")
               .SetEventTopics(
                   "Bookstore.Domain.{EventName}",
                   "Bookstore.Interchange.{EventName}"
                )
               .EnableAutoRegistration();
    },
    services =>
    {
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
