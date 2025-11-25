/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.IO;
using System.Net;

namespace SAM.API
{
    /// <summary>
    /// Secured WebClient with size limits, timeouts, and retry logic
    /// </summary>
    public class SafeWebClient : WebClient
    {
        private readonly int _maxDownloadSize;

        public SafeWebClient(int maxDownloadSize)
        {
            _maxDownloadSize = maxDownloadSize;
            Proxy = null; // Disable proxy for faster downloads
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);

            if (request is HttpWebRequest httpRequest)
            {
                httpRequest.Timeout = SecurityConfig.DOWNLOAD_TIMEOUT_MS;
                httpRequest.ReadWriteTimeout = SecurityConfig.DOWNLOAD_TIMEOUT_MS;
                httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);

            // Validate Content-Length header if present
            if (response.ContentLength > _maxDownloadSize)
            {
                response.Close();
                string url = request.RequestUri?.ToString() ?? "unknown";
                SecurityLogger.LogWithRateLimit(LogLevel.Warning, LogContext.HTTP, url,
                    $"Download exceeded size limit: {url} ({response.ContentLength} bytes)");
                throw new WebException(
                    $"Download size ({response.ContentLength} bytes) exceeds maximum allowed ({_maxDownloadSize} bytes)",
                    WebExceptionStatus.MessageLengthLimitExceeded);
            }

            return response;
        }

        public new byte[] DownloadData(Uri address)
        {
            return DownloadDataWithRetry(address, 0);
        }

        public new byte[] DownloadData(string address)
        {
            return DownloadData(new Uri(address));
        }

        private byte[] DownloadDataWithRetry(Uri address, int attemptNumber)
        {
            try
            {
                byte[] data;
                using (var stream = OpenRead(address))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        byte[] buffer = new byte[8192];
                        int totalRead = 0;
                        int bytesRead;

                        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalRead += bytesRead;

                            // Check size limit on decompressed data (critical for gzip/deflate)
                            if (totalRead > _maxDownloadSize)
                            {
                                string url = address?.ToString() ?? "unknown";
                                SecurityLogger.LogWithRateLimit(LogLevel.Warning, LogContext.HTTP, url,
                                    $"Download exceeded size limit: {url} ({totalRead} bytes decompressed)");
                                throw new WebException(
                                    $"Download exceeded maximum size of {_maxDownloadSize} bytes",
                                    WebExceptionStatus.MessageLengthLimitExceeded);
                            }

                            memoryStream.Write(buffer, 0, bytesRead);
                        }

                        data = memoryStream.ToArray();
                    }
                }

                return data;
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.Timeout && attemptNumber < SecurityConfig.MAX_DOWNLOAD_RETRIES)
            {
                // Retry with exponential backoff for timeouts
                int delayMs = (int)Math.Pow(2, attemptNumber) * 1000; // 1s, 2s, 4s...
                string url = address?.ToString() ?? "unknown";
                SecurityLogger.LogWithRateLimit(LogLevel.Warning, LogContext.HTTP, $"{url}_retry_{attemptNumber}",
                    $"Download timeout: {url}, retrying ({attemptNumber + 1}/{SecurityConfig.MAX_DOWNLOAD_RETRIES})");

                System.Threading.Thread.Sleep(delayMs);
                return DownloadDataWithRetry(address, attemptNumber + 1);
            }
            catch (WebException) when (attemptNumber >= SecurityConfig.MAX_DOWNLOAD_RETRIES)
            {
                string url = address?.ToString() ?? "unknown";
                SecurityLogger.LogWithRateLimit(LogLevel.Error, LogContext.HTTP, url,
                    $"Download failed after retries: {url}");
                throw;
            }
        }
    }
}
