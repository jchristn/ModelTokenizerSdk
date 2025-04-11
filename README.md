![ModelTokenizerSdk](https://github.com/jchristn/modelytokenizersdk/blob/main/assets/icon.ico)

# Model Tokenizer SDK

Model tokenizer SDK.  This SDK uses the modeltokenizer docker image found [here](https://hub.docker.com/r/jchristn/modeltokenizer) (repository for the Docker image is [here](https://github.com/jchristn/modeltokenizer)).

## New in v1.0.x

- Initial release

## Help or Feedback

Need help or have feedback? Please file an issue here!

## Simple Example

```csharp
using ModelTokenizerSdk;

ModelTokenizer tokenizer = new ModelTokenizer(endpoint);
bool connected = await tokenizer.ValidateConnectivity();

TokenizationResult result1 = await tokenizer.Tokenize(
  "sentence-transformers/all-MiniLM-L6-v2", // model
  "this is a very simple sentence", // sentence
  null // Huggingface API key
  );
// {"text":"this is a very simple sentence","tokens":["this","is","a","very","simple","sentence"]}

BatchTokenizationResult result2 = await tokenizer.Tokenize(
  "sentence-transformers/all-MiniLM-L6-v2", // model
  new List<string> {
    "this is a very simple sentence",
    "hello, how's your day going today?"
  },
  null // Huggingface API key
  );
// {"results":[{"text":"this is a very simple sentence","tokens":["this","is","a","very","simple","sentence"]},{"text":"hello, how's your day going today?","tokens":["hello",",","how","'","s","your","day","going","today","?"]}]}
```

## Version History

Please refer to CHANGELOG.md.
