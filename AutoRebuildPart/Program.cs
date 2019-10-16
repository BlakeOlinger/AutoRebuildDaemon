﻿using SldWorks;
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

            // (prefix, dimension negative state) - both can be either negative or positive
            // (-, -) write positive/rebuild/write positive
            // (-, +) write negative/rebuild/write positive
            // (+, -) write negative/rebuild/write positive
            // (+, +) write positive/rebuild/write positive

            // populate variable/line number dict
            var variableLineNumberDict = new Dictionary<int, string>();
            var negativeStateString = "";
            var index = 0;
            foreach (string line in partConfigContentsLines)
            {
                if (line.Contains("in") && line.Contains("Offset"))
                {
                    variableLineNumberDict.Add(index, line);
                }

                if (line.Contains("Negative"))
                {
                    negativeStateString += line + "$";
                }

                ++index;
            }

            var negativeStateArray = negativeStateString.Split('$');

            // populate line number/negative state dict
            // do so by inputing the corresponding line variable number and
            // if its negative state variable is negative
            var lineNumberNegativeStateDict = new Dictionary<int, string>(); //(<#>, (<"-" | "+">, <"-" | "+">)) 
            foreach (int lineNumber in variableLineNumberDict.Keys)
            {
               foreach(string negativeState in negativeStateArray)
                {
                    if (negativeState.Length > 0)
                    {
                        var negativeStateSegments = negativeState.Split(' ');
                        var negativeStateHoleNumber = "Hole " + negativeStateSegments[1].Trim();
                        var negativeStateXZ = negativeStateSegments[3].Trim();

                        if (variableLineNumberDict[lineNumber].Contains(negativeStateHoleNumber) &&
                            variableLineNumberDict[lineNumber].Contains(negativeStateXZ))
                        {
                            var negativeStateIsNegative = negativeStateSegments[5].Trim()
                                .Contains("1");
                            var lineIsNegative = variableLineNumberDict[lineNumber].Contains("-");
                            var lineState = (lineIsNegative ? "-" : "+") + " " +
                                (negativeStateIsNegative ? "-" : "+");

                            lineNumberNegativeStateDict.Add(lineNumber, lineState);
                        }
                    }
                }
            }

            // generate first write config lines
            foreach (int lineNumber in lineNumberNegativeStateDict.Keys)
            {
                var state = lineNumberNegativeStateDict[lineNumber];
                var newLine = "";

                switch (state)
                {
                    case "- -": // write positive/rebuild/write positive
                        newLine = partConfigContentsLines[lineNumber].Replace("-", "");
                        partConfigContentsLines[lineNumber] = newLine;
                        break;
                    case "- +": // write negative/rebuild/write positive
                        break;
                    case "+ -": // write negative/rebuild/write positive
                        var equalIndex = partConfigContentsLines[lineNumber].IndexOf('=');
                        newLine = partConfigContentsLines[lineNumber].Insert(equalIndex + 2, "-");
                        partConfigContentsLines[lineNumber] = newLine;
                        break;
                    case "+ +":
                        break;
                }
            }

            // first write
            var builder = "";
            foreach (string line in partConfigContentsLines)
            {
                builder += line + "\n";
            }
            System.IO.File.WriteAllText(partConfigPath, builder);

            // wait a moment
            Thread.Sleep(500);

            // rebuild
            model.ForceRebuild3(true);

            // wait a moment
            Thread.Sleep(500);

            // populate negation variable line number dict
            var lineNumberNegationStateLineNumberDict = new Dictionary<int, int>();
            foreach (int lineNumber in variableLineNumberDict.Keys)
            {
                var lineSegments = variableLineNumberDict[lineNumber].Split(' ');
                var lineHoleNumber = "Hole " + lineSegments[1];
                var lineXZ = lineSegments[3];

                index = 0;
                foreach (string line in partConfigContentsLines)
                {
                    if (line.Contains(lineHoleNumber) &&
                        line.Contains(lineXZ) &&
                        line.Contains("Negative"))
                    {
                        lineNumberNegationStateLineNumberDict.Add(lineNumber, index);
                    }
                    ++index;
                }
            }

            // generate second write config lines
            // this daemon must write after a switch if a feature is now
            // in a negative quadrant
            foreach (int lineNumber in lineNumberNegativeStateDict.Keys)
            {
                var state = lineNumberNegativeStateDict[lineNumber];
                var newLine = "";
                var negationLineNumber = 0;
                var newNegationLine = "";

                switch (state)
                {
                    case "- -": // write positive/rebuild/write positive
                        break;
                        // write positive state to negative
                    case "- +": // write negative/rebuild/write positive
                        newLine = partConfigContentsLines[lineNumber].Replace("-", "");
                        partConfigContentsLines[lineNumber] = newLine;

                        negationLineNumber = lineNumberNegationStateLineNumberDict[lineNumber];
                        newNegationLine = partConfigContentsLines[negationLineNumber].Replace('0', '1');
                        partConfigContentsLines[negationLineNumber] = newNegationLine;
                        break;
                        // write negative state to positive
                    case "+ -": // write negative/rebuild/write positive
                        newLine = partConfigContentsLines[lineNumber].Replace("-", "");
                        partConfigContentsLines[lineNumber] = newLine;

                        negationLineNumber = lineNumberNegationStateLineNumberDict[lineNumber];
                        var stateVariableIndex = partConfigContentsLines[negationLineNumber].LastIndexOf('1');
                        newNegationLine = partConfigContentsLines[negationLineNumber].Remove(stateVariableIndex, 1);
                        newNegationLine += "0";
                        partConfigContentsLines[negationLineNumber] = newNegationLine;
                        break;
                    case "+ +":
                        break;
                }
            }

            // second write
            var secondBuilder = "";
            foreach (string line in partConfigContentsLines)
            {
                secondBuilder += line + "\n";
            }
            System.IO.File.WriteAllText(partConfigPath, secondBuilder);
        }
    }
}
