using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Signals
{
    public class SignalValue
    {
        private readonly double Value;
        private readonly System.DateTime TimeStamp;

        public SignalValue(double value, DateTime timeStamp)
        {
            Value = value;
            TimeStamp = timeStamp;
        }

        public override string ToString()
        {
            return $"Value: {Value}, TimeStamp: {TimeStamp}";
        }

        public DateTime getTimeStamp()
        {
            return TimeStamp;
        }

        public double getValue()
        {
            return Value;
        }
    }
}