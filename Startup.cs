using connectBase.Services;
using connectBase.Services.COM;
using connectBase.Services.swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace connectBase
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
            // Add all controllers
            services.AddControllers();

            // Add SchemeService scoped
            services.AddScoped<ISchemeService, SchemeService>();
            // Add PutService Scoped
            services.AddScoped<IPutService, PutService>();
            // Add PostService scoped
            services.AddScoped<IPostService, PostService>();
            // Add COMConnection as Singleton
            services.AddSingleton<COMConnection>();
            //Add DeleteService scoped
            services.AddScoped<IDeleteService, DeleteService>();
            // Add ValidationService Scoped
            services.AddScoped<IValidationService, ValidationService>();
            // Add GetService Scoped
            services.AddScoped<IGetService, GetService>();
            // Add GetService as Singleton
            services.AddSingleton<ISchedulerService, SchedulerService>();

            // add swagger request examples
            services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

            services.AddMvc();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ConnectBase",
                    Description = "Adapter to access Büro+ with JSON",
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("API-Key", new OpenApiSecurityScheme
                {
                    Description = "API-Key Authorization header using the x-api-key scheme. \r\n\r\n Enter then your token in the text input below.\r\n\r\nExample: \"12345abcdef\"",
                    Name = "x-api-key",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "API-Key"
                });
                c.OperationFilter<BasicAuthOperationsFilter>();
                // [SwaggerRequestExample] & [SwaggerResponseExample]
                // version < 3.0 like this: c.OperationFilter<ExamplesOperationFilter>(); 
                // version 3.0 like this: c.AddSwaggerExamples(services.BuildServiceProvider());
                // version > 4.0 like this:
                c.ExampleFilters();
                c.OperationFilter<AddResponseHeadersFilter>(); // [SwaggerResponseHeader]
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISchemeService schemeService)
        {

            app.UseRouting();

            app.UseAuthorization();

            // Endpoints routing
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSwagger();       // see swagger on "/swagger/v1/ ."

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");     // see auto-generated, interactive docs at "/swagger"
            });

            CleanupDirectories(schemeService);
            Task.Factory.StartNew(() => InitializeMicrotechDatabase(schemeService));
        }

        public void CleanupDirectories(ISchemeService schemeService)
        {
            schemeService.CleanupDirectories(Configuration["CleanupDirectories"]);
        }

        public void InitializeMicrotechDatabase(ISchemeService schemeService)
        {
            schemeService.CreateDatabaseScheme();
        }
    }
}
