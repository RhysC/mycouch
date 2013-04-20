﻿using System;
using System.Net;
using System.Net.Http;

namespace MyCouch
{
    [Serializable]
    public abstract class Response : IResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess
        {
            get { return (int)StatusCode >= 200 && (int)StatusCode < 300; }
        }
        public Uri RequestUri { get; set; }
        public HttpMethod RequestMethod { get; set; }
        public string Error { get; set; }
        public string Reason { get; set; }
        public override string ToString()
        {
#if DEBUG
            return GenerateToStringDebugVersion();
#else
            return base.ToString();
#endif
        }

        protected virtual string GenerateToStringDebugVersion()
        {
            return string.Format("RequestUri: {1}{0}RequestMethod: {2}{0}Status: {3}({4}){0}Error:{5}{0}Reason: {6}",
                Environment.NewLine,
                RequestUri,
                RequestMethod,
                StatusCode,
                (int)StatusCode,
                Error ?? "<NULL>",
                Reason ?? "<NULL>");
        }
    }

    internal static class ResponseExtensions
    {
        internal static bool ContentShouldHaveIdAndRev(this IResponse response)
        {
            return
                response.RequestMethod == HttpMethod.Post ||
                response.RequestMethod == HttpMethod.Put ||
                response.RequestMethod == HttpMethod.Delete;
        }
    }
}