using Bookstore.Domain.Model.Ports;
using OpenDDD.Main.Extensions;
using Bookstore.Infrastructure.Persistence.EfCore;
using Bookstore.Infrastructure.Service.EmailSender.Fake;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add OpenDDD services
builder.Services.AddOpenDDD<BookstoreDbContext>(builder.Configuration);

builder.Services.AddTransient<IEmailSender, FakeEmailSender>();

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
