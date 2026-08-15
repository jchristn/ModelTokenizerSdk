namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ModelTokenizerSdk;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering <see cref="TokenEstimator"/> - the pure, offline token estimator.
    /// </summary>
    public static partial class ModelTokenizerTestSuites
    {
        private const string EstimatorTokenizationSuiteId = "Estimator.Tokenization";
        private const string EstimatorChunkingSuiteId = "Estimator.Chunking";
        private const string EstimatorSplittingSuiteId = "Estimator.Splitting";
        private const string EstimatorConfigSuiteId = "Estimator.Configuration";

        #region Tokenization

        /// <summary>
        /// Basic tokenization behavior of <see cref="TokenEstimator.EstimateTokenCount(string, int?, int?, int?)"/>.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor EstimatorTokenizationSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(EstimatorTokenizationSuiteId, "SimpleText", "Simple text produces expected tokens and indices", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount("Hello world");

                    TestAssert.Equal("Hello world", result.Text);
                    TestAssert.SequenceEqual(new List<string> { "Hello", "world" }, result.Tokens);
                    TestAssert.Equal(2, result.TokenCount);
                    TestAssert.Equal(0, result.TokenIndexStart);
                    TestAssert.Equal(1, result.TokenIndexEnd);
                    TestAssert.Single(result.Chunks);
                    TestAssert.Equal(2, result.Chunks[0].TokenCount);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "TokenCountMatchesTokens", "TokenCount equals the number of tokens", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount("the quick brown fox jumped");

                    TestAssert.Equal(5, result.TokenCount);
                    TestAssert.Equal(result.Tokens.Count, result.TokenCount);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "Sha256IsHex", "SHA-256 is a 64-character hex string", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount("Hello world");

                    TestAssert.NotNull(result.SHA256);
                    TestAssert.Equal(64, result.SHA256.Length);
                    foreach (char c in result.SHA256) TestAssert.True(Uri.IsHexDigit(c), "Non-hex char in SHA256: " + c);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "Sha256IsDeterministic", "Same input yields the same SHA-256", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    string a = est.EstimateTokenCount("determinism check").SHA256;
                    string b = est.EstimateTokenCount("determinism check").SHA256;

                    TestAssert.Equal(a, b);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "NullInput", "Null input returns an empty result", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount((string)null);

                    TestAssert.Null(result.Text);
                    TestAssert.Empty(result.Tokens);
                    TestAssert.Equal(0, result.TokenCount);
                    TestAssert.Empty(result.Chunks);
                    TestAssert.NotNull(result.SHA256);
                    TestAssert.Equal(64, result.SHA256.Length);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "EmptyInput", "Empty input returns an empty result", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount(string.Empty);

                    TestAssert.Equal(string.Empty, result.Text);
                    TestAssert.Empty(result.Tokens);
                    TestAssert.Equal(0, result.TokenCount);
                    TestAssert.Empty(result.Chunks);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "SeparatorBecomesToken", "A separator character becomes its own token", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount("a,b");

                    TestAssert.SequenceEqual(new List<string> { "a", ",", "b" }, result.Tokens);
                    TestAssert.Equal(3, result.TokenCount);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "MultipleSeparators", "Multiple separator characters each become their own token", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    // '!' is intentionally NOT in the default separator set; ',' and '.' are.
                    TokenizationResult result = est.EstimateTokenCount("hello, world.");

                    TestAssert.SequenceEqual(new List<string> { "hello", ",", "world", "." }, result.Tokens);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "NonSeparatorPunctuationStaysAttached", "Punctuation outside the separator set stays attached to its word", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    // '!' and '?' are not separators, so they remain part of the adjacent token.
                    TokenizationResult result = est.EstimateTokenCount("hi! ok?");

                    TestAssert.SequenceEqual(new List<string> { "hi!", "ok?" }, result.Tokens);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "OnlySeparators", "A run of separators tokenizes to individual separators", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount("...");

                    TestAssert.SequenceEqual(new List<string> { ".", ".", "." }, result.Tokens);
                    TestAssert.Equal(3, result.TokenCount);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "TabAndNewlineDelimiters", "Tabs and newlines split words", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount("a\tb\nc\rd");

                    TestAssert.SequenceEqual(new List<string> { "a", "b", "c", "d" }, result.Tokens);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "LeadingTrailingWhitespaceIgnored", "Leading/trailing/duplicate whitespace is ignored", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount("   hello    world   ");

                    TestAssert.SequenceEqual(new List<string> { "hello", "world" }, result.Tokens);
                    TestAssert.Equal(2, result.TokenCount);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "WhitespaceOnlyInput", "Whitespace-only input falls back to a single artificial token", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount("   ");

                    // No word survives whitespace splitting, so the estimator emits artificial
                    // character-chunk tokens sized by AvgCharsPerToken (default 4).
                    TestAssert.Equal(1, result.TokenCount);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "SingleChunkWhenNoConstraints", "Without chunk constraints a single chunk holds every token", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount("one two three four");

                    TestAssert.Single(result.Chunks);
                    TestAssert.Equal(4, result.Chunks[0].TokenCount);
                    TestAssert.Equal(0, result.Chunks[0].TokenIndexStart);
                    TestAssert.Equal(3, result.Chunks[0].TokenIndexEnd);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "BatchReturnsResultPerInput", "Batch estimation returns one result per input", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    BatchTokenizationResult result = est.EstimateTokenCount(
                        new List<string> { "first line", "second line here" });

                    TestAssert.Equal(2, result.Results.Count);
                    TestAssert.Equal(2, result.Results[0].TokenCount);
                    TestAssert.Equal(3, result.Results[1].TokenCount);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "NullBatch", "Null batch input returns an empty batch result", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    BatchTokenizationResult result = est.EstimateTokenCount((List<string>)null);

                    TestAssert.NotNull(result);
                    TestAssert.Empty(result.Results);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "EmptyBatch", "Empty batch input returns an empty batch result", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    BatchTokenizationResult result = est.EstimateTokenCount(new List<string>());

                    TestAssert.NotNull(result);
                    TestAssert.Empty(result.Results);
                    return Task.CompletedTask;
                }),

                Case(EstimatorTokenizationSuiteId, "BatchWithEmptyElement", "Batch containing an empty string still yields a result for it", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    BatchTokenizationResult result = est.EstimateTokenCount(
                        new List<string> { "hello world", string.Empty });

                    TestAssert.Equal(2, result.Results.Count);
                    TestAssert.Equal(2, result.Results[0].TokenCount);
                    TestAssert.Equal(0, result.Results[1].TokenCount);
                    return Task.CompletedTask;
                }),
            };

            return Suite(EstimatorTokenizationSuiteId, "TokenEstimator: Tokenization", cases);
        }

        #endregion

        #region Chunking

        /// <summary>
        /// Chunking behavior of <see cref="TokenEstimator"/> across length, token-count, and overlap constraints.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor EstimatorChunkingSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(EstimatorChunkingSuiteId, "MaxTokensPerChunk", "maxTokensPerChunk splits into multiple chunks", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount("one two three four five", maxTokensPerChunk: 2);

                    // 5 tokens, 2 per chunk => 2, 2, 1.
                    TestAssert.Equal(3, result.Chunks.Count);
                    TestAssert.Equal("one two", result.Chunks[0].Text);
                    TestAssert.Equal(2, result.Chunks[0].TokenCount);
                    TestAssert.Equal(2, result.Chunks[1].TokenCount);
                    TestAssert.Equal(1, result.Chunks[2].TokenCount);
                    TestAssert.Equal(4, result.Chunks[2].TokenIndexStart);
                    TestAssert.Equal(4, result.Chunks[2].TokenIndexEnd);
                    return Task.CompletedTask;
                }),

                Case(EstimatorChunkingSuiteId, "MaxChunkLength", "maxChunkLength splits by character length", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount("the quick brown fox", maxChunkLength: 9);

                    // "the quick" (9), "brown fox" (9).
                    TestAssert.Equal(2, result.Chunks.Count);
                    TestAssert.Equal("the quick", result.Chunks[0].Text);
                    TestAssert.Equal("brown fox", result.Chunks[1].Text);
                    foreach (TokenizationResult chunk in result.Chunks)
                        TestAssert.True(chunk.Text.Length <= 9, "Chunk exceeds max length: '" + chunk.Text + "'");
                    return Task.CompletedTask;
                }),

                Case(EstimatorChunkingSuiteId, "TokenOverlap", "tokenOverlap makes consecutive chunks share tokens", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount(
                        "one two three four five", maxTokensPerChunk: 2, tokenOverlap: 1);

                    TestAssert.True(result.Chunks.Count > 3, "Overlap should yield more chunks than without overlap");
                    TestAssert.Equal(0, result.Chunks[0].TokenIndexStart);
                    TestAssert.Equal(1, result.Chunks[1].TokenIndexStart);
                    return Task.CompletedTask;
                }),

                Case(EstimatorChunkingSuiteId, "MixedConstraints", "Combined length and token-count constraints are both respected", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount(
                        "alpha beta gamma delta epsilon zeta", maxChunkLength: 12, maxTokensPerChunk: 2);

                    TestAssert.True(result.Chunks.Count >= 3, "Expected multiple chunks under mixed constraints");
                    foreach (TokenizationResult chunk in result.Chunks)
                    {
                        TestAssert.True(chunk.TokenCount <= 2, "Chunk exceeded maxTokensPerChunk");
                        TestAssert.True(chunk.Text.Length <= 12, "Chunk exceeded maxChunkLength: '" + chunk.Text + "'");
                    }
                    return Task.CompletedTask;
                }),

                Case(EstimatorChunkingSuiteId, "SingleTokenExceedingLengthTruncated", "A lone token longer than maxChunkLength is truncated", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount("abcdefghij", maxChunkLength: 4);

                    TestAssert.Single(result.Chunks);
                    TestAssert.Equal("abcd", result.Chunks[0].Text);
                    // The token list itself is unchanged; only the chunk text is truncated.
                    TestAssert.SequenceEqual(new List<string> { "abcdefghij" }, result.Tokens);
                    return Task.CompletedTask;
                }),

                Case(EstimatorChunkingSuiteId, "ChunksCoverAllTokens", "Non-overlapping chunks cover every token exactly once", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount(
                        "a b c d e f g", maxTokensPerChunk: 3);

                    int total = 0;
                    foreach (TokenizationResult chunk in result.Chunks) total += chunk.TokenCount.Value;
                    TestAssert.Equal(result.TokenCount, total);
                    return Task.CompletedTask;
                }),

                Case(EstimatorChunkingSuiteId, "ChunkHasSha256", "Every chunk carries a valid SHA-256", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount("one two three four", maxTokensPerChunk: 2);

                    foreach (TokenizationResult chunk in result.Chunks)
                    {
                        TestAssert.NotNull(chunk.SHA256);
                        TestAssert.Equal(64, chunk.SHA256.Length);
                    }
                    return Task.CompletedTask;
                }),

                Case(EstimatorChunkingSuiteId, "EmptyInputEmptyChunks", "Empty input produces no chunks even with constraints", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount(string.Empty, maxTokensPerChunk: 2, tokenOverlap: 1);

                    TestAssert.Empty(result.Chunks);
                    return Task.CompletedTask;
                }),

                Case(EstimatorChunkingSuiteId, "ReadmeChunkingExample", "The README chunking example reproduces its four chunks", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TokenizationResult result = est.EstimateTokenCount(
                        "The quick brown fox jumped quietly over the lazy dog sitting under the tree",
                        maxChunkLength: 128, maxTokensPerChunk: 5, tokenOverlap: 2);

                    // The estimator preserves original case (unlike the real model, which lowercases),
                    // so the first chunk keeps its capital "The".
                    TestAssert.Equal(4, result.Chunks.Count);
                    TestAssert.Equal("The quick brown fox jumped", result.Chunks[0].Text);
                    TestAssert.Equal("fox jumped quietly over the", result.Chunks[1].Text);
                    TestAssert.Equal("over the lazy dog sitting", result.Chunks[2].Text);
                    TestAssert.Equal("dog sitting under the tree", result.Chunks[3].Text);
                    return Task.CompletedTask;
                }),
            };

            return Suite(EstimatorChunkingSuiteId, "TokenEstimator: Chunking", cases);
        }

        #endregion

        #region Token-Splitting

        /// <summary>
        /// Behavior of the <see cref="TokenEstimator.TokenSplitThreshold"/> long-token splitting.
        /// These cases mutate shared static configuration and restore it in a finally block.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor EstimatorTokenSplittingSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(EstimatorSplittingSuiteId, "DefaultDoesNotSplit", "By default (null threshold) long tokens are not split", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    int? original = est.TokenSplitThreshold;
                    try
                    {
                        est.TokenSplitThreshold = null;
                        TokenizationResult result = est.EstimateTokenCount("supercalifragilistic");
                        TestAssert.SequenceEqual(new List<string> { "supercalifragilistic" }, result.Tokens);
                    }
                    finally { est.TokenSplitThreshold = original; }
                    return Task.CompletedTask;
                }),

                Case(EstimatorSplittingSuiteId, "ThresholdSplitsLongToken", "A long token is split into even segments", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    int? original = est.TokenSplitThreshold;
                    try
                    {
                        est.TokenSplitThreshold = 3;
                        TokenizationResult result = est.EstimateTokenCount("abcdefg");
                        // 7 chars, threshold 3 => 3 segments of ceil(7/3)=3 => "abc","def","g".
                        TestAssert.SequenceEqual(new List<string> { "abc", "def", "g" }, result.Tokens);
                    }
                    finally { est.TokenSplitThreshold = original; }
                    return Task.CompletedTask;
                }),

                Case(EstimatorSplittingSuiteId, "TokenEqualToThresholdNotSplit", "A token exactly at the threshold length is not split", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    int? original = est.TokenSplitThreshold;
                    try
                    {
                        est.TokenSplitThreshold = 5;
                        TokenizationResult result = est.EstimateTokenCount("abcde");
                        TestAssert.SequenceEqual(new List<string> { "abcde" }, result.Tokens);
                    }
                    finally { est.TokenSplitThreshold = original; }
                    return Task.CompletedTask;
                }),

                Case(EstimatorSplittingSuiteId, "ShortTokensUnaffected", "Tokens shorter than the threshold are left intact", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    int? original = est.TokenSplitThreshold;
                    try
                    {
                        est.TokenSplitThreshold = 7;
                        TokenizationResult result = est.EstimateTokenCount("the quick brown fox");
                        TestAssert.SequenceEqual(new List<string> { "the", "quick", "brown", "fox" }, result.Tokens);
                    }
                    finally { est.TokenSplitThreshold = original; }
                    return Task.CompletedTask;
                }),
            };

            return Suite(EstimatorSplittingSuiteId, "TokenEstimator: Token Splitting", cases);
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Property validation and configuration surface of <see cref="TokenEstimator"/>.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor EstimatorConfigurationSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(EstimatorConfigSuiteId, "AvgCharsPerTokenBelowOneThrows", "AvgCharsPerToken below 1 throws", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    double original = est.AvgCharsPerToken;
                    try
                    {
                        TestAssert.Throws<ArgumentOutOfRangeException>(() => est.AvgCharsPerToken = 0.0);
                        TestAssert.Throws<ArgumentOutOfRangeException>(() => est.AvgCharsPerToken = 0.5);
                        TestAssert.Throws<ArgumentOutOfRangeException>(() => est.AvgCharsPerToken = -1.0);
                    }
                    finally { est.AvgCharsPerToken = original; }
                    return Task.CompletedTask;
                }),

                Case(EstimatorConfigSuiteId, "AvgCharsPerTokenValidAccepted", "A valid AvgCharsPerToken is accepted", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    double original = est.AvgCharsPerToken;
                    try
                    {
                        est.AvgCharsPerToken = 3.5;
                        TestAssert.Equal(3.5, est.AvgCharsPerToken);
                    }
                    finally { est.AvgCharsPerToken = original; }
                    return Task.CompletedTask;
                }),

                Case(EstimatorConfigSuiteId, "SeparatorTokensNullThrows", "Setting SeparatorTokens to null throws", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TestAssert.Throws<ArgumentNullException>(() => est.SeparatorTokens = null);
                    return Task.CompletedTask;
                }),

                Case(EstimatorConfigSuiteId, "SeparatorTokensEmptyThrows", "Setting SeparatorTokens to an empty array throws", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    TestAssert.Throws<ArgumentException>(() => est.SeparatorTokens = new char[0]);
                    return Task.CompletedTask;
                }),

                Case(EstimatorConfigSuiteId, "SeparatorTokensCustomAccepted", "A custom separator set is accepted and applied", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    char[] original = est.SeparatorTokens;
                    try
                    {
                        est.SeparatorTokens = new[] { '|' };
                        TokenizationResult result = est.EstimateTokenCount("a|b");
                        TestAssert.SequenceEqual(new List<string> { "a", "|", "b" }, result.Tokens);
                    }
                    finally { est.SeparatorTokens = original; }
                    return Task.CompletedTask;
                }),

                Case(EstimatorConfigSuiteId, "TokenSplitThresholdBelowOneThrows", "TokenSplitThreshold below 1 throws", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    int? original = est.TokenSplitThreshold;
                    try
                    {
                        TestAssert.Throws<ArgumentOutOfRangeException>(() => est.TokenSplitThreshold = 0);
                        TestAssert.Throws<ArgumentOutOfRangeException>(() => est.TokenSplitThreshold = -5);
                    }
                    finally { est.TokenSplitThreshold = original; }
                    return Task.CompletedTask;
                }),

                Case(EstimatorConfigSuiteId, "TokenSplitThresholdNullAllowed", "TokenSplitThreshold accepts null", _ =>
                {
                    TokenEstimator est = new TokenEstimator();
                    int? original = est.TokenSplitThreshold;
                    try
                    {
                        est.TokenSplitThreshold = null;
                        TestAssert.Null(est.TokenSplitThreshold);
                    }
                    finally { est.TokenSplitThreshold = original; }
                    return Task.CompletedTask;
                }),

                Case(EstimatorConfigSuiteId, "DisposeThenRestore", "Dispose completes; shared separator state is restored afterward", _ =>
                {
                    // TokenEstimator.Dispose() nulls the shared separator set, so capture and restore it
                    // to keep later cases deterministic regardless of execution order.
                    char[] original = new TokenEstimator().SeparatorTokens;
                    try
                    {
                        TokenEstimator est = new TokenEstimator();
                        est.Dispose();
                        est.Dispose(); // idempotent, must not throw
                    }
                    finally
                    {
                        new TokenEstimator().SeparatorTokens = original;
                    }
                    return Task.CompletedTask;
                }),
            };

            return Suite(EstimatorConfigSuiteId, "TokenEstimator: Configuration", cases);
        }

        #endregion
    }
}
