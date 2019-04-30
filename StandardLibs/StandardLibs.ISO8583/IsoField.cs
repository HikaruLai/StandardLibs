using StandardLibs.Utility;

namespace StandardLibs.ISO8583
{
    public class IsoField : JsonComparable
    {
        public virtual int FuncNo { get; set; }
        public virtual string FuncData { get; set; }
        public virtual int NextFuncNo { get; set; }
    }
}
