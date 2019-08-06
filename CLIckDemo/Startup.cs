using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLickDemo.DAL;
using CLickDemo.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace CLickDemo
{
    public class Startup
    {
        private IConfiguration _Config;
        public Startup(IConfiguration Configuration)
        {
            _Config = Configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
          
            services.AddMvc();

            
            //services.AddEntityFrameworkSqlServer().AddDbContext<CollegeContext>(options => options.UseSqlServer(_Config.GetConnectionString("CollegeDatabase")));
            services.AddEntityFrameworkSqlServer().AddDbContext<UserDbContext>(Options => Options.UseSqlServer(_Config.GetConnectionString("SecurityDatabase")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;

            }).AddEntityFrameworkStores<UserDbContext>().AddDefaultTokenProviders();
           
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            //app.UseSession();
            //app.UseStaticFiles();
            //app.UseCookiePolicy();
            //app.UseSession();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
            
        }
    }
}
