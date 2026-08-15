namespace Test.Nunit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using global::Test.Shared;
    using NUnit.Framework;
    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    /// <summary>
    /// Fact-style host: every shared descriptor runs inside a single [Test], and any failures are
    /// aggregated into one assertion.
    /// </summary>
    [TestFixture]
    public sealed class ModelTokenizerNunitFactTests : TouchstoneNunitBase
    {
        /// <inheritdoc />
        protected override IReadOnlyList<TestSuiteDescriptor> Suites
        {
            get { return ModelTokenizerTestSuites.All; }
        }

        /// <summary>
        /// Run every shared descriptor as a single test.
        /// </summary>
        /// <returns>Task.</returns>
        [Test]
        public async Task RunAll()
        {
            await RunAllAsync();
        }
    }
}
