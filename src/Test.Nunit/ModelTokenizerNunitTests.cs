namespace Test.Nunit
{
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;

    using global::Test.Shared;
    using NUnit.Framework;
    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    /// <summary>
    /// Data-driven host: each non-skipped shared descriptor becomes its own NUnit test case via
    /// TestCaseSource, so the test explorer shows one entry per case.
    /// </summary>
    [TestFixture]
    public sealed class ModelTokenizerNunitTests
    {
        private static IEnumerable TestCases()
        {
            return new TouchstoneTestCaseSource(ModelTokenizerTestSuites.All);
        }

        /// <summary>
        /// Execute a single shared descriptor.
        /// </summary>
        /// <param name="testCase">Descriptor to run.</param>
        /// <returns>Task.</returns>
        [Test]
        [TestCaseSource(nameof(TestCases))]
        public async Task RunTest(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
