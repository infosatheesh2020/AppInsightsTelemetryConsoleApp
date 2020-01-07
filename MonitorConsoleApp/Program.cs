using System;
using System.Net.Http;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DataContracts;
using System.Threading.Tasks;

namespace MonitorConsoleApp
{
    class Program
    {
        private static HttpClient _httpClient = new HttpClient();
        private static TelemetryConfiguration configuration;
        public static TelemetryClient client;

        public static int Main(string[] args)
        {
            // Test if input arguments were supplied.
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter AppInsights Instrumentation key and file containing URLs to monitor as arguments.");
                Console.WriteLine("Usage: AppInsightsConsoleApp.exe  '0c329c27-cf39-4c15-9623-5b8ce4b232bb' 'c:\\test.txt'");
                return 1;
            }

            string Ikey = args[0];
            string filepath = args[1];

            int counter = 0;
            string line;

            Console.WriteLine("Telemetry will be sent to IKEY: {0}", Ikey);

            configuration = new TelemetryConfiguration();
            client = new TelemetryClient(configuration);
            client.InstrumentationKey = Ikey;

            Console.WriteLine("Executing Health Check....");

            // Read the file and display it line by line.  
            System.IO.StreamReader file = new System.IO.StreamReader(@filepath);
            while ((line = file.ReadLine()) != null)
            {
                Console.WriteLine($"Run(): About to hit URL: '{line}'");
                _ = hitUrl(line);
                counter++;
            }

            file.Close();
            System.Console.WriteLine("Finished Sending telemetry for {0} URLs.", counter);

            Console.WriteLine($"Run(): Completed..");
            return 0;
        }


        private static HttpResponseMessage hitUrl(string url)
        {
            HttpResponseMessage response = null;
            AvailabilityTelemetry telemetry = new AvailabilityTelemetry();
            telemetry.Name = url;
            telemetry.Timestamp = DateTime.Now;
            try
            {
                response = _httpClient.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"hitUrl(): Successfully hit URL: '{url}'");
                }
                else
                {
                    Console.WriteLine($"hitUrl(): Failed to hit URL: '{url}'. Response: {(int)response.StatusCode + " : " + response.ReasonPhrase}");
                }

                telemetry.Success = response.IsSuccessStatusCode;
                telemetry.Message = response.ReasonPhrase;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"hitUrl(): Failed to hit URL (Exception) : '{url}'");
                telemetry.Success = false;
                telemetry.Message = "URL not reachable";
            }
            finally
            {
                client.TrackAvailability(telemetry);
                Console.WriteLine($"Telemetry sent for URL: '{url}'");
                client.Flush();
                Task.Delay(1000).Wait();
            }
            return response;
        }
    }
}
