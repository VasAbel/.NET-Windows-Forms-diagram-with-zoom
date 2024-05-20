using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Signals
{
    public class SignalDocument : DocView.Document
    {
        private System.Collections.Generic.List<SignalValue> signals = new List<SignalValue>();

        public IReadOnlyList<SignalValue> Signals
        {
            get { return signals; }
        }

        private SignalValue[] testValues = new SignalValue[]
        {
            new SignalValue(4, new DateTime(2021, 2, 25, 1, 2, 3, 23)),
            new SignalValue(8, new DateTime(2021, 2, 12, 4, 2, 35, 23)),
            new SignalValue(2, new DateTime(2022, 5, 2, 5, 2, 36, 54)),
            new SignalValue(2, new DateTime(2021, 3, 12, 5, 2, 2, 45)),
            new SignalValue(0, new DateTime(2023, 9, 4, 1, 3, 43, 16)),
            new SignalValue(5, new DateTime(2021, 11, 1, 1, 6, 3, 22)),
        };
        public SignalDocument(string name) : base(name) 
        {
            signals.AddRange(testValues);
        }

        public override void SaveDocument(string filePath)
        {
            using(StreamWriter sw = new StreamWriter(filePath)) 
            {
                foreach (SignalValue sv in signals)
                {
                    string dt = sv.getTimeStamp().ToUniversalTime().ToString("o");
                    sw.WriteLine(sv.getValue() + "\t" + dt);

                }
                sw.Close();
            }
            
        }

        public override void LoadDocument(string filePath)
        {
            signals.Clear();
            using (StreamReader sr = new StreamReader(filePath))
            {
                
                string line = string.Empty;
                while((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if(line.Length != 0)
                    {
                        var columns = line.Split("\t");

                        signals.Add(new SignalValue(Double.Parse(columns[0]), DateTime.Parse(columns[1]).ToLocalTime()));
                    }
                }

                sr.Close();
            }

            TraceValues();
                UpdateAllViews();
        }

        private void TraceValues()
        {
            foreach (var signal in signals)
                Trace.WriteLine(signal.ToString());
        }
    }
}