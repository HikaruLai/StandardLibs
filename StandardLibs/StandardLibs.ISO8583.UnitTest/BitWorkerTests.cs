using Microsoft.Extensions.Logging;
using LightInject;
using NLog;
using Xunit;
using StandardLibs.Dna;
using StandardLibs.Dna.Construction;

namespace StandardLibs.ISO8583.UnitTest
{
    public class BitWorkerTests
    {
        private IServiceContainer ctx;
        private BitWorker commonBitWorker = null;
        private BitWorker df61BitWorker = null;
        private static ILoggerFactory loggerFactory = new LoggerFactory();
        private static NLog.ILogger logger = LogManager.GetCurrentClassLogger(); 
        public BitWorkerTests()
        {
            Framework.Construct<DefaultFrameworkConstruction>().Build();
            Framework.Container
                .Register<BitWorker>(
                    f => new BitWorker(logger: loggerFactory.CreateLogger<BitWorker>(), iso8583Info: f.GetInstance<IIso8583Info>("commonInfoGetter")),
                    "commonBitWorker",
                    new PerContainerLifetime()
                 ).Register<BitWorker>(
                    f => new BitWorker(logger: loggerFactory.CreateLogger<BitWorker>(), iso8583Info: f.GetInstance<IIso8583Info>("df61InfoGetter")),
                    "df61BitWorker",
                    new PerContainerLifetime()
                 )
                 .Register<IIso8583Info>(
                    f => new Iso8583Info(
                        logger: loggerFactory.CreateLogger<Iso8583Info>(),
                        assemblyName: "StandardLibs.ISO8583",
                        cfgFileName: "StandardLibs.ISO8583.Config.iso8583Fn.xml",
                        xPath: @"//Message[@name='Common' and @peer='Common']"
                    ),
                    "commonInfoGetter",
                    new PerContainerLifetime()
                )
               .Register<IIso8583Info>(
                    f => new Iso8583Info(
                        logger: loggerFactory.CreateLogger<Iso8583Info>(),
                        assemblyName: "StandardLibs.ISO8583",
                        cfgFileName: "StandardLibs.ISO8583.Config.iso8583Fn.xml",
                        xPath: @"//Message[@name='DF61' and @peer='Common']"
                    ),
                    "df61InfoGetter",
                    new PerContainerLifetime()
                );
            this.ctx = Framework.Container;
            this.commonBitWorker = this.ctx.GetInstance<BitWorker>("commonBitWorker");
            this.df61BitWorker = this.ctx.GetInstance<BitWorker>("df61BitWorker");
        }

        [Fact]
        public void TestGetCommonInfos()
        {
            for (int i = 0; i < this.commonBitWorker.Length(); i++)
            {
                BitIndex bi = this.commonBitWorker[i];
                logger.Debug($"{0}", bi);
            }
        }

        [Fact]
        public void TestGetDf61Infos()
        {
            for (int i = 0; i < this.df61BitWorker.Length(); i++)
            {
                BitIndex bi = this.df61BitWorker[i];
                logger.Debug($"{0}", bi);
            }
        }
    }
}
