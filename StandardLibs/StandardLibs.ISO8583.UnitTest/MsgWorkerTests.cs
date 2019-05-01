using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using LightInject;
using NLog;
using Xunit;
using StandardLibs.Utility;
using StandardLibs.Dna;
using StandardLibs.Dna.Construction;

namespace StandardLibs.ISO8583.UnitTest
{
    public class MsgWorkerTests
    {
        private IServiceContainer ctx;
        private IMessageWorker mainMsgWorker = null;
        private IMessageWorker df61MsgWorker = null;
        private ISerializer<IsoField> jworker = new JsonSerializer<IsoField>();
        private static ILoggerFactory loggerFactory = new LoggerFactory();
        private static NLog.ILogger logger = LogManager.GetCurrentClassLogger();

        public MsgWorkerTests()
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

            this.ctx = Framework.Container;
            this.mainMsgWorker = this.ctx.GetInstance<IMessageWorker>("mainMsgWorker");
            this.df61MsgWorker = this.ctx.GetInstance<IMessageWorker>("df61MsgWorker");
        }

        [Fact]
        public void Test01Build0100Df61()
        {
            string expected = @"3160000020000000000000010012015012818300571116801234567890000005586000000000000000001";
            string[] srcList = new string[65]; // primary + 1           
            /*
            03 : 00000001
            04 : 001
            08 : 20150128183005
            10 : 7111680123456789
            11 : 55
            35 : 86000000000000000001
            */
            srcList[3] = "00000001";
            srcList[4] = "001";
            srcList[8] = "20150128183005";
            srcList[10] = "7111680123456789";
            srcList[11] = "55";
            srcList[35] = "86000000000000000001";
            
            MessageContext df61MsgContext = this.df61MsgWorker.Build(null, null, srcList);
            
            IList<IsoField> fList = df61MsgContext.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                logger.Debug(field);
            }
            logger.Debug(df61MsgContext.SrcMessage);
            Assert.Equal(expected, df61MsgContext.SrcMessage);
        }

        [Fact]
        public void Test02Parse0100Df61()
        {
            IList<IsoField> expected = new List<IsoField>();
            /*
            03 : 00000001
            04 : 001
            08 : 20150128183005
            10 : 7111680123456789
            11 : 55
            35 : 86000000000000000001
            */
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":0,\"FuncData\":null,\"NextFuncNo\":3}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":3,\"FuncData\":\"00000001\",\"NextFuncNo\":4}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":4,\"FuncData\":\"001\",\"NextFuncNo\":8}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":8,\"FuncData\":\"20150128183005\",\"NextFuncNo\":10}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":10,\"FuncData\":\"7111680123456789\",\"NextFuncNo\":11}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":11,\"FuncData\":\"00000055\",\"NextFuncNo\":35}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":35,\"FuncData\":\"86000000000000000001\",\"NextFuncNo\":-1}"));

            string msg = @"3160000020000000000000010012015012818300571116801234567890000005586000000000000000001";
            
            MessageContext msgContext = this.df61MsgWorker.Parse(msg);

            IList<IsoField> fList = msgContext.FieldList.ToList();
            for (int i = 0; i < fList.Count; i++)
            {
                IsoField field = fList[i];
                logger.Debug($"{0}", field);
                Assert.Equal(expected[i], field);
            }

        }

        [Fact]
        public void Test03Build0100()
        {
            string expected =
                //"2318888082201007220000108C000080000000000000000160000000000000000990174000000000055012818300555555510st00000001502818555555000000010000000225550030853160000020000000000000010012015012818300571116801234567890000005586000000000000000001";
                "{0,215}8888082201007220000108C00008160000000000000000990174000000000055012818300555555510st00000001502818555555000000010000000225550030853160000020000000000000010012015012818300571116801234567890000005586000000000000000001";
            string fromTo = "88880822";
            string mti = "0100";
            string[] srcList = new string[65]; // primary + 1
            /* 
            02 : 160000000000000000
            03 : 990174
            04 : 000000000055
            07 : 0128183005
            11 : 555555
            32 : 10st00000001
            37 : 502818555555
            41 : 00000001
            42 : 000000022555003
            61 : 0853160000020000000000000010012015012818300571116801234567890000005586000000000000000001*/
            srcList[2] = "0000000000000000";
            srcList[3] = "990174";
            srcList[4] = "000000000055";
            srcList[7] = "0128183005";
            srcList[11] = "555555";
            srcList[32] = "st00000001";
            srcList[37] = "502818555555";
            srcList[41] = "00000001";
            srcList[42] = "000000022555003";
            // need construct latter
            srcList[61] = this.getBuild0100Df61();
            
            MessageContext msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);
            
            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                logger.Debug(field);
            }
            //logger.Debug(msgContextMain.SrcMessage);
            logger.Debug(msgContextMain);
            Assert.Equal(expected, msgContextMain.ToString());
        }

        private string getBuild0100Df61()
        {
            string[] srcList = new string[65];
            /*
            03 : 00000001
            04 : 001
            08 : 20150128183005
            10 : 7111680123456789
            11 : 55
            35 : 86000000000000000001
            */
            srcList[3] = "00000001";
            srcList[4] = "001";
            srcList[8] = "20150128183005";
            srcList[10] = "7111680123456789";
            srcList[11] = "55";
            srcList[35] = "86000000000000000001";
            
            MessageContext msgContext = this.df61MsgWorker.Build(null, null, srcList);
            return msgContext.SrcMessage;
        }

        [Fact]
        public void Test04Parse0100()
        {
            IList<IsoField> expected = new List<IsoField>();
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":0,\"FuncData\":null,\"NextFuncNo\":2}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":2,\"FuncData\":\"0000000000000000\",\"NextFuncNo\":3}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":3,\"FuncData\":\"990174\",\"NextFuncNo\":4}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":4,\"FuncData\":\"000000000055\",\"NextFuncNo\":7}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":7,\"FuncData\":\"0128183005\",\"NextFuncNo\":11}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":11,\"FuncData\":\"555555\",\"NextFuncNo\":32}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":32,\"FuncData\":\"st00000001\",\"NextFuncNo\":37}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":37,\"FuncData\":\"502818555555\",\"NextFuncNo\":41}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":41,\"FuncData\":\"00000001\",\"NextFuncNo\":42}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":42,\"FuncData\":\"000000022555003\",\"NextFuncNo\":61}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":61,\"FuncData\":\"3160000020000000000000010012015012818300571116801234567890000005586000000000000000001\",\"NextFuncNo\":-1}"));
            byte[] head = new byte[] { 0, 215 };
            string msg =
                //@"231888808220100F220000108C000080000000000000000160000000000000000990174000000000055012818300555555510st00000001502818555555000000010000000225550030853160000020000000000000010012015012818300571116801234567890000005586000000000000000001";
                "8888082201007220000108C00008160000000000000000990174000000000055012818300555555510st00000001502818555555000000010000000225550030853160000020000000000000010012015012818300571116801234567890000005586000000000000000001";
            MessageContext msgContextMain = this.mainMsgWorker.Parse(msg);
            Assert.Equal(head[0] << 8 | head[1], msgContextMain.MessageSize);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            for (int i = 0; i < fList.Count; i++)
            {
                IsoField field = fList[i];
                logger.Debug($"{0}", field);
                Assert.Equal(expected[i], field);
            }
            
            expected.Clear();
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":0,\"FuncData\":null,\"NextFuncNo\":3}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":3,\"FuncData\":\"00000001\",\"NextFuncNo\":4}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":4,\"FuncData\":\"001\",\"NextFuncNo\":8}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":8,\"FuncData\":\"20150128183005\",\"NextFuncNo\":10}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":10,\"FuncData\":\"7111680123456789\",\"NextFuncNo\":11}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":11,\"FuncData\":\"00000055\",\"NextFuncNo\":35}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":35,\"FuncData\":\"86000000000000000001\",\"NextFuncNo\":-1}"));
            
            msg = msgContextMain.GetField(61).FuncData;
            MessageContext msgContextDf61 = this.df61MsgWorker.Parse(msg);
            fList = msgContextDf61.FieldList.ToList();
            for (int i = 0; i < fList.Count; i++)
            {
                IsoField field = fList[i];
                logger.Debug($"{0}", field);
                Assert.Equal(expected[i], field);
            }
            logger.Debug($"{0}", msgContextMain);
        }

        [Fact]
        public void Test05Build0110()
        {
            string expected =
                //@"888808220100F220000108C000080000000000000000160000000000000000990174000000000055012818300099999910st00000001502818999999000000010000000225550030853160000020000000000000010012015012818300000000000000000000000005511111111111111111111";
                //@"888808220100F220000108C000080000000000000000160000000000000000990174000000000055012818300555555510st00000001502818555555000000010000000225550030853160000020000000000000010012015012818300500000000000000000000005586000000000000000001";
                //@"145082288880110F22000010AC000000000000000000000160000000000000000990174000000000055012818300555555510st000000015028185555550000000001000000022555003";
                @"{0,129}082288880110722000010AC00000160000000000000000990174000000000055012818300555555510st000000015028185555550000000001000000022555003";
            string fromTo = "08228888";
            string mti = "0110";
            string[] srcList = new string[65];
            /*
            02 : 160000000000000000
            03 : 990174
            04 : 000000000055
            07 : 0128183005
            11 : 555555
            32 : 10st00000001
            37 : 502818555555
            39 : 00
            41 : 00000001
            42 : 000000022555003
             */
            srcList[2] = "0000000000000000";
            srcList[3] = "990174";
            srcList[4] = "000000000055";
            srcList[7] = "0128183005";
            srcList[11] = "555555";
            srcList[32] = "st00000001";
            srcList[37] = "502818555555";
            srcList[39] = "00";
            srcList[41] = "00000001";
            srcList[42] = "000000022555003";
            
            MessageContext msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);
            
            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                logger.Debug($"{0}", field);
            }
            logger.Debug($"{0}", msgContextMain);
            Assert.Equal(expected, msgContextMain.ToString());
        }

        [Fact]
        public void Test06Parse0110()
        {
            IList<IsoField> expected = new List<IsoField>();
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":0,\"FuncData\":null,\"NextFuncNo\":2}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":2,\"FuncData\":\"0000000000000000\",\"NextFuncNo\":3}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":3,\"FuncData\":\"990174\",\"NextFuncNo\":4}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":4,\"FuncData\":\"000000000055\",\"NextFuncNo\":7}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":7,\"FuncData\":\"0128183005\",\"NextFuncNo\":11}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":11,\"FuncData\":\"555555\",\"NextFuncNo\":32}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":32,\"FuncData\":\"st00000001\",\"NextFuncNo\":37}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":37,\"FuncData\":\"502818555555\",\"NextFuncNo\":39}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":39,\"FuncData\":\"00\",\"NextFuncNo\":41}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":41,\"FuncData\":\"00000001\",\"NextFuncNo\":42}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":42,\"FuncData\":\"000000022555003\",\"NextFuncNo\":-1}"));
            
            byte[] head = new byte[] { 0, 129 };
            string message =
            //@"888808220100F220000108C000080000000000000000160000000000000000990174000000000055012818300099999910st00000001502818999999000000010000000225550030853160000020000000000000010012015012818300000000000000000000000005511111111111111111111";
            //@"888808220100F220000108C000080000000000000000160000000000000000990174000000000055012818300555555510st00000001502818555555000000010000000225550030853160000020000000000000010012015012818300500000000000000000000005586000000000000000001";
            //@"145082288880110F22000010AC000000000000000000000160000000000000000990174000000000055012818300555555510st000000015028185555550000000001000000022555003";
            //  "129"
            "082288880110722000010AC00000160000000000000000990174000000000055012818300555555510st000000015028185555550000000001000000022555003";
            MessageContext msgContext = this.mainMsgWorker.Parse(message);
            Assert.Equal(head[0] << 8 | head[1], msgContext.MessageSize);
            
            IList<IsoField> fList = msgContext.FieldList.ToList();
            for (int i = 0; i < fList.Count; i++)
            {
                IsoField field = fList[i];
                logger.Debug($"{0}", field);
                Assert.Equal(expected[i], field);
            }
            logger.Debug($"{0}", msgContext);
        }

        [Fact]
        public void Test07Build0302()
        {
            MessageContext msgContextMain = null;
            string expected = @"{0,101}082288880302E2240000080000000000002000000000160000000000000000990176012818000777777715125028187777771";
            string fromTo = "08228888";
            string mti = "0302";
            
            string[] srcList = new string[129];
            /* 
            02 : 160000000000000000
            03 : 990176
            07 : 0128180007
            11 : 777777
            14 : 1512
            37 : 502818777777
            91 : 1
             */
            srcList[2] = "0000000000000000";
            srcList[3] = "990176";
            srcList[7] = "0128180007";
            srcList[11] = "777777";
            srcList[14] = "1512";
            srcList[37] = "502818777777";
            srcList[91] = "1";
            
            msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                logger.Debug($"{0}", field);
            }
            //string result = msgContextMain.SrcMessage;
            logger.Debug($"{0}", msgContextMain);
            Assert.Equal(expected, msgContextMain.ToString());

        }

        [Fact]
        public void Test08Parse0302()
        {
            IList<IsoField> expected = new List<IsoField>();
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":0,\"FuncData\":null,\"NextFuncNo\":2}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":2,\"FuncData\":\"0000000000000000\",\"NextFuncNo\":3}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":3,\"FuncData\":\"990176\",\"NextFuncNo\":7}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":7,\"FuncData\":\"0128180007\",\"NextFuncNo\":11}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":11,\"FuncData\":\"777777\",\"NextFuncNo\":14}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":14,\"FuncData\":\"1512\",\"NextFuncNo\":37}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":37,\"FuncData\":\"502818777777\",\"NextFuncNo\":91}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":91,\"FuncData\":\"1\",\"NextFuncNo\":-1}"));
            //101
            byte[] header = new byte[] { 0, 101 };
            string msg = @"082288880302E2240000080000000000002000000000160000000000000000990176012818000777777715125028187777771";
            MessageContext msgContextMain = this.mainMsgWorker.Parse(msg);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            for (int i = 0; i < fList.Count; i++)
            {
                IsoField field = fList[i];
                logger.Debug($"{0}", field);
                Assert.Equal(expected[i], field);
            }

            logger.Debug($"{0}", msgContextMain.FromTo);
            logger.Debug($"{0}", msgContextMain.Mti);
            logger.Debug($"{0}", msgContextMain);
        }

        [Fact]
        public void Test09Build0312()
        {
            MessageContext msgContextMain = null;
            string expected =
                //@"135888808220312E22400000A00000800000000000000001600000000000000009901760128180007777777151250281877777700030010000000000000020150128180007";
                @"{0,119}888808220312622400000A0000081600000000000000009901760128180007777777151250281877777700030010000000000000020150128180007";
            string fromTo = "88880822";
            string mti = "0312";

            string[] srcList = new string[65];
            /* 
            02 : 0000000000000000
            03 : 990176
            07 : 0128180007
            11 : 777777
            14 : 1512
            37 : 502818777777
            39 : 00
            61 : 030010000000000000020150128180007            
             */
            srcList[2] = "0000000000000000";
            srcList[3] = "990176";
            srcList[7] = "0128180007";
            srcList[11] = "777777";
            srcList[14] = "1512";
            srcList[37] = "502818777777";
            srcList[39] = "00";
            srcList[61] = getBuild0312Df61();
            
            msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                logger.Debug($"{0}", field);
            }
            //string result = msgContextMain.SrcMessage;
            logger.Debug($"{0}", msgContextMain);
            Assert.Equal(expected, msgContextMain.ToString());

            // check df61
            expected = "010000000000000020150128180007";
            Assert.Equal(expected, srcList[61]);
        }

        private string getBuild0312Df61()
        {
            string[] srcList = new string[65]; // primary + 1
            /*
            08: 20150128180007
            */
            srcList[8] = "20150128180007";
            
            MessageContext df61MsgContext = this.df61MsgWorker.Build(null, null, srcList);
            IList<IsoField> fList = df61MsgContext.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                logger.Debug($"{0}", field);
            }
            //string result = msgContextMain.SrcMessage;
            logger.Debug($"{0}", df61MsgContext);
            return df61MsgContext.SrcMessage;
        }

        [Fact]
        public void Test10Parse0312()
        {
            IList<IsoField> expected = new List<IsoField>();
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":0,\"FuncData\":null,\"NextFuncNo\":2}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":2,\"FuncData\":\"0000000000000000\",\"NextFuncNo\":3}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":3,\"FuncData\":\"990176\",\"NextFuncNo\":7}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":7,\"FuncData\":\"0128180007\",\"NextFuncNo\":11}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":11,\"FuncData\":\"777777\",\"NextFuncNo\":14}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":14,\"FuncData\":\"1512\",\"NextFuncNo\":37}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":37,\"FuncData\":\"502818777777\",\"NextFuncNo\":39}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":39,\"FuncData\":\"00\",\"NextFuncNo\":61}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":61,\"FuncData\":\"010000000000000020150128180007\",\"NextFuncNo\":-1}"));
            //
            MessageContext msgContextMain = null;
            byte[] head = new byte[] { 0, 135 };
            string msg = @"888808220312E22400000A00000800000000000000001600000000000000009901760128180007777777151250281877777700030010000000000000020150128180007";
            msgContextMain = this.mainMsgWorker.Parse(msg);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            for (int i = 0; i < fList.Count; i++)
            {
                IsoField field = fList[i];
                logger.Debug($"{0}", field);
                Assert.Equal(expected[i], field);
            }
            logger.Debug($"{0}", msgContextMain.FromTo);
            logger.Debug($"{0}", msgContextMain.Mti);
            logger.Debug($"{0}", msgContextMain);

            // parse df61
            expected.Clear();
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":0,\"FuncData\":null,\"NextFuncNo\":8}"));
            expected.Add(this.jworker.Deserialize("{\"FuncNo\":8,\"FuncData\":\"20150128180007\",\"NextFuncNo\":-1}"));
            //
            IsoField fd = null;
            MessageContext msgContextDf61 = null;

            if (null != (fd = msgContextMain.GetField(61)))
            {
                msg = fd.FuncData;
                msgContextDf61 = this.df61MsgWorker.Parse(msg);
                fList = msgContextDf61.FieldList.ToList();
                for (int i = 0; i < fList.Count; i++)
                {
                    IsoField field = fList[i];
                    logger.Debug($"{0}", field);
                    Assert.Equal(expected[i], field);
                }
            }
        }

