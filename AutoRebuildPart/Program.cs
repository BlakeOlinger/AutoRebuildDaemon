using SldWorks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AutoRebuildPart
{
    class Program
    {
        static void Main(string[] args)
        {
            var swInstance = new SldWorks.SldWorks();
            var model = (ModelDoc2)swInstance.ActiveDoc;

            // read rebuild.txt
            var rebuildPath = @"C:\Users\bolinger\Documents\SolidWorks Projects\Prefab Blob - Cover Blob\app data\rebuild.txt";
            var partConfigPath = System.IO.File.ReadAllLines(rebuildPath)[0];
            
            // read contents of config file
            var partConfigContentsLines = System.IO.File.ReadAllLines(partConfigPath);

            // populate variable/line number dict
            var variableLineNumberDict = new Dictionary<string, int>();
            var index = 0;
            foreach (string line in partConfigContentsLines)
            {
                if (line.Contains("in") || line.Contains("deg"))
                {
                    variableLineNumberDict.Add(line, index);
                }
                ++index;
            }

            // populate variable-line number/dimension prefix dict
            var dimensionPrefixDict = new Dictionary<int, string>();
            foreach (string line in variableLineNumberDict.Keys)
            {
                var dimesion = line.Split('=')[1];
                // if dimension prefix is:
                // !- : write negative/rebuild/write positive,
                // !(+) : write negative/rebuild/write negative,
                // (+) : write positive/rebuild/write positive,
                // -  : write positive/rebuild/write negative
                if (dimesion.Contains("!-"))
                {
                    dimensionPrefixDict.Add(variableLineNumberDict[line], "- +");
                } else if (dimesion.Contains("!"))
                {
                    dimensionPrefixDict.Add(variableLineNumberDict[line], "- -");
                } else if (dimesion.Contains("-"))
                {
                    dimensionPrefixDict.Add(variableLineNumberDict[line], "+ -");
                } else
                {
                    dimensionPrefixDict.Add(variableLineNumberDict[line], "+ +");
                }
            }

            // if dimension prefix is:
            // !- : write negative/rebuild/write positive,
            // !(+) : write negative/rebuild/write negative,
            // (+) : write positive/rebuild/write positive,
            // -  : write positive/rebuild/write negative

            // first write to config based on dimension prefix dictionary
            foreach (int lineNumber in dimensionPrefixDict.Keys)
            {
                var variable = partConfigContentsLines[lineNumber].Split('=')[0];
                var dimension = partConfigContentsLines[lineNumber].Split('=')[1].Trim();
                if (dimension.Contains("!"))
                {
                    dimension = dimension.Replace("!", "");
                }
                if (dimension.Contains("-"))
                {
                    dimension = dimension.Replace("-", "");
                }
                var dimensionPrefix = dimensionPrefixDict[lineNumber];
                var firstWriteCondition = dimensionPrefix.Split(' ')[0];
                var firstNewLine = variable + "= " + (firstWriteCondition.Contains("+") ? "" : "-") + dimension;

                partConfigContentsLines[lineNumber] = firstNewLine;
                Console.WriteLine(partConfigContentsLines[lineNumber]);
            }
            var builder = "";
            foreach (string line in partConfigContentsLines)
            {
                builder += line + "\n";
            }
            System.IO.File.WriteAllText(partConfigPath, builder);

            // wait a moment
            Thread.Sleep(1_000);

            // rebuild
            model.ForceRebuild3(true);
            
            // second write to config based on dimension prefix dictionary
            foreach (int lineNumber in dimensionPrefixDict.Keys)
            {
                var variable = partConfigContentsLines[lineNumber].Split('=')[0];
                var dimension = partConfigContentsLines[lineNumber].Split('=')[1].Trim();
                if (dimension.Contains("!"))
                {
                    dimension = dimension.Replace("!", "");
                }
                if (dimension.Contains("-"))
                {
                    dimension = dimension.Replace("-", "");
                }
                var dimensionPrefix = dimensionPrefixDict[lineNumber];
                var secondWriteCondition = dimensionPrefix.Split(' ')[1];
                var secondNewLine = variable + "= " + (secondWriteCondition.Contains("+") ? "" : "-") + dimension;

                partConfigContentsLines[lineNumber] = secondNewLine;
            }
            var secondBuilder = "";
            foreach (string line in partConfigContentsLines)
            {
                secondBuilder += line + "\n";
            }
            System.IO.File.WriteAllText(partConfigPath, secondBuilder);
        }
    }
}
