using Microsoft.Extensions.Logging;
using LightInject;
using StandardLibs.Utility;
using StandardLibs.Dna;
using StandardLibs.Dna.Construction;

namespace StandardLibs.ISO8583.UnitTest
{
    public class ISO8583TestsFixture
    {
        private static ILoggerFactory loggerFactory = new LoggerFactory();

        public ISO8583TestsFixture()
        {
            Framework.Construct<DefaultFrameworkConstruction>().Build();
            Framework.Container
                .Register<IHexConverter>(
                    f => new HexConverter(),
                    "hexConverter",
                    new PerContainerLifetime()
                 )
                .Register<IBitConverter>(
                    f => new BitConverter(f.GetInstance<IHexConverter>("hexConverter")),
                    "bitConverter",
                    new PerContainerLifetime()
                 )
                .Register<IBitMapWorker>(
                    f => new BitMapWorker(
                        hexConverter: f.GetInstance<IHexConverter>("hexConverter"),
                        bitConverter: f.GetInstance<IBitConverter>("bitConverter")
                    ),
                    "bitMapWorker",
                    new PerContainerLifetime()
                 )
                .Register<BitWorker>(
                    f => new BitWorker(logger: loggerFactory.CreateLogger<BitWorker>(), iso8583Info: f.GetInstance<IIso8583Info>("commonInfo")),
                    "commonBitWorker",
                    new PerContainerLifetime()
                 ).Register<BitWorker>(
                    f => new BitWorker(logger: loggerFactory.CreateLogger<BitWorker>(), iso8583Info: f.GetInstance<IIso8583Info>("df61Info")),
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
                )
                .Register<IMessageWorker>(
                    f => new MainMessageWorker(
                        logger: loggerFactory.CreateLogger<MainMessageWorker>(),
                        bitMapWorker: f.GetInstance<IBitMapWorker>("bitMapWorker"),
                        bitWorker: f.GetInstance<BitWorker>("commonBitWorker")
                    ),
                    "mainMsgWorker",
                    new PerContainerLifetime()
                 )
                .Register<IMessageWorker>(
                    f => new Df61MessageWorker(
                        logger: loggerFactory.CreateLogger<Df61MessageWorker>(),
                        bitMapWorker: f.GetInstance<IBitMapWorker>("bitMapWorker"),
                        bitWorker: f.GetInstance<BitWorker>("df61BitWorker")
                    ),
                    "df61MsgWorker",
                    new PerContainerLifetime()
                 );
        }
    }
}