//        [Fact]
//        public void Test11Buid0420()
//        {
//            string expected = @"{0,212}888808220420F220000108C000080000004000000000160000000000000000990174000000000055012818000666666610st00000001502818666666000000010000000225550030240080000000000000000000000100012818300555555500000001502818666666  ";
//            string fromTo = "88880822";
//            string mti = "0420";

//            string[] srcList = new string[129];
//            for (int i = 0; i < srcList.Length; i++)
//            {
//                srcList[i] = "";
//            }
//            /* 
//02 : 0000000000000000
//03 : 990174
//04 : 000000000055
//07 : 0128183006
//11 : 666666
//32 : st00000001
//37 : 502818666666
//41 : 00000001
//42 : 000000022555003
//61 : 008000000000000000000000
//90 : 0100 0128183005 555555 10st00000001  
//             */
//            srcList[2] = "0000000000000000";
//            srcList[3] = "990174";
//            srcList[4] = "000000000055";
//            srcList[7] = "0128180006";
//            srcList[11] = "666666";
//            srcList[32] = "st00000001";
//            srcList[37] = "502818666666";
//            srcList[41] = "00000001";
//            srcList[42] = "000000022555003";

//            string[] srcListDf61 = new string[65];
//            for (int i = 0; i < srcListDf61.Length; i++)
//            {
//                srcListDf61[i] = "";
//            }
//            srcListDf61[9] = "00000000";
//            MessageContext msgContextDf61 = this.df61MsgWorker.Build(null, null, srcListDf61);

