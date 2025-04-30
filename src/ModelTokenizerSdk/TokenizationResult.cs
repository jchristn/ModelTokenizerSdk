namespace ModelTokenizerSdk
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    /// <summary>
    /// Tokenization result.
    /// </summary>
    public class TokenizationResult
    {
        /// <summary>
        /// Text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = null;

        /// <summary>
        /// SHA-256.
        /// </summary>
        [JsonPropertyName("sha256")]
        public string SHA256 { get; set; } = null;

        /// <summary>
        /// Tokens.
        /// </summary>
        [JsonPropertyName("tokens")]
        public List<string> Tokens { get; set; } = new List<string>();

        /// <summary>
        /// Token count.
        /// </summary>
        [JsonPropertyName("token_count")]
        public int? TokenCount { get; set; } = null;

        /// <summary>
        /// Index of the first token in the original input.
        /// </summary>
        [JsonPropertyName("token_index_start")]
        public int? TokenIndexStart { get; set; } = null;

        /// <summary>
        /// Index of the last token in the original input.
        /// </summary>
        [JsonPropertyName("token_index_end")]
        public int? TokenIndexEnd { get; set; } = null;

        /// <summary>
        /// Chunks.
        /// </summary>
        [JsonPropertyName("chunks")]
        public List<TokenizationResult> Chunks
        {
            get
            {
                return _Chunks;
            }
            set
            {
                if (value == null) value = new List<TokenizationResult>();
                _Chunks = value;
            }
        }

        private List<TokenizationResult> _Chunks = new List<TokenizationResult>();

        /// <summary>
        /// Tokenization result.
        /// </summary>
        public TokenizationResult()
        {

        }
    }
}
