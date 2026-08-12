using Microsoft.OpenApi.Models;
using VacationManager.Services;
using Microsoft.AspNetCore.Server.IISIntegration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        Description = "Enter 'Bearer' following by space and the token.",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5501") // Live Server port
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var useNegotiate =
    builder.Configuration.GetValue<bool>("Authorization:UseNegotiate");

if(useNegotiate)
{
    builder.Services.AddAuthentication("Negotiate").AddNegotiate();
}
else
{
    builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
}

builder.Services.AddAuthorization();

builder.Services.AddSignalR();

builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<DbService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<InfoService>();
builder.Services.AddScoped<Utilities>();
builder.Services.AddScoped<VacationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationsHub>("/notifications");

app.Run();
