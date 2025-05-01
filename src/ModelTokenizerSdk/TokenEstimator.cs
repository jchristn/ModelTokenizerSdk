namespace ModelTokenizerSdk
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides a generic estimation of token counts for any input string.
    /// This is a simple approximation and not a precise implementation of any specific tokenizer.
    /// </summary>
    public static class TokenEstimator
    {
        #region Public-Members

        /// <summary>
        /// Average ratio of charactesr to tokens based on general observations.
        /// </summary>
        public static double AvgCharsPerToken
        {
            get
            {
                return _AvgCharsPerToken;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(AvgCharsPerToken));
                _AvgCharsPerToken = value;
            }
        }

        /// <summary>
        /// Characters that commonly become individual tokens in many tokenizers.
        /// </summary>
        public static char[] SeparatorTokens
        {
            get
            {
                return _SeparatorTokens;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(SeparatorTokens));
                if (value.Length < 1) throw new ArgumentException("At least one separator token must be supplied.");
                _SeparatorTokens = value;
            }
        }

        /// <summary>
        /// Maximum length of a token before it gets split into multiple tokens.
        /// Tokens longer than this threshold will be counted as multiple tokens.
        /// If null, tokens will never be split regardless of length.
        /// </summary>
        public static int? TokenSplitThreshold
        {
            get
            {
                return _TokenSplitThreshold;
            }
            set
            {
                if (value.HasValue && value.Value < 1)
                    throw new ArgumentOutOfRangeException(nameof(TokenSplitThreshold), "When specified, token split threshold must be greater than zero.");
                _TokenSplitThreshold = value;
            }
        }

        #endregion

        #region Private-Members

        private static double _AvgCharsPerToken = 4.0;

        private static char[] _SeparatorTokens = {
            '{', '}', '[', ']', '(', ')', '<', '>', ':', ';', ',', '.', '"', '\'', '`',
            '|', '=', '+', '-', '*', '/', '\\', '@', '#', '}', '%', '^', '&'
        };

        private static int? _TokenSplitThreshold = 7;

        #endregion

        #region Public-Methods

        /// <summary>
        /// Estimates the number of tokens in the input string without making assumptions about content type.
        /// </summary>
        /// <param name="input">The input string to analyze.</param>
        /// <param name="maxChunkLength">When set, no chunk of text shall exceed this length in characters.</param>
        /// <param name="maxTokensPerChunk">When set, no chunk of text shall have more than this number of tokens.</param>
        /// <param name="tokenOverlap">When set, this number of tokens from the end of one chunk shall be the starting point for the next chunk.</param>
        /// <returns>TokenizationResult containing estimated token information and chunks.</returns>
        public static TokenizationResult EstimateTokenCount(
            string input,
            int? maxChunkLength = null,
            int? maxTokensPerChunk = null,
            int? tokenOverlap = null)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new TokenizationResult
                {
                    Text = input,
                    SHA256 = ToHexString(SHA256Hash(input ?? string.Empty)),
                    Tokens = new List<string>(),
                    TokenCount = 0,
                    TokenIndexStart = 0,
                    TokenIndexEnd = 0,
                    Chunks = new List<TokenizationResult>() // Initialize an empty chunks list
                };
            }

            // Generate approximate tokens based on our heuristics
            List<string> approximateTokens = GenerateApproximateTokens(input);

            var result = new TokenizationResult
            {
                Text = input,
                SHA256 = ToHexString(SHA256Hash(input)),
                Tokens = approximateTokens,
                TokenCount = approximateTokens.Count,
                TokenIndexStart = 0,
                TokenIndexEnd = approximateTokens.Count - 1
            };

            // Create chunks in all cases, even when no chunking parameters are specified
            result.Chunks = CreateChunks(input, approximateTokens, maxChunkLength, maxTokensPerChunk, tokenOverlap);

            return result;
        }

        /// <summary>
        /// Estimates token counts for multiple input strings.
        /// </summary>
        /// <param name="inputs">List of input strings to analyze.</param>
        /// <param name="maxChunkLength">When set, no chunk of text shall exceed this length in characters.</param>
        /// <param name="maxTokensPerChunk">When set, no chunk of text shall have more than this number of tokens.</param>
        /// <param name="tokenOverlap">When set, this number of tokens from the end of one chunk shall be the starting point for the next chunk.</param>
        /// <returns>BatchTokenizationResult containing results for each input.</returns>
        public static BatchTokenizationResult EstimateTokenCount(
            List<string> inputs,
            int? maxChunkLength = null,
            int? maxTokensPerChunk = null,
            int? tokenOverlap = null)
        {
            var batchResult = new BatchTokenizationResult();

            if (inputs == null)
            {
                return batchResult;
            }

            foreach (var input in inputs)
            {
                var result = EstimateTokenCount(input, maxChunkLength, maxTokensPerChunk, tokenOverlap);
                batchResult.Results.Add(result);
            }

            return batchResult;
        }

        #endregion

        #region Private-Methods

        /// <summary>
        /// Generates a list of approximate tokens based on our heuristics.
        /// </summary>
        /// <param name="input">The input string to tokenize.</param>
        /// <returns>List of approximate tokens.</returns>
        private static List<string> GenerateApproximateTokens(string input)
        {
            var tokens = new List<string>();

            // Split by whitespace first
            var words = input.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                if (string.IsNullOrEmpty(word))
                    continue;

                // Check for special characters within the word
                int lastPos = 0;
                for (int i = 0; i < word.Length; i++)
                {
                    if (_SeparatorTokens.Contains(word[i]))
                    {
                        // Add the substring before the special character
                        if (i > lastPos)
                        {
                            string subToken = word.Substring(lastPos, i - lastPos);
                            AddTokenWithPossibleSplit(tokens, subToken);
                        }

                        // Add the special character as its own token
                        tokens.Add(word[i].ToString());

                        lastPos = i + 1;
                    }
                }

                // Add the rest of the word if there's anything left
                if (lastPos < word.Length)
                {
                    string remainingToken = word.Substring(lastPos);
                    AddTokenWithPossibleSplit(tokens, remainingToken);
                }
            }

            // Handle empty strings or strings with only special characters
            if (tokens.Count == 0 && !string.IsNullOrEmpty(input))
            {
                // Create some artificial tokens based on character chunks
                for (int i = 0; i < input.Length; i += (int)_AvgCharsPerToken)
                {
                    int length = Math.Min((int)_AvgCharsPerToken, input.Length - i);
                    tokens.Add(input.Substring(i, length));
                }
            }

            return tokens;
        }

        /// <summary>
        /// Adds a token to the token list, potentially splitting it if it exceeds the token split threshold.
        /// </summary>
        /// <param name="tokens">The list of tokens to add to.</param>
        /// <param name="token">The token to add, potentially splitting it.</param>
        private static void AddTokenWithPossibleSplit(List<string> tokens, string token)
        {
            if (string.IsNullOrEmpty(token))
                return;

            // If TokenSplitThreshold is null or the token is shorter than or equal to the threshold, add it as is
            if (!_TokenSplitThreshold.HasValue || token.Length <= _TokenSplitThreshold.Value)
            {
                tokens.Add(token);
                return;
            }

            // Split the token into approximately equal segments
            int threshold = _TokenSplitThreshold.Value;

            // Calculate how many segments we need
            int numSegments = (int)Math.Ceiling((decimal)token.Length / threshold);

            // Calculate the optimal segment length to make segments even
            int segmentLength = (int)Math.Ceiling((decimal)token.Length / numSegments);

            // Split the token into evenly-sized segments
            for (int i = 0; i < numSegments; i++)
            {
                int startIndex = i * segmentLength;

                // If we've gone past the end of the string, we're done
                if (startIndex >= token.Length)
                    break;

                // Calculate length of this segment (might be shorter for the last segment)
                int length = Math.Min(segmentLength, token.Length - startIndex);

                if (length > 0)
                {
                    string splitToken = token.Substring(startIndex, length);
                    tokens.Add(splitToken);
                }
            }
        }

        /// <summary>
        /// Computes the SHA-256 hash of the input byte array.
        /// </summary>
        /// <param name="data">Input byte array to hash.</param>
        /// <returns>Byte array containing the hash.</returns>
        private static byte[] SHA256Hash(byte[] data)
        {
            if (data == null || data.Length < 1) data = Array.Empty<byte>();
            using (SHA256 hash = SHA256.Create())
            {
                return hash.ComputeHash(data);
            }
        }

        /// <summary>
        /// Computes the SHA-256 hash of the input string.
        /// </summary>
        /// <param name="str">Input string to hash.</param>
        /// <returns>Byte array containing the hash.</returns>
        private static byte[] SHA256Hash(string str)
        {
            if (String.IsNullOrEmpty(str)) str = "";
            return SHA256Hash(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Checks if a token is a punctuation mark.
        /// </summary>
        /// <param name="token">Token to check.</param>
        /// <returns>True if the token is a punctuation mark, false otherwise.</returns>
        private static bool IsPunctuation(string token)
        {
            return token.Length == 1 && (_SeparatorTokens.Contains(token[0]) || token[0] == '.');
        }

        /// <summary>
        /// Reconstructs text from a list of tokens with consistent spacing.
        /// </summary>
        /// <param name="tokens">List of tokens to reconstruct.</param>
        /// <returns>Reconstructed text string.</returns>
        private static string ReconstructText(List<string> tokens)
        {
            if (tokens == null || tokens.Count == 0)
            {
                return string.Empty;
            }

            // The simplest approach: Always add a space between tokens
            StringBuilder textBuilder = new StringBuilder(tokens[0]);

            for (int i = 1; i < tokens.Count; i++)
            {
                // Always add a space before each token (except the first)
                textBuilder.Append(" ");
                textBuilder.Append(tokens[i]);
            }

            return textBuilder.ToString();
        }

        /// <summary>
        /// Creates chunks based on specified parameters.
        /// </summary>
        /// <param name="input">Original input text.</param>
        /// <param name="tokens">List of tokens.</param>
        /// <param name="maxChunkLength">Maximum character length per chunk.</param>
        /// <param name="maxTokensPerChunk">Maximum tokens per chunk.</param>
        /// <param name="tokenOverlap">Number of overlapping tokens between chunks.</param>
        /// <returns>List of TokenizationResult objects representing chunks.</returns>
        private static List<TokenizationResult> CreateChunks(
            string input,
            List<string> tokens,
            int? maxChunkLength,
            int? maxTokensPerChunk,
            int? tokenOverlap)
        {
            var chunks = new List<TokenizationResult>();

            // If no constraints are given, return a single chunk with the entire input
            if (!maxChunkLength.HasValue && !maxTokensPerChunk.HasValue)
            {
                chunks.Add(new TokenizationResult
                {
                    Text = input,
                    SHA256 = ToHexString(SHA256Hash(input)),
                    Tokens = tokens,
                    TokenCount = tokens.Count,
                    TokenIndexStart = 0,
                    TokenIndexEnd = tokens.Count - 1
                });
                return chunks;
            }

            // Use default value of 0 for overlap if not specified
            int overlap = tokenOverlap ?? 0;

            // Start processing chunks
            int tokenIndex = 0;

            while (tokenIndex < tokens.Count)
            {
                int chunkStartIndex = tokenIndex;
                int chunkEndIndex = chunkStartIndex;
                int currentLength = 0;
                int currentTokenCount = 0;

                // Add tokens to the chunk until we hit a constraint
                while (chunkEndIndex < tokens.Count)
                {
                    string currentToken = tokens[chunkEndIndex];
                    int tokenLength = currentToken.Length;

                    // Check if adding this token would exceed max chunk length
                    bool exceedsLength = false;
                    if (maxChunkLength.HasValue)
                    {
                        // Calculate the length including whitespace where needed
                        int additionalLength = tokenLength;
                        if (currentTokenCount > 0)
                        {
                            additionalLength += 1; // Add space
                        }

                        if (currentLength + additionalLength > maxChunkLength.Value)
                        {
                            exceedsLength = true;
                        }
                    }

                    // Check token count constraint if specified
                    bool exceedsTokenCount = maxTokensPerChunk.HasValue && currentTokenCount >= maxTokensPerChunk.Value;

                    // If we exceed either constraint, break the loop
                    if (exceedsLength || exceedsTokenCount)
                    {
                        // If this is the first token and we already exceed length, we still need to include it
                        if (chunkEndIndex == chunkStartIndex)
                        {
                            // Force inclusion of at least one token
                            chunkEndIndex++;
                            currentTokenCount = 1;
                            // We'll handle length constraint later
                        }
                        break;
                    }

                    // Add this token to the chunk
                    if (currentTokenCount > 0)
                    {
                        currentLength += 1; // Count the space in length
                    }
                    currentLength += tokenLength;
                    currentTokenCount++;
                    chunkEndIndex++;
                }

                // Create a list of tokens for this chunk
                var chunkTokens = new List<string>();
                for (int i = chunkStartIndex; i < chunkEndIndex; i++)
                {
                    chunkTokens.Add(tokens[i]);
                }

                // Reconstruct the chunk text with proper spacing
                string reconstructedText = ReconstructText(chunkTokens);

                // Check if we need to truncate for length constraint
                if (maxChunkLength.HasValue && reconstructedText.Length > maxChunkLength.Value && chunkTokens.Count == 1)
                {
                    // Special case: single token exceeds max length, truncate it
                    reconstructedText = reconstructedText.Substring(0, maxChunkLength.Value);
                }

                // Add the chunk
                chunks.Add(new TokenizationResult
                {
                    Text = reconstructedText,
                    SHA256 = ToHexString(SHA256Hash(reconstructedText)),
                    Tokens = chunkTokens,
                    TokenCount = chunkTokens.Count,
                    TokenIndexStart = chunkStartIndex,
                    TokenIndexEnd = chunkEndIndex - 1
                });

                // Move to the next position accounting for overlap
                tokenIndex = chunkEndIndex - overlap;

                // If we've processed all tokens or the next index would be beyond the end,
                // break out of the loop
                if (tokenIndex >= tokens.Count || chunkEndIndex >= tokens.Count)
                {
                    break;
                }

                // Ensure we don't get stuck in an infinite loop
                if (tokenIndex <= chunkStartIndex)
                {
                    tokenIndex = chunkStartIndex + 1;

                    // If we can't advance, we're done
                    if (tokenIndex >= tokens.Count)
                    {
                        break;
                    }
                }
            }

            return chunks;
        }

        private static string ToHexString(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            char[] c = new char[bytes.Length * 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                byte b = bytes[i];
                c[i * 2] = GetHexValue(b / 16);
                c[i * 2 + 1] = GetHexValue(b % 16);
            }

            return new string(c);
        }

        private static char GetHexValue(int value)
        {
            if (value < 10)
                return (char)('0' + value);
            else
                return (char)('A' + (value - 10));
        }

        #endregion
    }
}