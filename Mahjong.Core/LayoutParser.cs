using System;
using System.Collections.Generic;
using System.IO;

namespace Mahjong.Core
{
    /// <summary>
    /// Reads the .layout text format:
    /// <code>
    /// name: My Layout
    /// hidden: true   (optional; keeps the layout out of the menu)
    /// # comment
    /// 0 1 0      (x y z, x and y in half-tile units)
    /// </code>
    /// </summary>
    public static class LayoutParser
    {
        public static LayoutDefinition Parse(string text, string source = "layout")
        {
            using (var reader = new StringReader(text))
            {
                return Parse(reader, source);
            }
        }

        public static LayoutDefinition Parse(TextReader reader, string source = "layout")
        {
            string name = null;
            bool hidden = false;
            var positions = new List<Position>();
            int lineNumber = 0;
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                line = line.Trim();

                if (line.Length == 0 || line.StartsWith("#"))
                {
                    continue;
                }

                if (line.StartsWith("name:", StringComparison.OrdinalIgnoreCase))
                {
                    name = line.Substring("name:".Length).Trim();
                    continue;
                }

                if (line.StartsWith("hidden:", StringComparison.OrdinalIgnoreCase))
                {
                    string value = line.Substring("hidden:".Length).Trim();
                    if (!bool.TryParse(value, out hidden))
                    {
                        throw new FormatException($"{source}, line {lineNumber}: \"hidden:\" must be true or false, not \"{value}\".");
                    }

                    continue;
                }

                string[] parts = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3
                    || !int.TryParse(parts[0], out int x)
                    || !int.TryParse(parts[1], out int y)
                    || !int.TryParse(parts[2], out int z))
                {
                    throw new FormatException($"{source}, line {lineNumber}: expected \"x y z\" but found \"{line}\".");
                }

                positions.Add(new Position(x, y, z));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new FormatException($"{source}: missing a \"name:\" line.");
            }

            return new LayoutDefinition(name, positions, hidden);
        }
    }
}
