
namespace StandardLibs.ISO8583
{
    /// <summary>
    /// The 'Flyweight' interface
    /// </summary>
    public interface IPattern
    {
        void Build(MessageContext msgContext);
        void Parse(MessageContext msgContext);
    }
}
