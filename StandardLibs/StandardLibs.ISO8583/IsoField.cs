using System;
using System.Collections.Generic;
using System.Text;

namespace StandardLibs.ISO8583
{
    public class IsoField
    {
        public virtual int FuncNo { get; set; }
        public virtual string FuncData { get; set; }
        public virtual int NextFuncNo { get; set; }
    }
}
