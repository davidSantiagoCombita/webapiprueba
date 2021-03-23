using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

//usamos el contexto y en entityFreameworkCore para la base de datos
using webApiPruebaInformesç.Models;

using Microsoft.Extensions.Options;
using webApiPruebaInformes.Services;
using webApiPruebaInformes.Models;
using Microsoft.Extensions.Logging;

namespace webApiPruebaInformes
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
            services.AddControllers();
            //agregar contexto a la base de datos al contenedor DI
            services.Configure<ContextDataBase>(
                Configuration.GetSection(nameof(ContextDataBase))
                );

            services.AddSingleton<IContextDataBase>(sp =>
                    sp.GetRequiredService<IOptions<ContextDataBase>>().Value
                );

            services.Configure<ContextInfoInformes>(
                Configuration.GetSection(nameof(ContextInfoInformes))
                );

            services.AddSingleton<IContextInfoInformes>(sp =>
                    sp.GetRequiredService<IOptions<ContextInfoInformes>>().Value
                );

            services.AddSingleton<ProcesosService>();
            services.AddSingleton<DataInfoInformesService>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
