using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileSystem.Models
{
    public class DirectoryEntry : FileEntry
    {
        [JsonProperty("Children")]
        public List<FileEntry> Children { get; set; }

        public DirectoryEntry() : base()
        {
            Children = new List<FileEntry>();
            Type = EntryType.Directory;
        }

        public DirectoryEntry(string name) : base(name, EntryType.Directory)
        {
            Children = new List<FileEntry>();
        }

        public void AddChild(FileEntry child)
        {
            if (child.Parent != null)
                throw new InvalidOperationException("文件/目录已存在于其他位置");

            Children.Add(child);
            child.Parent = this;
            ModifiedTime = DateTime.Now;
        }

        public void RemoveChild(FileEntry child)
        {
            Children.Remove(child);
            ModifiedTime = DateTime.Now;
        }

        public void SaveToFile(string filePath)
        {
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
            string json = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(filePath, json);
        }

        public static DirectoryEntry LoadFromFile(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
               // Console.WriteLine("========== 原始JSON内容 ==========");
              //  Console.WriteLine(json);
              //  Console.WriteLine("==================================");

                var settings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = { new FileSystemConverter() }
                };

                var root = JsonConvert.DeserializeObject<DirectoryEntry>(json, settings);
                // 重建父引用
                RebuildParentReferences(root);
                // Console.WriteLine("\n========== 反序列化后的对象结构 ==========");
                //  PrintDirectoryTree(root, 0);
                //  Console.WriteLine("========================================");
                /*
                var aEntry = root.Children.Find(e => e.Name == "a");

                if (aEntry != null && aEntry.IndexBlock != null)
                {
                    Console.WriteLine($"a 的 IndexBlock ID 是: {aEntry.IndexBlock.indexBlockId}");
                }
                else
                {
                    Console.WriteLine("没找到 a 或 a 的 IndexBlock 为空");
                }*/

                return root;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载目录状态出错: {ex.Message}，已初始化空目录。");
                return new DirectoryEntry("root");
            }
        }

        private static void RebuildParentReferences(DirectoryEntry node)
        {
            if (node?.Children == null) return;
            foreach (var child in node.Children)
            {
                child.Parent = node;
                if (child is DirectoryEntry dir)
                    RebuildParentReferences(dir);
            }
        }

        private static void PrintDirectoryTree(DirectoryEntry dir, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);
            Console.WriteLine($"{indent}[DIR] {dir.Name} (Parent: {(dir.Parent == null ? "null" : dir.Parent.Name)})");
            foreach (var child in dir.Children)
            {
                if (child is DirectoryEntry subDir)
                    PrintDirectoryTree(subDir, indentLevel + 1);
                else
                    Console.WriteLine($"{indent}  [FILE] {child.Name} (Parent: {child.Parent?.Name ?? "null"})");
            }
        }
    }
}