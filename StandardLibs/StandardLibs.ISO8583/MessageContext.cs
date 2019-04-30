using System;
using System.Collections.Generic;
using System.Text;

namespace StandardLibs.ISO8583
{
    public class MessageContext
    {
        private LinkedListNode<IsoField> currentNode = null;

        public int MessageSize { get; set; }
        public string FromTo { get; set; }
        public string Mti { get; set; }
        public string BitMap { get; set; }
        public string SrcMessage { get; set; }
        public int Start { get; set; }

        public LinkedList<IsoField> FieldList { get; set; }

        public Dictionary<int, LinkedListNode<IsoField>> FieldDic { get; set; }

        public IsoField CurrentField
        {
            get { return this.currentNode.Value; }
            set { this.currentNode.Value = value; }
        }

        public MessageContext()
        {
            this.FieldDic = new Dictionary<int, LinkedListNode<IsoField>>();
            this.Start = 0;
            this.FieldList = new LinkedList<IsoField>();
            this.currentNode = this.FieldList.AddFirst(new IsoField());
            this.SrcMessage = "";
            this.MessageSize = 0;
        }

        public void AddField(int funcNo, string funcData)
        {
            IsoField isoField = new IsoField
            {
                FuncNo = funcNo,
                FuncData = funcData,
                NextFuncNo = -1
            };
            this.CurrentField.NextFuncNo = funcNo;
            this.currentNode = this.FieldList.AddAfter(this.currentNode, isoField);

            if (this.FieldDic.ContainsKey(funcNo))
                this.FieldDic[funcNo] = this.currentNode;
            else
                this.FieldDic.Add(funcNo, this.currentNode);
        }

        public IsoField GetField(int funcNo)
        {
            IsoField field = null;
            if (this.FieldDic.ContainsKey(funcNo))
            {
                this.currentNode = this.FieldDic[funcNo];
                field = this.CurrentField;
            }
            return field;
        }

        public bool HasField(int funcNo)
        {
            return "1".Equals(this.BitMap.Substring(funcNo - 1, 1));
        }

        public byte[] MessageToBytes()
        {
            byte[] resultBytes = new byte[2 + this.MessageSize];
            resultBytes[0] = (byte)(this.MessageSize >> 8);
            resultBytes[1] = (byte)(this.MessageSize & 0xff);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(this.SrcMessage), 0, resultBytes, 2, this.MessageSize);
            return resultBytes;
        }

        public override string ToString()
        {
            return "{" + (this.MessageSize >> 8) + "," + (this.MessageSize & 0xFF) + "}" + this.SrcMessage;
        }
    }
}
