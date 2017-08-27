using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OddsWebsite.Models;
using OddsWebsite.Services;

namespace OddsWebsite
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(config =>
            {
                if(_env.IsProduction())    
                    config.Filters.Add(new RequireHttpsAttribute());
            });

            services.AddIdentity<OddsAppUser, IdentityRole>(config =>
            {
                config.User.RequireUniqueEmail = true;
                
            })
            .AddEntityFrameworkStores<ArchiveContext>();

            services.AddAuthentication("MyCookieAuthenticationScheme")
                    .AddCookie(options =>
                    {
                        options.AccessDeniedPath = "/Account/Forbidden/";
                        options.LoginPath = "/Account/Unauthorized/";
                    });

            services.AddEntityFrameworkSqlite().AddDbContext<ArchiveContext>();
            services.AddScoped<IArchiveDataRepository, ArchiveDataRepository>();
            services.AddTransient<ArchiveContextSeedData>();

            services.AddTransient<IEmailService, EmailService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ArchiveContextSeedData archiveContextSeed)
        {            
            app.UseStaticFiles();

            AuthAppBuilderExtensions.UseAuthentication(app);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "Default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
                
            });

            archiveContextSeed.EnsureDataSeed().Wait();
        }
    }
}
