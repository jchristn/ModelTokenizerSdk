namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Exception thrown when a Touchstone test assertion fails. Touchstone treats any thrown
    /// exception from a test case delegate as a failure, capturing the message and stack trace.
    /// </summary>
    public sealed class TestAssertException : Exception
    {
        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="message">Failure message.</param>
        public TestAssertException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Minimal, runner-agnostic assertion helpers used by the shared Touchstone descriptors.
    /// Every helper throws <see cref="TestAssertException"/> on failure so the same descriptor
    /// behaves identically under the console runner, xUnit, and NUnit.
    /// </summary>
    public static class TestAssert
    {
        /// <summary>
        /// Assert that a condition is true.
        /// </summary>
        /// <param name="condition">Condition.</param>
        /// <param name="message">Optional message.</param>
        public static void True(bool condition, string message = null)
        {
            if (!condition)
                throw new TestAssertException(message ?? "Expected condition to be true but it was false.");
        }

        /// <summary>
        /// Assert that a condition is false.
        /// </summary>
        /// <param name="condition">Condition.</param>
        /// <param name="message">Optional message.</param>
        public static void False(bool condition, string message = null)
        {
            if (condition)
                throw new TestAssertException(message ?? "Expected condition to be false but it was true.");
        }

        /// <summary>
        /// Assert equality using the default equality comparer.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="expected">Expected value.</param>
        /// <param name="actual">Actual value.</param>
        /// <param name="message">Optional message.</param>
        public static void Equal<T>(T expected, T actual, string message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new TestAssertException(
                    message ?? ("Expected [" + Describe(expected) + "] but got [" + Describe(actual) + "]."));
        }

        /// <summary>
        /// Assert inequality using the default equality comparer.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="notExpected">Value that must not equal the actual.</param>
        /// <param name="actual">Actual value.</param>
        /// <param name="message">Optional message.</param>
        public static void NotEqual<T>(T notExpected, T actual, string message = null)
        {
            if (EqualityComparer<T>.Default.Equals(notExpected, actual))
                throw new TestAssertException(
                    message ?? ("Expected value to differ from [" + Describe(actual) + "] but it did not."));
        }

        /// <summary>
        /// Assert that a value is null.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="message">Optional message.</param>
        public static void Null(object value, string message = null)
        {
            if (value != null)
                throw new TestAssertException(message ?? "Expected value to be null but it was not.");
        }

        /// <summary>
        /// Assert that a value is not null.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="message">Optional message.</param>
        public static void NotNull(object value, string message = null)
        {
            if (value == null)
                throw new TestAssertException(message ?? "Expected value to be non-null but it was null.");
        }

        /// <summary>
        /// Assert that a sequence is empty.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="collection">Collection.</param>
        /// <param name="message">Optional message.</param>
        public static void Empty<T>(IEnumerable<T> collection, string message = null)
        {
            if (collection == null) throw new TestAssertException(message ?? "Expected empty collection but it was null.");
            if (collection.Any())
                throw new TestAssertException(message ?? "Expected collection to be empty but it was not.");
        }

        /// <summary>
        /// Assert that a sequence has exactly one element.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="collection">Collection.</param>
        /// <param name="message">Optional message.</param>
        public static void Single<T>(IEnumerable<T> collection, string message = null)
        {
            Count(1, collection, message);
        }

        /// <summary>
        /// Assert that a sequence contains an exact number of elements.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="expected">Expected count.</param>
        /// <param name="collection">Collection.</param>
        /// <param name="message">Optional message.</param>
        public static void Count<T>(int expected, IEnumerable<T> collection, string message = null)
        {
            if (collection == null) throw new TestAssertException(message ?? "Expected a collection but it was null.");
            int actual = collection.Count();
            if (actual != expected)
                throw new TestAssertException(
                    message ?? ("Expected collection of " + expected + " element(s) but it had " + actual + "."));
        }

        /// <summary>
        /// Assert that two sequences are equal element-by-element.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="expected">Expected sequence.</param>
        /// <param name="actual">Actual sequence.</param>
        /// <param name="message">Optional message.</param>
        public static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message = null)
        {
            List<T> e = expected == null ? null : expected.ToList();
            List<T> a = actual == null ? null : actual.ToList();

            if (e == null || a == null)
            {
                if (!ReferenceEquals(e, a))
                    throw new TestAssertException(message ?? "One sequence was null and the other was not.");
                return;
            }

            if (!e.SequenceEqual(a))
                throw new TestAssertException(
                    message ?? ("Sequences differ. Expected [" + string.Join(", ", e) + "] but got [" + string.Join(", ", a) + "]."));
        }

        /// <summary>
        /// Assert that a substring is present in a string.
        /// </summary>
        /// <param name="expectedSubstring">Substring that must be present.</param>
        /// <param name="actual">Actual string.</param>
        /// <param name="message">Optional message.</param>
        public static void Contains(string expectedSubstring, string actual, string message = null)
        {
            if (actual == null || !actual.Contains(expectedSubstring))
                throw new TestAssertException(
                    message ?? ("Expected string to contain [" + expectedSubstring + "] but it was [" + (actual ?? "null") + "]."));
        }

        /// <summary>
        /// Assert that a synchronous action throws an exception assignable to <typeparamref name="TException"/>.
        /// </summary>
        /// <typeparam name="TException">Expected exception type.</typeparam>
        /// <param name="action">Action.</param>
        /// <param name="message">Optional message.</param>
        public static void Throws<TException>(Action action, string message = null) where TException : Exception
        {
            if (action == null) throw new TestAssertException("No action supplied to Throws.");

            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new TestAssertException(
                    message ?? ("Expected " + typeof(TException).Name + " but " + ex.GetType().Name + " was thrown: " + ex.Message));
            }

            throw new TestAssertException(
                message ?? ("Expected " + typeof(TException).Name + " but no exception was thrown."));
        }

        /// <summary>
        /// Assert that an asynchronous action throws an exception assignable to <typeparamref name="TException"/>.
        /// </summary>
        /// <typeparam name="TException">Expected exception type.</typeparam>
        /// <param name="action">Asynchronous action.</param>
        /// <param name="message">Optional message.</param>
        /// <returns>Task.</returns>
        public static async Task ThrowsAsync<TException>(Func<Task> action, string message = null) where TException : Exception
        {
            if (action == null) throw new TestAssertException("No action supplied to ThrowsAsync.");

            try
            {
                await action().ConfigureAwait(false);
            }
            catch (TException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new TestAssertException(
                    message ?? ("Expected " + typeof(TException).Name + " but " + ex.GetType().Name + " was thrown: " + ex.Message));
            }

            throw new TestAssertException(
                message ?? ("Expected " + typeof(TException).Name + " but no exception was thrown."));
        }

        /// <summary>
        /// Fail unconditionally.
        /// </summary>
        /// <param name="message">Failure message.</param>
        public static void Fail(string message)
        {
            throw new TestAssertException(message ?? "Test failed.");
        }

        private static string Describe(object value)
        {
            return value == null ? "null" : value.ToString();
        }
    }
}
