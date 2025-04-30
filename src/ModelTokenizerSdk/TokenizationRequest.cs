namespace ModelTokenizerSdk
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    /// <summary>
    /// Tokenization request.
    /// </summary>
    public class TokenizationRequest
    {
        /// <summary>
        /// Model.
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = null;

        /// <summary>
        /// Huggingface API key.
        /// </summary>
        [JsonPropertyName("huggingface_api_key")]
        public string HuggingFaceApiKey { get; set; } = null;

        /// <summary>
        /// Maximum chunk length in characters.
        /// </summary>
        [JsonPropertyName("max_chunk_length")]
        public int? MaxChunkLength
        {
            get
            {
                return _MaxChunkLength;
            }
            set
            {
                if (value != null && value.Value < 1) throw new ArgumentOutOfRangeException(nameof(MaxChunkLength));
                _MaxChunkLength = value;
            }
        }

        /// <summary>
        /// Maximum tokens per chunk.
        /// </summary>
        [JsonPropertyName("max_tokens_per_chunk")]
        public int? MaxTokensPerChunk
        {
            get
            {
                return _MaxTokensPerChunk;
            }
            set
            {
                if (value != null && value.Value < 1) throw new ArgumentOutOfRangeException(nameof(MaxTokensPerChunk));
                _MaxTokensPerChunk = value;
            }
        }

        /// <summary>
        /// Token overlap.
        /// </summary>
        [JsonPropertyName("token_overlap")]
        public int? TokenOverlap
        {
            get
            {
                return _TokenOverlap;
            }
            set
            {
                if (value != null && value.Value < 1) throw new ArgumentOutOfRangeException(nameof(TokenOverlap));
                _TokenOverlap = value;
            }
        }

        /// <summary>
        /// Text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = null;

        /// <summary>
        /// Texts.
        /// </summary>
        [JsonPropertyName("texts")]
        public List<string> Texts { get; set; } = null;

        private int? _MaxChunkLength = null;
        private int? _MaxTokensPerChunk = null;
        private int? _TokenOverlap = null;

        /// <summary>
        /// Tokenization request.
        /// </summary>
        public TokenizationRequest()
        {

        }
    }
}
