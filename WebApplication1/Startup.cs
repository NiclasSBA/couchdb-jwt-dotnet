using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        //Article on JWT vulnerabilities when messing up implementation of algoritms
        //https://auth0.com/blog/critical-vulnerabilities-in-json-web-token-libraries/

        //Article on Hmac vs rsa
        //https://security.stackexchange.com/questions/220185/jwt-choosing-between-hmac-and-rsa
        
        
        //GOTCHA: couchdb servíce on host machine needs to be restarted, even if you reload config through the config api



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Prerequisite
            //https://docs.couchdb.org/en/latest/setup/single-node.html
            //If you want to set a cluster up instead: https://docs.couchdb.org/en/latest/setup/cluster.html


            //1. Create Authentication
            //2. When authenticated, fetch Database(couchdb)connectionstring
            //3. Users can be in a seperate db, and that db can hold all users for all tenants
            //3.1 Having the api registered on multiple Uri's can also solve this, as we can get the host. value which would be the uri of the host
            //3.1 EXAMPLE:  var host = accessor.HttpContext.Request.Host.Value;

            //When calling the api, how should we protect that against DDOS attacks? as it serves as the one point for all customers
            //We really want a high degree of tenant isolation, as no one from a different tenant should be able to see data unrelated to them.
            //Create token authentcation, jwt tokens
            //https://docs.couchdb.org/en/latest/api/server/authn.html
            //Should contain values for accessing couchdb installations
            //services.AddIdentity<ApplicationUser, IdentityRole>()
            //    .AddEntityFrameworkStores<MulitTenantContext>()
            //    .AddDefaultTokenProviders();
            //Adding admins can be done through http api
            //https://docs.couchdb.org/en/stable/intro/security.html
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication1", Version = "v1" });
            });
            //services.AddIdentity<IdentityUser, IdentityRole>();
            //multitenancy 
            //https://www.codingame.com/playgrounds/5518/multi-tenant-asp-net-core-5---implementing-database-per-tenant-strategy

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication1 v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
