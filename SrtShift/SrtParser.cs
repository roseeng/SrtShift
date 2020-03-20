using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Globalization;

namespace SrtShift
{
    public class SrtParser
    {
        public void ShiftFiles(string filename)
        {
            string outFilename = Path.GetFileNameWithoutExtension(filename) + ".out.srt";

            using (var infile = new StreamReader(filename))
            using (var outfile = new StreamWriter(outFilename, append: false))
            {
                ShiftStream(infile, outfile);
            }
        }

        public void ShiftPipe()
        {
            using (var infile = new StreamReader(Console.OpenStandardInput()))
            using (var outfile = new StreamWriter(Console.OpenStandardOutput()))
            {
                outfile.AutoFlush = true;
                ShiftStream(infile, outfile);
            }
        }

        public TimeSpan _delta;
        public double? _rate = null;

        public void ShiftStream(StreamReader infile, StreamWriter outfile)
        {
            Console.Error.WriteLine("");
            
            while (!infile.EndOfStream)
            {
                var countLine = infile.ReadLine();
                outfile.WriteLine(countLine);
                if (string.IsNullOrWhiteSpace(countLine)) // Can happen at beginning or end of file
                    continue;

                Console.Error.Write(countLine + "\r");

                var timeLine = infile.ReadLine();
                if (!timeLine.Contains("-->"))
                    throw new ApplicationException("Expected a timing, got: '" + timeLine + "'");
                if (timeLine.Length != 17+12)
                    throw new ApplicationException("Bad length on timing, got: '" + timeLine + "'");
                
                var tfrom = timeLine.Substring(0, 12);
                var tsfrom = ParseTime(tfrom);
                tsfrom = Adjust(tsfrom);
                if (tsfrom.TotalMilliseconds < 0)
                    tsfrom = TimeSpan.FromMilliseconds(0);

                var tto = timeLine.Substring(17, 12);
                var tsto = ParseTime(tto);
                tsto = Adjust(tsto);
                if (tsto.TotalMilliseconds < 0)
                    tsto = TimeSpan.FromMilliseconds(500);

                OutputTiming(outfile, tsfrom, tsto);

                string text;
                do
                {
                    text = infile.ReadLine();
                    outfile.WriteLine(text);
                } while (text.Length > 0);
            }
        }

        public static CultureInfo MyCulture = CultureInfo.CreateSpecificCulture("fr-FR");

        public TimeSpan ParseTime(string s)
        {
            var ts = TimeSpan.Parse(s, MyCulture);

            return ts;
        }

        public TimeSpan NiceParse(string s)
        {
            int parts = s.Split(':').Count();
            if (parts == 2)
                s = "00:" + s;
            if (parts == 1)
                s = "00:00:" + s;

            return TimeSpan.Parse(s, MyCulture);
        }

        public void RateCalc1(string firstSound, string firstText)
        {
            var sound = NiceParse(firstSound);
            var text = NiceParse(firstText);

            _delta = sound - text;
        }

        public void RateCalc2(string firstSound, string firstText, string lastSound, string lastText)
        {
            var sound1 = NiceParse(firstSound);
            var text1 = NiceParse(firstText);
            var sound2 = NiceParse(lastSound);
            var text2 = NiceParse(lastText);

            _rate =  (sound2 - sound1).TotalMilliseconds / (text2 - text1).TotalMilliseconds;
            _delta = sound1 - TimeSpan.FromMilliseconds(text1.TotalMilliseconds * _rate.Value);
        }

        public TimeSpan Adjust(TimeSpan ts)
        {
            TimeSpan ts2;
            if (_rate.HasValue)
                ts2 = TimeSpan.FromMilliseconds(ts.TotalMilliseconds * _rate.Value).Add(_delta);
            else
                ts2 = ts.Add(_delta);
            return ts2;
        }

        public void OutputTiming(StreamWriter sw, TimeSpan t1, TimeSpan t2)
        {
            sw.Write(t1.ToString(@"hh\:mm\:ss\,fff"));// "c", MyCulture));
            sw.Write(" --> ");
            sw.WriteLine(t2.ToString(@"hh\:mm\:ss\,fff"));
        }
    }
}
