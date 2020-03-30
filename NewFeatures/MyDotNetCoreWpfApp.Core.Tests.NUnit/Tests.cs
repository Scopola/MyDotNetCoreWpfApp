﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyDotNetCoreWpfApp.Core.Contracts.Services;
using MyDotNetCoreWpfApp.Core.Services;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MyDotNetCoreWpfApp.Core.Tests.NUnit
{
    // TODO WTS: Add appropriate unit tests.
    public class Tests
    {
        private IHost _host;

        public Tests()
        {
            var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            _host = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration(c => c.SetBasePath(appLocation))
                    .ConfigureServices(ConfigureServices)
                    .Build();
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddHttpClient("msgraph", client =>
            {
                client.BaseAddress = new System.Uri("https://graph.microsoft.com/v1.0/");
            });

            // Core Services
            services.AddTransient<ISampleDataService, SampleDataService>();
            services.AddTransient<IHttpDataService, HttpDataService>();
            services.AddSingleton<IMicrosoftGraphService, MicrosoftGraphService>();
            services.AddSingleton<IIdentityService, IdentityService>();
            services.AddSingleton<IFileService, FileService>();
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [Test]
        public void TestFileService()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folderPath = Path.Combine(localAppData, "UnitTests");
            var fileService = _host.Services.GetService(typeof(IFileService)) as IFileService;
            var fileName = "Tests.json";
            fileService.Save(folderPath, fileName, "Lorem ipsum dolor sit amet");
            var cacheData = fileService.Read<string>(folderPath, fileName);
            Assert.AreEqual("Lorem ipsum dolor sit amet", cacheData);
        }

        [Test]
        public async Task TestHttpDataService()
        {
            var httpDataService = _host.Services.GetService(typeof(IHttpDataService)) as IHttpDataService;
            httpDataService.Initialize("testClient", "https://postman-echo.com/");
            var result = await httpDataService.GetAsync<JObject>("get");
            Assert.IsNotNull(result);
        }

        // TODO WTS: Remove or update this once your app is using real data and not the SampleDataService.
        // This test serves only as a demonstration of testing functionality in the Core library.
        [Test]
        public async Task EnsureSampleDataServiceReturnsWebApiSampleDataAsync()
        {
            var sampleDataService = _host.Services.GetService(typeof(ISampleDataService)) as ISampleDataService;
            var actual = await sampleDataService.GetWebApiSampleDataAsync();

            Assert.AreNotEqual(0, actual.Count());
        }
    }
}
