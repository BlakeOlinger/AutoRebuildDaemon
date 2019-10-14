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
            
            // rebuild current model
            var model = (ModelDoc2)swInstance.ActiveDoc;
            model.ForceRebuild3(true);
            
            // read rebuild.txt file written to by Java GUI
            var rebuildPath = @"C:\Users\bolinger\Documents\SolidWorks Projects\Prefab Blob - Cover Blob\app data\rebuild.txt";
            var fileContent = System.IO.File.ReadAllLines(rebuildPath);

            // read contents of config file
            var configContentsLines = System.IO.File.ReadAllLines(@fileContent[0]);

            // replace any '-' characters with '' to swap the negative values after the first rebuild
            for(var i = 0; i < configContentsLines.Length; ++i)
            {
                if (configContentsLines[i].Contains("-") && configContentsLines[i].Contains("in"))
                {
                    configContentsLines[i] = configContentsLines[i].Replace("-", "");
                }
            }
            var newContent = "";
            foreach (string line in configContentsLines)
            {
                newContent += line + "\n";
            }

            // write new config contents
            System.IO.File.WriteAllText(fileContent[0], newContent);

            // wait a few seconds
            Thread.Sleep(2_000);

            // rebuild a second time
            model.ForceRebuild3(true);
        }
    }
}
