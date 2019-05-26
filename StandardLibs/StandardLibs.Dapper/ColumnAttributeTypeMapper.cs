using System.Linq;
using Dapper;

namespace StandardLibs.Dapper
{
    /// <summary>
    /// Uses the Name value of the <see cref="ColumnAttribute"/> specified to determine
    /// the association between the name of the column in the query results and the member to
    /// which it will be extracted. If no column mapping is present all members are mapped as
    /// usual.
    /// ref: https://gist.github.com/kalebpederson
    /// </summary>
    /// <typeparam name="T">The type of the object that this association between the mapper applies to.</typeparam>
    public class ColumnAttributeTypeMapper<T> : FallbackTypeMapper
    {
        public ColumnAttributeTypeMapper() : base(
            new SqlMapper.ITypeMap[] {
                new CustomPropertyTypeMap(typeof(T), (type, columnName) =>
                    type.GetProperties().FirstOrDefault(prop => 
                        prop.GetCustomAttributes(false)
                            .OfType<ColumnAttribute>()
                            .Any(attr => attr.Name == columnName)
                    )
                ),
                new DefaultTypeMap(typeof(T))
            }
        )
        { }
    }
}
