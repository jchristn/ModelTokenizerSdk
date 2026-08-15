using Xunit;

// The shared descriptors exercise TokenEstimator, whose configuration (AvgCharsPerToken,
// SeparatorTokens, TokenSplitThreshold) is backed by static state. Disable parallelization so the
// fact-style and theory-style hosts cannot mutate that shared state concurrently.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
