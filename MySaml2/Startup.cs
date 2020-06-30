using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Recognizers.Text;
using Sustainsys.Saml2;
using Sustainsys.Saml2.AspNetCore2;
using Sustainsys.Saml2.Metadata;

namespace MySaml2
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
            services.AddControllersWithViews(options => {
                options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            });

            services.AddAuthentication(options => 
                {
                    options.DefaultScheme = "Application";
                    options.DefaultSignInScheme = "Application";
                    options.DefaultChallengeScheme = Saml2Defaults.Scheme;
                })
                .AddCookie("Application", options =>
                {
                    options.Cookie.Name = "AppCookie";
                    //options.LoginPath = "/Account/Login";//This should login page path
                    //options.ExpireTimeSpan = TimeSpan.FromDays(1);
                    //options.SlidingExpiration = false;
                })
                .AddSaml2(options =>
                {
                    options.SPOptions.EntityId = new EntityId("https://localhost:44354/Saml2");
                    //options.SPOptions.ReturnUrl = new Uri($"https://localhost:44354/Account/Callback?returnUrl=%2F");
                    options.IdentityProviders.Add(
                        new IdentityProvider(
                            new EntityId("https://localhost:44300/Metadata"), options.SPOptions)
                        {
                            LoadMetadata = true,
                            AllowUnsolicitedAuthnResponse = true
                        });

                    options.SPOptions.ServiceCertificates.Add(new X509Certificate2("Sustainsys.Saml2.Tests.pfx"));
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
