using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using LightInject;
using NLog;
using Xunit;
using StandardLibs.Dna;
using StandardLibs.Dna.Construction;


namespace StandardLibs.ISO8583.UnitTest
{
    public class Iso8583InfoTests
    {
        private IServiceContainer ctx;
        private IIso8583Info commonInfo = null;
        private IIso8583Info df61Info = null;
        private static ILoggerFactory loggerFactory = new LoggerFactory();
        private static NLog.ILogger logger = LogManager.GetCurrentClassLogger();

        public Iso8583InfoTests()
        {
            Framework.Construct<DefaultFrameworkConstruction>().Build();
            Framework.Container
               .Register<IIso8583Info>(
                    f => new Iso8583Info(
                        logger: loggerFactory.CreateLogger<Iso8583Info>(),
                        assemblyName: "StandardLibs.ISO8583",
                        cfgFileName: "StandardLibs.ISO8583.Config.iso8583Fn.xml",
                        xPath: @"//Message[@name='Common' and @peer='Common']"
                    ),
                    "commonInfo",
                    new PerContainerLifetime()
                )
               .Register<IIso8583Info>(
                    f => new Iso8583Info(
                        logger: loggerFactory.CreateLogger<Iso8583Info>(),
                        assemblyName: "StandardLibs.ISO8583",
                        cfgFileName: "StandardLibs.ISO8583.Config.iso8583Fn.xml",
                        xPath: @"//Message[@name='DF61' and @peer='Common']"
                    ),
                    "df61Info",
                    new PerContainerLifetime()
                );
            this.ctx = Framework.Container;
            this.commonInfo = this.ctx.GetInstance<IIso8583Info>("commonInfo");
            this.df61Info = this.ctx.GetInstance<IIso8583Info>("df61Info");
        }

        [Fact]
        public void TestCommonGetInfos()
        {
            IList<BitIndex> bitList = this.commonInfo.GetPosInfos();
            foreach (BitIndex bi in bitList)
            {
                logger.Debug($"{0}", bi);
            }
        }

        [Fact]
        public void TestD61GetInfos()
        {
            IList<BitIndex> bitList = this.df61Info.GetPosInfos();
            foreach (BitIndex bi in bitList)
            {
                logger.Debug($"{0}", bi);
            }
        }
    }
}
