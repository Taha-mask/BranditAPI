using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RMSProjectAPI.Database;
using RMSProjectAPI.Database.Entity;
using System.Text;
using RMSProjectAPI.Hubs;

namespace RMSProjectAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });




            builder.Services.AddDbContext<AppDbContext>(o =>
            {
                // Online Database
                o.UseSqlServer("Server=db17706.databaseasp.net; Database=db17706; User Id=db17706; Password=X#w24xF@T%t7 ; Encrypt=False; MultipleActiveResultSets=True;");


                // Local Database
                o.UseSqlServer("Data Source=.;Initial Catalog=DB;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");
            });

            builder.Services.AddCors();

            // QR Code Service
            builder.Services.AddSingleton<QRCodeService>();
            builder.Services.AddSignalR(options =>
            {
                // Increase timeout for better connection reliability
                options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
                options.KeepAliveInterval = TimeSpan.FromMinutes(1);
            });


            // Add services to the container.
            builder.Services.AddIdentity<User, IdentityRole<Guid>>()
                     .AddEntityFrameworkStores<AppDbContext>()
                     .AddDefaultTokenProviders();

            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Don't require HTTPS for development
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    ClockSkew = TimeSpan.Zero
                };
                
                // Configure JWT authentication for SignalR
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        
                        // If no token in query string, check Authorization header
                        if (string.IsNullOrEmpty(accessToken))
                        {
                            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                            {
                                accessToken = authHeader.Substring("Bearer ".Length);
                            }
                        }
                        
                        var path = context.HttpContext.Request.Path;
                        
                        // Make sure we extract the token for SignalR connections
                        if (!string.IsNullOrEmpty(accessToken) && 
                            (path.StartsWithSegments("/chatHub") || path.StartsWithSegments("/orderHub")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        // Add response headers to avoid CORS issues
                        context.NoResult();
                        context.Response.StatusCode = 401;
                        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        // Skip the default challenge behavior for SignalR endpoints
                        var path = context.Request.Path;
                        if (path.StartsWithSegments("/chatHub") || path.StartsWithSegments("/orderHub"))
                        {
                            context.HandleResponse();
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            builder.Services.AddAuthorization();
            //builder.Services.AddCors(options =>
            //{
            //    options.AddDefaultPolicy(i =>
            //    {
            //        i.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            //    });
            //});

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true) // Allow any origin for development
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .WithExposedHeaders("Content-Disposition");
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseSwagger();
            app.UseSwaggerUI();

            // Disable automatic HTTPS redirection for development to avoid CORS issues
            // app.UseHttpsRedirection();

            // Uploading Images
            app.UseStaticFiles();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors();

            app.MapHub<ChatHub>("/chatHub");
            app.MapHub<OrderHub>("/orderHub");

            app.MapControllers();

            app.Run();


        }
    }

    public enum UserType
    {
        Customer = 0,
        Marketer = 1
    }
}