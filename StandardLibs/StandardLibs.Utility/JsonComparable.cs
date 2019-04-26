using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StandardLibs.Utility
{
    /// <summary>
    /// 將子類別instance內容以json方式列出 
    /// </summary>
    [Serializable()]
    public abstract class JsonComparable : IComparable
    {
        /// <summary>
        /// 逐一比對各property value,若均相同則回傳 true
        /// </summary>
        /// <param name="obj">待比對物件</param>
        /// <returns>true:各屬性均相同</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!this.GetType().IsInstanceOfType(obj))
            {
                return false;
            }

            PropertyInfo[] pi = this.GetType().GetProperties();

            try
            {
                foreach (PropertyInfo p in pi)
                {
                    if (p.GetValue(this, null) == null && p.GetValue(obj, null) == null)
                    {
                        continue;
                    }

                    if (p.PropertyType == typeof(byte[]))
                    {
                        byte[] thisVal = (byte[])p.GetValue(this, null);
                        byte[] objVal = (byte[])p.GetValue(obj, null);
                        if (thisVal.Length != objVal.Length)
                        {
                            return false;
                        }
                        for (int pt = 0; pt < thisVal.Length; pt++)
                        {
                            if (!thisVal[pt].Equals(objVal[pt]))
                            {
                                return false;
                            }
                        }
                    }
                    else if (!p.GetValue(this, null).Equals(p.GetValue(obj, null)))
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 取得物件 Hash Code, ref:
        /// 若程式有重新定義Operator==，則我們必須跟著重新定義Object.GetHashCode
        /// </summary>
        /// <returns>value of hash code</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// 重新定義Object.ToString
        /// </summary>
        /// <returns>json to serialize string</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// 傳出該 Dependent Object 之排序字串值
        /// </summary>
        /// <returns>Dependent Object 之排序字串</returns>
        public virtual string GetSortKey()
        {
            PropertyInfo p = (this.GetType().GetProperties())[0];
            return p.GetValue(this, null).ToString();
        }

        /// <summary>
        /// 根據排序字串比較該 Dependent Object 與傳入物件之大小
        /// </summary>
        /// <param name="obj">傳入物件,需為相同之 Dependent Object</param>
        /// <returns>int 傳回值 負值:本物件較小; 0:兩物件相同; 正值:本物件較大</returns>
        public virtual int CompareTo(object obj)
        {
            if ((obj != null) && (this.GetType().IsInstanceOfType(obj)))
            {
                JsonComparable ci = (JsonComparable)obj;
                return this.GetSortKey().CompareTo(ci.GetSortKey());
            }
            else
            {
                throw new Exception(
                     "傳入物件型別錯誤,無法比較: " +
                     "parameter class " +
                     this.GetType().Name +
                     " is not the same of my class " +
                     obj.GetType().Name
                );
            }
        }
    }
}
