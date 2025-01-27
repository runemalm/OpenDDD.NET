using Bookstore.Infrastructure.Persistence.EfCore;
using OpenDDD.Main.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add OpenDDD services
builder.Services.AddOpenDDD<BookstoreDbContext>(builder.Configuration, options =>
{
    options.UseEfCore();
    options.UseSQLite("DataSource=Main/EfCore/Bookstore.db;Cache=Shared");
});

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
