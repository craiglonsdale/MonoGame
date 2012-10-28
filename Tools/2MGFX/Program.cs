using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace TwoMGFX
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var fxFileList = new List<String>();
            var options = new Options();
            var parser = new Utilities.CommandLineParser(options);
            parser.Title = "2MGFX - Converts Microsoft FX files to a compiled MonoGame Effect.";

            if (!parser.ParseCommandLine(args))
                return 1;

            if (options.IsFile && options.IsFolder)
            {
                Console.Error.WriteLine("Trying to parse input as both a File and a Folder");
            }

            // Validate the input source exits.
            if (!File.Exists(options.Source) && !Directory.Exists(options.Source))
            {
                Console.Error.WriteLine("The input source '{0}' was not found!", options.Source);
                return 1;
            }

            // If we dont' chose a folder type input, set to file, keeping legacy behaviour the same
            if (!options.IsFolder)
            {
                options.IsFile = true;
                fxFileList.Add(options.Source);
            }
            else
            {
                fxFileList = Directory.EnumerateFiles(options.Source, "*.fx", SearchOption.AllDirectories).ToList();
            }

            if (!Directory.Exists(options.Output) && options.IsFolder)
            {
                Directory.CreateDirectory(options.Output);
            }

            // TODO: This would be where we would decide the user
            // is trying to convert an FX file to a MGFX glsl file.
            //
            // For now we assume we're going right to a compiled MGFXO file.

            // Parse the MGFX file expanding includes, macros, and returning the techniques.
            var compiledResultsList = new List<String>();
            var failedResultsList = new List<String>();
            String result;
            foreach (var fxFile in fxFileList)
            {
                if (CompileFX(fxFile, options, out result) == 1)
                {
                    failedResultsList.Add(result);
                }
                else
                {
                    compiledResultsList.Add(result);
                }
            }

            compiledResultsList.ForEach(successResult => Console.Out.WriteLine(successResult));

            if (failedResultsList.Count != 0)
            {
                failedResultsList.ForEach(failedResult => Console.Error.WriteLine(failedResult));
                return 1;
            }


            return 0;
        }

        private static int CompileFX(String inputFile, Options options, out string returnString)
        {
            ShaderInfo shaderInfo;
            string outputFile = String.Empty;
            string outputDir = options.Output;

            try
            {
                shaderInfo = ShaderInfo.FromFile(inputFile, options);
            }
            catch (Exception ex)
            {
                returnString = String.Format("Failed to parse the input file '{0}'!\n{1}\n", inputFile, ex.Message);
                return 1;
            }

            // Create the effect object.
            DXEffectObject effect;
            try
            {
                effect = DXEffectObject.FromShaderInfo(shaderInfo);
            }
            catch (Exception ex)
            {
                returnString = String.Format("Fatal exception when creating the effect!\n{0}\n", ex.ToString());
                return 1;
            }

            //If we have chosen a folder as an output we will use the old file names but change the extension for output files
            if (options.IsFolder)
            {
                //If no output dir we use the source dir
                if (outputDir == String.Empty)
                    options.Output = options.Source;

                outputFile = Path.Combine(options.Output, Path.GetFileName(inputFile));
                outputFile = Path.ChangeExtension(options.Output, ".mgfxo");
            }
            else
            { 
                // Get the output file path.
                if (options.Output == string.Empty)
                    outputFile = Path.GetFileNameWithoutExtension(inputFile) + ".mgfxo";
            }


            // Write out the effect to a runtime format.
            try
            {
                using (var stream = new FileStream(options.Output, FileMode.Create, FileAccess.Write))
                using (var writer = new BinaryWriter(stream))
                    effect.Write(writer, options);
            }
            catch (Exception ex)
            {
                returnString = String.Format("Failed to write the output file '{0}'!\n{1}\n", outputFile, ex.Message);
                return 1;
            }
            finally
            {
                //If we have failed out we should reset the output in case we are doing a batched conversion.
                options.Output = outputDir;
            }

            // We finished succesfully.
            returnString = String.Format("Compiled '{0}' to '{1}'.", inputFile, outputFile);
            return 0;
        }
    }
}
