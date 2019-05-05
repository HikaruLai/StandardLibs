using System.Collections.Generic;

namespace StandardLibs.Cache
{
    public interface ICacheHelper<T>
    {
        string KeyPrefix { get; set; }
        /// <summary>
        /// Add an object to cache with specified key
        /// </summary>
        /// <param name="key">A key value to asscociate with given object</param>
        /// <param name="obj">An object to store into cache</param>
        void Add(string key, T obj);

        /// <summary>
        /// Retrieve an object from cache using given key value
        /// </summary>
        /// <param name="key">A key value to retrieve object from cache</param>        
        /// <returns>An object that is associated with given key value</returns>
        T Get(string key);

        /// <summary>
        /// Remove an object from cache using given key value
        /// </summary>
        /// <param name="key">A key value to remove item from cache</param>
        void Remove(string key);

        /// <summary>
        /// List all objects present in cache
        /// </summary>
        /// <returns>List of objects from cache</returns>
        IList<T> GetAll();

        IList<string> GetAllKeys();

        long Count();

        bool Exists(string key);
    }
}
