using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

namespace SmokeTestAutomation
{
    [TestFixture]
    public class SmokeTest
    {
        private IWebDriver driver;
        private string email;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Navigate().GoToUrl("https://demo.nopcommerce.com/");
        }

        [TearDown]
        public void Cleanup()
        {
            driver.Quit();
        }

        private void GenerateUniqueUserData()
        {
            // Generate unique data for registration
            string uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            email = $"user_{uniqueId}@example.com";
        }

        private void RegisterNewUser()
        {
            driver.Navigate().GoToUrl("https://demo.nopcommerce.com/register");
            GenerateUniqueUserData();

            // Fill out the registration form
            driver.FindElement(By.Id("FirstName")).SendKeys("John");
            driver.FindElement(By.Id("LastName")).SendKeys("Doe");
            driver.FindElement(By.Id("Email")).SendKeys(email);
            driver.FindElement(By.Id("Password")).SendKeys("password");
            driver.FindElement(By.Id("ConfirmPassword")).SendKeys("password");

            // Click on the registration button
            driver.FindElement(By.Id("register-button")).Click();

            // Verify registration success message
            Assert.That(driver.PageSource.Contains("Your registration completed"));
        }

        private void Login()
        {
            driver.Navigate().GoToUrl("https://demo.nopcommerce.com/login");

            // Fill out the login form
            driver.FindElement(By.Id("Email")).SendKeys(email);
            driver.FindElement(By.Id("Password")).SendKeys("password");

            // Click on the login button
            driver.FindElement(By.CssSelector(".login-button")).Click();

            // Verify that we are logged in
            Assert.That(driver.PageSource.Contains("Welcome to our store"));
        }

        [Test]
        [Order(1)]
        public void RegisterAndLogin()
        {
            RegisterNewUser();
            Login();
        }

        [Test]
        [Order(2)]
        public void CheckoutProcess()
        {
            // Add a product to the cart
            driver.Navigate().GoToUrl("https://demo.nopcommerce.com/apple-macbook-pro-13-inch");
            driver.FindElement(By.Id("add-to-cart-button-4")).Click();

            // Click on the shopping cart link to proceed to checkout
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".cart-label"))).Click();

            // Check the terms of service checkbox
            IWebElement termsOfServiceCheckbox = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("#termsofservice")));
            if (!termsOfServiceCheckbox.Selected)
            {
                termsOfServiceCheckbox.Click();
            }

            // Click on the checkout button
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".checkout-button"))).Click();

            // Fill out the login form
            driver.FindElement(By.Id("Email")).SendKeys(email);
            driver.FindElement(By.Id("Password")).SendKeys("password");

            // Click on the login button
            driver.FindElement(By.CssSelector(".login-button")).Click();


            // Check the terms of service checkbox again (in case the page refreshed)
            termsOfServiceCheckbox = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("#termsofservice")));
            if (!termsOfServiceCheckbox.Selected)
            {
                termsOfServiceCheckbox.Click();
            }

            // Click on the checkout button again
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".checkout-button"))).Click();

            // Wait for the billing information form to be visible
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("BillingNewAddress_FirstName")));

            // Fill out the billing information form
            FillFieldIfEmpty(By.Id("BillingNewAddress_FirstName"), "John");
            FillFieldIfEmpty(By.Id("BillingNewAddress_LastName"), "Doe");
            FillFieldIfEmpty(By.Id("BillingNewAddress_Email"), email);
            driver.FindElement(By.Id("BillingNewAddress_CountryId")).SendKeys("Bosnia and Herzegowina");
            FillFieldIfEmpty(By.Id("BillingNewAddress_City"), "Sarajevo");
            FillFieldIfEmpty(By.Id("BillingNewAddress_Address1"), "Zmaja od Bosne");
            FillFieldIfEmpty(By.Id("BillingNewAddress_ZipPostalCode"), "10001");
            FillFieldIfEmpty(By.Id("BillingNewAddress_PhoneNumber"), "1234567890");

            // Click on the continue button
            driver.FindElement(By.CssSelector("#billing-buttons-container > button.button-1.new-address-next-step-button")).Click();

            // Click on the continue button for shipping
            driver.FindElement(By.CssSelector("#shipping-method-buttons-container > button")).Click();

            // Click on the continue button for payment method
            driver.FindElement(By.CssSelector("#payment-method-buttons-container > button")).Click();

            // Click on the continue button for payment information
            driver.FindElement(By.CssSelector("#payment-info-buttons-container > button")).Click();

            // Click on the continue button for payment confirmation
            driver.FindElement(By.CssSelector("#confirm-order-buttons-container > button")).Click();

            // Wait for checkout completion
            wait.Until(ExpectedConditions.UrlContains("checkout/completed"));

            // Verify that checkout has successfully passed
            Assert.That(driver.PageSource.Contains("Thank you"));

        }

        // Method to fill a field if it's empty
        private void FillFieldIfEmpty(By locator, string value)
        {
            IWebElement element = driver.FindElement(locator);
            if (string.IsNullOrEmpty(element.GetAttribute("value")))
            {
                element.SendKeys(value);
            }
        }

    }
}
