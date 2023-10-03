using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenDDD.NET.Extensions;
using OpenDDD.Tests;
using MyBoundedContext.Domain.Model.Property;
using MyBoundedContext.Domain.Model.Site;
using MyBoundedContext.Infrastructure.Ports.Adapters.Site;
using MyBoundedContext.Main;
using MyBoundedContext.Vertical;

namespace MyBoundedContext.Tests
{
    public class BaseActionUnitTests : ActionUnitTestsBase
    {
        protected BaseActionUnitTests()
        {
            
        }

        // Setup
        
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var builder = WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .UseStartup<Startup>();

            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true);
            });
            
            ConfigureTestServices(builder);

            return builder;
        }
        
        // Fixtures
        
        protected List<Property> Properties = new List<Property>();
        protected Property Property => Properties.FirstOrDefault();
        protected List<Site> Sites = new List<Site>();
        protected Site Site => Sites.FirstOrDefault();

        protected async Task<Site> FixtureAddSiteAsync()
        {
            var siteId = SiteId.Create(EnumExtensions.GetDescriptionFromEnumValue(SiteId.Strings.ThailandProperty));
            var site = Site.Create(siteId, "Test Site", ActionId);
            
            await SiteRepository.SaveAsync(site, ActionId, CancellationToken.None);
            
            Sites.Add(site);
            
            return site;
        }

        protected async Task<Property> FixtureAddPropertyAsync(SiteId siteId)
        {
            var property =
                Property.Create(
                    PropertyId.Create(ActionDatabaseConnection.NextIdentity()),
                    "1 Bedroom Condo for rent at Life One Wireless",
                    "This property is a 35 SqM condo with 1 bedroom and 1 bathroom that is available for sale. It is part of the Life One Wireless project in Lumphini, Bangkok. You can rent this condo long term for ฿28,000 per month. and was completed in Apr 2020",
                    Price.Create(28000, Currency.Baht),
                    Location.Create("Pathum Wan, Bangkok", 10.1, 20.2),
                    PropertyType.Condo,
                    ActionId);
            
            await PropertyRepository.SaveAsync(property, ActionId, CancellationToken.None);
            
            Properties.Add(property);

            return property;
        }
        
        // Actions
        
        protected CrawlSearchPage.Action CrawlSearchPageAction => Scope.ServiceProvider.GetRequiredService<CrawlSearchPage.Action>();
        protected GetProperties.Action GetPropertiesAction => Scope.ServiceProvider.GetRequiredService<GetProperties.Action>();

        // Repositories
        
        protected IPropertyRepository PropertyRepository => Scope.ServiceProvider.GetRequiredService<IPropertyRepository>();
        protected ISiteRepository SiteRepository => Scope.ServiceProvider.GetRequiredService<ISiteRepository>();

        // Mocks

        protected HtmlDocument MockAllAdaptersSearchPageResponse { get; set; }

        private void ConfigureTestServices(IWebHostBuilder builder)
        {
            var mock = new Mock<IMockedSearchPage>();
            mock
                .Setup(x => x.GetResponse())
                .Returns(() => MockAllAdaptersSearchPageResponse);
            
            builder.ConfigureTestServices(services =>
            {
                services.AddTransient<IMockedSearchPage>(sp => mock.Object);
            });
        }
        
        // // Assertions
        //
        // protected void AssertEmailSent(Email toEmail)
        //     => AssertEmailSent(toEmail: toEmail, msgContains: null);
        //
        // protected void AssertEmailSent(string toEmail, string? msgContains)
        //     => AssertEmailSent(Email.Create(toEmail), msgContains);
        //
        // protected void AssertEmailSent(Email toEmail, string? msgContains)
        // {
        //     var subString = "";
        //     
        //     if (msgContains != null)
        //         subString = $" containing '{msgContains}'";
        //     
        //     Assert.True(
        //         EmailAdapter.HasSent(
        //             toEmail: toEmail.ToString(), 
        //             msgContains: msgContains),
        //         $"Expected an email{subString} to be sent to {toEmail}.");
        // }
        
        // Run actions

        // protected async Task<User> CreateAccount(
        //     string email = "test.testsson@poweriam.com",
        //     string password = "test-password",
        //     string firstName = "Test",
        //     string lastName = "Testsson")
        // {
        //     var command = new CreateAccountCommand
        //     {
        //         FirstName = firstName,
        //         LastName = lastName,
        //         Email = Email.Create(email),
        //         Password = password,
        //         RepeatPassword = password
        //     };
        //
        //     var user = await CreateAccountAction.ExecuteAsync(command, ActionId, CancellationToken.None);
        //
        //     Users.Add(user);
        //
        //     return user;
        // }
        //
        // protected async Task<User> CreateAccount_SendEmailVerificationEmail(
        //     string email = "test.testsson@poweriam.com",
        //     string password = "test-password",
        //     string firstName = "Test",
        //     string lastName = "Testsson")
        // {
        //     var user = await CreateAccount(email: email, password: password, firstName: firstName, lastName: lastName);
        //     await SendEmailVerificationEmail(user.UserId);
        //     return user;
        // }
        //
        // protected async Task<User> CreateAccount_SendEmailVerificationEmail_VerifyEmail(
        //     string email = "test.testsson@poweriam.com",
        //     string password = "test-password",
        //     string firstName = "Test",
        //     string lastName = "Testsson")
        // {
        //     var user = await CreateAccount_SendEmailVerificationEmail(email, password, firstName, lastName);
        //     user = await RefreshAsync(user);
        //     await VerifyEmail(code: user.EmailVerification.Code);
        //     user = await RefreshAsync(user);
        //     return user;
        // }

        // Helpers

        protected HtmlDocument GetHtmlDocument(string relativePath)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(GetFilePath(relativePath));
            return htmlDoc;
        }
    }
}
