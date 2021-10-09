using System.Collections.Generic;
using Anything.Database;
using Anything.Utils;

namespace Anything.FileSystem.Tracker.Database
{
    public partial class HintFileTracker
    {
        public async IAsyncEnumerable<(string Path, FileHandle FileHandle, FileStats FileStats)> EnumerateAllFiles(string rootPath)
        {
            LinkedList<DatabaseTable.DataRow> stacks = new();

            using (var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Query))
            {
                var root = await _databaseTable.SelectByPathAsync(transaction, rootPath);
                if (root == null || !root.IsDirectory)
                {
                    yield break;
                }

                stacks.AddLast(root);
            }

            while (stacks.Count > 0)
            {
                var item = stacks.First!.Value;
                stacks.RemoveFirst();
                DatabaseTable.DataRow[] children;
                using (var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Query))
                {
                    children = await _databaseTable.SelectByParentAsync(transaction, item.Id);
                }

                foreach (var child in children)
                {
                    if (child.IsDirectory)
                    {
                        stacks.AddLast(child);
                    }

                    if (child.ContentTag != null && child.IdentifierTag != null)
                    {
                        yield return (child.Path, child.FileHandle!, child.FileStats!);
                    }
                }
            }
        }
    }
}
