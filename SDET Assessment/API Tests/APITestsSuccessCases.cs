using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using static SDET_Assessment.Resources;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace SDET_Assessment
{
    public class APITestsSuccessCases
    {

        HttpClient httpClient;

        [SetUp]
        public void Setup()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:8081/");
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json") );
        }

        [TearDown]
        public void TearDown()
        {
            httpClient.Dispose();
        }

        //Testing a modern date, min/max of DateOnly, leap year
        [TestCase(2024, 01, 01, 10, WeatherSummary.Mild)]
        [TestCase(0001, 01, 01, 10, WeatherSummary.Mild)]
        [TestCase(9999, 12, 31, 10, WeatherSummary.Mild)]
        [TestCase(2024, 02, 29, 10, WeatherSummary.Mild)]
        //Testing minimum Celsius, maximum integer for temperature
        [TestCase(2024, 01, 01, -273, WeatherSummary.Undefined)]
        [TestCase(2024, 01, 01, int.MaxValue, WeatherSummary.Undefined)]
        //Testing all valid inputs for the Summary
        [TestCase(2024, 01, 01, 0, WeatherSummary.Undefined)]
        [TestCase(2024, 01, 01, 0, WeatherSummary.Freezing)]
        [TestCase(2024, 01, 01, 0, WeatherSummary.Bracing)]
        [TestCase(2024, 01, 01, 0, WeatherSummary.Chilly)]
        [TestCase(2024, 01, 01, 0, WeatherSummary.Cool)]
        [TestCase(2024, 01, 01, 0, WeatherSummary.Mild)]
        [TestCase(2024, 01, 01, 0, WeatherSummary.Warm)]
        [TestCase(2024, 01, 01, 0, WeatherSummary.Balmy)]
        [TestCase(2024, 01, 01, 0, WeatherSummary.Hot)]
        [TestCase(2024, 01, 01, 0, WeatherSummary.Sweltering)]
        [TestCase(2024, 01, 01, 0, WeatherSummary.Scorching)]
        public async Task POST_Weatherforecast_Returns_Created_StatusCode(int year, int month, int day, int temperatureC, WeatherSummary summary)
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                "weatherforecast",
                new WeatherRecord(
                    Date: new DateOnly(year, month, day),
                    TemperatureC: temperatureC, 
                    Summary: summary
                ));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }


        [Test]
        public async Task GET_Weatherforecast_Returns_Valid_JSON()
        {
            using HttpResponseMessage getResponse = await httpClient.GetAsync($"weatherforecast");
            var jsonResponse = await getResponse.Content.ReadAsStringAsync();
            TestContext.WriteLine("JSON RESPONSE");
            TestContext.WriteLine(jsonResponse);
            
            Assert.DoesNotThrow(() => JArray.Parse(jsonResponse)); 
        }

        [TestCase(2024, 01, 01, 10, WeatherSummary.Mild)]
        public async Task GET_Weatherforecast_With_ID_Returns_Valid_JSON(int year, int month, int day, int temperatureC, WeatherSummary summary)
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
            var jsonResponse = await getResponse.Content.ReadAsStringAsync();

            Assert.DoesNotThrow(() => JObject.Parse(jsonResponse));
        }

        [TestCase(2024, 01, 01, -40, WeatherSummary.Undefined)]
        [TestCase(2024, 01, 01, -20, WeatherSummary.Undefined)]
        [TestCase(2024, 01, 01, 10, WeatherSummary.Undefined)]
        [TestCase(2024, 01, 01, 30, WeatherSummary.Undefined)]
        [TestCase(2024, 01, 01, 40, WeatherSummary.Undefined)]
        [TestCase(2024, 01, 01, 100, WeatherSummary.Undefined)]
        public async Task POST_Temperature_In_Celsius_Should_GET_Correct_Fahrenheit(int year, int month, int day, int temperatureC, WeatherSummary summary)
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

            var jsonResponse = await getResponse.Content.ReadAsStringAsync();
            int fahrenheitValue = int.Parse(JObject.Parse(jsonResponse).SelectToken("temperatureF").ToString());
            int celsiusValue = int.Parse(JObject.Parse(jsonResponse).SelectToken("temperatureC").ToString());

            int correctConversion = (int)Math.Round((celsiusValue * (9.0 / 5.0)) + 32);

            Assert.That(fahrenheitValue, Is.EqualTo(correctConversion));
        }

    }
}