//            srcList[61] = msgContextDf61.SrcMessage; //"008000000000000000000000";
//            Assert.Equal("008000000000000000000000", srcList[61]);

//            srcList[90] = "0100" + "0128183005" + "555555" + "00000001" + "502818666666" + "  ";

//            MessageContext msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

//            IList<IsoField> fList = msgContextMain.FieldList.ToList();
//            foreach (IsoField field in fList)
//            {
//                logger.Debug(m => m("{0}", field));
//            }
//            string result = msgContextMain.SrcMessage;
//            logger.Debug(m => m("{0}", msgContextMain));
//            Assert.Equal(expected, msgContextMain.ToString());
//        }

//        [Fact]
//        public void Test12Parse0420()
//        {
//            IList<IsoField> expected = new List<IsoField>();
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":0,\"FuncData\":null,\"NextFuncNo\":2}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":2,\"FuncData\":\"0000000000000000\",\"NextFuncNo\":3}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":3,\"FuncData\":\"990174\",\"NextFuncNo\":4}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":4,\"FuncData\":\"000000000055\",\"NextFuncNo\":7}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":7,\"FuncData\":\"0128180006\",\"NextFuncNo\":11}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":11,\"FuncData\":\"666666\",\"NextFuncNo\":32}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":32,\"FuncData\":\"st00000001\",\"NextFuncNo\":37}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":37,\"FuncData\":\"502818666666\",\"NextFuncNo\":41}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":41,\"FuncData\":\"00000001\",\"NextFuncNo\":42}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":42,\"FuncData\":\"000000022555003\",\"NextFuncNo\":61}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":61,\"FuncData\":\"008000000000000000000000\",\"NextFuncNo\":90}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":90,\"FuncData\":\"0100012818300555555500000001502818666666  \",\"NextFuncNo\":-1}"));

