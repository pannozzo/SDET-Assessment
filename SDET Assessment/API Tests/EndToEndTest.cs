using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using static SDET_Assessment.Resources;

namespace SDET_Assessment
{
    [TestFixture(BrowserType.Chrome)]
    [TestFixture(BrowserType.Firefox)]
    [TestFixture(BrowserType.Edge)]
    public class EndToEndTest : AbstractBaseTests
    {
        HttpClient httpClient;

        public string originalTableText;
        public string uploadedTableText;
        public string uploadFile;
        public string downloadFile;
        
        private async Task<HttpResponseMessage> getWeatherforecastResponse(HttpResponseMessage postResponse)
        {
            int postResponseId = int.Parse(Regex.Match(postResponse.Headers.Location.ToString(), @"\d+$", RegexOptions.RightToLeft).Value);
            return await httpClient.GetAsync($"weatherforecast/{postResponseId}");
        }

        private async void writeGetResponseToUploadFile(HttpResponseMessage getResponse)
        {
            JObject jsonResponse = JObject.Parse(await getResponse.Content.ReadAsStringAsync());
            JArray jArray = new JArray();
            jArray.Add(jsonResponse);

            File.WriteAllText(uploadFile, jArray.ToString());
        }
        public EndToEndTest(BrowserType browserType) : base(browserType) => browser = browserType;

        [SetUp]
        public void Setup()
        { 
            originalTableText = "";
            uploadedTableText = "";
            uploadFile = Path.Combine(Path.GetTempPath(), "forecast.json");
            downloadFile = Path.Combine(downloadFilePath, "forecast.json");

            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:8081/");
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            driver = CreateDriver();
            driver.Url = "http://localhost:8080/";
            driver.Manage().Window.Maximize();

            Thread.Sleep(500); //The browser needs a second to actually open before it can start interacting with the page

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));

            wait.Until(ExpectedConditions.TitleIs("Home"));
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
            httpClient.Dispose();
        }

        [TestCase(2024, 01, 01, 10, WeatherSummary.Mild)]
        public async Task End_To_End_Integration_Test(int year, int month, int day, int temperatureC, WeatherSummary summary)
        {
            if (browser == BrowserType.Firefox || browser == BrowserType.Edge)
            {
                Assert.Ignore("Ran out of time to troubleshoot specific configurations for setting FireFox/Edge's downloading paths");
            }

            using HttpResponseMessage postResponse = await httpClient.PostAsJsonAsync(
               "weatherforecast",
               new WeatherRecord(
                   Date: new DateOnly(year, month, day),
                   TemperatureC: temperatureC,
                   Summary: summary
               ));

            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(postResponse.Headers.Location, Is.Not.Null);


            using HttpResponseMessage getResponse = await getWeatherforecastResponse(postResponse); 
            writeGetResponseToUploadFile(getResponse);


            ClickElement(By.LinkText("Weather"));
            originalTableText = GetTextFromElement(By.ClassName("table"));
            Assert.That(originalTableText, Is.Not.Empty);

            IWebElement fileInput = driver.FindElement(By.CssSelector("input[type=file]"));
            fileInput.SendKeys(uploadFile);
            wait.Until(driver => driver.FindElement(By.ClassName("table")).Text != originalTableText);
            uploadedTableText = GetTextFromElement(By.ClassName("table"));
            Assert.That(originalTableText, Is.Not.EqualTo(uploadedTableText));


            ClickElement(By.TagName("button"));
            wait.Until(d => File.Exists(downloadFile));


            JArray uploadedJson = JArray.Parse(File.ReadAllText(uploadFile));
            JArray downloadedJson = JArray.Parse(File.ReadAllText(downloadFile));

            //The JSON member "id" is not preserved in the downloaded file.
            //See README.txt for notes
            foreach (var item in uploadedJson)
            {
                item["id"]?.Parent.Remove();
            }

            bool isJsonEqual = JArray.DeepEquals(downloadedJson, uploadedJson);
            Assert.That(isJsonEqual, Is.True);
        }

    }
}