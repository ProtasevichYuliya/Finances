using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Finances.ApiModels;
using Finances.Models;
using Finances.UnitTests;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Finances.UnitTests
{
    public class AccountCRUDTests : TestServerFixture
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task TestGetAll()
        {
            var response = await this.Client.GetAsync("/api/accounts");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task TestRegister()
        {
            var model = new AccountCreateModel
            {
                LastName = "�������",
                FirstName = "�������",
                MiddleName = "����������",
                BirthDate = new DateTime(1987, 10, 26),
                Balance = 200
            };

            // Test account registration
            var requestContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await this.Client.PostAsync("api/accounts", requestContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var content = await response.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<Account>(content);
            Assert.That(account.LastName, Is.EqualTo("�������"));
            Assert.That(account.FirstName, Is.EqualTo("�������"));
            Assert.That(account.MiddleName, Is.EqualTo("����������"));
            Assert.That(account.BirthDate, Is.EqualTo(new DateTime(1987, 10, 26)));
            Assert.That(account.Balance, Is.EqualTo(200));

            // Test getting account by id
            response = await this.Client.GetAsync($"/api/accounts/{account.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            content = await response.Content.ReadAsStringAsync();
            account = JsonConvert.DeserializeObject<Account>(content);
            Assert.That(account.LastName, Is.EqualTo("�������"));
            Assert.That(account.FirstName, Is.EqualTo("�������"));
            Assert.That(account.MiddleName, Is.EqualTo("����������"));
            Assert.That(account.BirthDate, Is.EqualTo(new DateTime(1987, 10, 26)));
            Assert.That(account.Balance, Is.EqualTo(200));

            // Test deleting account
            response = await this.Client.DeleteAsync($"/api/accounts/{account.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            // Test getting removed account by id
            response = await this.Client.GetAsync($"/api/accounts/{account.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}