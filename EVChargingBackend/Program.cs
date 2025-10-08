using EVChargingBackend.Mappings;
using EVChargingBackend.Models;
using EVChargingBackend.Services;  // For IUserService and UserService
using Microsoft.AspNetCore.Authentication.JwtBearer;  // For JwtBearerDefaults
using Microsoft.Extensions.Configuration;  // For IConfiguration
using Microsoft.IdentityModel.Tokens;  // For JwtBearerDefaults, TokenValidationParameters
using MongoDB.Bson.Serialization;
using MongoDB.Driver;  // For MongoDB-related functionality
using System.IdentityModel.Tokens.Jwt;
using System.Text;  // For encoding the SecretKey

var builder = WebApplication.CreateBuilder(args);

// Configure BSON class maps for MongoDB
if (!BsonClassMap.IsClassMapRegistered(typeof(GeoLocation)))
{
    BsonClassMap.RegisterClassMap<GeoLocation>(cm =>
    {
        cm.AutoMap();
        cm.MapMember(c => c.Latitude).SetElementName("lat");
        cm.MapMember(c => c.Longitude).SetElementName("lon");
    });
}

if (!BsonClassMap.IsClassMapRegistered(typeof(ChargingStation)))
{
    BsonClassMap.RegisterClassMap<ChargingStation>(cm =>
    {
        cm.AutoMap();
        cm.SetIgnoreExtraElements(true); // This will ignore any extra fields in the database that don't exist in the model
    });
}

// Configure MongoDB connection
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = new MongoClient(builder.Configuration.GetConnectionString("MongoDb"));
    return client.GetDatabase("EVChargingDB");  // Connect to your EVChargingDB
});

// Register services for User Management and JWT Authentication
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IEVOwnerService, EVOwnerService>();
builder.Services.AddScoped<IChargingStationService, ChargingStationService>();
builder.Services.AddScoped<IChargingSlotService, ChargingSlotService>();


JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = "http://localhost:5033",//  backend API runs on port 5000
            ValidAudience = "http://localhost:3000",// Assuming you plan to have your frontend running on port 3000 (common for React apps)
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),

        };
    });

// Define the CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowFrontendOrigin", // Give your policy a name
                      policy =>
                      {
                          // Allow requests from your React development server
                          policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
                          policy.WithOrigins("https://ev-charging-booking-system-booking.vercel.app").AllowAnyHeader().AllowAnyMethod();
                      });
});

builder.Services.AddControllers();
//AutoMapper
builder.Services.AddAutoMapper(typeof(EVChargingBackend.Mappings.AutoMapping));

var app = builder.Build();

// Apply the CORS Policy
app.UseCors("AllowFrontendOrigin");

app.UseAuthentication();
app.UseAuthorization();

//app.UseDeveloperExceptionPage();
app.MapControllers();

app.Run();
