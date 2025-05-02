![ModelTokenizerSdk](https://github.com/jchristn/modeltokenizersdk/blob/main/assets/icon.ico)

# Model Tokenizer SDK

Model tokenizer SDK.  This SDK uses the modeltokenizer docker image found [here](https://hub.docker.com/r/jchristn/modeltokenizer) (repository for the Docker image is [here](https://github.com/jchristn/modeltokenizer)).

## New in v2.0.x

- Compatibility with v2.0 of the Docker container

## Help or Feedback

Need help or have feedback? Please file an issue here!

## Simple Example

```csharp
using ModelTokenizerSdk;

ModelTokenizer tokenizer = new ModelTokenizer(endpoint);
bool connected = await tokenizer.ValidateConnectivity();

TokenizationResult result1 = await tokenizer.Tokenize(
  "sentence-transformers/all-MiniLM-L6-v2",  // model
  "this is a very simple sentence",          // sentence
  null,                                      // Huggingface API key
  null,                                      // max chunk length
  null,                                      // max tokens per chunk
  null,                                      // token overlap 
  );
/*
{
    "text": "The quick brown fox jumped quietly over the lazy dog sitting under the tree",
    "sha256": "97f4ebc3817b6b2016e7739dc31970b8a4a8cb5f8f06281cdedb21aa49affb24",
    "tokens": [
        "the",
        "quick",
        "brown",
        "fox",
        "jumped",
        "quietly",
        "over",
        "the",
        "lazy",
        "dog",
        "sitting",
        "under",
        "the",
        "tree"
    ],
    "chunks": [
        {
            "text": "The quick brown fox jumped quietly over the lazy dog sitting under the tree",
            "sha256": "97f4ebc3817b6b2016e7739dc31970b8a4a8cb5f8f06281cdedb21aa49affb24",
            "token_count": 14
        }
    ]
}
*/

BatchTokenizationResult result2 = await tokenizer.Tokenize(
  "sentence-transformers/all-MiniLM-L6-v2", // model
  new List<string> {
    "this is a very simple sentence",
    "hello, how's your day going today?"
  },
  null // Huggingface API key
  );

/*
{
    "results": [
        {
            "text": "this is a very simple sentence",
            "sha256": "32392aa65df45f53e4cc19597482acfa78060871ee9af502cc749f126d98f1c2",
            "tokens": [
                "this",
                "is",
                "a",
                "very",
                "simple",
                "sentence"
            ],
            "chunks": [
                {
                    "text": "this is a very simple sentence",
                    "sha256": "32392aa65df45f53e4cc19597482acfa78060871ee9af502cc749f126d98f1c2",
                    "token_count": 6
                }
            ]
        },
        {
            "text": "hello, how's your day going today?",
            "sha256": "8c09b7181ee47076617ac3fbe935d3a7f59fb53822a1302ab8131066f931f4b4",
            "tokens": [
                "hello",
                ",",
                "how",
                "'",
                "s",
                "your",
                "day",
                "going",
                "today",
                "?"
            ],
            "chunks": [
                {
                    "text": "hello, how's your day going today?",
                    "sha256": "8c09b7181ee47076617ac3fbe935d3a7f59fb53822a1302ab8131066f931f4b4",
                    "token_count": 10
                }
            ]
        },
        {
            "text": "The quick brown fox jumped quietly over the lazy dog sitting under the tree",
            "sha256": "97f4ebc3817b6b2016e7739dc31970b8a4a8cb5f8f06281cdedb21aa49affb24",
            "tokens": [
                "the",
                "quick",
                "brown",
                "fox",
                "jumped",
                "quietly",
                "over",
                "the",
                "lazy",
                "dog",
                "sitting",
                "under",
                "the",
                "tree"
            ],
            "chunks": [
                {
                    "text": "The quick brown fox jumped quietly over the lazy dog sitting under the tree",
                    "sha256": "97f4ebc3817b6b2016e7739dc31970b8a4a8cb5f8f06281cdedb21aa49affb24",
                    "token_count": 14
                }
            ]
        }
    ]
}
*/
```

## Chunking

ModelTokenizer can chunk based on three configurable parameters:

- `MaxChunkLength` - the maximum length, in characters, of any chunk
- `MaxTokensPerChunk` - the maximum number of tokens per chunk
- `TokenOverlap` - the number of tokens from the end of the current chunk to include in the next chunk.

Consider the sentence `The quick brown fox jumped quietly over the lazy dog sitting under the tree` with a `MaxChunkLength` of `128`, `MaxTokensPerChunk` of `5`, and a `TokenOverlap` of `2`.  The result is as follows:

- Chunk 1 - `the quick brown fox jumped`
- Chunk 2 - `fox jumped quietly over the`
- Chunk 3 - `over the lazy dog sitting`
- Chunk 4 - `dog sitting under the tree`

```csharp
using ModelTokenizerSdk;

ModelTokenizer tokenizer = new ModelTokenizer(endpoint);
bool connected = await tokenizer.ValidateConnectivity();

TokenizationResult result1 = await tokenizer.Tokenize(
  "sentence-transformers/all-MiniLM-L6-v2",  // model
  "this is a very simple sentence",          // sentence
  null,                                      // Huggingface API key
  128,                                       // max chunk length
  5,                                         // max tokens per chunk
  2,                                         // token overlap 
  );
/*
{
    "text": "The quick brown fox jumped quietly over the lazy dog sitting under the tree",
    "sha256": "97f4ebc3817b6b2016e7739dc31970b8a4a8cb5f8f06281cdedb21aa49affb24",
    "tokens": [
        "the",
        "quick",
        "brown",
        "fox",
        "jumped",
        "quietly",
        "over",
        "the",
        "lazy",
        "dog",
        "sitting",
        "under",
        "the",
        "tree"
    ],
    "chunks": [
        {
            "text": "the quick brown fox jumped",
            "sha256": "3f00e8ca186729a9df3f4228c4afe4c602ed30c0618777e305292df2e3aafb6c",
            "token_count": 5
        },
        {
            "text": "fox jumped quietly over the",
            "sha256": "7b509f90eccfe72ba029a7926f0f4d247179e2f579251a4b6c03262ba6436d08",
            "token_count": 5
        },
        {
            "text": "over the lazy dog sitting",
            "sha256": "c903e6835c9d3808eda7f44c4e871e902c5deedc426c9da26ed2550b96744c4e",
            "token_count": 5
        },
        {
            "text": "dog sitting under the tree",
            "sha256": "15e756beea1d97e33d12cbcab305625ca16201f72961e4fda0ca921d018fa02c",
            "token_count": 5
        }
    ]
}
*/
```

## Estimating Tokenization without a Model

Should you need to estimate tokenization without a model, use the `TokenEstimator` class, which follows the same signature and response as the `ModelTokenizerSdk`.  Note that `TokenEstimator` is only an estimator, and does not actually use a model, therefore, there is no dictionary specifying what the exact tokens are in a given model.  

```csharp
TokenEstimator.TokenSplitThreshold = 7; // automatically split tokens that are at least 7 characters in length
TokenEstimator.AvgCharsPerToken = 4.0;  // average number of characters per token
TokenEstimator.SeparatorTokens = [ '.', ',', ';', ... ];  // or use the default values

TokenizationResult result = TokenEstimator.EstimateTokenCount(
    "The quick brown fox jumped happily over the lazy brown dog chillaxing in his bed.",
    128, // max chunk length, or null for no chunking
    8,   // max tokens per chunk
    2    // token overlap amongst chunks
);

BatchTokenizationResult result = TokenEstimator.EstimateTokenCount(
    new List<string> { "Hello, world!" },
    128,
    8,
    2
);
```

## Version History

Please refer to CHANGELOG.md.
