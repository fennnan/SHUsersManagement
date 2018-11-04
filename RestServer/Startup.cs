using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Formatters;

using RestServer.Interfaces;
using RestServer.Services;

namespace RestServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMvc(config =>
            {
                // Add XML Content Negotiation
                config.RespectBrowserAcceptHeader = true;
                config.InputFormatters.Add(new XmlSerializerInputFormatter());
                config.OutputFormatters.Add(new XmlSerializerOutputFormatter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<IUserRepository,ListUserRepository>();// Scoped
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                var repository = app.ApplicationServices.GetService<IUserRepository>();
                InitializeDatabase(repository);
            }
            else
            {
                app.UseHsts();
            }

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            //app.UseHttpsRedirection();
            app.UseMvc();
        }

        private void InitializeDatabase(IUserRepository repository)
        {
            var userList = repository.GetAll();
            if (!userList.Any())
            {
                repository.Add(new RestServer.Model.User {UserName = "Admin", Password="admin1", Roles=new string[] {"ADMIN"}});
                repository.Add(new RestServer.Model.User {UserName = "User1", Password="pass1", Roles=new string[] {"PAGE_1"}});
                repository.Add(new RestServer.Model.User {UserName = "User2", Password="pass2", Roles=new string[] {"PAGE_2"}});
                repository.Add(new RestServer.Model.User {UserName = "User3", Password="pass3", Roles=new string[] {"PAGE_3"}});
            }
        }

    }
}
