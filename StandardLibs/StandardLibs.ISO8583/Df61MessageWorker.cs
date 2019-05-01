using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace StandardLibs.ISO8583
{
    public class Df61MessageWorker : IMessageWorker
    {
        private ILogger logger { get; set; }
        public IBitMapWorker BitMapWorker { private get; set; }
        public BitWorker Df61BitWorker { private get; set; }

        public Df61MessageWorker(ILogger<Df61MessageWorker> logger, IBitMapWorker bitMapWorker, BitWorker bitWorker)
        {
            this.logger = logger;
            this.BitMapWorker = bitMapWorker;
            this.Df61BitWorker = bitWorker;
        }

        public MessageContext Parse(string msg)
        {
            try
            {
                MessageContext msgContext = new MessageContext();
                // only 64 fields,No Second map
                msgContext.BitMap = "0";
                msgContext.SrcMessage = msg;
                
                int bitLength = 16;
                string bitMapHex = msgContext.SrcMessage.Substring(msgContext.Start, bitLength);
                msgContext.Start = msgContext.Start + bitLength;
                msgContext.BitMap = this.BitMapWorker.GetBitMapBits(bitMapHex);
                
                for (int fno = 2; fno <= msgContext.BitMap.Length; fno++)
                {
                    if (msgContext.HasField(fno))
                    {
                        BitIndex bi = this.Df61BitWorker[fno];
                        IPattern pattern = bi.PatternWorker;
                        msgContext.CurrentField.NextFuncNo = fno;
                        pattern.Parse(msgContext);
                    }
                }
                return msgContext;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
            return null;
        }

        public MessageContext Build(string fromTo, string mti, string[] funcDatas)
        {
            try
            {
                MessageContext msgContext = new MessageContext();
                // only 64 fields
                msgContext.BitMap = "0";
                string[] srcList = new string[this.Df61BitWorker.Length()];
                for (int i = 0; i < srcList.Length; i++)
                {
                    if (i < funcDatas.Length)
                    {
                        srcList[i] = funcDatas[i];
                    }
                    else
                    {
                        srcList[i] = "";
                    }
                }
                
                for (int fno = 2; fno < srcList.Length; fno++)
                {
                    if (!((null == srcList[fno]) || ("".Equals(srcList[fno]))))
                    {
                        BitIndex bi = this.Df61BitWorker[fno];
                        IPattern pattern = bi.PatternWorker;
                        msgContext.AddField(fno, srcList[fno]);
                        pattern.Build(msgContext);
                    }
                    else
                    {
                        msgContext.BitMap += "0";
                    }
                }
                msgContext.SrcMessage = this.BitMapWorker.GetBitMapHex(msgContext.BitMap) + msgContext.SrcMessage;

                return msgContext;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
            return null;
        }
    }
}
