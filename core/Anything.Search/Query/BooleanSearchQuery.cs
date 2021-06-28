using System.Collections;
using System.Collections.Generic;

namespace Anything.Search.Query
{
    public record BooleanSearchQuery : SearchQuery, IEnumerable<BooleanSearchQuery.BooleanClause>
    {
        public enum Occur
        {
            /// <summary>
            ///     Use this operator for clauses that <i>must</i> appear in the matching documents.
            /// </summary>
            Must,

            /// <summary>
            ///     Use this operator for clauses that <i>should</i> appear in the matching documents.
            /// </summary>
            Should,

            /// <summary>
            ///     Use this operator for clauses that <i>must not</i> appear in the matching documents.
            /// </summary>
            MustNot
        }

        private readonly IList<BooleanClause> _clauses = new List<BooleanClause>();

        public IEnumerator<BooleanClause> GetEnumerator()
        {
            return _clauses.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public BooleanSearchQuery Add(BooleanClause clause)
        {
            _clauses.Add(clause);
            return this;
        }

        public record BooleanClause(SearchQuery Query, Occur Occur);
    }
}
