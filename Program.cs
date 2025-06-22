using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Career_Tracker_Backend;
using Career_Tracker_Backend.Services.UserServices;

using Career_Tracker_Backend.Services.JobService;
using Microsoft.Extensions.FileProviders;
using System.Text.Json.Serialization;
using Career_Tracker_Backend.Services.FormationService;
using Career_Tracker_Backend.Services.CourseService;

using Career_Tracker_Backend.Services.QuizService;
using Career_Tracker_Backend.Services.InscriptionService;
using Career_Tracker_Backend.Services.CertificateService;

using Career_Tracker_Backend.Services;
using System.Net.Http.Headers;
using Career_Tracker_Backend.Services.Interfaces;
using Resend;




var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("SecureFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configure Moodle settings
builder.Services.Configure<MoodleSettings>(builder.Configuration.GetSection("Moodle"));

// Configure JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) || Encoding.UTF8.GetBytes(jwtKey).Length < 32)
{
    throw new ArgumentException("JWT key must be at least 256 bits (32 bytes).");
}

var key = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpClient<IMoodleService, MoodleService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IFormationService, FormationService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IInscriptionService, InscriptionService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddScoped<IEmailService, EmailService>();
//builder.Services.AddScoped<Career_Tracker_Backend.IFeedbackService, Career_Tracker_Backend.FeedbackService>();
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = Environment.GetEnvironmentVariable("RESEND_APITOKEN") ?? "re_jg6TAGsk_FGsUNnENDDkFHHUjmFiwA7oX"; // Fallback to hardcoded key
});
builder.Services.AddTransient<IResend, ResendClient>();

// In Program.cs, update your HttpClient configuration
builder.Services.AddHttpClient<ICvService, CvService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CvProcessing:BaseUrl"] ?? "http://localhost:8000");
    client.Timeout = TimeSpan.FromSeconds(30);
});
//builder.Services.AddScoped< RecommendationService>();
// In Program.cs

// In Program.cs (or Startup.cs for .NET 5)
// Add this to your service configuration
// Add this to your service configuration
builder.Services.AddHttpClient<IRecommendationService, RecommendationService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8000"); // No trailing slash
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
// Add Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddScoped<QuizController>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Handle circular references
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Ignore null values
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply CORS before authentication
app.UseCors("AllowAllOrigins");
app.UseCors("SecureFrontend");

// Global error handling
app.UseExceptionHandler("/error");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();