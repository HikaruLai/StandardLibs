using System;
using System.Collections.Generic;
using System.Text;

namespace StandardLibs.Utility
{
    public interface IBitConverter
    {
        /// <summary>
        /// Transfer Hex string to bit string
        /// </summary>
        /// <param name="hex">hex string</param>
        /// <returns>bit string contains only [0,1]</returns>
        string HexToBits(string hex);

        /// <summary>
        /// Transfer bit string to Hex string
        /// </summary>
        /// <param name="bits">bit string contains only [0,1]</param>
        /// <returns>hex string</returns>
        string BitsToHex(string bits);

        /// <summary>
        ///  Transfer Hex string to bit array each element contains only [0,1]
        /// </summary>
        /// <param name="hex">hex string</param>
        /// <returns>bit Array</returns>
        int[] HexToBitArr(string hex);

        /// <summary>
        ///  Transfer Bit Array to hex string
        /// </summary>
        /// <param name="bitArr">bit arrary</param>
        /// <returns>hex string</returns>
        string BitArrToHex(int[] bitArr);
    }
}
