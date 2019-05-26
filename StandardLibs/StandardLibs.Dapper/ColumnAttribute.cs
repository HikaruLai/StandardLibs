using System;

namespace StandardLibs.Dapper
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]  // default true
    public class ColumnAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
