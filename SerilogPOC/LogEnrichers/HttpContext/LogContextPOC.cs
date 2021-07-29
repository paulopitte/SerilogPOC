namespace SerilogPOC.LogEnrichers.HttpContext
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Serilog.Context;
    using Serilog.Core;

    public class LogContextPOC
    {
        private readonly RequestDelegate _next;
        private readonly LogContextPOCOptions _options;

        public LogContextPOC(RequestDelegate next, IOptions<LogContextPOCOptions> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            IEnumerable<ILogEventEnricher> enrichers = null;
            if (_options.EnrichersForContextFactory != null)
            {
                try
                {
                    enrichers = _options.EnrichersForContextFactory(context);
                }
                catch
                {
                    if (_options.ReThrowEnricherFactoryExceptions)
                        throw;
                }
            }

            var nextExecuted = false;
            if (enrichers != null)
            {
                var array = enrichers.ToArray();
                if (array.Any())
                {
                    using (LogContext.Push(array))
                    {
                        await _next(context);
                        nextExecuted = true;
                    }
                }                
            }

            if (!nextExecuted)
                await _next(context);
        }
    }
}