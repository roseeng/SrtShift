﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SrtShift
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() != 3 && args.Count() != 5)
                Usage();
            
            CreateParamsFile(args);

            try
            {
                string filename = args[0];

                var s = new SrtParser();

                // s.RateCalc1("08", "22");
                // s.RateCalc2("44:41", "45:06");

                s.RateCalc1(args[1], args[2]);
                if (args.Count() == 5)
                    s.RateCalc2(args[3], args[4]);

                /*

                var sw = new StreamWriter(Console.OpenStandardOutput());
                sw.AutoFlush = true;
                Console.SetOut(sw);

                */

                s.ShiftFiles(filename);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine("");
                Console.Error.WriteLine(ex.Message);
            }
        }

        private static void Usage()
        {
            Console.Error.WriteLine("Usage: SrtShift <filename> <firstSound> <firstText> { <lastSound> <lastText> }");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("where firstSound is a line you can hear as early as possible in the film, and");
            Console.Error.WriteLine("firstText is the timestamp of the corresponding textline.");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("This is normally all that's needed, but if the speed of the film does not match the");
            Console.Error.WriteLine("subtitles, you can add a sound/text pair from the end of the film, and SrtShift ");
            Console.Error.WriteLine("will calculate a rate factor so that lastText matches lastSound.");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("Output file will be \"<filenameWithoutExtension>.out.srt\"");
            Environment.Exit(0);
        }

        private static void CreateParamsFile(string[] args)
        {
            using (var pfile = new StreamWriter(Path.GetFileNameWithoutExtension(args[0]) + ".cmd"))
            {
                pfile.Write("SrtShift.exe \"" + args[0] + "\" ");
                pfile.Write(args[1] + " " + args[2] + " ");
                if (args.Count() == 5)
                    pfile.Write(args[3] + " " + args[4] + " ");
                pfile.WriteLine("");
            }
        }
    }
}
