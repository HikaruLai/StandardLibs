
namespace StandardLibs.ISO8583
{
    public interface IMessageWorker
    {
        MessageContext Parse(string msg);
        MessageContext Build(string fromTo, string mti, string[] srcList);
    }
}
