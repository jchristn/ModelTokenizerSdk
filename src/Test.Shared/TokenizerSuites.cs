namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using ModelTokenizerSdk;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering <see cref="ModelTokenizer"/> - construction, endpoint handling, argument
    /// validation, and graceful failure. None of these require the modeltokenizer microservice.
    /// </summary>
    public static partial class ModelTokenizerTestSuites
    {
        private const string TokenizerEndpointSuiteId = "Tokenizer.Endpoint";
        private const string TokenizerValidationSuiteId = "Tokenizer.Validation";
        private const string TokenizerConnectivitySuiteId = "Tokenizer.Connectivity";
        private const string TokenizerLiveSuiteId = "Tokenizer.LiveService";

        #region Endpoint

        /// <summary>
        /// Construction and endpoint-normalization behavior.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor TokenizerEndpointSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(TokenizerEndpointSuiteId, "DefaultEndpoint", "Default constructor uses localhost:8000", _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer())
                        TestAssert.Equal("http://localhost:8000/", tokenizer.Endpoint);
                    return Task.CompletedTask;
                }),

                Case(TokenizerEndpointSuiteId, "TrailingSlashAppended", "An endpoint without a trailing slash gets one appended", _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer("http://localhost:9000"))
                        TestAssert.Equal("http://localhost:9000/", tokenizer.Endpoint);
                    return Task.CompletedTask;
                }),

                Case(TokenizerEndpointSuiteId, "TrailingSlashPreserved", "An endpoint already ending in a slash is unchanged", _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer("http://localhost:9000/"))
                        TestAssert.Equal("http://localhost:9000/", tokenizer.Endpoint);
                    return Task.CompletedTask;
                }),

                Case(TokenizerEndpointSuiteId, "NullEndpointKeepsDefault", "Null constructor endpoint keeps the default", _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer(null))
                        TestAssert.Equal("http://localhost:8000/", tokenizer.Endpoint);
                    return Task.CompletedTask;
                }),

                Case(TokenizerEndpointSuiteId, "EmptyEndpointKeepsDefault", "Empty constructor endpoint keeps the default", _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer(string.Empty))
                        TestAssert.Equal("http://localhost:8000/", tokenizer.Endpoint);
                    return Task.CompletedTask;
                }),

                Case(TokenizerEndpointSuiteId, "CustomEndpointWithPath", "A custom endpoint with a path is normalized with a trailing slash", _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer("http://example.com:1234/api"))
                        TestAssert.Equal("http://example.com:1234/api/", tokenizer.Endpoint);
                    return Task.CompletedTask;
                }),

                Case(TokenizerEndpointSuiteId, "EndpointNullThrows", "Assigning a null endpoint throws", _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer())
                        TestAssert.Throws<ArgumentNullException>(() => tokenizer.Endpoint = null);
                    return Task.CompletedTask;
                }),

                Case(TokenizerEndpointSuiteId, "EndpointEmptyThrows", "Assigning an empty endpoint throws", _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer())
                        TestAssert.Throws<ArgumentNullException>(() => tokenizer.Endpoint = string.Empty);
                    return Task.CompletedTask;
                }),

                Case(TokenizerEndpointSuiteId, "EndpointInvalidUriThrows", "Assigning a malformed URI throws", _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer())
                        TestAssert.Throws<UriFormatException>(() => tokenizer.Endpoint = "not a valid uri");
                    return Task.CompletedTask;
                }),

                Case(TokenizerEndpointSuiteId, "HeaderHasDefault", "Header has its documented default value", _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer())
                        TestAssert.Equal("[ModelTokenizer] ", tokenizer.Header);
                    return Task.CompletedTask;
                }),

                Case(TokenizerEndpointSuiteId, "LoggerAssignable", "Logger can be assigned and retrieved", _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer())
                    {
                        Action<string> logger = _msg => { };
                        tokenizer.Logger = logger;
                        TestAssert.True(ReferenceEquals(logger, tokenizer.Logger), "Logger did not round-trip");
                    }
                    return Task.CompletedTask;
                }),

                Case(TokenizerEndpointSuiteId, "DisposeIsIdempotent", "Dispose can be called more than once without throwing", _ =>
                {
                    ModelTokenizer tokenizer = new ModelTokenizer();
                    tokenizer.Dispose();
                    tokenizer.Dispose();
                    return Task.CompletedTask;
                }),
            };

            return Suite(TokenizerEndpointSuiteId, "ModelTokenizer: Endpoint & Construction", cases);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Argument-validation behavior of the Tokenize / TokenizeBatch overloads.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor TokenizerValidationSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(TokenizerValidationSuiteId, "TokenizeNullRequest", "Tokenize(null request) throws", async _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer())
                        await TestAssert.ThrowsAsync<ArgumentNullException>(
                            () => tokenizer.Tokenize((TokenizationRequest)null));
                }),

                Case(TokenizerValidationSuiteId, "TokenizeEmptyText", "Tokenize(request with empty text) throws", async _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer())
                    {
                        TokenizationRequest req = new TokenizationRequest { Text = string.Empty, Model = "m" };
                        await TestAssert.ThrowsAsync<ArgumentNullException>(() => tokenizer.Tokenize(req));
                    }
                }),

                Case(TokenizerValidationSuiteId, "TokenizeNullTextOverload", "Tokenize(null text, model) throws", async _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer())
                        await TestAssert.ThrowsAsync<ArgumentNullException>(
                            () => tokenizer.Tokenize(null, "some-model"));
                }),

                Case(TokenizerValidationSuiteId, "TokenizeBatchNullRequest", "TokenizeBatch(null request) throws", async _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer())
                        await TestAssert.ThrowsAsync<ArgumentNullException>(
                            () => tokenizer.TokenizeBatch((TokenizationRequest)null));
                }),

                Case(TokenizerValidationSuiteId, "TokenizeBatchNullTexts", "TokenizeBatch(request with null texts) throws", async _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer())
                    {
                        TokenizationRequest req = new TokenizationRequest { Texts = null, Model = "m" };
                        await TestAssert.ThrowsAsync<ArgumentNullException>(() => tokenizer.TokenizeBatch(req));
                    }
                }),

                Case(TokenizerValidationSuiteId, "TokenizeBatchEmptyTexts", "TokenizeBatch(request with empty texts) throws", async _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer())
                    {
                        TokenizationRequest req = new TokenizationRequest { Texts = new List<string>(), Model = "m" };
                        await TestAssert.ThrowsAsync<ArgumentNullException>(() => tokenizer.TokenizeBatch(req));
                    }
                }),

                Case(TokenizerValidationSuiteId, "TokenizeBatchNullTextsOverload", "TokenizeBatch(null texts, model) throws", async _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer())
                        await TestAssert.ThrowsAsync<ArgumentNullException>(
                            () => tokenizer.TokenizeBatch((List<string>)null, "some-model"));
                }),
            };

            return Suite(TokenizerValidationSuiteId, "ModelTokenizer: Argument Validation", cases);
        }

        #endregion

        #region Connectivity

        /// <summary>
        /// Graceful-failure behavior when the service is unreachable.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor TokenizerConnectivitySuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(TokenizerConnectivitySuiteId, "UnreachableReturnsFalse", "ValidateConnectivity against an unreachable endpoint returns false", async _ =>
                {
                    using (ModelTokenizer tokenizer = new ModelTokenizer("http://localhost:1/"))
                    using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
                    {
                        bool connected = await tokenizer.ValidateConnectivity(cts.Token);
                        TestAssert.False(connected);
                    }
                }),

                Case(TokenizerConnectivitySuiteId, "FailureIsLogged", "A failed connectivity check emits at least one log message", async _ =>
                {
                    List<string> messages = new List<string>();
                    using (ModelTokenizer tokenizer = new ModelTokenizer("http://localhost:1/"))
                    using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
                    {
                        tokenizer.Logger = m => messages.Add(m);
                        await tokenizer.ValidateConnectivity(cts.Token);
                        TestAssert.True(messages.Count > 0, "Expected the logger to receive at least one message");
                    }
                }),
            };

            return Suite(TokenizerConnectivitySuiteId, "ModelTokenizer: Connectivity", cases);
        }

        #endregion

        #region Live-Service

        /// <summary>
        /// Tests that require a running modeltokenizer microservice. They are declared as skipped so
        /// they are visible (with reasons) in every runner without failing offline/CI builds. The
        /// interactive Test console application exercises these paths against a live endpoint.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor TokenizerLiveServiceSuite()
        {
            const string reason = "Requires a running modeltokenizer microservice (see the Test console app).";

            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                SkippedCase(TokenizerLiveSuiteId, "TokenizeSingle",
                    "POST /tokenize returns tokens for a single sentence", reason),
                SkippedCase(TokenizerLiveSuiteId, "TokenizeBatch",
                    "POST /tokenize returns results for a batch of sentences", reason),
                SkippedCase(TokenizerLiveSuiteId, "TokenizeWithChunking",
                    "POST /tokenize honors chunking parameters", reason),
                SkippedCase(TokenizerLiveSuiteId, "ValidateConnectivitySucceeds",
                    "HEAD / against a live service returns true", reason),
            };

            return Suite(TokenizerLiveSuiteId, "ModelTokenizer: Live Service (skipped)", cases);
        }

        #endregion
    }
}
