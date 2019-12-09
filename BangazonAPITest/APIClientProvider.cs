using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using BangazonAPI;
using System.Net.Http;

namespace BangazonAPITest
{
    class APIClientProvider : IClassFixture<WebApplicationFactory<Startup>>
    {
        public HttpClient Client { get; private set; }
        private readonly WebApplicationFactory<Startup> _factory = new WebApplicationFactory<Startup>();

        public APIClientProvider()
        {
            Client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _factory?.Dispose();
            Client?.Dispose();
        }
    }
}
