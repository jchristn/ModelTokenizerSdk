namespace Test.Estimator
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using ModelTokenizerSdk;

    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("TokenEstimator Demo");
            Console.WriteLine("===================");
            Console.WriteLine();

            // Demo with simple string - no chunking and default token splitting
            SimpleStringDemo();

            // Demo with longer text - with chunking by character length (default token splitting)
            ChunkingByLengthDemo();

            // Demo with longer text - with chunking by token count (default token splitting)
            ChunkingByTokenCountDemo();

            // Demo with overlap (default token splitting)
            ChunkingWithOverlapDemo();

            // Demo with batch processing (default token splitting)
            BatchProcessingDemo();

            // Demo with mixed constraints (default token splitting)
            MixedConstraintsDemo();

            // Demo with extremely short constraints (default token splitting)
            ShortConstraintsDemo();

            // Demo with punctuation (default token splitting)
            PunctuationDemo();

            // Demo with token splitting disabled
            DisabledTokenSplittingDemo();

            // Demo with custom token splitting threshold
            CustomTokenSplittingDemo();

            // Demo showing impact of token splitting on a text with long words
            LongWordsTokenSplittingDemo();

            Console.WriteLine("");
        }

        private static void SimpleStringDemo()
        {
            Console.WriteLine("DEMO 1: Simple string without chunking");
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("Constraints: None (no chunking)");
            PrintTokenSplittingInfo();
            Console.WriteLine();

            string input = "This is a simple test string with some {special} characters!";

            var result = TokenEstimator.EstimateTokenCount(input);

            PrintTokenizationResult(result);
            Console.WriteLine();
        }

        private static void ChunkingByLengthDemo()
        {
            Console.WriteLine("DEMO 2: Chunking by character length");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Constraints: maxChunkLength = 50, maxTokensPerChunk = null, tokenOverlap = null");
            PrintTokenSplittingInfo();
            Console.WriteLine();

            string input = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. " +
                "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris " +
                "nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in " +
                "reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.";

            // Set max chunk length to 50 characters
            var result = TokenEstimator.EstimateTokenCount(input, maxChunkLength: 50);

            PrintTokenizationResult(result);
            Console.WriteLine();
        }

        private static void ChunkingByTokenCountDemo()
        {
            Console.WriteLine("DEMO 3: Chunking by token count");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Constraints: maxChunkLength = null, maxTokensPerChunk = 10, tokenOverlap = null");
            PrintTokenSplittingInfo();
            Console.WriteLine();

            string input = "The TokenEstimator class provides a generic estimation of token counts " +
                "for any input string. This is a simple approximation and not a precise implementation " +
                "of any specific tokenizer. It uses heuristics to estimate token counts based on " +
                "character counts, word counts, and the presence of special characters.";

            // Set max tokens per chunk to 10
            var result = TokenEstimator.EstimateTokenCount(input, maxTokensPerChunk: 10);

            PrintTokenizationResult(result);
            Console.WriteLine();
        }

        private static void ChunkingWithOverlapDemo()
        {
            Console.WriteLine("DEMO 4: Chunking with overlap");
            Console.WriteLine("-----------------------------");
            Console.WriteLine("Constraints: maxChunkLength = null, maxTokensPerChunk = 8, tokenOverlap = 3");
            PrintTokenSplittingInfo();
            Console.WriteLine();

            string input = "The quick brown fox jumps over the lazy dog. " +
                "A pangram is a sentence that contains every letter of the alphabet. " +
                "This sentence is a commonly used pangram in English.";

            // Set max tokens per chunk to 8 with 3 token overlap
            var result = TokenEstimator.EstimateTokenCount(input, maxTokensPerChunk: 8, tokenOverlap: 3);

            PrintTokenizationResult(result);
            Console.WriteLine();
        }

        private static void BatchProcessingDemo()
        {
            Console.WriteLine("DEMO 5: Batch processing multiple strings");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Constraints: maxChunkLength = null, maxTokensPerChunk = 10, tokenOverlap = null");
            PrintTokenSplittingInfo();
            Console.WriteLine();

            var inputs = new List<string>
            {
                "First test string with [brackets].",
                "Second string with {curly braces}.",
                "Third string with some \"quoted text\" inside.",
                "Fourth string with $%^& special characters."
            };

            // Process batch with max 10 tokens per chunk
            var result = TokenEstimator.EstimateTokenCount(inputs, maxTokensPerChunk: 10);

            PrintBatchTokenizationResult(result);
            Console.WriteLine();
        }

        private static void MixedConstraintsDemo()
        {
            Console.WriteLine("DEMO 6: Using mixed constraints");
            Console.WriteLine("-------------------------------");
            Console.WriteLine("Constraints: maxChunkLength = 60, maxTokensPerChunk = 12, tokenOverlap = 2");
            PrintTokenSplittingInfo();
            Console.WriteLine();

            string input = "Programming languages like C# provide static classes " +
                "that contain methods that can be called without creating an instance " +
                "of the class. In this example, TokenEstimator is implemented as a " +
                "static class with methods for estimating token counts.";

            // Set both max length and max tokens with overlap
            var result = TokenEstimator.EstimateTokenCount(
                input,
                maxChunkLength: 60,
                maxTokensPerChunk: 12,
                tokenOverlap: 2);

            PrintTokenizationResult(result);
            Console.WriteLine();
        }

        private static void ShortConstraintsDemo()
        {
            Console.WriteLine("DEMO 7: Using extremely short constraints");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("Constraints: maxChunkLength = 10, maxTokensPerChunk = 2, tokenOverlap = null");
            PrintTokenSplittingInfo();
            Console.WriteLine();

            string input = "Testing with extremely short chunk constraints.";

            // Set very short constraints to demonstrate how the algorithm handles edge cases
            var result = TokenEstimator.EstimateTokenCount(
                input,
                maxChunkLength: 10,
                maxTokensPerChunk: 2);

            PrintTokenizationResult(result);
            Console.WriteLine();
        }

        private static void PunctuationDemo()
        {
            Console.WriteLine("DEMO 8: Handling punctuation in text chunks");
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Constraints: maxChunkLength = null, maxTokensPerChunk = 10, tokenOverlap = 2");
            PrintTokenSplittingInfo();
            Console.WriteLine();

            string input = "Hello, world! This is a test. How are you? I'm fine, thanks! " +
                "Special characters like {}, [], and () should be handled properly, " +
                "as should semicolons; colons: and other marks!";

            // Set constraints to create multiple chunks with punctuation
            var result = TokenEstimator.EstimateTokenCount(
                input,
                maxTokensPerChunk: 10,
                tokenOverlap: 2);

            PrintTokenizationResult(result);
            Console.WriteLine();
        }

        private static void DisabledTokenSplittingDemo()
        {
            Console.WriteLine("DEMO 9: Disabled token splitting");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Constraints: maxChunkLength = null, maxTokensPerChunk = 8, tokenOverlap = null");

            // Temporarily disable token splitting
            int? originalThreshold = TokenEstimator.TokenSplitThreshold;
            TokenEstimator.TokenSplitThreshold = null;

            PrintTokenSplittingInfo(); // Will show as disabled
            Console.WriteLine();

            string input = "Supercalifragilisticexpialidocious is an extraordinarily long " +
                "English word that should not be split when token splitting is disabled. " +
                "Pneumonoultramicroscopicsilicovolcanoconiosis is another extremely long word.";

            var result = TokenEstimator.EstimateTokenCount(input, maxTokensPerChunk: 8);

            PrintTokenizationResult(result);

            // Restore the original threshold
            TokenEstimator.TokenSplitThreshold = originalThreshold;

            Console.WriteLine();
        }

        private static void CustomTokenSplittingDemo()
        {
            Console.WriteLine("DEMO 10: Custom token splitting threshold");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("Constraints: maxChunkLength = null, maxTokensPerChunk = 8, tokenOverlap = null");

            // Set a custom token splitting threshold
            int? originalThreshold = TokenEstimator.TokenSplitThreshold;
            TokenEstimator.TokenSplitThreshold = 4; // Split tokens longer than 4 characters

            PrintTokenSplittingInfo(); // Will show the custom threshold
            Console.WriteLine();

            string input = "The quick brown fox jumps over the lazy dog. " +
                "This sentence contains several words that exceed our custom threshold of 4 characters.";

            var result = TokenEstimator.EstimateTokenCount(input, maxTokensPerChunk: 8);

            PrintTokenizationResult(result);

            // Restore the original threshold
            TokenEstimator.TokenSplitThreshold = originalThreshold;

            Console.WriteLine();
        }

        private static void LongWordsTokenSplittingDemo()
        {
            Console.WriteLine("DEMO 11: Comparing token splitting approaches with long words");
            Console.WriteLine("-----------------------------------------------------------");

            string input = "Supercalifragilisticexpialidocious Pneumonoultramicroscopicsilicovolcanoconiosis Antidisestablishmentarianism Floccinaucinihilipilification Hippopotomonstrosesquippedaliophobia";

            // Set chunking parameters to ensure we see multiple chunks
            int maxTokensPerChunk = 4;

            // First, process with default token splitting (threshold = 7)
            Console.WriteLine("WITH DEFAULT TOKEN SPLITTING (threshold = 7):");
            int? originalThreshold = TokenEstimator.TokenSplitThreshold;
            TokenEstimator.TokenSplitThreshold = 7;
            PrintTokenSplittingInfo();
            Console.WriteLine($"Chunking constraint: maxTokensPerChunk = {maxTokensPerChunk}");
            Console.WriteLine();

            var resultWithSplitting = TokenEstimator.EstimateTokenCount(input, maxTokensPerChunk: maxTokensPerChunk);
            PrintTokenizationResult(resultWithSplitting);

            // Then process with token splitting disabled
            Console.WriteLine("\nWITH TOKEN SPLITTING DISABLED:");
            TokenEstimator.TokenSplitThreshold = null;
            PrintTokenSplittingInfo();
            Console.WriteLine($"Chunking constraint: maxTokensPerChunk = {maxTokensPerChunk}");
            Console.WriteLine();

            var resultWithoutSplitting = TokenEstimator.EstimateTokenCount(input, maxTokensPerChunk: maxTokensPerChunk);
            PrintTokenizationResult(resultWithoutSplitting);

            // Finally process with a custom threshold
            Console.WriteLine("\nWITH CUSTOM TOKEN SPLITTING (threshold = 3):");
            TokenEstimator.TokenSplitThreshold = 3;
            PrintTokenSplittingInfo();
            Console.WriteLine($"Chunking constraint: maxTokensPerChunk = {maxTokensPerChunk}");
            Console.WriteLine();

            var resultWithCustomSplitting = TokenEstimator.EstimateTokenCount(input, maxTokensPerChunk: maxTokensPerChunk);
            PrintTokenizationResult(resultWithCustomSplitting);

            // Restore the original threshold
            TokenEstimator.TokenSplitThreshold = originalThreshold;

            Console.WriteLine();
        }

        private static void PrintTokenSplittingInfo()
        {
            if (TokenEstimator.TokenSplitThreshold.HasValue)
            {
                Console.WriteLine($"Token splitting: ENABLED (threshold = {TokenEstimator.TokenSplitThreshold.Value} characters)");
            }
            else
            {
                Console.WriteLine("Token splitting: DISABLED");
            }
        }

        private static void PrintTokenizationResult(TokenizationResult result)
        {
            Console.WriteLine($"Text: {result.Text}");
            Console.WriteLine($"SHA256: {result.SHA256}");
            Console.WriteLine($"Token count: {result.TokenCount}");
            Console.WriteLine($"Token range: {result.TokenIndexStart} - {result.TokenIndexEnd}");

            Console.WriteLine("Tokens:");
            for (int i = 0; i < result.Tokens.Count; i++)
            {
                Console.WriteLine($"  [{i}] '{result.Tokens[i]}'");
            }

            Console.WriteLine($"Chunks: {result.Chunks.Count}");

            // Print chunk information
            if (result.Chunks.Count > 0)
            {
                for (int c = 0; c < result.Chunks.Count; c++)
                {
                    var chunk = result.Chunks[c];
                    Console.WriteLine($"  Chunk {c}:");
                    Console.WriteLine($"    Text: \"{chunk.Text}\"");
                    Console.WriteLine($"    Token count: {chunk.TokenCount}");
                    Console.WriteLine($"    Token range: {chunk.TokenIndexStart} - {chunk.TokenIndexEnd}");
                }
            }
        }

        private static void PrintBatchTokenizationResult(BatchTokenizationResult result)
        {
            Console.WriteLine($"Total results: {result.Results.Count}");

            for (int i = 0; i < result.Results.Count; i++)
            {
                Console.WriteLine($"\nResult {i + 1}:");
                PrintTokenizationResult(result.Results[i]);
            }
        }

        // Removed the truncation method since we're not using it anymore
        private static void PrintAsJson(object obj)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(obj, options);
            Console.WriteLine(json);
        }
    }
}