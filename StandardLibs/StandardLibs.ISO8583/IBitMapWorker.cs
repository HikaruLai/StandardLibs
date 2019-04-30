
namespace StandardLibs.ISO8583
{
    public interface IBitMapWorker
    {
        bool HasExtend(string hexStr);
        string GetBitMapBits(string hexStr);
        string GetBitMapHex(string bitStr);
    }
}
