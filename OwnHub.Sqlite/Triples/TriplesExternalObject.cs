using System.Threading.Tasks;

namespace OwnHub.Sqlite.Triples
{
    public abstract class TriplesExternalObject : TriplesObject
    {
        protected override abstract void Create(TriplesTransaction transaction);

        protected override abstract void Release(TriplesTransaction transaction);

        protected override abstract ValueTask CreateAsync(TriplesTransaction transaction);

        protected override abstract ValueTask ReleaseAsync(TriplesTransaction transaction);
    }
}
