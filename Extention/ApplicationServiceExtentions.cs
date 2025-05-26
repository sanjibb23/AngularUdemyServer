using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiQuespond.DataAccess;
using WebApiQuespond.Interfaces;
using WebApiQuespond.Models;
using WebApiQuespond.Services;
using WebApiQuespond.Services.NewFolder;

namespace WebApiQuespond.Extention
{
    public static class ApplicationServiceExtentions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    builder => builder.WithOrigins("http://localhost:4200") // Update based on your Angular app URL
                                      .AllowAnyMethod()
                                      .AllowAnyHeader()
                                      .AllowCredentials()
                                      .SetIsOriginAllowedToAllowWildcardSubdomains());
            });
            string connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddSingleton(new DbHelper(connectionString));
            services.AddScoped<UserRepository>();
            services.AddScoped<UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, AppUserRepository>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<ILikesService, LikesService>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IConnectionRepository, ConnectionRepository>();
            services.Configure<CloudinarySettings>(Configuration.GetSection("cloudinarySettings"));
            services.AddSingleton<PresenceTracker>();
            return services;
        }
    }
}
