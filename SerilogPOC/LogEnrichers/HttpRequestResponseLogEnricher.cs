using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace SerilogPOC.LogEnrichers
{
    internal class HttpRequestResponseLogEnricher : ILogEventEnricher
    {
        public const string RequestMessageTemplate = "[{MessageDirection}] {RequestProtocol} [{RequestMethod}] {RequestPath} requested...";
        public const string ResponseMessageTemplate = "[{MessageDirection}] {RequestProtocol} [{RequestMethod}] {RequestPath} responded {ResponseStatusCode} in {ElapsedTime}ms";

        private const string MessageDirectionPropertyName = "MessageDirection";

        private const string RequestHeaderPropertyName = "RequestHeader";
        private const string RequestMethodPropertyName = "RequestMethod";
        private const string RequestPathPropertyName = "RequestPath";
        private const string RequestFormPropertyName = "RequestForm";
        private const string RequestBodyPropertyName = "RequestBody";
        private const string RequestHostPropertyName = "RequestHost";
        private const string RequestProtocolPropertyName = "RequestProtocol";

        private const string ElapsedTimePropertyName = "ElapsedTime";
        private const string ResponseStatusCodePropertyName = "ResponseStatusCode";
        private const string ResponseBodyPropertyName = "ResponseBody";

        private LogEventProperty _cachedMessageDirectionProperty;

        private LogEventProperty _cachedRequestHeaderProperty;
        private LogEventProperty _cachedRequestMethodProperty;
        private LogEventProperty _cachedRequestPathProperty;
        private LogEventProperty _cachedRequestFormProperty;
        private LogEventProperty _cachedRequestBodyProperty;
        private LogEventProperty _cachedRequestHostProperty;
        private LogEventProperty _cachedRequestProtocolProperty;

        private LogEventProperty _cachedElapsedTimeProperty;
        private LogEventProperty _cachedResponseStatusCodeProperty;
        private LogEventProperty _cachedResponseBodyProperty;

        private HttpRequest Request { get; }

        private string RequestBody { get; }

        private bool HasResponseData { get; set; }

        private int StatusCode { get; set; }

        private TimeSpan ElapsedTime { get; set; }

        private string ResponseBody { get; set; }

        public HttpRequestResponseLogEnricher(HttpRequest request, string requestBody)
        {
            Request = request;
            RequestBody = requestBody;
        }

        public void SetResponseData(int statusCode, TimeSpan elapsedTime, string responseBody)
        {
            _cachedMessageDirectionProperty = null;

            StatusCode = statusCode;
            ElapsedTime = elapsedTime;
            ResponseBody = responseBody;
            HasResponseData = true;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            EnsurePropertiesCreated(propertyFactory);

            EnsurePropertiesAdded(logEvent);
        }

        private void EnsurePropertiesCreated(ILogEventPropertyFactory propertyFactory)
        {
            if (HasResponseData)
            {
                _cachedMessageDirectionProperty = _cachedMessageDirectionProperty ?? propertyFactory.CreateProperty(MessageDirectionPropertyName, GetMessageDirection());
                _cachedElapsedTimeProperty = _cachedElapsedTimeProperty ?? propertyFactory.CreateProperty(ElapsedTimePropertyName, GetElapsedTime());
                _cachedResponseStatusCodeProperty = _cachedResponseStatusCodeProperty ?? propertyFactory.CreateProperty(ResponseStatusCodePropertyName, GetResponseStatusCode());
                _cachedResponseBodyProperty = _cachedResponseBodyProperty ?? propertyFactory.CreateProperty(ResponseBodyPropertyName, GetResponseBody());
            }

            _cachedMessageDirectionProperty = _cachedMessageDirectionProperty ?? propertyFactory.CreateProperty(MessageDirectionPropertyName, GetMessageDirection());
            _cachedRequestHeaderProperty = _cachedRequestHeaderProperty ?? propertyFactory.CreateProperty(RequestHeaderPropertyName, GetRequestHeaders(), true);
            _cachedRequestMethodProperty = _cachedRequestMethodProperty ?? propertyFactory.CreateProperty(RequestMethodPropertyName, GetRequestMethod());
            _cachedRequestPathProperty = _cachedRequestPathProperty ?? propertyFactory.CreateProperty(RequestPathPropertyName, GetRequestPath());
            _cachedRequestFormProperty = _cachedRequestFormProperty ?? propertyFactory.CreateProperty(RequestFormPropertyName, GetRequestForm(), true);
            _cachedRequestBodyProperty = _cachedRequestBodyProperty ?? propertyFactory.CreateProperty(RequestBodyPropertyName, GetRequestBody());
            _cachedRequestHostProperty = _cachedRequestHostProperty ?? propertyFactory.CreateProperty(RequestHostPropertyName, GetRequestHost());
            _cachedRequestProtocolProperty = _cachedRequestProtocolProperty ?? propertyFactory.CreateProperty(RequestProtocolPropertyName, GetRequestProtocol());
        }

        private void EnsurePropertiesAdded(LogEvent logEvent)
        {
            logEvent.AddPropertyIfAbsent(_cachedMessageDirectionProperty);

            if (HasResponseData)
            {
                logEvent.AddPropertyIfAbsent(_cachedElapsedTimeProperty);
                logEvent.AddPropertyIfAbsent(_cachedResponseStatusCodeProperty);
                logEvent.AddPropertyIfAbsent(_cachedResponseBodyProperty);
            }

            logEvent.AddPropertyIfAbsent(_cachedRequestHeaderProperty);
            logEvent.AddPropertyIfAbsent(_cachedRequestMethodProperty);
            logEvent.AddPropertyIfAbsent(_cachedRequestPathProperty);
            logEvent.AddPropertyIfAbsent(_cachedRequestFormProperty);
            logEvent.AddPropertyIfAbsent(_cachedRequestBodyProperty);
            logEvent.AddPropertyIfAbsent(_cachedRequestHostProperty);
            logEvent.AddPropertyIfAbsent(_cachedRequestProtocolProperty);
        }

        private string GetMessageDirection() => HasResponseData ? "Out" : "In";

        private string GetElapsedTime() => $"{ElapsedTime.Duration().TotalMilliseconds:0.00}";

        private Dictionary<string, string> GetRequestHeaders() =>
            Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

        private string GetRequestMethod() => Request.Method;

        private string GetRequestPath() => Request.Path;

        private Dictionary<string, string> GetRequestForm() => Request.HasFormContentType
            ? Request.Form.ToDictionary(v => v.Key, v => v.Value.ToString())
            : null;

        private string GetRequestBody() => RequestBody;

        private string GetRequestHost() => Request.Host.ToString();

        private string GetRequestProtocol() => Request.Protocol;

        private int GetResponseStatusCode() => StatusCode;

        private string GetResponseBody() => ResponseBody;

        public string GetRequestMessageTemplate() => RequestMessageTemplate;

        public object[] GetRequestMessageProperties()
        {
            return new object[]
            {
                GetMessageDirection(),
                GetRequestProtocol(),
                GetRequestMethod(),
                GetRequestPath()
            };
        }

        public string GetResponseMessageTemplate() => ResponseMessageTemplate;

        public object[] GetResponseMessageProperties()
        {
            return new object[]
            {
                GetMessageDirection(),
                GetRequestProtocol(),
                GetRequestMethod(),
                GetRequestPath(),
                GetResponseStatusCode(),
                GetElapsedTime()
            };
        }
    }
}
