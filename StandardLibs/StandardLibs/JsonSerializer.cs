using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace StandardLibs.Utility
{
    public class JsonSerializer<T> : ISerializer<T>
    {
        public byte[] Serialize2Bytes(T entity)
        {
            return Encoding.UTF8.GetBytes(this.Serialize(entity));
        }

        public string Serialize(T entity)
        {
            return JsonConvert.SerializeObject(entity);
        }

        public T Deserialize(byte[] serialized)
        {
            return this.Deserialize(Encoding.UTF8.GetString(serialized));
        }

        public T Deserialize(string serialized)
        {
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
