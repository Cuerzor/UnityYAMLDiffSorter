using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace YAMLSorter.Core
{
    public class Reader
    {
        public UnityObject[] Read(string path, out string[] headers)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                return Read(reader, out headers);
            }
        }
        public UnityObject[] Read(TextReader reader, out string[] headers)
        {
            List<string> headerList = new List<string>();
            List<UnityObject> objs = new List<UnityObject>();
            UnityObject current = null;

            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    if (current != null)
                        objs.Add(current);
                    break;
                }
                if (line.StartsWith("#"))
                {
                    continue;
                }
                if (line.StartsWith("%"))
                {
                    headerList.Add(line);
                    continue;
                }
                if (line.StartsWith("--- !u!"))
                {
                    if (current != null)
                    {
                        objs.Add(current);
                    }

                    var match = Regex.Match(line, "--- !u!(\\d+) &(\\d+)(( stripped)*)");
                    int typeId = int.Parse(match.Groups[1].Value);
                    long id = long.Parse(match.Groups[2].Value);
                    bool stripped = !string.IsNullOrEmpty(match.Groups[4].Value);
                    current = new UnityObject(typeId, id, stripped);
                    continue;
                }
                if (current != null)
                {
                    current.AddLine(line);
                }
            }
            headers = headerList.ToArray();
            return objs.ToArray();
        }

        public void Write(TextWriter writer, string[] headers, UnityObject[] objs)
        {
            foreach (var header in headers)
            {
                writer.WriteLine(header);
            }
            foreach (var obj in objs)
            {
                writer.WriteLine(obj);
            }
        }
    }
    public class UnityObject : IComparable<UnityObject>
    {
        public int typeId;
        public long id;
        public bool stripped;
        public List<string> lines;
        public static readonly IDComparer equalityComparer = new IDComparer();
        public UnityObject(int typeId, long id, bool stripped)
        {
            this.typeId = typeId;
            this.id = id;
            this.stripped = stripped;
            lines = new List<string>();
        }
        public void AddLine(string line)
        {
            lines.Add(line);
        }
        public int GetLineCount()
        {
            return 1 + lines.Count;
        }
        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(ToString());
            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }
            return sb.ToString().Trim('\r', '\n');
        }
        public override string ToString()
        {
            if (stripped)
            {
                return $"--- !u!{typeId} &{id} stripped";
            }
            return $"--- !u!{typeId} &{id}";
    }
        public int CompareTo(UnityObject other)
        {
            if (other == null) return 1;
            return id.CompareTo(other.id);
        }
        public bool CompareUnityObject(UnityObject obj)
        {
            if (obj == null) return false;
            if (obj.id != id) return false;
            if (obj.lines.Count != lines.Count) return false;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line != obj.lines[i])
                {
                    return false;
                }
            }
            return true;
        }
        public override bool Equals(object obj)
        {
            if (obj is UnityObject other)
            {
                return other.id == id;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
        public class IDComparer : EqualityComparer<UnityObject> 
        {
            public override bool Equals(UnityObject x, UnityObject y)
            {
                return x.id == y.id;
            }

            public override int GetHashCode(UnityObject obj)
            {
                return obj.id.GetHashCode();
            }
        }
    }
}
