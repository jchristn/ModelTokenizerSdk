using NUnit.Framework;

// The shared descriptors exercise TokenEstimator, whose configuration (AvgCharsPerToken,
// SeparatorTokens, TokenSplitThreshold) is backed by static state. Keep execution serial so the
// data-driven host and the fact-style host cannot mutate that shared state concurrently.
[assembly: NonParallelizable]
