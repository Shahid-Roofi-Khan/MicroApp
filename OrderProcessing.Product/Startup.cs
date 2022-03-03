using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
        //  private readonly ILoggerFactory _loggerFactory;             //Shahid: This logger will be added Not in main() in program.cs so it won't be available at this point, it will be created rather in this class itself later

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


            services.AddHttpContextAccessor();

            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));      //Shahid: This is very critical single line which tells in case logging fails to serilog the reason of actual failure


            // ------------ Shahid: below were needed to override how Elastic Search service was hit from dotnet
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };


            Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", _env)
            .Enrich.WithMachineName()                   //Shahid: This is must for machine name bifurcation later on source of events which container is sending these events
            .Enrich.WithEnvironmentUserName()           //Shahid: Although mostly this would be system account, but sometimes for security, difference of creds come in, so it might be useful
            .Enrich.WithClientIp()                      //Shahid: This is of great value add. This is coming from nuget package: https://github.com/mo-esmp/serilog-enrichers-clientinfo
            .Enrich.WithClientAgent()                   //Shahid: This also from above nuget package
            .Enrich.With<EnricherForCMTraceForLogTypeField>()  //Shahid: This is my own class for small enricher so that log type property which CMTrace need to have to render its logs properly
            //.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(_configuration["ElasticConfiguration:Uri"]))
            //{

            //    TypeName = null,                            //Shahid: Since Elastic Search 8.0.0 and onwards this filed is no more there, so need to set null else, it will break the sink
            //    BatchAction = ElasticOpType.Create,
            //    IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
            //    CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
            //    AutoRegisterTemplate = true,
            //    ModifyConnectionSettings = (c) => c.BasicAuthentication("elastic", "bYq_PZIk0827sGu0*4rE"),
            //    NumberOfShards = 2,
            //    NumberOfReplicas = 1,
            //    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
            //    MinimumLogEventLevel = Serilog.Events.LogEventLevel.Information

            //})
            .ReadFrom.Configuration(_configuration)     //Shahid: i hope this will help overrider settings here from setting in appsettings which should be ideal
            .CreateLogger();

            

           

            // Shahid: below was needed to fall back to normal logging in case of failures. Uncomment this to log to local file in case of failures
            //FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
            //EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
            //                                   EmitEventFailureHandling.WriteToFailureSink |
            //                                   EmitEventFailureHandling.RaiseCallback,
            //FailureSink = new FileSink("./failures.txt", new JsonFormatter(), null)

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

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

            loggerFactory.AddSerilog();             //Shahid: This is the only change to add SeriLog to project. I've moved it from Program.cs from HostBuilder's end statement which makes it clean

        }
    }

}
