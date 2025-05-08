using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAMLSorter.Core;

namespace YAMLSorterFrameworks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("输入要进行的YAML操作：");
                Console.WriteLine("Diff: 合并冲突整理");
                Console.WriteLine("Sort: 排序");
                string mode = Console.ReadLine() ?? string.Empty;
                switch (mode)
                {
                    case "Diff":
                        MergeDiff();
                        break;
                    case "Sort":
                        Order();
                        break;
                }

            }
        }

        static void MergeDiff()
        {
            var reader = new Reader();
            var diff = new Diff();
            Console.WriteLine("输入要合并整理的YAML文件路径");
            var path = Console.ReadLine();
            if (!File.Exists(path))
            {
                Console.WriteLine("合并整理失败，文件不存在。");
                return;
            }
            try
            {
                string ext = Path.GetExtension(path);
                string basePath = path + ".BASE" + ext;
                string remotePath = path + ".REMOTE" + ext;
                string localPath = path + ".LOCAL" + ext;
                var baseObjects = reader.Read(basePath, out string[] baseHeaders);
                var remoteObjects = reader.Read(remotePath, out string[] remoteHeaders);
                var localObjects = reader.Read(localPath, out string[] localHeaders);
                var mergedDiff = diff.MergeDiff(baseObjects, remoteObjects, localObjects);

                using (StreamWriter baseWriter = new StreamWriter(basePath))
                {
                    using (StreamWriter remoteWriter = new StreamWriter(remotePath))
                    {
                        using (StreamWriter localWriter = new StreamWriter(localPath))
                        {
                            mergedDiff.Write(baseWriter, remoteWriter, localWriter, baseHeaders, remoteHeaders, localHeaders);
                        }
                    }
                }

                Console.WriteLine("合并整理成功。");
            }
            catch (Exception e)
            {
                Console.WriteLine("合并整理失败。");
                Console.WriteLine(e);
            }
        }
        static void Order()
        {
            var reader = new Reader();
            var diff = new Diff();
            Console.WriteLine("输入排序的依据YAML文件");
            var basePath = Console.ReadLine();
            Console.WriteLine("输入要排序的YAML文件");
            var orderPath = Console.ReadLine();
            if (basePath == null || orderPath == null || !File.Exists(basePath) || !File.Exists(orderPath))
            {
                Console.WriteLine("排序失败，文件不存在。");
                return;
            }
            try
            {
                var baseObjects = reader.Read(basePath, out string[] baseHeaders);
                var orderObjects = reader.Read(orderPath, out string[] orderHeaders);
                var orderedObjects = diff.Sort(baseObjects, orderObjects);

                using (StreamWriter orderWriter = new StreamWriter(orderPath))
                {
                    foreach (var header in orderHeaders)
                    {
                        orderWriter.WriteLine(header);
                    }
                    foreach (var item in orderedObjects)
                    {
                        if (item != null)
                        {
                            orderWriter.WriteLine(item.Serialize());
                        }
                    }
                }

                Console.WriteLine("排序成功。");
            }
            catch (Exception e)
            {
                Console.WriteLine("排序失败。");
                Console.WriteLine(e);
            }
        }
    }
}
