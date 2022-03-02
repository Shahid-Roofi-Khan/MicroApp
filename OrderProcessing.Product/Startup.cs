using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;

namespace OrderProcessing.Product
{
    public class Startup
    {

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        //  private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration configuration, IWebHostEnvironment env) //, ILoggerFactory loggerFactory)
        {
            //  _loggerFactory = loggerFactory;
            _configuration = configuration;
            _env = env;

        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderProcessing.Product", Version = "v1" });
            });


            //  var loggerConfig = new LoggerConfiguration();

            //  loggerConfig.MinimumLevel.Debug()
            //.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
            //{
            //    CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
            //    AutoRegisterTemplate = true,
            //});

            //  var logger = loggerConfig.CreateLogger();


            Log.Logger = new LoggerConfiguration()
.Enrich.FromLogContext()
.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(_configuration["ElasticConfiguration:Uri"]))
{
    AutoRegisterTemplate = true,
    IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
    CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),

    FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback,
    FailureSink = new FileSink("./failures.txt", new JsonFormatter(), null)
})
.Enrich.WithProperty("Environment", _env)
.ReadFrom.Configuration(_configuration)
.CreateLogger();



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {


            //var configuration = new ConfigurationBuilder()
            //     .SetBasePath(env.ContentRootPath)
            //     .AddJsonFile("appsettings.json")
            //     .Build();

            //var logger = new LoggerConfiguration()
            //    .ReadFrom.Configuration(configuration)
            //    .CreateLogger();



            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderProcessing.Product v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });




        }
    }
}
