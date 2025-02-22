﻿using BrainStormEra.Controllers;
using BrainStormEra.Models;
using BrainStormEra.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace BrainStormEra
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpContextAccessor();
            // Register GeminiApiService to be injected when needed
            builder.Services.AddHttpClient<GeminiApiService>();

            // Add services to the container, such as Controllers with Views
            builder.Services.AddControllersWithViews();

            // Configure DbContext with SQL Server
            builder.Services.AddDbContext<SwpMainFpContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SwpMainFpContext")));

            // Add authentication services for cookies
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login/LoginPage";  // Redirect to login page if unauthorized
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Set cookie expiration time
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true; // Secure the cookie
                    options.Cookie.IsEssential = true;  // Ensure it's essential for GDPR compliance
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Enable HTTPS redirection and static file serving
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Enable cookie authentication
            app.UseAuthentication();

            // Enable authorization middleware
            app.UseAuthorization();

            // Map the controller routes with default route settings
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