//            212
//            string msg = @"888808220420F220000108C000080000004000000000160000000000000000990174000000000055012818000666666610st00000001502818666666000000010000000225550030240080000000000000000000000100012818300555555500000001502818666666  ";

//            MsgContext msgContextMain = this.mainMsgWorker.Parse(msg);

//            IList<IsoField> fList = msgContextMain.FieldList.ToList();
//            for (int i = 0; i < fList.Count; i++)
//            {
//                IsoField field = fList[i];
//                logger.Debug(m => m("{0}", field));
//                Assert.Equal(expected[i], field);
//            }
//            logger.Debug(m => m("{0}:{1}", msgContextMain.FromTo, msgContextMain.Mti));
//            logger.Debug(m => m("{0}", msgContextMain));
//        }

//        [Fact]
//        public void Test13Buid0430()
//        {
//            string expected = //@"187082288880420F220000108C000080000004000000000160000000000000000990174000000000055012818000666666610st00000001502818666666000000010000000225550030240080000000000000000000000100012818300555555500000001502818666666  ";
//                                @"{0,187}082288880430F22000010AC000000000004000000000160000000000000000990174000000000055012818000666666610st0000000150281866666600000000010000000225550030100012818300555555500000001502818666666  ";
//            string fromTo = "08228888";
//            string mti = "0430";

