using System;
using System.Collections.Generic;
using System.Text;

namespace StandardLibs.Utility
{
    public abstract class AbsTagGenWorker<T> : ITagGenWorker<T>
    {
        /// <summary>
        /// key: value =>  ex. IccNo : NegaInfo
        /// </summary>
        protected IDictionary<string, T> tiDic = new Dictionary<string, T>();

        /// <summary>
        /// key: value => ex. IccNo: index of nega list
        /// </summary>
        protected IDictionary<string, int> idxDic = new Dictionary<string, int>();

        /// <summary>
        /// list of iccNo
        /// </summary>
        protected IList<string> tagIndexList = new List<string>();

        public virtual int Length()
        {
            return this.tagIndexList.Count;
        }

        // Indexer
        public virtual T this[int index]
        {
            get
            {
                try
                {
                    return this.tiDic[this.tagIndexList[index]];
                }
                catch
                {
                    return default(T);
                }
            }
            set
            {
                if (index < this.tagIndexList.Count)
                {
                    this.tiDic.Remove(this.tagIndexList[index]);
                    this.idxDic.Remove(this.tagIndexList[index]);
                    this.tagIndexList.RemoveAt(index);
                }
                // must implement this...                    
                this.SetTag(index, value);
            }
        }

        public virtual T this[string tagName]
        {
            get
            {
                if (this.tiDic.ContainsKey(tagName))
                {
                    return this.tiDic[tagName];
                }
                else
                {
                    return default(T);
                }
            }
            set
            {
                if (this.tiDic.ContainsKey(tagName))
                {
                    this.tiDic[tagName] = value;
                }
                else // append it 
                {
                    int index = this.tagIndexList.Count;
                    this.tagIndexList.Insert(index, tagName);
                    this.tiDic.Add(tagName, value);
                    this.idxDic.Add(tagName, index);
                }
            }
        }

        public virtual void SetTagList(IList<T> tList)
        {
            for (int idx = 0; idx < tList.Count; idx++)
            {
                this[idx] = tList[idx];
            }
        }

        public abstract void SetTag(int index, T tag);

        public abstract string GetMsgTypeName();
    }
}
