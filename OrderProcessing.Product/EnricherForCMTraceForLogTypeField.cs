using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace OrderProcessing.Product
{
    class EnricherForCMTraceForLogTypeField : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.GetValueOrDefault("SourceContext") is null)
            {
                //   var pos = typeName.LastIndexOf('.');
                //   typeName = typeName.Substring(pos + 1, typeName.Length - pos - 2);
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("SourceContext", "Empty"));
            }
            string level = logEvent.Properties.GetValueOrDefault("SourceContext").ToString();

            int lvl = 1;
            if (logEvent.Level == LogEventLevel.Debug || logEvent.Level == LogEventLevel.Information || logEvent.Level == LogEventLevel.Verbose) lvl = 1;
            else if (logEvent.Level == LogEventLevel.Warning) lvl = 2;
            else if (logEvent.Level == LogEventLevel.Fatal || logEvent.Level == LogEventLevel.Error) lvl = 3;

            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("cmtracetype", lvl));
        }
    }

}
