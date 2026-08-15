using Xunit;

// TokenEstimator exposes configuration via static backing fields (AvgCharsPerToken,
// SeparatorTokens, TokenSplitThreshold). Disable parallelization so tests that mutate
// and restore that shared state cannot race each other.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
