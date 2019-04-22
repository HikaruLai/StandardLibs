using System;
using System.Collections.Generic;
using System.Text;

namespace StandardLibs.Utility
{
    public interface ISerializer<T>
    {
        /// <summary>
        /// Serialize POCO entity to string
        /// </summary>
        /// <param name="entity">POCO entity</param>
        /// <returns>utf8 string</returns>
        string Serialize(T entity);

        /// <summary>
        /// Serialize POCO entity to byte[]
        /// </summary>
        /// <param name="entity">POCO entity</param>
        /// <returns>byte[]</returns>
        byte[] Serialize2Bytes(T entity);

        /// <summary>
        /// Deserialize string back to POCO
        /// </summary>
        /// <param name="serialized">serialized string</param>
        /// <returns>POCO entity</returns>
        T Deserialize(string serialized);

        /// <summary>
        /// Deserialize byte[] back to POCO
        /// </summary>
        /// <param name="serialized">serialized byte[]</param>
        /// <returns>POCO entity</returns>
        T Deserialize(byte[] serialized);
    }
}
