namespace ModelTokenizerSdk.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using ModelTokenizerSdk;
    using Xunit;

    /// <summary>
    /// Tests for <see cref="ModelTokenizer"/>. The argument-validation and endpoint tests
    /// require no service; the connectivity test only asserts graceful failure against an
    /// unreachable endpoint.
    /// </summary>
    public class ModelTokenizerTests
    {
        #region Positive-Cases

        [Fact]
        public void Constructor_Default_UsesLocalhostEndpoint()
        {
            using (ModelTokenizer tokenizer = new ModelTokenizer())
            {
                Assert.Equal("http://localhost:8000/", tokenizer.Endpoint);
            }
        }

        [Fact]
        public void Endpoint_WithoutTrailingSlash_GetsSlashAppended()
        {
            using (ModelTokenizer tokenizer = new ModelTokenizer("http://localhost:9000"))
            {
                Assert.Equal("http://localhost:9000/", tokenizer.Endpoint);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_NullOrEmptyEndpoint_KeepsDefault(string endpoint)
        {
            using (ModelTokenizer tokenizer = new ModelTokenizer(endpoint))
            {
                Assert.Equal("http://localhost:8000/", tokenizer.Endpoint);
            }
        }

        [Fact]
        public async Task ValidateConnectivity_UnreachableEndpoint_ReturnsFalse()
        {
            using (ModelTokenizer tokenizer = new ModelTokenizer("http://localhost:1/"))
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
            {
                bool connected = await tokenizer.ValidateConnectivity(cts.Token);
                Assert.False(connected);
            }
        }

        #endregion

        #region Negative-Cases

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Endpoint_NullOrEmpty_Throws(string endpoint)
        {
            using (ModelTokenizer tokenizer = new ModelTokenizer())
            {
                Assert.Throws<ArgumentNullException>(() => tokenizer.Endpoint = endpoint);
            }
        }

        [Fact]
        public void Endpoint_InvalidUri_Throws()
        {
            using (ModelTokenizer tokenizer = new ModelTokenizer())
            {
                Assert.Throws<UriFormatException>(() => tokenizer.Endpoint = "not a valid uri");
            }
        }

        [Fact]
        public async Task Tokenize_NullRequest_Throws()
        {
            using (ModelTokenizer tokenizer = new ModelTokenizer())
            {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => tokenizer.Tokenize((TokenizationRequest)null));
            }
        }

        [Fact]
        public async Task Tokenize_EmptyText_Throws()
        {
            using (ModelTokenizer tokenizer = new ModelTokenizer())
            {
                TokenizationRequest req = new TokenizationRequest { Text = "", Model = "m" };
                await Assert.ThrowsAsync<ArgumentNullException>(() => tokenizer.Tokenize(req));
            }
        }

        [Fact]
        public async Task Tokenize_NullTextConvenienceOverload_Throws()
        {
            using (ModelTokenizer tokenizer = new ModelTokenizer())
            {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => tokenizer.Tokenize(null, "some-model"));
            }
        }

        [Fact]
        public async Task TokenizeBatch_NullRequest_Throws()
        {
            using (ModelTokenizer tokenizer = new ModelTokenizer())
            {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => tokenizer.TokenizeBatch((TokenizationRequest)null));
            }
        }

        [Fact]
        public async Task TokenizeBatch_NullTexts_Throws()
        {
            using (ModelTokenizer tokenizer = new ModelTokenizer())
            {
                TokenizationRequest req = new TokenizationRequest { Texts = null, Model = "m" };
                await Assert.ThrowsAsync<ArgumentNullException>(() => tokenizer.TokenizeBatch(req));
            }
        }

        [Fact]
        public async Task TokenizeBatch_EmptyTexts_Throws()
        {
            using (ModelTokenizer tokenizer = new ModelTokenizer())
            {
                TokenizationRequest req = new TokenizationRequest { Texts = new List<string>(), Model = "m" };
                await Assert.ThrowsAsync<ArgumentNullException>(() => tokenizer.TokenizeBatch(req));
            }
        }

        [Fact]
        public async Task TokenizeBatch_NullTextsConvenienceOverload_Throws()
        {
            using (ModelTokenizer tokenizer = new ModelTokenizer())
            {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => tokenizer.TokenizeBatch((List<string>)null, "some-model"));
            }
        }

        #endregion
    }
}
