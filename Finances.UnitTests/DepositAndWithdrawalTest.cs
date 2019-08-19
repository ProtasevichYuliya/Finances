using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Finances.ApiModels;
using Finances.Models;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Finances.UnitTests
{
    public class DepositAndWithdrawalTest : TestServerFixture
    {
        private const int accountsNumber = 50;
        private const int threadsNumber = 10;
        private const int operationsPerListNumber = 1000;
        private const decimal initialBalance = 1000000;

        private List<Account> accounts = new List<Account>();
        private List<AccountOperation>[] accountOperationsLists = new List<AccountOperation>[threadsNumber];
        private Random rnd = new Random();

        [SetUp]
        public async Task Setup()
        {
            var model = new AccountCreateModel
            {
                BirthDate = new DateTime(1990, 1, 1),
                Balance = initialBalance
            };

            var lastName = "Фамилия";
            var firstName = "Имя";
            var middleName = "Отчество";

            // Creating accounts
            for (int i = 1; i <= accountsNumber; i++)
            {
                model.LastName = lastName + i.ToString();
                model.FirstName = firstName + i.ToString();
                model.MiddleName = middleName + i.ToString();

                var requestContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var response = await this.Client.PostAsync("api/accounts", requestContent);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

                var content = await response.Content.ReadAsStringAsync();
                var account = JsonConvert.DeserializeObject<Account>(content);
                this.accounts.Add(account);
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            // Delete accounts
            foreach (var account in accounts)
            {
                var response = await this.Client.DeleteAsync($"/api/accounts/{account.Id}");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            }
        }

        [Test]
        public async Task Test1()
        {
            // Arrange
            // Generate 10 lists with 1000 steps (operations) in each
            for (int i = 0; i < threadsNumber; i++)
            {
                this.accountOperationsLists[i] = this.GenerateOperationsList(operationsPerListNumber);
            }

            // Act
            // Sending requests in 10 threads
            Task[] tasks = new Task[threadsNumber];
            for (int i = 0; i < tasks.Length; i++)
            {
                var taskNumber = i;
                tasks[taskNumber] = await Task.Factory.StartNew(async () =>
                {
                    // Console.WriteLine($"Starting thread {taskNumber + 1}");
                    foreach (var operation in this.accountOperationsLists[taskNumber])
                    {
                        // Console.WriteLine($"Thread {taskNumber + 1}: operation {operation.OperationType}, sum {operation.Sum}, account {operation.Account.Id}");
                        var requestContent = new StringContent(JsonConvert.SerializeObject(operation.Sum),
                            Encoding.UTF8, "application/json");
                        var response = await this.Client.PostAsync(
                            $"/api/accounts/{operation.Account.Id}/{(operation.OperationType == AccountOperationType.Deposit ? "deposit" : "withdraw")}",
                            requestContent);

                        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
                    }
                }, TaskCreationOptions.LongRunning);
            }

            Task.WaitAll(tasks);

            // Assert
            foreach (var account in accounts)
            {
                var response = await this.Client.GetAsync($"/api/accounts/{account.Id}");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                var content = await response.Content.ReadAsStringAsync();
                var receivedAccount = JsonConvert.DeserializeObject<Account>(content);

                var singleAccountOperations = this.accountOperationsLists
                    .SelectMany(x => x)
                    .Where(x => x.Account.Id == account.Id)
                    .ToList();
                var expectedSum = this.CalculateExpectedSum(initialBalance, singleAccountOperations);
                Assert.That(receivedAccount.Balance, Is.EqualTo(expectedSum), $"Balance test failed for account {account.Id}!");
            }
        }

        private List<AccountOperation> GenerateOperationsList(int number)
        {
            var accountOperations = new List<AccountOperation>();
            for (int i = 0; i < number; i++)
            {
                var accountOperation = new AccountOperation
                {
                    Account = accounts[rnd.Next(accounts.Count)],
                    OperationType = rnd.Next(2) == 0 ? AccountOperationType.Deposit : AccountOperationType.Withdraw,
                    Sum = rnd.Next(1, 10) * 100
                };
                accountOperations.Add(accountOperation);
            }

            return accountOperations;
        }

        private decimal CalculateExpectedSum(decimal initialBalance, List<AccountOperation> singleAccountOperations)
        {
            var expectedSum = initialBalance
                              + singleAccountOperations
                                  .Where(x => x.OperationType == AccountOperationType.Deposit)
                                  .Sum(x => x.Sum)
                              - singleAccountOperations
                                  .Where(x => x.OperationType == AccountOperationType.Withdraw)
                                  .Sum(x => x.Sum);
            return expectedSum;
        }
    }
}
