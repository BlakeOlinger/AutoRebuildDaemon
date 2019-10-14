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
            // model.ForceRebuild3(true);

            // read rebuild.txt file written to by Java GUI
            /*var rebuildPath = @"C:\Users\bolinger\Documents\SolidWorks Projects\Prefab Blob - Cover Blob\app data\rebuild.txt";
            var fileContent = System.IO.File.ReadAllLines(rebuildPath);
            */
            // read contents of config file
            // var configContentsLines = System.IO.File.ReadAllLines(@fileContent[0]);

            // TODO - if line contains a negative and the rebuild.txt file (fileContent[0]) contains a *.SLDASM
            // - have that given line's dimension - search out the equation - 'flipped'
            // - will be necessary to 'flip' them for each feature related to the negative offset
            // - make sure to only flip the dimension once - maybe I'll simply send a 'should flip' bit for a given dimension
            // - maybe I'll simply write the dimension to flip in the rebuild.txt file
            // replace any '-' characters with '' to swap the negative values after the first rebuild
            /* for(var i = 0; i < configContentsLines.Length; ++i)
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
             */
            // write new config contents
            // System.IO.File.WriteAllText(fileContent[0], newContent);

            // wait a few seconds
            // Thread.Sleep(2_000);

            // rebuild a second time
            //model.ForceRebuild3(true);

            var cutOff = 500; 
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
                        Console.WriteLine(mateGroup.Name + " : " + mate.MateEntity(0).ReferenceComponent.Name);
                        if (index == 8)
                        {
                            // mate.Flipped = !mate.Flipped;
                            // Thread.Sleep(2_000);
                           // model.ForceRebuild3(false);
                        }
                        mateGroup = (Feature)mateGroup.GetNextSubFeature();
                        ++index;
                    }
                }
                firstFeature = (Feature)firstFeature.GetNextFeature();
            }
        }
    }
}