//            string[] srcList = new string[129];
//            /* 
//02 : 160000000000000000
//03 : 990174
//04 : 000000000055
//07 : 0128183006
//11 : 666666
//32 : 10st00000001
//37 : 502818666666
//39 : 00
//41 : 00000001
//42 : 000000022555003
//90 : 0100012818300555555500000001502818666666  "   
//             */
//            srcList[2] = "0000000000000000";
//            srcList[3] = "990174";
//            srcList[4] = "000000000055";
//            srcList[7] = "0128180006";
//            srcList[11] = "666666";
//            srcList[32] = "st00000001";
//            srcList[37] = "502818666666";
//            srcList[39] = "00";
//            srcList[41] = "00000001";
//            srcList[42] = "000000022555003";
//            srcList[90] = "0100" + "0128183005" + "555555" + "00000001" + "502818666666" + "  ";

//            MsgContext msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

//            IList<IsoField> fList = msgContextMain.FieldList.ToList();
//            for (int i = 0; i < fList.Count; i++)
//            {
//                IsoField field = fList[i];
//                logger.Debug(m => m("{0}", field));
//            }
//            logger.Debug(m => m("{0}:{1}", msgContextMain.FromTo, msgContextMain.Mti));
//            logger.Debug(m => m("{0}", msgContextMain));
//            string result = msgContextMain.SrcMessage;
//            logger.Debug(m => m("[{0}]", msgContextMain));
//            Assert.Equal(expected, msgContextMain.ToString());
//        }

