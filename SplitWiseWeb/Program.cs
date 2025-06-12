using Microsoft.EntityFrameworkCore;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Implementation;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Implementation;
using SplitWiseService.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Configure Context Accessor
builder.Services.AddHttpContextAccessor();

// Configure Database Connection
builder.Services.AddDbContext<SplitWiseDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("SplitWiseDB")));

// Ripositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Helpers
builder.Services.AddScoped<PasswordHelper>();

// Add services to the container.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Auth/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
