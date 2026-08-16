namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;

    using ModelTokenizerSdk;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering the data models (<see cref="TokenizationRequest"/>,
    /// <see cref="TokenizationResult"/>, <see cref="BatchTokenizationResult"/>) and the JSON
    /// contract those models expose to the microservice.
    /// </summary>
    public static partial class ModelTokenizerTestSuites
    {
        private const string RequestSuiteId = "Model.TokenizationRequest";
        private const string ResultSuiteId = "Model.TokenizationResult";
        private const string BatchSuiteId = "Model.BatchTokenizationResult";
        private const string SerializationSuiteId = "Model.Serialization";

        #region TokenizationRequest

        /// <summary>
        /// Property validation and defaults on <see cref="TokenizationRequest"/>.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor TokenizationRequestSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(RequestSuiteId, "ValidValuesAccepted", "Valid numeric values are accepted", _ =>
                {
                    TokenizationRequest req = new TokenizationRequest
                    {
                        Text = "hello",
                        Model = "some-model",
                        MaxChunkLength = 100,
                        MaxTokensPerChunk = 10,
                        TokenOverlap = 2
                    };

                    TestAssert.Equal(100, req.MaxChunkLength);
                    TestAssert.Equal(10, req.MaxTokensPerChunk);
                    TestAssert.Equal(2, req.TokenOverlap);
                    return Task.CompletedTask;
                }),

                Case(RequestSuiteId, "NullNumericValuesAccepted", "Null numeric values are accepted", _ =>
                {
                    TokenizationRequest req = new TokenizationRequest
                    {
                        MaxChunkLength = null,
                        MaxTokensPerChunk = null,
                        TokenOverlap = null
                    };

                    TestAssert.Null(req.MaxChunkLength);
                    TestAssert.Null(req.MaxTokensPerChunk);
                    TestAssert.Null(req.TokenOverlap);
                    return Task.CompletedTask;
                }),

                Case(RequestSuiteId, "DefaultsAreNull", "A fresh request has null defaults", _ =>
                {
                    TokenizationRequest req = new TokenizationRequest();

                    TestAssert.Null(req.Text);
                    TestAssert.Null(req.Texts);
                    TestAssert.Null(req.Model);
                    TestAssert.Null(req.HuggingFaceApiKey);
                    TestAssert.Null(req.MaxChunkLength);
                    TestAssert.Null(req.MaxTokensPerChunk);
                    TestAssert.Null(req.TokenOverlap);
                    return Task.CompletedTask;
                }),

                Case(RequestSuiteId, "MaxChunkLengthBelowOneThrows", "MaxChunkLength below 1 throws", _ =>
                {
                    TokenizationRequest req = new TokenizationRequest();
                    TestAssert.Throws<ArgumentOutOfRangeException>(() => req.MaxChunkLength = 0);
                    TestAssert.Throws<ArgumentOutOfRangeException>(() => req.MaxChunkLength = -1);
                    return Task.CompletedTask;
                }),

                Case(RequestSuiteId, "MaxTokensPerChunkBelowOneThrows", "MaxTokensPerChunk below 1 throws", _ =>
                {
                    TokenizationRequest req = new TokenizationRequest();
                    TestAssert.Throws<ArgumentOutOfRangeException>(() => req.MaxTokensPerChunk = 0);
                    TestAssert.Throws<ArgumentOutOfRangeException>(() => req.MaxTokensPerChunk = -1);
                    return Task.CompletedTask;
                }),

                Case(RequestSuiteId, "TokenOverlapBelowOneThrows", "TokenOverlap below 1 throws", _ =>
                {
                    TokenizationRequest req = new TokenizationRequest();
                    TestAssert.Throws<ArgumentOutOfRangeException>(() => req.TokenOverlap = 0);
                    TestAssert.Throws<ArgumentOutOfRangeException>(() => req.TokenOverlap = -1);
                    return Task.CompletedTask;
                }),

                Case(RequestSuiteId, "TextAndTextsCoexist", "Text and Texts can both be assigned independently", _ =>
                {
                    TokenizationRequest req = new TokenizationRequest
                    {
                        Text = "single",
                        Texts = new List<string> { "one", "two" }
                    };

                    TestAssert.Equal("single", req.Text);
                    TestAssert.Equal(2, req.Texts.Count);
                    return Task.CompletedTask;
                }),
            };

            return Suite(RequestSuiteId, "TokenizationRequest", cases);
        }

        #endregion

        #region TokenizationResult

        /// <summary>
        /// Defaults and coercion behavior of <see cref="TokenizationResult"/>.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor TokenizationResultSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(ResultSuiteId, "DefaultsAreInitialized", "Tokens and Chunks default to empty lists", _ =>
                {
                    TokenizationResult result = new TokenizationResult();

                    TestAssert.NotNull(result.Tokens);
                    TestAssert.Empty(result.Tokens);
                    TestAssert.NotNull(result.Chunks);
                    TestAssert.Empty(result.Chunks);
                    TestAssert.Null(result.Text);
                    TestAssert.Null(result.SHA256);
                    TestAssert.Null(result.TokenCount);
                    return Task.CompletedTask;
                }),

                Case(ResultSuiteId, "ChunksNullCoercedToEmpty", "Assigning null Chunks coerces to an empty list", _ =>
                {
                    TokenizationResult result = new TokenizationResult();
                    result.Chunks = null;

                    TestAssert.NotNull(result.Chunks);
                    TestAssert.Empty(result.Chunks);
                    return Task.CompletedTask;
                }),

                Case(ResultSuiteId, "PropertiesRoundTrip", "All properties round-trip their assigned values", _ =>
                {
                    TokenizationResult result = new TokenizationResult
                    {
                        Text = "hello",
                        SHA256 = "abc",
                        Tokens = new List<string> { "hello" },
                        TokenCount = 1,
                        TokenIndexStart = 0,
                        TokenIndexEnd = 0
                    };

                    TestAssert.Equal("hello", result.Text);
                    TestAssert.Equal("abc", result.SHA256);
                    TestAssert.Single(result.Tokens);
                    TestAssert.Equal(1, result.TokenCount);
                    TestAssert.Equal(0, result.TokenIndexStart);
                    TestAssert.Equal(0, result.TokenIndexEnd);
                    return Task.CompletedTask;
                }),

                Case(ResultSuiteId, "NestedChunksSupported", "Chunks can hold nested TokenizationResult instances", _ =>
                {
                    TokenizationResult result = new TokenizationResult
                    {
                        Chunks = new List<TokenizationResult>
                        {
                            new TokenizationResult { Text = "chunk-a", TokenCount = 1 }
                        }
                    };

                    TestAssert.Single(result.Chunks);
                    TestAssert.Equal("chunk-a", result.Chunks[0].Text);
                    return Task.CompletedTask;
                }),
            };

            return Suite(ResultSuiteId, "TokenizationResult", cases);
        }

        #endregion

        #region BatchTokenizationResult

        /// <summary>
        /// Defaults of <see cref="BatchTokenizationResult"/>.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor BatchTokenizationResultSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(BatchSuiteId, "DefaultResultsEmpty", "Results defaults to an empty list", _ =>
                {
                    BatchTokenizationResult batch = new BatchTokenizationResult();

                    TestAssert.NotNull(batch.Results);
                    TestAssert.Empty(batch.Results);
                    return Task.CompletedTask;
                }),

                Case(BatchSuiteId, "ResultsAreAppendable", "Results can be appended to", _ =>
                {
                    BatchTokenizationResult batch = new BatchTokenizationResult();
                    batch.Results.Add(new TokenizationResult { Text = "x", TokenCount = 1 });
                    batch.Results.Add(new TokenizationResult { Text = "y", TokenCount = 1 });

                    TestAssert.Equal(2, batch.Results.Count);
                    TestAssert.Equal("x", batch.Results[0].Text);
                    return Task.CompletedTask;
                }),
            };

            return Suite(BatchSuiteId, "BatchTokenizationResult", cases);
        }

        #endregion

        #region Serialization

        /// <summary>
        /// The JSON contract that the models present to the microservice (snake_case names).
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor SerializationSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(SerializationSuiteId, "RequestUsesSnakeCaseNames", "TokenizationRequest serializes with the documented JSON property names", _ =>
                {
                    TokenizationRequest req = new TokenizationRequest
                    {
                        Model = "m",
                        Text = "t",
                        HuggingFaceApiKey = "k",
                        MaxChunkLength = 5,
                        MaxTokensPerChunk = 3,
                        TokenOverlap = 1
                    };

                    string json = JsonSerializer.Serialize(req);

                    TestAssert.Contains("\"model\":\"m\"", json);
                    TestAssert.Contains("\"text\":\"t\"", json);
                    TestAssert.Contains("\"huggingface_api_key\":\"k\"", json);
                    TestAssert.Contains("\"max_chunk_length\":5", json);
                    TestAssert.Contains("\"max_tokens_per_chunk\":3", json);
                    TestAssert.Contains("\"token_overlap\":1", json);
                    return Task.CompletedTask;
                }),

                Case(SerializationSuiteId, "RequestBatchTextsName", "TokenizationRequest serializes Texts as 'texts'", _ =>
                {
                    TokenizationRequest req = new TokenizationRequest
                    {
                        Model = "m",
                        Texts = new List<string> { "a", "b" }
                    };

                    string json = JsonSerializer.Serialize(req);

                    TestAssert.Contains("\"texts\":[", json);
                    return Task.CompletedTask;
                }),

                Case(SerializationSuiteId, "ResultDeserializesServicePayload", "TokenizationResult deserializes a service-style payload", _ =>
                {
                    string json =
                        "{\"text\":\"hi there\",\"sha256\":\"ABCDEF\",\"tokens\":[\"hi\",\"there\"]," +
                        "\"token_count\":2,\"token_index_start\":0,\"token_index_end\":1,\"chunks\":[]}";

                    TokenizationResult result = JsonSerializer.Deserialize<TokenizationResult>(json);

                    TestAssert.NotNull(result);
                    TestAssert.Equal("hi there", result.Text);
                    TestAssert.Equal("ABCDEF", result.SHA256);
                    TestAssert.Equal(2, result.Tokens.Count);
                    TestAssert.Equal(2, result.TokenCount);
                    TestAssert.Equal(0, result.TokenIndexStart);
                    TestAssert.Equal(1, result.TokenIndexEnd);
                    TestAssert.Empty(result.Chunks);
                    return Task.CompletedTask;
                }),

                Case(SerializationSuiteId, "ResultDeserializesNestedChunks", "TokenizationResult deserializes nested chunks", _ =>
                {
                    string json =
                        "{\"text\":\"a b\",\"tokens\":[\"a\",\"b\"],\"token_count\":2," +
                        "\"chunks\":[{\"text\":\"a\",\"token_count\":1},{\"text\":\"b\",\"token_count\":1}]}";

                    TokenizationResult result = JsonSerializer.Deserialize<TokenizationResult>(json);

                    TestAssert.Equal(2, result.Chunks.Count);
                    TestAssert.Equal("a", result.Chunks[0].Text);
                    TestAssert.Equal(1, result.Chunks[1].TokenCount);
                    return Task.CompletedTask;
                }),

                Case(SerializationSuiteId, "ResultNullChunksCoercedOnDeserialize", "A null 'chunks' value in the payload deserializes to an empty list", _ =>
                {
                    // The Chunks setter coerces null to an empty list; this must hold when the null
                    // arrives via deserialization, not just direct assignment.
                    string json = "{\"text\":\"hi\",\"tokens\":[\"hi\"],\"token_count\":1,\"chunks\":null}";

                    TokenizationResult result = JsonSerializer.Deserialize<TokenizationResult>(json);

                    TestAssert.NotNull(result);
                    TestAssert.NotNull(result.Chunks);
                    TestAssert.Empty(result.Chunks);
                    return Task.CompletedTask;
                }),

                Case(SerializationSuiteId, "BatchDeserializesResults", "BatchTokenizationResult deserializes a results array", _ =>
                {
                    string json =
                        "{\"results\":[{\"text\":\"a\",\"token_count\":1},{\"text\":\"b\",\"token_count\":1}]}";

                    BatchTokenizationResult batch = JsonSerializer.Deserialize<BatchTokenizationResult>(json);

                    TestAssert.NotNull(batch);
                    TestAssert.Equal(2, batch.Results.Count);
                    TestAssert.Equal("a", batch.Results[0].Text);
                    return Task.CompletedTask;
                }),

                Case(SerializationSuiteId, "RequestRoundTrips", "TokenizationRequest survives a serialize/deserialize round trip", _ =>
                {
                    TokenizationRequest original = new TokenizationRequest
                    {
                        Model = "model-x",
                        Text = "round trip",
                        MaxChunkLength = 64,
                        MaxTokensPerChunk = 8,
                        TokenOverlap = 2
                    };

                    string json = JsonSerializer.Serialize(original);
                    TokenizationRequest restored = JsonSerializer.Deserialize<TokenizationRequest>(json);

                    TestAssert.Equal("model-x", restored.Model);
                    TestAssert.Equal("round trip", restored.Text);
                    TestAssert.Equal(64, restored.MaxChunkLength);
                    TestAssert.Equal(8, restored.MaxTokensPerChunk);
                    TestAssert.Equal(2, restored.TokenOverlap);
                    return Task.CompletedTask;
                }),
            };

            return Suite(SerializationSuiteId, "Model: JSON Serialization Contract", cases);
        }

        #endregion
    }
}
