using StandardLibs.Utility;

namespace StandardLibs.ISO8583
{
    public class BitIndex : JsonComparable 
    {
        public int Id { get; set; }
        public string Representation { get; set; }
        public string Name { get; set; }
        public IPattern PatternWorker { get; set; }
    }
}
