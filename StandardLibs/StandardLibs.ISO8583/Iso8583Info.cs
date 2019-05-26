using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;

namespace StandardLibs.ISO8583
{
    public class Iso8583Info : IIso8583Info
    {
        private XElement root { get; set; }
        private ILogger logger { get; set; }
        private IList<BitIndex> posList { get; set; }

        public Iso8583Info(ILogger logger, string assemblyName, string cfgFileName, string xPath)
        {
            this.logger = logger;
            posList = new List<BitIndex>();
            this.setPosInfo(assemblyName, cfgFileName);
            this.getPosInfo(xPath);
        }

        private void setPosInfo(string assemblyName, string cfgFileName)
        {
            try
            {
                Assembly assembly = Assembly.Load(assemblyName);
                using (Stream s = assembly.GetManifestResourceStream(cfgFileName))
                {
                    this.root = XDocument.Load(s).Root;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.StackTrace);
            }
        }

        private void setPosInfo(Stream st)
        {
            try
            {
                this.root = XDocument.Load(st).Root;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.StackTrace);
            }
        }

        private void getPosInfo(string xPathStr)
        {
            XElement xtr = null;
            try
            {
                xtr = this.root.XPathSelectElement(xPathStr);
                xtr.Element("BITS").Elements("BIT").ToList()
                    .ForEach(x =>
                    {
                        this.posList.Add(new BitIndex
                        {
                            Id = (int)x.Attribute("id"),
                            Representation = (string)x.Attribute("representation"),
                            Name = (string)x.Attribute("name")
                        });
                    });
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        public IList<BitIndex> GetPosInfos()
        {
            return this.posList;
        }

        public void ResetInfos()
        {
            this.posList.Clear();
        }
    }
}
