using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Utils
{
    /// <summary>
    /// Provides a read-only wrapper for a <see cref="ISet{T}"/> instance. Based on a gist by ArdorDeosis.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the set.</typeparam>
    public sealed class ReadOnlySet<T> : IReadOnlySet<T>
    {
        private readonly ISet<T> set;

        public ReadOnlySet(ISet<T> original)
        {
            set = original;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => set.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public bool Contains(T item) => set.Contains(item);

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<T> other) => set.IsProperSubsetOf(other);

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<T> other) => set.IsProperSupersetOf(other);

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<T> other) => set.IsSubsetOf(other);

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<T> other) => set.IsSupersetOf(other);

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<T> other) => set.Overlaps(other);

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<T> other) => set.SetEquals(other);

        /// <inheritdoc />
        public int Count => set.Count;
    }

    public static class ISetExtensions
    {
        /// <summary>
        /// Creates a new read-only wrapper for the specified set.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the set.</typeparam>
        /// <param name="set">The set to wrap.</param>
        /// <returns>A read-only wrapper for the specified set.</returns>
        public static ReadOnlySet<T> AsReadOnly<T>(this ISet<T> set) => new(set);
    }
}
