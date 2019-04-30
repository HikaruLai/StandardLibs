using System;
using NLog;

namespace StandardLibs.ISO8583
{
    /// <summary>
    /// The 'ConcreteFlyweight' class , for none numeric fields
    /// </summary>
    public class VariablePattern : IPattern
    {
        private ILogger logger { get; set; }

        private int maxLength = 0;

        private int lengthSize = 0;

        public VariablePattern(ILogger logger, int lengthSize, int maxLength)
        {
            this.logger = logger;
            this.lengthSize = lengthSize;
            this.maxLength = maxLength;
        }

        public void Build(MessageContext msgContext)
        {
            try
            {
                // get raw data length
                IsoField currentField = msgContext.CurrentField;
                int dataLength = currentField.FuncData.Length;
                string padLi = Convert.ToString(dataLength).PadLeft(this.lengthSize, '0');
                string result = padLi + currentField.FuncData;
                currentField.FuncData = result;
                msgContext.SrcMessage += result;
                msgContext.Start += result.Length;
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
                int dataLength = Convert.ToInt32(msgContext.SrcMessage.Substring(msgContext.Start, this.lengthSize));
                msgContext.Start += this.lengthSize;
                string currentFuncData = msgContext.SrcMessage.Substring(msgContext.Start, dataLength);
                msgContext.Start += dataLength;
                msgContext.AddField(currentFuncNo, currentFuncData);
            }
            catch
            {
                throw;
            }
        }
    }
}
