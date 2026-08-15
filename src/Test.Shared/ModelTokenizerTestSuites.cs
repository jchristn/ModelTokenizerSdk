namespace Test.Shared
{
    using System.Collections.Generic;

    using Touchstone.Core;

    /// <summary>
    /// Central source of truth for all ModelTokenizerSdk tests.
    ///
    /// Every test is defined once here as a runner-agnostic Touchstone descriptor and is exposed
    /// through <see cref="All"/>. The console runner (Test.Automated), the xUnit adapter
    /// (Test.Xunit), and the NUnit adapter (Test.Nunit) all execute exactly these descriptors, so
    /// coverage never diverges between hosts.
    ///
    /// The suites are pure/offline: <see cref="ModelTokenizerSdk.TokenEstimator"/> is exercised in
    /// full, while <see cref="ModelTokenizerSdk.ModelTokenizer"/> is exercised through its
    /// construction, endpoint handling, argument validation, and graceful-failure paths, none of
    /// which require the modeltokenizer microservice to be running.
    /// </summary>
    public static partial class ModelTokenizerTestSuites
    {
        /// <summary>
        /// All test suites, in a stable order.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All
        {
            get
            {
                return new List<TestSuiteDescriptor>
                {
                    EstimatorTokenizationSuite(),
                    EstimatorChunkingSuite(),
                    EstimatorTokenSplittingSuite(),
                    EstimatorConfigurationSuite(),
                    TokenizerEndpointSuite(),
                    TokenizerValidationSuite(),
                    TokenizerConnectivitySuite(),
                    TokenizerLiveServiceSuite(),
                    TokenizationRequestSuite(),
                    TokenizationResultSuite(),
                    BatchTokenizationResultSuite(),
                    SerializationSuite(),
                };
            }
        }
    }
}
