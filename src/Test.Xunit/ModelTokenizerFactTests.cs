namespace Test.Xunit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using global::Test.Shared;
    using global::Xunit;
    using Touchstone.Core;
    using Touchstone.XunitAdapter;

    /// <summary>
    /// Fact-style host: every shared descriptor runs inside a single [Fact], and any failures are
    /// aggregated into one assertion.
    /// </summary>
    public sealed class ModelTokenizerFactTests : TouchstoneFactBase
    {
        /// <inheritdoc />
        protected override IReadOnlyList<TestSuiteDescriptor> Suites
        {
            get { return ModelTokenizerTestSuites.All; }
        }

        /// <summary>
        /// Run every shared descriptor as a single fact.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task RunAll()
        {
            await RunAllAsync();
        }
    }
}
