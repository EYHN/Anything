using OwnHub.Database.Orm;

namespace OwnHub.Database.Triples
{
    public class TriplesTransaction : OrmTransaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriplesTransaction"/> class.
        /// </summary>
        /// <param name="ormSystem">the orm system associated with the transaction.</param>
        /// <param name="mode">the transaction mode for the transaction.</param>
        internal TriplesTransaction(OrmSystem ormSystem, ITransaction.TransactionMode mode)
            : base(ormSystem, mode)
        {
        }

        private TriplesRoot? _root;

        public TriplesRoot Root
        {
            get
            {
                return _root ??= GetObjectOrDefault<TriplesRoot>(0)!;
            }
        }
    }
}
