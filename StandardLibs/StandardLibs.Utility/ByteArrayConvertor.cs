using System;
using System.Linq;
using Newtonsoft.Json;

namespace StandardLibs.Utility
{
    /// <summary>
    /// Byte array converter for json
    /// </summary>
    public class ByteArrayConvertor : Newtonsoft.Json.JsonConverter
    {
        private readonly string prefix = string.Empty; // "0x","{HEX}";
        private readonly IHexConverter hexConverter = new HexConverter();
        public override bool CanConvert(Type objectType)
        {
            return typeof(byte[]).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var hex = serializer.Deserialize<string>(reader);
                if (string.IsNullOrEmpty(hex))
                {
                    return Enumerable.Empty<byte>();
                }
                else
                {
                    int start = 0;
                    if ((!string.Empty.Equals(this.prefix)) && hex.StartsWith(this.prefix))
                    {
                        start = this.prefix.Length;
                    }
                    //return Enumerable.Range( start, hex.Length )
                    //                 .Where( x => x % 2 == 0 )
                    //                 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                    //                 .ToArray();
                    return this.hexConverter.HexToBytes(hex.Substring(start));
                }

            }
            return Enumerable.Empty<byte>();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string hexStr = string.Empty;
            if (value is byte[] bytes)
            {
                hexStr = this.hexConverter.BytesToHex(bytes);
            }
            serializer.Serialize(writer, this.prefix + hexStr);
        }
    }
}
