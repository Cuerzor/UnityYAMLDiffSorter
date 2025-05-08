using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YAMLSorter.Core
{
    public enum DiffState
    {
        None,
        Modified,
        Added,
        Deleted
    }
    public class Diff
    {
        public UnityObject[] Sort(UnityObject[] baseObjs, UnityObject[] newObjs)
        {
            List<UnityObject> baseList = baseObjs.ToList();
            return newObjs.OrderBy(o => baseList.FindIndex(i => i.id == o.id)).ToArray();
        }
        public MergedDiff MergeDiff(UnityObject[] baseObjs, UnityObject[] remoteObjs, UnityObject[] localObjs)
        {
            List<DiffItem> diffItemList = new List<DiffItem>();
            var unioned = baseObjs.Union(remoteObjs, UnityObject.equalityComparer);
            unioned = unioned.Union(localObjs, UnityObject.equalityComparer);
            foreach (var obj in unioned)
            {
                var baseObj = baseObjs.FirstOrDefault(o => o.id == obj.id);
                var remote = remoteObjs.FirstOrDefault(o => o.id == obj.id);
                var local = localObjs.FirstOrDefault(o => o.id == obj.id);
                diffItemList.Add(new DiffItem(obj.id, baseObj, remote, local));
            }

            return new MergedDiff(diffItemList.ToArray());
        }
    }
    public class MergedDiff
    {
        public DiffItem[] diffItems;
        public MergedDiff(DiffItem[] diffItems)
        {
            this.diffItems = diffItems;
        }

        public void Write(TextWriter baseWriter, TextWriter remoteWriter, TextWriter localWriter, string[] baseHeaders, string[] remoteHeaders, string[] localHeaders)
        {
            foreach (var header in baseHeaders)
            {
                baseWriter.WriteLine(header);
            }
            foreach (var header in remoteHeaders)
            {
                remoteWriter.WriteLine(header);
            }
            foreach (var header in localHeaders)
            {
                localWriter.WriteLine(header);
            }
            List<DiffItem> pool = new List<DiffItem>(diffItems);
            var shared = pool.Where(i => i.GetRemoteDiffState() == DiffState.None && i.GetLocalDiffState() == DiffState.None).OrderBy(o => o.id).ToArray();
            foreach (var item in shared)
            {
                if (item.baseObj != null)
                {
                    baseWriter.WriteLine(item.baseObj.Serialize());
                }
                if (item.remote != null)
                {
                    remoteWriter.WriteLine(item.remote.Serialize());
                }
                if (item.local != null)
                {
                    localWriter.WriteLine(item.local.Serialize());
                }
                pool.Remove(item);
            }

            var modifiedItems = pool.Where(i => i.GetRemoteDiffState() == DiffState.Modified || i.GetLocalDiffState() == DiffState.Modified).OrderBy(o => o.id);
            WriteRegion(baseWriter, remoteWriter, localWriter, "modifies", pool, modifiedItems, false);

            var remoteAddItems = pool.Where(i => i.GetRemoteDiffState() == DiffState.Added).OrderBy(o => o.id);
            WriteRegion(baseWriter, remoteWriter, localWriter, "remote adds", pool, remoteAddItems, false);

            var localAddItems = pool.Where(i => i.GetLocalDiffState() == DiffState.Added).OrderBy(o => o.id);
            WriteRegion(baseWriter, remoteWriter, localWriter, "local adds", pool, localAddItems, false);

            var remoteDeleteItems = pool.Where(i => i.GetRemoteDiffState() == DiffState.Deleted).OrderBy(o => o.id);
            WriteRegion(baseWriter, remoteWriter, localWriter, "remote deletes", pool, remoteDeleteItems, false);

            var localDeleteItems = pool.Where(i => i.GetLocalDiffState() == DiffState.Deleted).OrderBy(o => o.id);
            WriteRegion(baseWriter, remoteWriter, localWriter, "local deletes", pool, localDeleteItems, false);
        }

        private void WriteRegion(TextWriter baseWriter, TextWriter remoteWriter, TextWriter localWriter, string regionName, List<DiffItem> pool, IEnumerable<DiffItem> items, bool writeRegions)
        {
            string startRegion = $"#{regionName}#";
            baseWriter.WriteLine(startRegion);
            remoteWriter.WriteLine(startRegion);
            localWriter.WriteLine(startRegion);

            WriteItems(baseWriter, remoteWriter, localWriter, items, false);
            pool.RemoveAll(o => items.Contains(o));
        }
        private void WriteItems(TextWriter baseWriter, TextWriter remoteWriter, TextWriter localWriter, IEnumerable<DiffItem> items, bool writeRegions)
        {
            int baseLineCount = 0;
            int remoteLineCount = 0;
            int localLineCount = 0;
            int itemIndex = 0;
            foreach (var item in items)
            {
                if (item.baseObj == null && item.remote == null && item.local == null)
                    continue;

                itemIndex++;
                int baseLines = item.baseObj?.GetLineCount() ?? 0;
                int remoteLines = item.remote?.GetLineCount() ?? 0;
                int localLines = item.local?.GetLineCount() ?? 0;

                int lineCounts = Math.Max(baseLines, remoteLines);
                lineCounts = Math.Max(lineCounts, localLines);

                int curLineCount = Math.Max(baseLineCount, remoteLineCount);
                curLineCount = Math.Max(curLineCount, localLineCount);

                int nextLineCount = curLineCount + lineCounts;


                baseLineCount += WriteItem(baseWriter, item.baseObj, nextLineCount - baseLineCount, item.id, writeRegions);
                remoteLineCount += WriteItem(remoteWriter, item.remote, nextLineCount - remoteLineCount, item.id, writeRegions);
                localLineCount += WriteItem(localWriter, item.local, nextLineCount - localLineCount, item.id, writeRegions);
            }
        }
        private int WriteItem(TextWriter writer, UnityObject obj, int linesToFill, long id, bool writeRegions)
        {
            int lineCount = 0;
            if (writeRegions)
            {
                linesToFill += 6;
                for (int i = 0; i < 3; i++)
                {
                    writer.Write("#");
                    for (int j = 0; j < i; j++)
                    {
                        writer.Write("-");
                    }
                    writer.WriteLine();
                    lineCount++;
                }
            }

            if (obj != null)
            {
                writer.WriteLine(obj.Serialize());
                lineCount += obj.GetLineCount();
            }

            for (long i = linesToFill - lineCount; i > 0; i--)
            {
                writer.WriteLine("#" + id);
                lineCount++;
            }


            if (writeRegions)
            {
                for (int i = 3; i >= 0; i--)
                {
                    writer.Write("#");
                    for (int j = 0; j <= i; j++)
                    {
                        writer.Write("-");
                    }
                    writer.WriteLine();
                    lineCount++;
                }
            }
            return lineCount;
        }
    }
    public class DiffItem
    {
        public long id;
        public UnityObject baseObj;
        public UnityObject remote;
        public UnityObject local;
        public DiffItem(long id, UnityObject baseObj, UnityObject remote, UnityObject local)
        {
            this.id = id;
            this.baseObj = baseObj;
            this.remote = remote;
            this.local = local;
        }
        public DiffState GetRemoteDiffState()
        {
            return GetDiffState(baseObj, remote);
        }
        public DiffState GetLocalDiffState()
        {
            return GetDiffState(baseObj, local);
        }
        public DiffState GetLocal2RemoteState()
        {
            return GetDiffState(remote, local);
        }
        private static DiffState GetDiffState(UnityObject obj1, UnityObject obj2)
        {

            if (obj1 == null && obj2 == null)
            {
                return DiffState.None;
            }
            else if (obj1 == null) // 没有这个
            {
                return DiffState.Added;
            }
            else if (obj2 == null) // 没有这个
            {
                return DiffState.Deleted;
            }
            else if (!obj2.CompareUnityObject(obj1)) // 有这个，但是不一样
            {
                return DiffState.Modified;
            }
            else // 有完全一样的
            {
                return DiffState.None;
            }
        }
    }
}
