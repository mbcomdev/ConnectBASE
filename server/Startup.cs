using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using connectBase.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using connectBase.Helper;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using connectBase.Services.swagger;
using connectBase.Services.COM;
using connectBase.Entities;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;

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
            // Add AppSettings 
            var _settings = new AppSettings();

            // Add all controllers
            services.AddControllers();

            // Add cors
            services.AddCors();

            // Add SchemeService scoped
            services.AddScoped<ISchemeService, SchemeService>();
            // Add UserService Scoped
            services.AddScoped<IUserService, UserService>();
            // Add PutService Scoped
            services.AddScoped<IPutService, PutService>();
            // Add PostService scoped
            services.AddScoped<IPostService, PostService>();
            // Add COMConnection as Singleton
            services.AddSingleton<COMConnection>();
            //Add DeleteService scoped
            services.AddScoped<IDeleteService, DeleteService>();
            // Add AuthenticationService Scoped
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            // Add ValidationService Scoped
            services.AddScoped<IValidationService, ValidationService>();
            // Add GetService Scoped
            services.AddScoped<IGetService, GetService>();
            // Add GetService as Singleton
            services.AddSingleton<ISchedulerService, SchedulerService>();

            // add swagger request examples
            services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();

            // Encode jwt secret
            var key = Encoding.ASCII.GetBytes(_settings.getSecret());

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

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

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var _settings = new AppSettings();

            app.UseHttpsRedirection();

            app.UseRouting();

            // global cors policy
            // Should be changed in production mode
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            // Authentication middleware - checks user identity
            app.UseAuthentication();

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
            Task.Factory.StartNew(() => InitializeMicrotechDatabase(_settings.getServiceUser(), schemeService));
        }

        public void CleanupDirectories(ISchemeService schemeService)
        {
            schemeService.CleanupDirectories(Configuration["CleanupDirectories"]);
        }

        public void InitializeMicrotechDatabase(User user, ISchemeService schemeService)
        {
            schemeService.CreateDatabaseScheme(user);
        }
    }
}