//        [Test]
//        public void Test14Parse0430()
//        {
//            IList<IsoField> expected = new List<IsoField>();
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":0,\"FuncData\":null,\"NextFuncNo\":2}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":2,\"FuncData\":\"0000000000000000\",\"NextFuncNo\":3}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":3,\"FuncData\":\"990174\",\"NextFuncNo\":4}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":4,\"FuncData\":\"000000000055\",\"NextFuncNo\":7}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":7,\"FuncData\":\"0128180006\",\"NextFuncNo\":11}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":11,\"FuncData\":\"666666\",\"NextFuncNo\":32}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":32,\"FuncData\":\"st00000001\",\"NextFuncNo\":37}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":37,\"FuncData\":\"502818666666\",\"NextFuncNo\":39}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":39,\"FuncData\":\"00\",\"NextFuncNo\":41}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":41,\"FuncData\":\"00000001\",\"NextFuncNo\":42}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":42,\"FuncData\":\"000000022555003\",\"NextFuncNo\":90}"));
//            expected.Add(this.jworker.Deserialize("{\"FuncNo\":90,\"FuncData\":\"0100012818300555555500000001502818666666  \",\"NextFuncNo\":-1}"));
//            string msg =
//                @"082288880430F22000010AC000000000004000000000160000000000000000990174000000000055012818000666666610st0000000150281866666600000000010000000225550030100012818300555555500000001502818666666  ";
//            @"888808220312622400000A0000081600000000000000009901760128180007777777151250281877777700030010000000000000020150128180007";
//            MsgContext msgContextMain = this.mainMsgWorker.Parse(msg);

