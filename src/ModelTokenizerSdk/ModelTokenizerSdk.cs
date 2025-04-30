namespace ModelTokenizerSdk
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.Serialization.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using RestWrapper;
    using SerializationHelper;
    using static System.Net.Mime.MediaTypeNames;

    /// <summary>
    /// SDK for model tokenizer microservice (see https://hub.docker.com/r/jchristn/modeltokenizer).
    /// </summary>
    public class ModelTokenizer : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Endpoint URL, of the form http://localhost:8000/.
        /// </summary>
        public string Endpoint
        {
            get
            {
                return _Endpoint;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Endpoint));
                Uri uri = new Uri(value);
                if (!value.EndsWith("/")) value += "/";
                _Endpoint = value;
            }
        }

        #endregion

        #region Private-Members

        private string _Endpoint = "http://localhost:8000/";
        private Serializer _Serializer = new Serializer();
        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// SDK for model tokenizer microservice (see https://hub.docker.com/r/jchristn/modeltokenizer).
        /// </summary>
        /// <param name="endpoint">Endpoint URL, of the form http://localhost:8000/.</param>
        public ModelTokenizer(string endpoint = "http://localhost:8000/")
        {
            if (!String.IsNullOrEmpty(endpoint)) Endpoint = endpoint;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Validate connectivity to the model tokenizer.
        /// HEAD /
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if connected.</returns>
        public async Task<bool> ValidateConnectivity(CancellationToken token = default)
        {
            using (RestRequest req = new RestRequest(Endpoint, HttpMethod.Head))
            {
                using (RestResponse resp = await req.SendAsync(token).ConfigureAwait(false))
                {
                    if (resp != null && resp.StatusCode == 200) return true;
                    return false;
                }
            }
        }

        /// <summary>
        /// Tokenize a line of text.
        /// POST /tokenize
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="model">Model.</param>
        /// <param name="hfApiKey">Huggingface API key.</param>
        /// <param name="maxChunkLength">Maximum chunk length.</param>
        /// <param name="maxTokensPerChunk">Maximum tokens per chunk.</param>
        /// <param name="tokenOverlap">Token overlap.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Tokenization result.</returns>
        public async Task<TokenizationResult> Tokenize(
            string text, 
            string model, 
            string hfApiKey = null, 
            int? maxChunkLength = null,
            int? maxTokensPerChunk = null,
            int? tokenOverlap = null,
            CancellationToken token = default)
        {
            return await Tokenize(new TokenizationRequest
            {
                Text = text,
                Model = model,
                HuggingFaceApiKey = hfApiKey,
                MaxChunkLength = maxChunkLength,
                MaxTokensPerChunk = maxTokensPerChunk,
                TokenOverlap = tokenOverlap
            });
        }

        /// <summary>
        /// Tokenize a list of lines of text.
        /// </summary>
        /// <param name="req">Tokenization request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Batch tokenization result.</returns>
        public async Task<TokenizationResult> Tokenize(TokenizationRequest req, CancellationToken token = default)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            if (String.IsNullOrEmpty(req.Text)) throw new ArgumentNullException(nameof(req.Text));

            using (RestRequest restRequest = new RestRequest(Endpoint + "tokenize", HttpMethod.Post, "application/json"))
            {
                string json = _Serializer.SerializeJson(req, true);

                using (RestResponse resp = await restRequest.SendAsync(json, token).ConfigureAwait(false))
                {
                    if (resp != null && resp.StatusCode == 200 && !String.IsNullOrEmpty(resp.DataAsString))
                    {
                        return _Serializer.DeserializeJson<TokenizationResult>(resp.DataAsString);
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Tokenize a list of lines of text.
        /// POST /tokenize
        /// </summary>
        /// <param name="texts">Lines of text.</param>
        /// <param name="model">Model.</param>
        /// <param name="hfApiKey">Huggingface API key.</param>
        /// <param name="maxChunkLength">Maximum chunk length.</param>
        /// <param name="maxTokensPerChunk">Maximum tokens per chunk.</param>
        /// <param name="tokenOverlap">Token overlap.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Batch tokenization result.</returns>
        public async Task<BatchTokenizationResult> TokenizeBatch(
            List<string> texts, 
            string model, 
            string hfApiKey = null,
            int? maxChunkLength = null,
            int? maxTokensPerChunk = null,
            int? tokenOverlap = null,
            CancellationToken token = default)
        {
            return await TokenizeBatch(new TokenizationRequest
            {
                Texts = texts,
                Model = model,
                HuggingFaceApiKey = hfApiKey,
                MaxChunkLength = maxChunkLength,
                MaxTokensPerChunk = maxTokensPerChunk,
                TokenOverlap = tokenOverlap
            });
        }

        /// <summary>
        /// Tokenize a list of lines of text.
        /// </summary>
        /// <param name="req">Tokenization request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Batch tokenization result.</returns>
        public async Task<BatchTokenizationResult> TokenizeBatch(TokenizationRequest req, CancellationToken token = default)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            if (req.Texts == null || req.Texts.Count < 1) throw new ArgumentNullException(nameof(req.Texts));

            using (RestRequest restRequest = new RestRequest(Endpoint + "tokenize", HttpMethod.Post, "application/json"))
            {
                string json = _Serializer.SerializeJson(req, true);

                using (RestResponse resp = await restRequest.SendAsync(json, token).ConfigureAwait(false))
                {
                    if (resp != null && resp.StatusCode == 200 && !String.IsNullOrEmpty(resp.DataAsString))
                    {
                        return _Serializer.DeserializeJson<BatchTokenizationResult>(resp.DataAsString);
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                }

                _Endpoint = null;
                _Serializer = null;
                _Disposed = true;
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
