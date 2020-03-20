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

        public double _milliseconds;
        public double? _rate = null;

        public void ShiftStream(StreamReader infile, StreamWriter outfile)
        {
            Console.Error.WriteLine("");
            TimeSpan delta = TimeSpan.FromMilliseconds(_milliseconds);

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
                tsfrom = tsfrom.Add(delta);
                if (_rate.HasValue)
                    tsfrom = RateAdd(tsfrom, _rate.Value);
                if (tsfrom.TotalMilliseconds < 0)
                    tsfrom = TimeSpan.FromMilliseconds(0);

                var tto = timeLine.Substring(17, 12);
                var tsto = ParseTime(tto);
                tsto = tsto.Add(delta);
                if (_rate.HasValue)
                    tsto = RateAdd(tsto, _rate.Value);
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

        public void RateCalc1(string firstSound, string firstText)
        {
            var sound = NiceParse(firstSound);
            var text = NiceParse(firstText);

            _milliseconds = sound.TotalMilliseconds - text.TotalMilliseconds;
        }

        public void RateCalc2(string lastSound, string lastText)
        {
            var sound = NiceParse(lastSound);
            var text = NiceParse(lastText);

            _rate = (sound.TotalMilliseconds - (text.TotalMilliseconds + _milliseconds)) / sound.TotalMilliseconds;            
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

        public TimeSpan RateAdd(TimeSpan ts, double rate)
        {
            var ts2 = ts + TimeSpan.FromMilliseconds(ts.TotalMilliseconds * rate);
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
