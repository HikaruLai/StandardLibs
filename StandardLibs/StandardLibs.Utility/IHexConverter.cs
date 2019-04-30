using System;
using System.Collections.Generic;
using System.Text;

namespace StandardLibs.Utility
{
    public interface IHexConverter
    {
        /// <summary>
        /// Unpack encoded string to Hex string according to specified encoding
        /// </summary>
        /// <param name="str">string data to be unpacked(with specified encoding)</param>
        /// <returns>hex string</returns>
        string StrToHex(string str);

        /// <summary>
        /// Pack hex string to encoded string according to specified encoding
        /// </summary>
        /// <param name="hexStr">hex string to be packed</param>
        /// <returns>string data(with specified encoding)</returns>
        string HexToStr(string hexStr);

        /// <summary>
        /// Pack hex string to byte array
        /// </summary>
        /// <param name="hexStr">hex string</param>
        /// <returns>byte array</returns>
        byte[] HexToBytes(string hexStr);

        /// <summary>
        /// Pack hex byte array to byte array
        /// </summary>
        /// <param name="hexBytes">hex byte array( [0..9A..Fa..f] )</param>
        /// <returns>byte array</returns>
        byte[] HexToBytes(byte[] hexBytes);

        /// <summary>
        /// Unpack byte array to hex string
        /// </summary>
        /// <param name="dataBytes">byte array to be unpacked</param>
        /// <returns>Hex string</returns>
        string BytesToHex(byte[] dataBytes);

        /// <summary>
        ///   unpack 1 byte to 2 hex
        /// </summary>
        /// <param name="b">byte</param>
        /// <returns>hex</returns>
        string ByteToHex(byte b);
    }
}
