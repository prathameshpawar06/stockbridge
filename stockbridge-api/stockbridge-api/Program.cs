using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using stockbridge_api.Filters;
using stockbridge_api.Helper;
using stockbridge_api.Services;
using stockbridge_DAL.domainModels;
using stockbridge_DAL.IRepositories;
using stockbridge_DAL.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddMemoryCache(); // Add this line

// Define schemes to clarify usage
const string JwtBearerScheme = JwtBearerDefaults.AuthenticationScheme;
const string OpenIdConnectScheme = OpenIdConnectDefaults.AuthenticationScheme;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectScheme; // Use OpenID Connect for interactive user challenges
})
    .AddCookie()
    .AddOpenIdConnect(OpenIdConnectScheme, options =>
    {
        options.Authority = "https://login.microsoftonline.com/3e7fd730-9835-4180-8ed8-b8594b8d6486";
        options.ClientId = "b7dfaa29-aa07-4b8d-87f0-e7bf8610347b";
        options.ResponseType = "code";
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("User.Read");
        options.CallbackPath = new PathString("/signin-oidc");
        options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
        options.SaveTokens = true;
    })
    .AddJwtBearer(JwtBearerScheme, o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, // Ensure this is true for security reasons
            ValidateIssuerSigningKey = true
        };
    });


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin() // React frontend URL
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
   {
     new OpenApiSecurityScheme
     {
       Reference = new OpenApiReference
       {
         Type = ReferenceType.SecurityScheme,
         Id = "Bearer"
       }
      },
      new string[] { }
    }
  });
});
// Configure database connection
builder.Services.AddDbContext<StockbridgeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StockbridgeDbConnection")));

// Add services to the container.
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ITemplatePrincipalRepository, TemplatePrincipalRepository>();
builder.Services.AddScoped<ICarrierRepository, CarrierRepository>();
builder.Services.AddScoped<IBrokerRepository, BrokerRepository>();
builder.Services.AddScoped<PrintPolicy>();
builder.Services.AddScoped<TokenValidationService>();
builder.Services.AddScoped<AddLogoService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers(options =>
{
    // Add the filter
    options.Filters.Add<ValidateModelStateAttribute>();
})
.AddJsonOptions(jsonOptions =>
{
    // Configure JSON options
    jsonOptions.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
