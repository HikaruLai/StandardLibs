using System;
using StandardLibs.Utility;
using NLog;

namespace StandardLibs.ISO8583
{
    public class BitWorker : AbsTagGenWorker<BitIndex>
    {
        private ILogger logger { get; set; }

        public BitWorker(ILogger logger, IIso8583Info iso8583Info)
        {
            this.logger = logger;
            this.SetTagList(iso8583Info.GetPosInfos());
        }

        public override void SetTag(int index, BitIndex bitIndex)
        {
            if (index < this.tagIndexList.Count)
            {
                this.tiDic.Remove(this.tagIndexList[index]);
                this.idxDic.Remove(this.tagIndexList[index]);
                this.tagIndexList.RemoveAt(index);
            }
            // must implement this...
            logger.Debug("SetTag[{0}]: {1}", index, bitIndex);
            // add pattern
            bitIndex.PatternWorker = PatternFactory.GetInstance().GetPattern(bitIndex);
            this.tiDic.Add(string.Format("{0:D3}", bitIndex.Id), bitIndex);
            // buid index
            this.tagIndexList.Insert(index, string.Format("{0:D3}", bitIndex.Id));
            this.idxDic.Add(string.Format("{0:D3}", bitIndex.Id), index);
        }

        public override string GetMsgTypeName()
        {
            throw new NotImplementedException();
        }
    }
}
