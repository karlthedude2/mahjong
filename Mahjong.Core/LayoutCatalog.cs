using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mahjong.Core
{
    /// <summary>
    /// All known layouts: the .layout files built into this assembly, plus any found by
    /// <see cref="AddFromDirectory"/> (the app looks in a Layouts folder next to the exe).
    /// </summary>
    public static class LayoutCatalog
    {
        private static readonly List<LayoutDefinition> layouts = LoadEmbedded();

        /// <summary>Every layout, including hidden ones.</summary>
        public static IReadOnlyList<LayoutDefinition> All => layouts;

        /// <summary>The layouts that belong in the layout menu.</summary>
        public static IReadOnlyList<LayoutDefinition> Visible => layouts.Where(l => !l.Hidden).ToList();

        public static LayoutDefinition Default => Visible.FirstOrDefault() ?? layouts[0];

        public static LayoutDefinition Find(string name)
        {
            return layouts.FirstOrDefault(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Adds every *.layout file in <paramref name="directory"/>. A file with the same name as an
        /// existing layout replaces it. Returns a message for each file that couldn't be loaded.
        /// </summary>
        public static IReadOnlyList<string> AddFromDirectory(string directory)
        {
            var problems = new List<string>();
            if (!Directory.Exists(directory))
            {
                return problems;
            }

            foreach (string file in Directory.GetFiles(directory, "*.layout").OrderBy(f => f))
            {
                try
                {
                    var layout = LayoutParser.Parse(File.ReadAllText(file), Path.GetFileName(file));
                    var errors = layout.Validate();
                    if (errors.Count > 0)
                    {
                        problems.Add($"{Path.GetFileName(file)}: {string.Join(" ", errors)}");
                        continue;
                    }

                    layouts.RemoveAll(l => string.Equals(l.Name, layout.Name, StringComparison.OrdinalIgnoreCase));
                    layouts.Add(layout);
                }
                catch (Exception ex) when (ex is IOException || ex is FormatException || ex is UnauthorizedAccessException)
                {
                    problems.Add($"{Path.GetFileName(file)}: {ex.Message}");
                }
            }

            layouts.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            return problems;
        }

        private static List<LayoutDefinition> LoadEmbedded()
        {
            var assembly = typeof(LayoutCatalog).GetTypeInfo().Assembly;
            var result = new List<LayoutDefinition>();

            foreach (string resource in assembly.GetManifestResourceNames()
                .Where(r => r.StartsWith("Layouts.") && r.EndsWith(".layout")))
            {
                using (var reader = new StreamReader(assembly.GetManifestResourceStream(resource)))
                {
                    result.Add(LayoutParser.Parse(reader, resource));
                }
            }

            return result.OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
