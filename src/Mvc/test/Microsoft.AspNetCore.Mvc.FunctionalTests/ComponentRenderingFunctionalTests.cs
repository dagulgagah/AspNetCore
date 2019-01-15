// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using BasicWebSite.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class ComponentRenderingFunctionalTests : IClassFixture<MvcTestFixture<BasicWebSite.Startup>>
    {
        public ComponentRenderingFunctionalTests(MvcTestFixture<BasicWebSite.Startup> fixture)
        {
            Client = fixture.WithWebHostBuilder(b => b.ConfigureServices(ConfigureServices)).CreateClient();
            LoopHandler.InnerHandler = fixture.Server.CreateHandler();

            void ConfigureServices(IServiceCollection services) =>
                services.AddSingleton(new WeatherForecastService(new HttpClient(LoopHandler)
                {
                    BaseAddress = fixture.ClientOptions.BaseAddress
                }));
        }

        private class LoopHttpHandler : DelegatingHandler
        {
        }

        public HttpClient Client { get; }

        private LoopHttpHandler LoopHandler { get; set; } = new LoopHttpHandler();

        [Fact]
        public async Task Renders_BasicComponent()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/components");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();

            AssertComponent("\n    <p>Hello world!</p>\n", "Greetings", content);
        }

        [Fact]
        public async Task Renders_AsyncComponent()
        {
            // Arrange & Act
            var expectedHtml = string.Join(
                "",
                "\n    <h1>Weather forecast</h1>",
                "\n",
                "\n<p>This component demonstrates fetching data from the server.</p>",
                "\n",
                "\n    <p>Weather data for 01/15/2019</p>\r",
                "\n    <table class=\"table\">\r",
                "\n        <thead>",
                "\n            <tr>",
                "\n                <th>Date</th>",
                "\n                <th>Temp. (C)</th>",
                "\n                <th>Temp. (F)</th>",
                "\n                <th>Summary</th>",
                "\n            </tr>",
                "\n        </thead>",
                "\n        <tbody>\r",
                "\n                <tr>\r",
                "\n                    <td>06/05/2018</td>\r",
                "\n                    <td>1</td>\r",
                "\n                    <td>33</td>\r",
                "\n                    <td>Freezing</td>\r",
                "\n                </tr>\r",
                "\n                <tr>\r",
                "\n                    <td>07/05/2018</td>\r",
                "\n                    <td>14</td>\r",
                "\n                    <td>57</td>\r",
                "\n                    <td>Bracing</td>\r",
                "\n                </tr>\r",
                "\n                <tr>\r",
                "\n                    <td>08/05/2018</td>\r",
                "\n                    <td>-13</td>\r",
                "\n                    <td>9</td>\r",
                "\n                    <td>Freezing</td>\r",
                "\n                </tr>\r",
                "\n                <tr>\r",
                "\n                    <td>09/05/2018</td>\r",
                "\n                    <td>-16</td>\r",
                "\n                    <td>4</td>\r",
                "\n                    <td>Balmy</td>\r",
                "\n                </tr>\r",
                "\n                <tr>\r",
                "\n                    <td>10/05/2018</td>\r",
                "\n                    <td>2</td>\r",
                "\n                    <td>29</td>\r",
                "\n                    <td>Chilly</td>\r",
                "\n                </tr>\r",
                "\n        </tbody>\r",
                "\n    </table>\r",
                "\n",
                "\n");

            var response = await Client.GetAsync("http://localhost/components");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();

            AssertComponent(expectedHtml, "FetchData", content);
        }


        private void AssertComponent(string expectedConent, string divId, string responseContent)
        {
            var parser = new HtmlParser();
            var htmlDocument = parser.Parse(responseContent);
            var div = htmlDocument.Body.QuerySelector($"#{divId}");
            Assert.Equal(expectedConent, div.InnerHtml);
        }
    }
}
