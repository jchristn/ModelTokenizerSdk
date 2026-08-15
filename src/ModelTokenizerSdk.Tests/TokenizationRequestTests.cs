namespace ModelTokenizerSdk.Tests
{
    using System;
    using System.Collections.Generic;
    using ModelTokenizerSdk;
    using Xunit;

    /// <summary>
    /// Tests for validation on <see cref="TokenizationRequest"/> properties.
    /// </summary>
    public class TokenizationRequestTests
    {
        #region Positive-Cases

        [Fact]
        public void ValidValues_AreAccepted()
        {
            TokenizationRequest req = new TokenizationRequest
            {
                Text = "hello",
                Model = "some-model",
                MaxChunkLength = 100,
                MaxTokensPerChunk = 10,
                TokenOverlap = 2
            };

            Assert.Equal(100, req.MaxChunkLength);
            Assert.Equal(10, req.MaxTokensPerChunk);
            Assert.Equal(2, req.TokenOverlap);
        }

        [Fact]
        public void NullNumericValues_AreAccepted()
        {
            TokenizationRequest req = new TokenizationRequest
            {
                MaxChunkLength = null,
                MaxTokensPerChunk = null,
                TokenOverlap = null
            };

            Assert.Null(req.MaxChunkLength);
            Assert.Null(req.MaxTokensPerChunk);
            Assert.Null(req.TokenOverlap);
        }

        #endregion

        #region Negative-Cases

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void MaxChunkLength_LessThanOne_Throws(int value)
        {
            TokenizationRequest req = new TokenizationRequest();

            Assert.Throws<ArgumentOutOfRangeException>(() => req.MaxChunkLength = value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void MaxTokensPerChunk_LessThanOne_Throws(int value)
        {
            TokenizationRequest req = new TokenizationRequest();

            Assert.Throws<ArgumentOutOfRangeException>(() => req.MaxTokensPerChunk = value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void TokenOverlap_LessThanOne_Throws(int value)
        {
            TokenizationRequest req = new TokenizationRequest();

            Assert.Throws<ArgumentOutOfRangeException>(() => req.TokenOverlap = value);
        }

        #endregion
    }
}
