using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using static SDET_Assessment.Resources;

namespace SDET_Assessment
{
    public class APITestsFailureCases
    {
        HttpClient httpClient;

        [SetUp]
        public void Setup()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:8081/");
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [TearDown]
        public void TearDown()
        {
            httpClient.Dispose();
        }

        [TestCase(int.MaxValue)]
        [TestCase(int.MinValue)]
        public async Task GET_Invalid_Id_Should_Return_NotFound(int invalidId)
        {
            using HttpResponseMessage getResponse = await httpClient.GetAsync($"weatherforecast/{invalidId}");

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        }

        
        [TestCase(2024, 01, 01, -300, WeatherSummary.Mild),
            Description("Minimum temperature Celsius is -273.15, values under this should be invalid & rejected")]
        public async Task POST_Invalid_Temperature_Should_Return_BadRequest(int year, int month, int day, int temperatureC, WeatherSummary summary)
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                "weatherforecast",
                new WeatherRecord(
                    Date: new DateOnly(year, month, day),
                    TemperatureC: temperatureC,
                    Summary: summary
                ));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCase(2024, 01, 01, 10, InvalidWeatherSummary.LowInvalidSummary)]
        [TestCase(2024, 01, 01, 10, InvalidWeatherSummary.HighInvalidSummary),
            Description("Summaries outside the enumeration range of [0,10] should not be accepted")]
        public async Task POST_Invalid_Summary_Should_Return_BadRequest(int year, int month, int day, int temperatureC, InvalidWeatherSummary summary)
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                "weatherforecast",
                new InvalidWeatherRecord(
                    Date: new DateOnly(year, month, day),
                    TemperatureC: temperatureC,
                    Summary: summary
                ));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCase(2024, 01, 01, int.MaxValue, WeatherSummary.Undefined),
            Description("The API should handle integer overflows, see README.md for notes")]
        public async Task POST_High_Celsius_Temperature_Should_Not_Integer_Overflow_Fahrenheit(int year, int month, int day, int temperatureC, WeatherSummary summary)
        {
            using HttpResponseMessage postResponse = await httpClient.PostAsJsonAsync(
                "weatherforecast",
                new WeatherRecord(
                    Date: new DateOnly(year, month, day),
                    TemperatureC: temperatureC,
                    Summary: summary
                ));

            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(postResponse.Headers.Location, Is.Not.Null);

            int postResponseId = int.Parse(Regex.Match(postResponse.Headers.Location.ToString(), @"\d+$", RegexOptions.RightToLeft).Value);

            using HttpResponseMessage getResponse = await httpClient.GetAsync($"weatherforecast/{postResponseId}");

            Assert.That(getResponse.StatusCode, Is.Not.EqualTo(HttpStatusCode.NotFound));

            var jsonResponse = await getResponse.Content.ReadAsStringAsync();

            int fahrenheitValue = int.Parse(JObject.Parse(jsonResponse).SelectToken("temperatureF").ToString());

            Assert.That(fahrenheitValue, Is.Not.Negative);
        }

        [TestCase(2024, 01, 01, 10, WeatherSummary.Mild)]
        public async Task POST_API_Should_Rate_Limit_After_Rapid_Requests(int year, int month, int day, int temperatureC, WeatherSummary summary)
        {
            bool POST_request_failed = false;

            for (int i = 0; i < 10000; i++)
            {
                using HttpResponseMessage postResponse = await httpClient.PostAsJsonAsync(
                    "weatherforecast",
                    new WeatherRecord(
                        Date: new DateOnly(year, month, day),
                        TemperatureC: temperatureC,
                        Summary: summary
                    ));

                if (postResponse.StatusCode is not HttpStatusCode.Created || postResponse.Headers.Location is null)
                {
                    POST_request_failed = true;
                    break;
                }

            }

            Assert.That(POST_request_failed, Is.True);
        }
    }
}