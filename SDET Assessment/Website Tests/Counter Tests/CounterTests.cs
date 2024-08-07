using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V125.Target;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using static SDET_Assessment.Resources;

namespace SDET_Assessment
{
    [TestFixture(BrowserType.Chrome)]
    [TestFixture(BrowserType.Firefox)]
    [TestFixture(BrowserType.Edge)]
    public class CounterTests : AbstractBaseTests
    {
        public CounterTests(BrowserType browserType) : base(browserType) => browser = browserType;

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
        }

        [Test]
        public void Counter_Starts_At_Zero()
        {
            ClickElement(By.LinkText("Counter"));
            wait.Until(ExpectedConditions.TitleIs("Counter"));

            bool isFound = TextInElementIsFound(By.CssSelector("[role='status']"), "0");
            Assert.That(isFound, Is.True);
        }

        [Test]
        public void Counter_Increments_By_One_When_Button_Is_Pressed()
        {
            ClickElement(By.LinkText("Counter"));
            wait.Until(ExpectedConditions.TitleIs("Counter"));
            ClickElement(By.ClassName("btn-primary"));

            bool isFound = TextInElementIsFound(By.CssSelector("[role='status']"), "1");
            Assert.That(isFound, Is.True);
        }

        [Test]
        public void Counter_Increments_Correctly_When_Clicked_Multiple_Times()
        {
            ClickElement(By.LinkText("Counter"));
            wait.Until(ExpectedConditions.TitleIs("Counter"));

            for (int i = 0; i < 5; i++)
            {
                ClickElement(By.ClassName("btn-primary"));
            }

            bool isFound = TextInElementIsFound(By.CssSelector("[role='status']"), "5"); 
            Assert.That(isFound, Is.True);
        }

        [Test]
        public void Counter_Resets_When_Page_Refreshes()
        {
            ClickElement(By.LinkText("Counter"));
            wait.Until(ExpectedConditions.TitleIs("Counter"));
            ClickElement(By.ClassName("btn-primary"));

            bool isFound = TextInElementIsFound(By.CssSelector("[role='status']"), "1");
            Assert.That(isFound, Is.True);

            driver.Navigate().Refresh();
            wait.Until(ExpectedConditions.TitleIs("Counter"));

            isFound = TextInElementIsFound(By.CssSelector("[role='status']"), "0");
            Assert.That(isFound, Is.True);
        }

        [Test]
        public void Counter_Resets_When_User_Navigates_To_Other_Pages()
        {
            ClickElement(By.LinkText("Counter"));
            wait.Until(ExpectedConditions.TitleIs("Counter"));
            ClickElement(By.ClassName("btn-primary"));

            bool isFound = TextInElementIsFound(By.CssSelector("[role='status']"), "1");
            Assert.That(isFound, Is.True);

            ClickElement(By.LinkText("Home"));
            wait.Until(ExpectedConditions.TitleIs("Home"));
            ClickElement(By.LinkText("Counter"));
            wait.Until(ExpectedConditions.TitleIs("Counter"));

            isFound = TextInElementIsFound(By.CssSelector("[role='status']"), "0");
            Assert.That(isFound, Is.True);
        }
    }
}