﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Testing;
using Azure.Identity.Tests.Mock;
using NUnit.Framework;

namespace Azure.Identity.Tests
{
    public class ManagedIdentityClientTests : ClientTestBase
    {
        [SetUp]
        public void ResetManagedIdenityClient()
        {
            typeof(ManagedIdentityClient).GetField("s_msiType", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, 0);
            typeof(ManagedIdentityClient).GetField("s_endpoint", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
        }

        public ManagedIdentityClientTests(bool isAsync) : base(isAsync)
        {
        }

        [NonParallelizable]
        [Test]
        public async Task VerifyImdsRequestMockAsync()
        {
            using (new TestEnvVar("MSI_ENDPOINT", null))
            using (new TestEnvVar("MSI_SECRET", null))
            {
                var response = new MockResponse(200);

                var expectedToken = "mock-msi-access-token";

                response.SetContent($"{{ \"access_token\": \"{expectedToken}\", \"expires_on\": \"3600\" }}");

                var mockTransport = new MockTransport(response, response);

                var options = new IdentityClientOptions() { Transport = mockTransport };

                ManagedIdentityClient client = InstrumentClient(new ManagedIdentityClient(options));

                AccessToken actualToken = await client.AuthenticateAsync(MockScopes.Default);

                Assert.AreEqual(expectedToken, actualToken.Token);

                MockRequest request = mockTransport.Requests[mockTransport.Requests.Count - 1];

                string query = request.UriBuilder.Query;

                Assert.IsTrue(query.Contains("api-version=2018-02-01"));

                Assert.IsTrue(query.Contains($"resource={Uri.EscapeDataString(ScopeUtilities.ScopesToResource(MockScopes.Default))}"));

                Assert.IsTrue(request.Headers.TryGetValue("Metadata", out string metadataValue));

                Assert.AreEqual("true", metadataValue);
            }
        }

        [NonParallelizable]
        [Test]
        public async Task VerifyImdsRequestWithClientIdMockAsync()
        {
            using (new TestEnvVar("MSI_ENDPOINT", null))
            using (new TestEnvVar("MSI_SECRET", null))
            {
                var response = new MockResponse(200);

                var expectedToken = "mock-msi-access-token";

                response.SetContent($"{{ \"access_token\": \"{expectedToken}\", \"expires_on\": \"3600\" }}");

                var mockTransport = new MockTransport(response, response);

                var options = new IdentityClientOptions() { Transport = mockTransport };

                ManagedIdentityClient client = InstrumentClient(new ManagedIdentityClient(options));

                AccessToken actualToken = await client.AuthenticateAsync(MockScopes.Default, clientId: "mock-client-id");

                Assert.AreEqual(expectedToken, actualToken.Token);

                MockRequest request = mockTransport.Requests[mockTransport.Requests.Count - 1];

                string query = request.UriBuilder.Query;

                Assert.IsTrue(query.Contains("api-version=2018-02-01"));

                Assert.IsTrue(query.Contains($"resource={Uri.EscapeDataString(ScopeUtilities.ScopesToResource(MockScopes.Default))}"));

                Assert.IsTrue(query.Contains($"client_id=mock-client-id"));

                Assert.IsTrue(request.Headers.TryGetValue("Metadata", out string metadataValue));

                Assert.AreEqual("true", metadataValue);
            }
        }

        [NonParallelizable]
        [Test]
        public async Task VerifyAppServiceMsiRequestMockAsync()
        {
            using (new TestEnvVar("MSI_ENDPOINT", "https://mock.msi.endpoint/"))
            using (new TestEnvVar("MSI_SECRET", "mock-msi-secret"))
            {
                var response = new MockResponse(200);

                var expectedToken = "mock-msi-access-token";

                response.SetContent($"{{ \"access_token\": \"{expectedToken}\", \"expires_on\": \"{DateTimeOffset.UtcNow.ToString()}\" }}");

                var mockTransport = new MockTransport(response);

                var options = new IdentityClientOptions() { Transport = mockTransport };

                ManagedIdentityClient client = InstrumentClient(new ManagedIdentityClient(options));

                AccessToken actualToken = await client.AuthenticateAsync(MockScopes.Default);

                Assert.AreEqual(expectedToken, actualToken.Token);

                MockRequest request = mockTransport.Requests[0];

                Assert.IsTrue(request.UriBuilder.ToString().StartsWith("https://mock.msi.endpoint/"));

                string query = request.UriBuilder.Query;

                Assert.IsTrue(query.Contains("api-version=2017-09-01"));

                Assert.IsTrue(query.Contains($"resource={Uri.EscapeDataString(ScopeUtilities.ScopesToResource(MockScopes.Default))}"));

                Assert.IsTrue(request.Headers.TryGetValue("secret", out string actSecretValue));

                Assert.AreEqual("mock-msi-secret", actSecretValue);
            }
        }

        [NonParallelizable]
        [Test]
        public async Task VerifyAppServiceMsiRequestWithClientIdMockAsync()
        {
            using (new TestEnvVar("MSI_ENDPOINT", "https://mock.msi.endpoint/"))
            using (new TestEnvVar("MSI_SECRET", "mock-msi-secret"))
            {
                var response = new MockResponse(200);

                var expectedToken = "mock-msi-access-token";

                response.SetContent($"{{ \"access_token\": \"{expectedToken}\", \"expires_on\": \"{DateTimeOffset.UtcNow.ToString()}\" }}");

                var mockTransport = new MockTransport(response);

                var options = new IdentityClientOptions() { Transport = mockTransport };

                ManagedIdentityClient client = InstrumentClient(new ManagedIdentityClient(options));

                AccessToken actualToken = await client.AuthenticateAsync(MockScopes.Default, "mock-client-id");

                Assert.AreEqual(expectedToken, actualToken.Token);

                MockRequest request = mockTransport.Requests[0];

                Assert.IsTrue(request.UriBuilder.ToString().StartsWith("https://mock.msi.endpoint/"));

                string query = request.UriBuilder.Query;

                Assert.IsTrue(query.Contains("api-version=2017-09-01"));

                Assert.IsTrue(query.Contains($"resource={Uri.EscapeDataString(ScopeUtilities.ScopesToResource(MockScopes.Default))}"));

                Assert.IsTrue(query.Contains($"client_id=mock-client-id"));

                Assert.IsTrue(request.Headers.TryGetValue("secret", out string actSecretValue));

                Assert.AreEqual("mock-msi-secret", actSecretValue);
            }
        }

        [NonParallelizable]
        [Test]
        public async Task VerifyCloudShellMsiRequestMockAsync()
        {
            using (new TestEnvVar("MSI_ENDPOINT", "https://mock.msi.endpoint/"))
            using (new TestEnvVar("MSI_SECRET", null))
            {
                var response = new MockResponse(200);

                var expectedToken = "mock-msi-access-token";

                response.SetContent($"{{ \"access_token\": \"{expectedToken}\", \"expires_on\": {(DateTimeOffset.UtcNow + TimeSpan.FromSeconds(3600)).ToUnixTimeSeconds()} }}");

                var mockTransport = new MockTransport(response);

                var options = new IdentityClientOptions() { Transport = mockTransport };

                ManagedIdentityClient client = InstrumentClient(new ManagedIdentityClient(options));

                AccessToken actualToken = await client.AuthenticateAsync(MockScopes.Default);

                Assert.AreEqual(expectedToken, actualToken.Token);

                MockRequest request = mockTransport.Requests[0];

                Assert.IsTrue(request.UriBuilder.ToString().StartsWith("https://mock.msi.endpoint/"));

                Assert.IsTrue(request.Content.TryComputeLength(out long contentLen));

                var content = new byte[contentLen];

                MemoryStream contentBuff = new MemoryStream(content);

                request.Content.WriteTo(contentBuff, default);

                string body = Encoding.UTF8.GetString(content);

                Assert.IsTrue(body.Contains($"resource={Uri.EscapeDataString(ScopeUtilities.ScopesToResource(MockScopes.Default))}"));

                Assert.IsTrue(request.Headers.TryGetValue("Metadata", out string actMetadata));

                Assert.AreEqual("true", actMetadata);
            }
        }

        [NonParallelizable]
        [Test]
        public async Task VerifyCloudShellMsiRequestWithClientIdMockAsync()
        {
            using (new TestEnvVar("MSI_ENDPOINT", "https://mock.msi.endpoint/"))
            using (new TestEnvVar("MSI_SECRET", null))
            {
                var response = new MockResponse(200);

                var expectedToken = "mock-msi-access-token";

                response.SetContent($"{{ \"access_token\": \"{expectedToken}\", \"expires_on\": {(DateTimeOffset.UtcNow + TimeSpan.FromSeconds(3600)).ToUnixTimeSeconds()} }}");

                var mockTransport = new MockTransport(response);

                var options = new IdentityClientOptions() { Transport = mockTransport };

                ManagedIdentityClient client = InstrumentClient(new ManagedIdentityClient(options));

                AccessToken actualToken = await client.AuthenticateAsync(MockScopes.Default, "mock-client-id");

                Assert.AreEqual(expectedToken, actualToken.Token);

                MockRequest request = mockTransport.Requests[0];

                Assert.IsTrue(request.UriBuilder.ToString().StartsWith("https://mock.msi.endpoint/"));

                Assert.IsTrue(request.Content.TryComputeLength(out long contentLen));

                var content = new byte[contentLen];

                MemoryStream contentBuff = new MemoryStream(content);

                request.Content.WriteTo(contentBuff, default);

                string body = Encoding.UTF8.GetString(content);

                Assert.IsTrue(body.Contains($"resource={Uri.EscapeDataString(ScopeUtilities.ScopesToResource(MockScopes.Default))}"));

                Assert.IsTrue(body.Contains($"client_id=mock-client-id"));

                Assert.IsTrue(request.Headers.TryGetValue("Metadata", out string actMetadata));

                Assert.AreEqual("true", actMetadata);
            }
        }
    }
}
