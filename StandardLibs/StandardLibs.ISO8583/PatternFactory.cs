using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace StandardLibs.ISO8583
{
    /// <summary>
    /// The ISO8583 pattern 'FlyweightFactory' class
    /// </summary>
    public class PatternFactory
    {
        private static ILoggerFactory loggerFactory { get; } = new LoggerFactory();
        private static ILogger logger => loggerFactory.CreateLogger("PatternFactory");
        private static PatternFactory patternFactory = new PatternFactory();
        private IDictionary<string, IPattern> dicPatterns = new Dictionary<string, IPattern>();
        private object obj = new object();

        // Constructor
        private PatternFactory()
        {
            
        }

        public static PatternFactory GetInstance()
        {
            return patternFactory;
        }

        public IPattern GetPattern(BitIndex bitIndex)
        {
            lock (this.obj)
            {
                if (!this.dicPatterns.ContainsKey(bitIndex.Representation))
                {
                    IPattern pattern = PatternSelector(bitIndex);
                    this.dicPatterns.Add(bitIndex.Representation, pattern);
                }
            }
            return this.dicPatterns[bitIndex.Representation];
        }

        private IPattern PatternSelector(BitIndex bitIndex)
        {
            string reSrc = bitIndex.Representation;
            
            string rePattern = @"^(?'TYPE'\S+?)\s+(?'LEN'\d+?)$";
            string type;
            int length;
            int variLen;
            
            Match m = Regex.Match(reSrc, rePattern);
            // fixed field
            if (m.Success)
            {
                type = (m.Groups["TYPE"]).Value;
                length = Convert.ToInt32((m.Groups["LEN"]).Value, 10);
                if ("n".Equals(type) || "x+n".Equals(type))
                {
                    return new FixedLengthNumPattern(logger, length);
                }
                else if ("b".Equals(type))
                {
                    //hex
                    return new FixedLengthStringPattern(logger, length / 4); 
                }
                else
                {
                    return new FixedLengthStringPattern(logger, length);
                }
            }

            rePattern = @"^(?'TYPE'\S+?)\s+(?'VAR'\.+?)(?'LEN'\d+?)$";
            m = Regex.Match(reSrc, rePattern);
            if (m.Success)
            {
                type = (m.Groups["TYPE"]).Value;
                variLen = (m.Groups["VAR"].Value).Length;
                length = Convert.ToInt32((m.Groups["LEN"]).Value, 10);
                return new VariablePattern(logger, variLen, length);
            }

            rePattern = @"\s+(?'LEN'\d+?)$";
            m = Regex.Match(reSrc, rePattern);
            if (m.Success)
            {
                length = Convert.ToInt32((m.Groups["LEN"]).Value, 10);
                return new FixedLengthStringPattern(logger, length);
            }
            return null;
        }
    }
}
