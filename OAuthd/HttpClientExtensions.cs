using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd
{
    public static class HttpClientExtensions
    {
        public static async Task<string> ToRawString(this HttpRequestMessage request, HttpClientHandler clientHandler)
        {
            var sb = new StringBuilder();

            var line1 = $"{request.Method} {request.RequestUri} HTTP/{request.Version}";
            sb.AppendLine(line1);

            foreach (var header in request.Headers)
                foreach (var val in header.Value)
                {
                    sb.AppendLine($"{header.Key}: {val}");
                }

            if (request.Content?.Headers != null)
            {
                foreach (var header in request.Content.Headers)
                    foreach (var val in header.Value)
                    {
                        sb.AppendLine($"{header.Key}: {val}");
                    }
            }
            var cookies = clientHandler.CookieContainer.GetCookies(request.RequestUri);
            if (cookies.Count > 0)
            {
                sb.Append("Cookie: ");
                bool next = false;
                foreach (System.Net.Cookie cookie in cookies)
                {
                    if (next)
                        sb.Append("; ");
                    sb.Append(cookie.Name).Append("=").Append(cookie.Value);
                    next = true;
                }
                sb.AppendLine();
            }
            sb.AppendLine();

            try
            {
                var body = await (request.Content?.ReadAsStringAsync() ?? Task.FromResult<string>(null));
                if (!string.IsNullOrWhiteSpace(body))
                    sb.AppendLine(body);
            }
            catch (Exception ex)
			{
                sb.AppendLine("ERROR: " + ex.ToString());
			}

            return sb.ToString();
        }

        public static async Task<string> ToRawString(this HttpResponseMessage response, HttpClientHandler clientHandler)
        {
            var sb = new StringBuilder();

            var statusCode = (int)response.StatusCode;
            var line1 = $"HTTP/{response.Version} {statusCode} {response.ReasonPhrase}";
            sb.AppendLine(line1);

            foreach (var header in response.Headers)
				foreach (var val in header.Value)
				{
					sb.AppendLine($"{header.Key}: {val}");
				}

            foreach (var header in response.Content.Headers)
                foreach (var val in header.Value)
                {
                    sb.AppendLine($"{header.Key}: {val}");
                }
            sb.AppendLine();

            try
            {
                var body = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(body))
                    sb.AppendLine(body);
            }
            catch (Exception ex)
            {
                sb.AppendLine("ERROR: " + ex.ToString());
            }

            return sb.ToString();
        }
    }
}
