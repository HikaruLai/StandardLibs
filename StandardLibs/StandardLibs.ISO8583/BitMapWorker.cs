using StandardLibs.Utility;

namespace StandardLibs.ISO8583
{
    public class BitMapWorker : IBitMapWorker
    {
        public IBitConverter BitConverter { private get; set; }
        public IHexConverter HexConverter { private get; set; }

        public BitMapWorker(IHexConverter hexConverter, IBitConverter bitConverter)
        {
            this.HexConverter = hexConverter;
            this.BitConverter = bitConverter;
        }

        public bool HasExtend(string hexStr)
        {
            byte first = this.HexConverter.HexToBytes(hexStr.Substring(0, 2))[0];
            return (0x80 == (first & 0x80));
        }

        public string GetBitMapBits(string hexStr)
        {
            return this.BitConverter.HexToBits(hexStr);
        }

        public string GetBitMapHex(string bitStr)
        {
            return this.BitConverter.BitsToHex(bitStr);
        }
    }
}