//            IList<IsoField> fList = msgContextMain.FieldList.ToList();
//            for (int i = 0; i < fList.Count; i++)
//            {
//                IsoField field = fList[i];
//                logger.Debug(m => m("{0}", field));
//                Assert.Equal(expected[i], field);
//            }
//            logger.Debug(m => m("{0}:{1}", msgContextMain.FromTo, msgContextMain.Mti));
//            logger.Debug(m => m("[{0}]", msgContextMain));
//        }

        //        [Fact]
        //        public void Test21ParseSignOnRequest()
        //        {
        //            IList<IsoField> expected = new List<IsoField>();
        //            expected.Add(this.jworker.Deserialize("{\"FuncNo\":0,\"FuncData\":null,\"NextFuncNo\":7}"));
        //            expected.Add(this.jworker.Deserialize("{\"FuncNo\":7,\"FuncData\":\"0128132501\",\"NextFuncNo\":11}"));
        //            expected.Add(this.jworker.Deserialize("{\"FuncNo\":11,\"FuncData\":\"111111\",\"NextFuncNo\":70}"));
        //            expected.Add(this.jworker.Deserialize("{\"FuncNo\":70,\"FuncData\":\"071\",\"NextFuncNo\":-1}"));
        //            //
        //            MessageContext msgContextMain = null;
        //            string msg = "082288880800822000000000000004000000000000000128132501111111071";
        //            msgContextMain = this.mainMsgWorker.Parse(msg);
        //            //
        //            IList<IsoField> fList = msgContextMain.FieldList.ToList();
        //            for (int i = 0; i < fList.Count; i++)
        //            {
        //                IsoField field = fList[i];
        //                logger.Debug(m => m("{0}", field));
        //                Assert.Equal(expected[i], field);
        //            }
        //            logger.Debug(m => m("{0}", msgContextMain.FromTo));
        //            logger.Debug(m => m("{0}", msgContextMain.Mti));
        //        }

        //        [Fact]
        //        public void Test22BuildSignOnResponse()
        //        {
        //            MessageContext msgContextMain = null;
        //            string expected =
        //          //@"082288880800822000000000000004000000000000000128132501111111071";
        //          "{0,63}888808220810822000000000000004000000000000000128132501111111071";
        //            string fromTo = "88880822";
        //            string mti = "0810";

        //            string[] srcList = new string[129];
        //            //07 : 0128132501
        //            //11 : 111111
        //            //70 : 071
        //            srcList[7] = "0128132501";
        //            srcList[11] = "111111";
        //            srcList[70] = "071"; // sign on response
        //            //
        //            msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

        //            IList<IsoField> fList = msgContextMain.FieldList.ToList();
        //            foreach (IsoField field in fList)
        //            {
        //                logger.Debug(m => m("{0}", field));
        //            }
        //            string result = msgContextMain.ToString();
        //            logger.Debug(m => m("{0}", result));
        //            Assert.Equal(expected, result);
        //        }

        //        [Fact]
        //        public void Test23ParseSignOffRequest()
        //        {
        //            IList<IsoField> expected = new List<IsoField>();
        //            expected.Add(this.jworker.Deserialize("{\"FuncNo\":0,\"FuncData\":null,\"NextFuncNo\":7}"));
        //            expected.Add(this.jworker.Deserialize("{\"FuncNo\":7,\"FuncData\":\"0128132502\",\"NextFuncNo\":11}"));
        //            expected.Add(this.jworker.Deserialize("{\"FuncNo\":11,\"FuncData\":\"222222\",\"NextFuncNo\":70}"));
        //            expected.Add(this.jworker.Deserialize("{\"FuncNo\":70,\"FuncData\":\"072\",\"NextFuncNo\":-1}"));
        //            MessageContext msgContextMain = null;
        //            string msg = "082288880800822000000000000004000000000000000128132502222222072";
        //            msgContextMain = this.mainMsgWorker.Parse(msg);

        //            IList<IsoField> fList = msgContextMain.FieldList.ToList();
        //            for (int i = 0; i < fList.Count; i++)
        //            {
        //                IsoField field = fList[i];
        //                logger.Debug(m => m("{0}", field));
        //                Assert.Equal(expected[i], field);
        //            }
        //            logger.Debug(m => m("{0}", msgContextMain.FromTo));
        //            logger.Debug(m => m("{0}", msgContextMain.Mti));
        //        }

        //        [Fact]
        //        public void Test24BuildSignOffResponse()
        //        {
        //            MessageContext msgContextMain = null;
        //            string expected = @"{0,63}888808220810822000000000000004000000000000000128132502222222072";
        //            string fromTo = "88880822";
        //            string mti = "0810";
        //            string[] srcList = new string[129];
        //            //07 : 0128132502
        //            //11 : 222222
        //            //70 : 072
        //            srcList[7] = "0128132502";
        //            srcList[11] = "222222";
        //            srcList[70] = "072"; // sign off response
        //            //
        //            msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

        //            IList<IsoField> fList = msgContextMain.FieldList.ToList();
        //            foreach (IsoField field in fList)
        //            {
        //                logger.Debug(m => m("{0}", field));
        //            }
        //            string result = msgContextMain.ToString();
        //            logger.Debug(m => m("{0}", result));
        //            Assert.Equal(expected, result);
        //        }

        //        [Fact]
        //        public void Test17ParseEcho1Request()
        //        {
        //            IList<IsoField> expected = new List<IsoField>();
        //            expected.Add(this.jworker.Deserialize("{\"FuncNo\":0,\"FuncData\":null,\"NextFuncNo\":7}"));
        //            expected.Add(this.jworker.Deserialize("{\"FuncNo\":7,\"FuncData\":\"0128132503\",\"NextFuncNo\":11}"));
        //            expected.Add(this.jworker.Deserialize("{\"FuncNo\":11,\"FuncData\":\"333333\",\"NextFuncNo\":70}"));
        //            expected.Add(this.jworker.Deserialize("{\"FuncNo\":70,\"FuncData\":\"301\",\"NextFuncNo\":-1}"));
        //            MessageContext msgContextMain = null;
        //            string msg = "082288880800822000000000000004000000000000000128132503333333301";
        //            msgContextMain = this.mainMsgWorker.Parse(msg);

        //            IList<IsoField> fList = msgContextMain.FieldList.ToList();
        //            for (int i = 0; i < fList.Count; i++)
        //            {
        //                IsoField field = fList[i];
        //                logger.Debug(m => m("{0}", field));
        //                Assert.Equal(expected[i], field);
        //            }
        //            logger.Debug(m => m("{0}", msgContextMain.FromTo));
        //            logger.Debug(m => m("{0}", msgContextMain.Mti));
        //        }

        //        [Fact]
        //        public void Test18BuildEcho1Response()
        //        {
        //            MessageContext msgContextMain = null;
        //            string expected = @"{0,65}88880822081082200000020000000400000000000000012813250333333300301";
        //            string fromTo = "88880822";
        //            string mti = "0810";
        //            string[] srcList = new string[129];
        //            //07 : 0128132503
        //            //11 : 333333
        //            //39 : 00
        //            //70 : 301
        //            srcList[7] = "0128132503";
        //            srcList[11] = "333333";
        //            srcList[39] = "00";
        //            srcList[70] = "301"; // sign off response
        //            //
        //            msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

        //            IList<IsoField> fList = msgContextMain.FieldList.ToList();
        //            foreach (IsoField field in fList)
        //            {
        //                logger.Debug(m => m("{0}", field));
        //            }
        //            string result = msgContextMain.ToString();
        //            logger.Debug(m => m("{0}", result));
        //            Assert.Equal(expected, result);
        //        }

        //        [Fact]
        //        public void Test20ParseEcho2Response()
        //        {
        //            MsgContext msgContextMain = null;
        //            string msg = //"{0,65}"
        //                "08228888081082200000020000000400000000000000012813250333333300301";
        //            msgContextMain = this.mainMsgWorker.Parse(msg);

        //            IList<IsoField> fList = msgContextMain.FieldList.ToList();
        //            foreach (IsoField field in fList)
        //            {
        //                logger.Debug(m => m("{0}", field));
        //            }
        //            logger.Debug(m => m("{0}", msgContextMain.FromTo));
        //            logger.Debug(m => m("{0}", msgContextMain.Mti));
        //        }

        //        [Fact]
        //        public void Test19BuildEcho2Request()
        //        {
        //            MessageContext msgContextMain = null;
        //            string expected = @"{0,63}888808220800822000000000000004000000000000000128132503333333301";
        //            string fromTo = "88880822";
        //            string mti = "0800";
        //            string[] srcList = new string[129];
        //            srcList[7] = "0128132503";
        //            srcList[11] = "333333";
        //            //srcList[39] = "00";
        //            srcList[70] = "301"; // echo 2 request
        //            //
        //            msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

        //            IList<IsoField> fList = msgContextMain.FieldList.ToList();
        //            foreach (IsoField field in fList)
        //            {
        //                logger.Debug(field);
        //            }
        //            string result = msgContextMain.ToString();
        //            logger.Debug(m => m("{0}", result));
        //            Assert.Equal(expected, result);
        //        }

        //        [Fact]
        //        public void Test25Build0120()
        //        {
        //            string expected = //"231"
        //                              //"8888082201007220000108C000080000000000000000160000000000000000990174000000000055012818300555555510st00000001502818555555000000010000000225550030853160000020000000000000010012015012818300571116801234567890000005586000000000000000001";
        //                              //"2158888082201007220000108C00008160000000000000000990174000000000055012818300555555510st00000001502818555555000000010000000225550030853160000020000000000000010012015012818300571116801234567890000005586000000000000000001";
        //                "8888082201207220000108C00008160000000000000000990175000000000055012818300666666610st00000001502818666666000000010000000225550030853160000020000000000000010012015012818300600000000000000000000005586000000000000000001";

        //            string fromTo = "88880822";
        //            string mti = "0120";

        //            string[] srcList = new string[65]; // primary + 1
        //            /* 
        //02 : 160000000000000000
        //03 : 990175
        //04 : 000000000055
        //07 : 0128183006
        //11 : 666666
        //32 : 10st00000001
        //37 : 502818666666
        //41 : 00000001
        //42 : 000000022555003
        //61 : 853160000200000000 000000010012015012818300600000000000000005586000000000000000001
        //             */
        //            srcList[2] = "0000000000000000";
        //            srcList[3] = "990175";
        //            srcList[4] = "000000000055";
        //            srcList[7] = "0128183006";
        //            srcList[11] = "666666";
        //            srcList[32] = "st00000001";
        //            srcList[37] = "502818666666";
        //            srcList[41] = "00000001";
        //            srcList[42] = "000000022555003";

        //            // need construct latter
        //            string[] df61List = new string[65];
        //            /*
        //            03 : 00000001
        //            04 : 001
        //            08 : 20150128183006
        //            10 : 0000000000000000
        //            11 : 55
        //            35 : 86000000000000000001
        //            */
        //            df61List[3] = "00000001";
        //            df61List[4] = "001";
        //            df61List[8] = "20150128183006";
        //            df61List[10] = "0000000000000000";
        //            df61List[11] = "55";
        //            df61List[35] = "86000000000000000001";
        //            //
        //            MessageContext df61MsgContext = this.df61MsgWorker.Build(null, null, df61List);
        //            srcList[61] = df61MsgContext.SrcMessage;
        //            //
        //            MessageContext msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);
        //            //
        //            IList<IsoField> fList = msgContextMain.FieldList.ToList();
        //            foreach (IsoField field in fList)
        //            {
        //                logger.Debug(field);
        //            }
        //            logger.Debug(msgContextMain.SrcMessage);
        //            Assert.Equal(expected, msgContextMain.SrcMessage);
        //        }
    }
}
