using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace YAMLSorter.Core
{
    public class Merger
    {
        public void Print(string basePath)
        {
            using (var reader = new StreamReader(basePath))
            {
                YamlStream stream = new YamlStream();
                stream.Load(reader);

                // Examine the stream
                for (int i = 0; i < stream.Documents.Count; i++)
                {
                    var doc = stream.Documents[i];
                    var mapping = (YamlMappingNode)doc.RootNode;
                    IterateNode(i.ToString(), mapping);
                }
            }
        }

        private void IterateNode(string path, YamlNode current)
        {
            switch (current.NodeType)
            {
                case YamlNodeType.Mapping:
                    var mapNode = (YamlMappingNode)current;
                    foreach (var pair in mapNode.Children)
                    {
                        var key = ((YamlScalarNode)pair.Key).Value;
                        var value = pair.Value;

                        IterateNode(path + (key ?? "UNKNOWN") + "/", value);
                    }
                    break;
                case YamlNodeType.Sequence:
                    var arrayNode = (YamlSequenceNode)current;
                    for (int i = 0; i < arrayNode.Count(); i++)
                    {
                        var value = arrayNode[i];
                        IterateNode(path + $"[{i}]", value);
                    }
                    break;
                case YamlNodeType.Scalar:
                    var scalarNode = (YamlScalarNode)current;
                    path = path.TrimEnd('/');
                    Console.WriteLine($"{path} = {scalarNode.Value}");
                    break;
            }
        }
        public void Merge(string basePath, string remotePath, string localPath)
        {
            using (var reader = new StreamReader(basePath))
            {
                YamlStream stream = new YamlStream();
                stream.Load(reader);

                // Examine the stream
                var mapping = (YamlMappingNode)stream.Documents[0].RootNode;

                Stack<YamlNode> stack = new Stack<YamlNode>();
                stack.Push(mapping);

                Stack<string> pathStack = new Stack<string>();
                pathStack.Push("");
                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    switch (current.NodeType)
                    {
                        case YamlNodeType.Mapping:
                            var mapNode = (YamlMappingNode)current;
                            foreach (var pair in mapNode.Children)
                            {
                                var key = ((YamlScalarNode)pair.Key).Value;
                                var value = pair.Value;

                                pathStack.Push(key ?? "UNKNOWN");
                                stack.Push(value);
                            }
                            break;
                        case YamlNodeType.Sequence:
                            var arrayNode = (YamlSequenceNode)current;
                            for (int i = 0; i < arrayNode.Count(); i++)
                            {
                                pathStack.Push($"[{i}]");
                                stack.Push(arrayNode[i]);
                            }
                            break;
                        case YamlNodeType.Scalar:
                            var scalarNode = (YamlScalarNode)current;
                            string path = string.Join("/", pathStack.ToArray());
                            Console.WriteLine(path, scalarNode.Value);
                            break;
                    }
                }

            }
        }
    }
}
