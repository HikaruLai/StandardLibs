using System;
using System.Collections.Generic;
using System.Text;

namespace StandardLibs.ISO8583
{
    public interface IBitMapHelper
    {
        bool HasExtend(string hexStr);
        string GetBitMapBits(string hexStr);
        string GetBitMapHex(string bitStr);
    }
}
