namespace ModelTokenizerSdk.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ModelTokenizerSdk;
    using Xunit;

    /// <summary>
    /// Tests for the pure-logic <see cref="TokenEstimator"/>. These require no running service.
    /// </summary>
    public class TokenEstimatorTests
    {
        #region Positive-Cases

        [Fact]
        public void EstimateTokenCount_SimpleText_ProducesExpectedTokens()
        {
            TokenEstimator estimator = new TokenEstimator();

            TokenizationResult result = estimator.EstimateTokenCount("Hello world");

            Assert.Equal("Hello world", result.Text);
            Assert.Equal(new List<string> { "Hello", "world" }, result.Tokens);
            Assert.Equal(2, result.TokenCount);
            Assert.Equal(0, result.TokenIndexStart);
            Assert.Equal(1, result.TokenIndexEnd);

            // With no chunking constraints, a single chunk holding everything is returned.
            Assert.Single(result.Chunks);
            Assert.Equal(2, result.Chunks[0].TokenCount);
        }

        [Fact]
        public void EstimateTokenCount_ProducesSha256HexOfInput()
        {
            TokenEstimator estimator = new TokenEstimator();

            TokenizationResult result = estimator.EstimateTokenCount("Hello world");

            Assert.NotNull(result.SHA256);
            Assert.Equal(64, result.SHA256.Length);
            Assert.All(result.SHA256, c => Assert.True(Uri.IsHexDigit(c)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void EstimateTokenCount_NullOrEmptyInput_ReturnsEmptyResult(string input)
        {
            TokenEstimator estimator = new TokenEstimator();

            TokenizationResult result = estimator.EstimateTokenCount(input);

            Assert.Equal(input, result.Text);
            Assert.Empty(result.Tokens);
            Assert.Equal(0, result.TokenCount);
            Assert.Empty(result.Chunks);
            Assert.NotNull(result.SHA256);
            Assert.Equal(64, result.SHA256.Length);
        }

        [Fact]
        public void EstimateTokenCount_SeparatorCharacters_BecomeIndividualTokens()
        {
            TokenEstimator estimator = new TokenEstimator();

            TokenizationResult result = estimator.EstimateTokenCount("a,b");

            Assert.Equal(new List<string> { "a", ",", "b" }, result.Tokens);
            Assert.Equal(3, result.TokenCount);
        }

        [Fact]
        public void EstimateTokenCount_MaxTokensPerChunk_SplitsIntoMultipleChunks()
        {
            TokenEstimator estimator = new TokenEstimator();

            TokenizationResult result = estimator.EstimateTokenCount("one two three four five", maxTokensPerChunk: 2);

            // 5 tokens, 2 per chunk => chunks of 2, 2, 1.
            Assert.Equal(3, result.Chunks.Count);
            Assert.Equal("one two", result.Chunks[0].Text);
            Assert.Equal(2, result.Chunks[0].TokenCount);
            Assert.Equal(1, result.Chunks[2].TokenCount);
            Assert.Equal(4, result.Chunks[2].TokenIndexStart);
        }

        [Fact]
        public void EstimateTokenCount_TokenOverlap_ChunksShareTokens()
        {
            TokenEstimator estimator = new TokenEstimator();

            TokenizationResult result = estimator.EstimateTokenCount(
                "one two three four five",
                maxTokensPerChunk: 2,
                tokenOverlap: 1);

            // With one token of overlap, each new chunk starts one token after the previous start.
            Assert.True(result.Chunks.Count > 3);
            Assert.Equal(0, result.Chunks[0].TokenIndexStart);
            Assert.Equal(1, result.Chunks[1].TokenIndexStart);
        }

        [Fact]
        public void EstimateTokenCount_TokenSplitThreshold_SplitsLongTokens()
        {
            TokenEstimator estimator = new TokenEstimator();
            int? original = estimator.TokenSplitThreshold;
            try
            {
                estimator.TokenSplitThreshold = 3;

                TokenizationResult result = estimator.EstimateTokenCount("abcdefg");

                // 7-character word split into ~3-char segments => "abc", "def", "g".
                Assert.Equal(new List<string> { "abc", "def", "g" }, result.Tokens);
            }
            finally
            {
                estimator.TokenSplitThreshold = original;
            }
        }

        [Fact]
        public void EstimateTokenCount_Batch_ReturnsResultPerInput()
        {
            TokenEstimator estimator = new TokenEstimator();

            BatchTokenizationResult result = estimator.EstimateTokenCount(
                new List<string> { "first line", "second line here" });

            Assert.Equal(2, result.Results.Count);
            Assert.Equal(2, result.Results[0].TokenCount);
            Assert.Equal(3, result.Results[1].TokenCount);
        }

        [Fact]
        public void EstimateTokenCount_NullBatch_ReturnsEmptyBatchResult()
        {
            TokenEstimator estimator = new TokenEstimator();

            BatchTokenizationResult result = estimator.EstimateTokenCount((List<string>)null);

            Assert.NotNull(result);
            Assert.Empty(result.Results);
        }

        #endregion

        #region Negative-Cases

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(-1.0)]
        public void AvgCharsPerToken_LessThanOne_Throws(double value)
        {
            TokenEstimator estimator = new TokenEstimator();

            Assert.Throws<ArgumentOutOfRangeException>(() => estimator.AvgCharsPerToken = value);
        }

        [Fact]
        public void SeparatorTokens_Null_Throws()
        {
            TokenEstimator estimator = new TokenEstimator();

            Assert.Throws<ArgumentNullException>(() => estimator.SeparatorTokens = null);
        }

        [Fact]
        public void SeparatorTokens_Empty_Throws()
        {
            TokenEstimator estimator = new TokenEstimator();

            Assert.Throws<ArgumentException>(() => estimator.SeparatorTokens = new char[0]);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void TokenSplitThreshold_LessThanOne_Throws(int value)
        {
            TokenEstimator estimator = new TokenEstimator();

            Assert.Throws<ArgumentOutOfRangeException>(() => estimator.TokenSplitThreshold = value);
        }

        [Fact]
        public void TokenSplitThreshold_Null_IsAllowed()
        {
            TokenEstimator estimator = new TokenEstimator();
            int? original = estimator.TokenSplitThreshold;
            try
            {
                estimator.TokenSplitThreshold = null;
                Assert.Null(estimator.TokenSplitThreshold);
            }
            finally
            {
                estimator.TokenSplitThreshold = original;
            }
        }

        #endregion
    }
}
