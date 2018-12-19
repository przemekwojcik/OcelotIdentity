using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;

namespace Identity
{
    public class Startup
    {

        public IHostingEnvironment Environment { get; }

        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
            IdentityModelEventSource.ShowPII = true;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                          b => b.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
            });
            var issuerUri = Configuration.GetSection("ConnectionStrings")?.GetSection("IssuerUri")?.Value;
            services.AddIdentityServer(options =>
                    {
                        options.Events.RaiseErrorEvents = true;
                        options.Events.RaiseInformationEvents = true;
                        options.Events.RaiseFailureEvents = true;
                        options.Events.RaiseSuccessEvents = true;
                        options.IssuerUri = "https://identity-test.northeurope.cloudapp.azure.com/";
                        options.PublicOrigin = Environment.IsDevelopment() ? "" : "https://identity-test.northeurope.cloudapp.azure.com/";
                    })
                    .AddDeveloperSigningCredential()
                    .AddInMemoryApiResources(InMemoryConfiguration.ApiResources())
                    .AddInMemoryClients(InMemoryConfiguration.Clients())
                    .AddTestUsers(InMemoryConfiguration.Users().ToList())
                    .AddJwtBearerClientAuthentication();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            if (!Environment.IsDevelopment())
            {
                app.UseForwardedHeaders();
            }

            app.UseCors("CorsPolicy");
            app.UseStaticFiles();

            var fordwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            fordwardedHeaderOptions.KnownNetworks.Clear();
            fordwardedHeaderOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(fordwardedHeaderOptions);

            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
