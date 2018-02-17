using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OddsScrapper.Mvc.Controllers;
using OddsScrapper.Repository.Repository;
using OddsScrapper.Shared.Repository;
using OddsScrapper.WebsiteScraping.Helpers;

namespace OddsScrapper.Mvc
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(config =>
            {
                if (_env.IsProduction())
                    config.Filters.Add(new RequireHttpsAttribute());
            });

            services.AddEntityFrameworkSqlite().AddDbContext<ArchiveContext>();
            services.AddScoped<IArchiveDataRepository, ArchiveDataRepository>();
            services.AddTransient<ArchiveContextSeedData>();
            services.AddSingleton<IHtmlContentReader, HtmlContentReader>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ArchiveContextSeedData archiveContextSeed)
        {
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "Default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = GamesController.ControllerName, action = nameof(GamesController.Index) });

            });
        }
    }
}
