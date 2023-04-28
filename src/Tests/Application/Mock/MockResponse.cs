using System.Net;
using System.Net.Http;

namespace Tests.Application.Mock
{
    public class MockResponse
    {
        public readonly HttpMethod Method;
        public readonly string Url;
        public readonly HttpStatusCode StatusCode;
        public readonly string ResponseString;

        public MockResponse(HttpMethod method, string url, HttpStatusCode statusCode, string responseString)
        {
            Method = method;
            Url = url;
            StatusCode = statusCode;
            ResponseString = responseString;
        }
    }
}
