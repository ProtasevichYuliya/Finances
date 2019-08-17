using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace Finances.UnitTests
{
    public class TestServerFixture : IDisposable
    {
        private readonly TestServer testServer;
        public HttpClient Client { get; }

        public TestServerFixture()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var builder = new WebHostBuilder()
                //"C:\\Users\\Yulya\\source\\repos\\Finances\\Finances"
                .UseContentRoot(this.GetContentRootPath()) 
                .UseEnvironment("Development")
                .ConfigureAppConfiguration(config => config.AddJsonFile("appsettings.json"))
                .UseStartup<Startup>(); // Uses Start up class from your API Host project to configure the test server

            this.testServer = new TestServer(builder);
            Client = this.testServer.CreateClient();

        }

        private string GetContentRootPath()
        {
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(Directory.GetCurrentDirectory(),
                @"..\..\..\..\Finances"));
        }

        public void Dispose()
        {
            Client.Dispose();
            this.testServer.Dispose();
        }
    }
}
