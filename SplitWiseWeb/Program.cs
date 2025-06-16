using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Implementation;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Implementation;
using SplitWiseService.Services.Interface;
using SplitWiseWeb.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Context Accessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();

// Configure Database Connection
builder.Services.AddDbContext<SplitWiseDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("SplitWiseDB")));

// Ripositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Helpers
builder.Services.AddScoped<AesHelper>();
builder.Services.AddSingleton<UrlBuilder>();

// Add services to the container.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IExceptionLogService, ExceptionLogService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddControllersWithViews();

// Configure Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1); // Set session timeout
    options.Cookie.HttpOnly = true; // Ensure session is only accessible via HTTP
    options.Cookie.IsEssential = true;
});

// JWT Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Extract token from the "JwtToken" cookie
                string token = context.Request.Cookies["JwtToken"]!;
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Auth/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
    context.Response.Headers.Add("Pragma", "no-cache");
    context.Response.Headers.Add("Expires", "0");

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

// app.UseStatusCodePagesWithReExecute("/Auth/Error/{0}");

app.UseAuthentication();
app.UseAuthorization();

// Global exception handler
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
