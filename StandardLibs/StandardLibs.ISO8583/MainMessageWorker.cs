using System;
using System.Text.RegularExpressions;
using NLog;

namespace StandardLibs.ISO8583
{
    public class MainMessageWorker : IMessageWorker
    {
        private ILogger logger { get; set; }
        public IBitMapWorker BitMapWorker { private get; set; }
        public BitWorker BitWorker { private get; set; }
        public string HasHeader { get; set; }

        public MainMessageWorker(ILogger logger, IBitMapWorker bitMapWorker, BitWorker bitWorker)
        {
            this.logger = logger;
            this.BitMapWorker = bitMapWorker;
            this.BitWorker = bitWorker;
        }

        public MessageContext Parse(string msg)
        {
            try
            {
                MessageContext msgContextMain = new MessageContext();
                if ((null != this.HasHeader) && ("Y".Equals(this.HasHeader)))
                {
                    this.parseHeader(msg, msgContextMain);
                }
                else
                {
                    msgContextMain.SrcMessage = msg;
                    msgContextMain.MessageSize = msg.Length;
                }

                msgContextMain.Start = 0;
                msgContextMain.FromTo = msgContextMain.SrcMessage.Substring(msgContextMain.Start, 8);
                msgContextMain.Start = msgContextMain.Start + 8;
                
                msgContextMain.Mti = msgContextMain.SrcMessage.Substring(msgContextMain.Start, 4);
                msgContextMain.Start = msgContextMain.Start + 4;
                
                bool hasSecondMap = this.BitMapWorker.HasExtend(msgContextMain.SrcMessage.Substring(msgContextMain.Start, 2));
                int bitLength = hasSecondMap ? 32 : 16;
                string bitMapHex = msgContextMain.SrcMessage.Substring(msgContextMain.Start, bitLength);
                msgContextMain.Start = msgContextMain.Start + bitLength;
                msgContextMain.BitMap = this.BitMapWorker.GetBitMapBits(bitMapHex);
                
                for (int fno = 2; fno <= msgContextMain.BitMap.Length; fno++)
                {
                    if (msgContextMain.HasField(fno))
                    {
                        BitIndex bi = this.BitWorker[fno];
                        IPattern pattern = bi.PatternWorker;
                        msgContextMain.CurrentField.NextFuncNo = fno;
                        pattern.Parse(msgContextMain);
                    }
                }
                return msgContextMain;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return null;
        }

        public MessageContext Build(string fromTo, string mti, string[] funcDatas)
        {
            try
            {
                MessageContext msgContextMain = new MessageContext();
                msgContextMain.FromTo = fromTo;
                msgContextMain.Mti = mti;
                // BitMap, always has SecondMap in Common
                if (65 == funcDatas.Length)
                {
                    msgContextMain.BitMap = "0";
                }
                else
                {
                    msgContextMain.BitMap = "1";
                }

                string[] srcList = new string[funcDatas.Length];
                for (int i = 0; i < srcList.Length; i++)
                {
                    srcList[i] = funcDatas[i];
                }
                
                for (int fno = 2; fno < srcList.Length; fno++)
                {
                    if (!((null == srcList[fno]) || ("".Equals(srcList[fno]))))
                    {
                        BitIndex bi = this.BitWorker[fno];
                        IPattern pattern = bi.PatternWorker;
                        msgContextMain.AddField(fno, srcList[fno]);
                        pattern.Build(msgContextMain);
                    }
                    else
                    {
                        msgContextMain.BitMap += "0";
                    }
                }
                msgContextMain.SrcMessage = msgContextMain.FromTo + msgContextMain.Mti + this.BitMapWorker.GetBitMapHex(msgContextMain.BitMap) + msgContextMain.SrcMessage;
                msgContextMain.MessageSize = msgContextMain.SrcMessage.Length;

                return msgContextMain;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return null;
        }

        private void parseHeader(string msg, MessageContext msgContext)
        {
            string pattern = @"^{(?'HB'[^,]*),(?'LB'[^}]*)}(?'MSG'.*)$";
            Match m = Regex.Match(msg, pattern);

            if (m.Success)
            {
                msgContext.MessageSize = Convert.ToInt32(m.Groups["HB"].Value, 10) * 256 + Convert.ToInt32(m.Groups["LB"].Value, 10);
                msgContext.SrcMessage = m.Groups["MSG"].Value;
                if (msgContext.MessageSize < msgContext.SrcMessage.Length)
                {
                    msgContext.SrcMessage = msgContext.SrcMessage.Substring(0, msgContext.MessageSize);
                }
            }
            else
            {
                string errMsg = string.Format("Error format message: {0}", msg);
                logger.Error(errMsg);
                throw new Exception(errMsg);
            }
        }
    }
}
