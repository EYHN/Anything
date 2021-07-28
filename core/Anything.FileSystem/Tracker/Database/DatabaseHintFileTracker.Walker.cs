using System.Collections.Generic;
using Anything.Database;
using Anything.Utils;

namespace Anything.FileSystem.Tracker.Database
{
    public partial class DatabaseHintFileTracker
    {
        public async IAsyncEnumerable<Url> EnumerateAllFiles(Url rootUrl)
        {
            LinkedList<FileTable.DataRow> stacks = new();

            await using (var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Query))
            {
                var root = await _fileTable.SelectByUrlAsync(transaction, rootUrl.ToString());
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
                FileTable.DataRow[] children;
                await using (var transaction = new SqliteTransaction(_context, ITransaction.TransactionMode.Query))
                {
                    children = await _fileTable.SelectByParentAsync(transaction, item.Id);
                }

                foreach (var child in children)
                {
                    if (child.IsDirectory)
                    {
                        stacks.AddLast(child);
                    }

                    if (child.ContentTag != null && child.IdentifierTag != null)
                    {
                        yield return Url.Parse(child.Url);
                    }
                }
            }
        }
    }
}
