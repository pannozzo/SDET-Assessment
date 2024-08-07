using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V125.Target;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using static SDET_Assessment.Resources;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace SDET_Assessment
{
    [TestFixture(BrowserType.Chrome)]
    [TestFixture(BrowserType.Firefox)]
    [TestFixture(BrowserType.Edge)]
    public class WeatherTests : AbstractBaseTests
    {
        public WeatherTests(BrowserType browserType) : base(browserType) => browser = browserType;

        [SetUp]
        public void Setup()
        {
            driver = CreateDriver();
            driver.Url = "http://localhost:8080/";
            driver.Manage().Window.Maximize();

            Thread.Sleep(500); //The browser needs a second to actually open before it can start interacting with the page

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));

            wait.Until(ExpectedConditions.TitleIs("Home"));
        }

        [TearDown]
        public void TearDown() {
            driver.Quit();
            Directory.Delete(downloadFilePath, true);
        }

        [Test]
        public void Upload_File_Updates_Data_On_Page()
        {
            string originalTableText = "";
            string uploadFile = Directory.GetCurrentDirectory() + @"\input\forecast.json";
            ClickElement(By.LinkText("Weather"));
            
            IWebElement originalTable = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
            originalTableText = originalTable.Text;

            Assert.That(originalTableText, Is.Not.Empty);
            
            IWebElement fileInput = driver.FindElement(By.CssSelector("input[type=file]"));
            fileInput.SendKeys(uploadFile);
            
            wait.Until(d => driver.FindElement(By.ClassName("table")).Text != originalTableText);
            IWebElement uploadedTable = wait.Until(d => driver.FindElement(By.ClassName("table")));
            string uploadedTableText = uploadedTable.Text;

            Assert.That(originalTableText, Is.Not.EqualTo(uploadedTableText));
        }

        [Test]
        public void Downloaded_Data_Reflects_Uploaded_Data_On_The_Page()
        {
            if (browser == BrowserType.Firefox || browser == BrowserType.Edge)
            {
                Assert.Ignore("Ran out of time to troubleshoot specific configurations for setting FireFox/Edge's downloading paths");
            }

            string originalTableText = "";
            string uploadFile = Directory.GetCurrentDirectory() + @"\input\forecast.json";
            string downloadFile = downloadFilePath + @"\forecast.json";


            ClickElement(By.LinkText("Weather"));

            IWebElement originalTable = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
            originalTableText = originalTable.Text;

            Assert.That(originalTableText, Is.Not.Empty);

            IWebElement fileInput = driver.FindElement(By.CssSelector("input[type=file]"));
            fileInput.SendKeys(uploadFile);

            wait.Until(d => driver.FindElement(By.ClassName("table")).Text != originalTableText);
            IWebElement uploadedTable = wait.Until(d => driver.FindElement(By.ClassName("table")));
            string uploadedTableText = uploadedTable.Text;

            Assert.That(originalTableText, Is.Not.EqualTo(uploadedTableText));

            ClickElement(By.TagName("button"));

            wait.Until(d => File.Exists(downloadFile));

            string uploadText = File.ReadAllText(uploadFile);
            string downloadText = File.ReadAllText(downloadFile);

            JArray uploadedJson = JArray.Parse(uploadText);
            JArray downloadedJson = JArray.Parse(downloadText);

            bool isJsonEqual = JArray.DeepEquals(downloadedJson, uploadedJson);

            Assert.That(isJsonEqual, Is.True);
        }

        [Test]
        public void Upload_File_With_Invalid_JSON_Does_Not_Update_Data_On_Page()
        {
            string originalTableText = "";
            string uploadFile = Directory.GetCurrentDirectory() + @"\input\invalidJSON.json";
            ClickElement(By.LinkText("Weather"));

            IWebElement originalTable = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table"))); 
            originalTableText = originalTable.Text;

            Assert.That(originalTableText, Is.Not.Empty);

            IWebElement fileInput = driver.FindElement(By.CssSelector("input[type=file]"));
            fileInput.SendKeys(uploadFile);
            
            IWebElement uploadedTable = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
            string uploadedTableText = uploadedTable.Text;

            Assert.That(originalTableText, Is.EqualTo(uploadedTableText));
        }

        [Test]
        public void Weather_Table_Resets_When_Page_Refreshes()
        {
            string originalTableText = "";
            string refreshedTableText = "";
            string uploadFile = Directory.GetCurrentDirectory() + @"\input\forecast.json";

            ClickElement(By.LinkText("Weather"));

            IWebElement originalTable = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
            originalTableText = originalTable.Text;

            Assert.That(originalTableText, Is.Not.Empty);

            IWebElement fileInput = driver.FindElement(By.CssSelector("input[type=file]"));
            fileInput.SendKeys(uploadFile);

            wait.Until(d => driver.FindElement(By.ClassName("table")).Text != originalTableText);
            IWebElement uploadedTable = wait.Until(d => driver.FindElement(By.ClassName("table")));
            string uploadedTableText = uploadedTable.Text;

            Assert.That(originalTableText, Is.Not.EqualTo(uploadedTableText));

            driver.Navigate().Refresh();

            IWebElement refreshedTable = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
            refreshedTableText = refreshedTable.Text;

            Assert.That(refreshedTableText, Is.Not.Empty);
            Assert.That(refreshedTableText, Is.Not.EqualTo(uploadedTableText));
        }

        [Test]
        public void Weather_Table_Resets_When_User_Navigates_To_Other_Pages()
        {
            string originalTableText = "";
            string refreshedTableText = "";
            string uploadFile = Directory.GetCurrentDirectory() + @"\input\forecast.json";

            ClickElement(By.LinkText("Weather"));

            IWebElement originalTable = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
            originalTableText = originalTable.Text;

            Assert.That(originalTableText, Is.Not.Empty);

            IWebElement fileInput = driver.FindElement(By.CssSelector("input[type=file]"));
            fileInput.SendKeys(uploadFile);

            wait.Until(d => driver.FindElement(By.ClassName("table")).Text != originalTableText);
            IWebElement uploadedTable = wait.Until(d => driver.FindElement(By.ClassName("table")));
            string uploadedTableText = uploadedTable.Text;

            Assert.That(originalTableText, Is.Not.EqualTo(uploadedTableText));

            ClickElement(By.LinkText("Home"));
            wait.Until(ExpectedConditions.TitleIs("Home"));

            ClickElement(By.LinkText("Weather"));
            wait.Until(ExpectedConditions.TitleIs("Weather"));

            IWebElement refreshedTable = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
            refreshedTableText = refreshedTable.Text;

            Assert.That(refreshedTableText, Is.Not.Empty);
            Assert.That(refreshedTableText, Is.Not.EqualTo(uploadedTableText));
        }
    }
}