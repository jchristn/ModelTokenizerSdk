namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Touchstone.Core;

    /// <summary>
    /// Small factory helpers that keep the suite definitions terse and consistent.
    /// </summary>
    public static partial class ModelTokenizerTestSuites
    {
        /// <summary>
        /// Build a test case descriptor.
        /// </summary>
        /// <param name="suiteId">Suite identifier.</param>
        /// <param name="caseId">Case identifier (unique within the suite).</param>
        /// <param name="displayName">Human-readable name.</param>
        /// <param name="executeAsync">Test body; throwing any exception marks the case as failed.</param>
        /// <returns>Test case descriptor.</returns>
        private static TestCaseDescriptor Case(
            string suiteId,
            string caseId,
            string displayName,
            Func<CancellationToken, Task> executeAsync)
        {
            return new TestCaseDescriptor(suiteId, caseId, displayName, executeAsync);
        }

        /// <summary>
        /// Build a skipped test case descriptor.
        /// </summary>
        /// <param name="suiteId">Suite identifier.</param>
        /// <param name="caseId">Case identifier (unique within the suite).</param>
        /// <param name="displayName">Human-readable name.</param>
        /// <param name="skipReason">Reason the case is skipped.</param>
        /// <returns>Test case descriptor.</returns>
        private static TestCaseDescriptor SkippedCase(
            string suiteId,
            string caseId,
            string displayName,
            string skipReason)
        {
            return new TestCaseDescriptor(
                suiteId,
                caseId,
                displayName,
                _ => Task.CompletedTask,
                skip: true,
                skipReason: skipReason);
        }

        /// <summary>
        /// Build a suite descriptor from a list of cases.
        /// </summary>
        /// <param name="suiteId">Suite identifier.</param>
        /// <param name="displayName">Human-readable name.</param>
        /// <param name="cases">Cases.</param>
        /// <returns>Suite descriptor.</returns>
        private static TestSuiteDescriptor Suite(
            string suiteId,
            string displayName,
            List<TestCaseDescriptor> cases)
        {
            return new TestSuiteDescriptor(suiteId, displayName, cases);
        }
    }
}
