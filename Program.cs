using ChatAppServer.Database;
using ChatAppServer.Hubs;
using ChatAppServer.Interfaces;
using ChatAppServer.Models;
using ChatAppServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetSection("MongoConnection")["ConnectionStrings"];
var databaseName = builder.Configuration.GetSection("MongoConnection")["DatabaseName"];
var jwtKey = builder.Configuration.GetSection("Secrets")["Jwt"];

builder.Services.AddDbContext<DataContext>(options =>
    options.UseMongoDB(connectionString!, databaseName!));
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    builder
    .WithOrigins("http://localhost:19006", "http://localhost:8081")
    //.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    );
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatApp API v1", Version = "v1" });
    options.AddSignalRSwaggerGen();
});
builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters.ValidateIssuerSigningKey = true;
            options.TokenValidationParameters.IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
            options.TokenValidationParameters.ValidateIssuer = false;
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.ValidateLifetime = true;
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/chathub"))
                    {
                        // Read the token out of the query string
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddScoped<IEntity<User>, EntityService<User>>();
builder.Services.AddScoped<IEntity<Group>, EntityService<Group>>();
builder.Services.AddScoped<IEntity<Message>, EntityService<Message>>();
builder.Services.AddScoped<IEntity<GroupUser>, EntityService<GroupUser>>();
builder.Services.AddScoped<IEntity<UserFriend>, EntityService<UserFriend>>();
builder.Services.AddSingleton<IAuthentication, AuthenticationService>();
builder.Services.AddScoped<IFileHandler, FileHandlerService>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseRouting();

app.UseCors();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");


app.Run();
