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

            // FIXME - for any write to *.txt config file - write 1 of three states; ! - rebuild as negate - write negate; "" - rebuild write positive/don't overwrite; (-) - rebuild positive write negative
            //  - this way it's possible to determine if the negation is versus a prior negative state or not -
            //  - adjust C# to allow for this to happen

            // read rebuild.txt file written to by Java GUI
            var rebuildPath = @"C:\Users\bolinger\Documents\SolidWorks Projects\Prefab Blob - Cover Blob\app data\rebuild.txt";
            var fileContent = System.IO.File.ReadAllLines(rebuildPath);
            var matesToFlip = new string[] { };
            // creates a new array for the mates that need to be flipped
            if (fileContent.Length > 1)
            {
                matesToFlip = new string[fileContent.Length - 1];
                for (var i = 1; i < fileContent.Length; ++i)
                {
                    matesToFlip[i - 1] = fileContent[i];
                }
                // flips the mate if the X/Z offset is negative relative to current position
                var cutOff = 5_000;
                var firstFeature = (Feature)model.FirstFeature();
                while (firstFeature != null && cutOff-- > 0)
                {
                    if ("MateGroup" == firstFeature.GetTypeName())
                    {
                        var mateGroup = (Feature)firstFeature.GetFirstSubFeature();
                        var index = 0;
                        while (mateGroup != null)
                        {
                            var mate = (Mate2)mateGroup.GetSpecificFeature2();
                            var mateName = mateGroup.Name;
                            foreach (string dimension in matesToFlip)
                            {
                                if (dimension == mateName)
                                {
                                    Console.WriteLine(mate.Flipped);
                                    mate.Flipped = !mate.Flipped;
                                    Console.WriteLine(mate.Flipped);
                                }
                            }

                            mateGroup = (Feature)mateGroup.GetNextSubFeature();
                            ++index;
                        }
                    }
                    firstFeature = (Feature)firstFeature.GetNextFeature();
                }
            }
            
            // read contents of config file
            var configContentsLines = System.IO.File.ReadAllLines(@fileContent[0]);

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
            //model.ForceRebuild3(true);
        }
    }
}
