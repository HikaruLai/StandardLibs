using System.Collections.Generic;

namespace StandardLibs.ISO8583
{
    public interface IIso8583Info
    {
        IList<BitIndex> GetPosInfos();
        void ResetInfos();
    }
}
