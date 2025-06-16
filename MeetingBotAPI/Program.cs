
//using MeetingBotAPI.Interface;
//using MeetingBotAPI.Models;
//using MeetingBotAPI.Service;
//using MeetingBotAPI.Services;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.Bot.Builder;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.OpenApi.Models;
//var builder = WebApplication.CreateBuilder(args);
//// Allow CORS from Angular localhost
////builder.Services.AddCors(options =>
////{
////    options.AddPolicy("AllowAngularApp",
////        policy => policy.WithOrigins("http://localhost:4200")
////                        .AllowAnyMethod()
////                        .AllowAnyHeader());
////});
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()  // Allow any origin
//              .AllowAnyMethod()  // Allow any HTTP method (GET, POST, etc.)
//              .AllowAnyHeader(); // Allow any header
//    });
//});
//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//builder.Services.AddControllers();
//builder.Services.AddSingleton<IBot, MeetingBot>();
//builder.Services.AddScoped<ActionService>();
//builder.Services.AddScoped<GraphService>();
//builder.Services.AddScoped<IEmployee, Employee>();

//// Register ApplicationDbContext with your connection string
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// Add Authentication with default scheme
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//})
//.AddCookie()
//.AddOpenIdConnect(options =>
//{
//  options.ClientId = "d0f89e85-54da-4e92-8f6a-80e18a9c6b1e";
//  options.Authority = "https://login.microsoftonline.com/3e6f87ce-fda6-48d7-a98e-5fbcd9318977/v2.0";
//  options.ResponseType = "code";
//  options.SaveTokens = true;
//  options.ClientSecret = "ZHn8Q~Anu9DDTSwFcG-yk5WyqHt3QJ8A-6RTNbbm"; // ❗ Replace with your actual client secret
//  options.Scope.Add("openid");
//  options.Scope.Add("profile");
//  options.Scope.Add("User.Read");
//});

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

//    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
//    {
//        Type = SecuritySchemeType.OAuth2,
//        Flows = new OpenApiOAuthFlows
//        {
//            AuthorizationCode = new OpenApiOAuthFlow
//            {
//                AuthorizationUrl = new Uri("https://login.microsoftonline.com/3e6f87ce-fda6-48d7-a98e-5fbcd9318977/oauth2/v2.0/authorize"),
//                TokenUrl = new Uri("https://login.microsoftonline.com/3e6f87ce-fda6-48d7-a98e-5fbcd9318977/oauth2/v2.0/token"),
//                Scopes = new Dictionary<string, string>
//                {
//                    { "openid", "Sign in" },
//                    { "profile", "User profile" },
//                    { "User.Read", "Read user data (Microsoft Graph)" }
//                }
//            }
//        }
//    });



//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
//            },
//            new[] { "openid", "profile", "User.Read" }
//        }
//    });
//});


//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");

//        c.OAuthClientId("d0f89e85-54da-4e92-8f6a-80e18a9c6b1e");
//        c.OAuthUsePkce(); // Required for Azure AD
//        c.OAuthScopes("openid", "profile", "User.Read");
//    });

//}


//app.UseHttpsRedirection();
//app.UseCors("AllowAll");

//app.UseAuthentication();

//app.UseAuthorization();

//app.MapControllers();
//app.Run();


using MeetingBotAPI.Interface;
using MeetingBotAPI.Models;
using MeetingBotAPI.Service;
using MeetingBotAPI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// CORS Configuration for Development
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()  // Allow any origin
//              .AllowAnyMethod()  // Allow any HTTP method (GET, POST, etc.)
//              .AllowAnyHeader(); // Allow any header
//    });
//});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // 👈 must exactly match browser origin
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();

    });
});

builder.Services.AddHttpClient();

// Register services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IBot, MeetingBot>();
builder.Services.AddScoped<ActionService>();
builder.Services.AddScoped<GraphService>();
builder.Services.AddScoped<IEmployee, Employee>();

// Database context setup
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication configuration
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//})
//.AddCookie()
//.AddOpenIdConnect(options =>
//{
//    options.ClientId = "d0f89e85-54da-4e92-8f6a-80e18a9c6b1e";
//    options.Authority = "https://login.microsoftonline.com/3e6f87ce-fda6-48d7-a98e-5fbcd9318977/v2.0";
//    options.ResponseType = "code";
//    options.SaveTokens = true;
//    options.ClientSecret = Environment.GetEnvironmentVariable("ZHn8Q~Anu9DDTSwFcG-yk5WyqHt3QJ8A-6RTNbbm"); // Store secrets securely
//    options.Scope.Add("openid");
//    options.Scope.Add("profile");
//    options.Scope.Add("User.Read");
//});
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//})
//.AddCookie()
//.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
//{
//    options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
//    options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
//    options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["Authentication:Microsoft:TenantId"]}/v2.0";
//    options.ResponseType = "code";
//    options.SaveTokens = true;
//    options.Scope.Add("email");
//    options.Scope.Add("openid");
//    options.CallbackPath = builder.Configuration["Authentication:Microsoft:CallbackPath"];
//});


//// Swagger configuration with OAuth2 (OpenID Connect)
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
//    c.AddServer(new OpenApiServer { Url = "https://localhost:7198" }); // Make sure scheme is correct
//    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
//    {
//        Type = SecuritySchemeType.OAuth2,
//        Flows = new OpenApiOAuthFlows
//        {
//            AuthorizationCode = new OpenApiOAuthFlow
//            {
//                AuthorizationUrl = new Uri("https://login.microsoftonline.com/3e6f87ce-fda6-48d7-a98e-5fbcd9318977/oauth2/v2.0/authorize"),
//                TokenUrl = new Uri("https://login.microsoftonline.com/3e6f87ce-fda6-48d7-a98e-5fbcd9318977/oauth2/v2.0/token"),
//                Scopes = new Dictionary<string, string>
//                {
//                    { "openid", "Sign in" },
//                    { "profile", "User profile" },
//                    { "User.Read", "Read user data (Microsoft Graph)" }
//                }
//            }
//        }
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
//            },
//            new[] { "openid", "profile", "User.Read" }
//        }
//    });
//});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Microsoft Login API", Version = "v1" });
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://login.microsoftonline.com/3e6f87ce-fda6-48d7-a98e-5fbcd9318977/oauth2/v2.0/authorize"),
                TokenUrl = new Uri("https://login.microsoftonline.com/3e6f87ce-fda6-48d7-a98e-5fbcd9318977/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "Sign in with Microsoft" },
                    { "profile", "User profile" },
                    { "User.Read.All", "Read all users' full profiles" }

                }
            }
        }
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            new[] { "openid" }
        }
    });
});

// Application pipeline setup
var app = builder.Build();

// Enable Swagger UI for development only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.OAuthClientId("d0f89e85-54da-4e92-8f6a-80e18a9c6b1e");
        c.OAuthUsePkce();  // Required for Azure AD
        //c.OAuthScopes("openid", "profile", "User.Read");
        c.OAuthScopeSeparator(" ");
        c.OAuthScopes("openid", "profile", "email");
        c.OAuth2RedirectUrl("https://localhost:7198/swagger/oauth2-redirect.html");
        c.EnableFilter();

    });
}
app.UseCors("AllowAll");

app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});


app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

