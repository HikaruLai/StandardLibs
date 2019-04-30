using System;
using NLog;

namespace StandardLibs.ISO8583
{
    /// <summary>
    /// The 'ConcreteFlyweight' class , for fixed length String fields
    /// </summary>
    public class FixedLengthStringPattern : IPattern
    {
        private ILogger logger { get; set; }

        private int length = 0;

        public FixedLengthStringPattern(ILogger logger, int length)
        {
            this.length = length;
            this.logger = logger;
        }

        public void Build(MessageContext msgContext)
        {
            try
            {
                IsoField currentField = msgContext.CurrentField;
                string padData = currentField.FuncData.PadRight(this.length, ' ');
                currentField.FuncData = padData.Substring(0, this.length);
                msgContext.SrcMessage += currentField.FuncData;
                msgContext.Start += this.length;
                msgContext.BitMap += "1";
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw ex;
            }
        }

        public void Parse(MessageContext msgContext)
        {
            try
            {
                //use isoField.NextFNo as current field number
                int currentFuncNo = msgContext.CurrentField.NextFuncNo;
                string currentFuncData = msgContext.SrcMessage.Substring(msgContext.Start, this.length);
                msgContext.Start += this.length;
                msgContext.AddField(currentFuncNo, currentFuncData);
            }
            catch
            {
                throw;
            }
        }
    }
}
