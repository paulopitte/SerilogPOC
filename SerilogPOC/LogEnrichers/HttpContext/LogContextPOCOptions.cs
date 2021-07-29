namespace SerilogPOC.LogEnrichers.HttpContext
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Options;
    using Serilog.Core;

    public class LogContextPOCOptions : IOptions<LogContextPOCOptions>
    {
        public LogContextPOCOptions Value => this;

        public Func<Microsoft.AspNetCore.Http.HttpContext, IEnumerable<ILogEventEnricher>> EnrichersForContextFactory { get; set; }

        public bool ReThrowEnricherFactoryExceptions { get; set; } = true;
    }
}