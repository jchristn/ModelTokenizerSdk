namespace Test.Xunit
{
    using System.Threading;
    using System.Threading.Tasks;

    using global::Test.Shared;
    using global::Xunit;
    using global::Xunit.Abstractions;
    using Touchstone.Core;

    /// <summary>
    /// Theory-style host: each non-skipped shared descriptor becomes its own theory row, so the
    /// test explorer shows one entry per case. Skipped descriptors are reported through xUnit's
    /// skip mechanism with their reasons.
    /// </summary>
    public sealed class ModelTokenizerTheoryTests
    {
        private readonly ITestOutputHelper _Output;

        /// <summary>
        /// Instantiate with the xUnit output helper.
        /// </summary>
        /// <param name="output">Output helper.</param>
        public ModelTokenizerTheoryTests(ITestOutputHelper output)
        {
            _Output = output;
        }

        /// <summary>
        /// Non-skipped descriptors as theory data.
        /// </summary>
        /// <returns>Theory data rows.</returns>
        public static TheoryData<TestCaseDescriptor> TestCases()
        {
            TheoryData<TestCaseDescriptor> data = new TheoryData<TestCaseDescriptor>();

            foreach (TestSuiteDescriptor suite in ModelTokenizerTestSuites.All)
                foreach (TestCaseDescriptor testCase in suite.Cases)
                    if (!testCase.Skip)
                        data.Add(testCase);

            return data;
        }

        /// <summary>
        /// Skipped descriptors as theory data.
        /// </summary>
        /// <returns>Theory data rows.</returns>
        public static TheoryData<TestCaseDescriptor> SkippedCases()
        {
            TheoryData<TestCaseDescriptor> data = new TheoryData<TestCaseDescriptor>();

            foreach (TestSuiteDescriptor suite in ModelTokenizerTestSuites.All)
                foreach (TestCaseDescriptor testCase in suite.Cases)
                    if (testCase.Skip)
                        data.Add(testCase);

            return data;
        }

        /// <summary>
        /// Execute a single shared descriptor.
        /// </summary>
        /// <param name="testCase">Descriptor to run.</param>
        /// <returns>Task.</returns>
        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task RunTest(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }

        /// <summary>
        /// Report skipped descriptors through xUnit's skip mechanism.
        /// </summary>
        /// <param name="testCase">Skipped descriptor.</param>
        /// <returns>Task.</returns>
        [Theory(Skip = "Dynamically skipped test cases")]
        [MemberData(nameof(SkippedCases))]
        public Task Skipped(TestCaseDescriptor testCase)
        {
            _Output.WriteLine("Skipped: " + testCase.SkipReason);
            return Task.CompletedTask;
        }
    }
}
