namespace Test.Automated
{
    using System;
    using System.Threading.Tasks;

    using global::Test.Shared;
    using Touchstone.Cli;

    /// <summary>
    /// Touchstone console runner for the ModelTokenizerSdk test suites.
    ///
    /// Runs every descriptor defined in <see cref="ModelTokenizerTestSuites.All"/> and prints a
    /// colored, tabular pass/fail/skip report. Exit code is 0 when all tests pass, 1 otherwise.
    ///
    /// Usage:
    ///   Test.Automated [--results &lt;path&gt;]
    ///     --results &lt;path&gt;   Export results as JSON to the given file path.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>Process exit code (0 = all passed, 1 = failures).</returns>
        public static async Task<int> Main(string[] args)
        {
            string resultsPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--results" && i + 1 < args.Length)
                {
                    resultsPath = args[i + 1];
                    i++;
                }
            }

            return await ConsoleRunner.RunAsync(ModelTokenizerTestSuites.All, resultsPath: resultsPath);
        }
    }
}
