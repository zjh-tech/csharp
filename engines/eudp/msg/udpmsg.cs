using System;
using System.IO;

namespace Engine
{
    public class UdpMsg
    {
        public UdpMsg()
        {
        }

        protected MemoryStream memoryStream = null;
        protected BinaryReader reader = null;
        protected BinaryWriter writer = null;
        public UInt32 msgid = 0;

        public void Serialize(out byte[] datas)
        {
            if (memoryStream == null && reader == null)
            {
                memoryStream = new MemoryStream();
                writer = new BinaryWriter(memoryStream);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            writer.Write(msgid);
            Serialize();            
            datas = memoryStream.ToArray();
        }

        public virtual void Serialize()
        {            

        }

        public void Deserialize(byte[] datas,int offset)
        {
            memoryStream = new MemoryStream(datas,offset,datas.Length - offset);
            reader = new BinaryReader(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            Deserialize();            
        }

        public virtual void Deserialize()
        {
        }

        public void Close()
        {
            if (memoryStream != null)
            {
                memoryStream.Close();
                memoryStream = null;
            }

            if (writer != null)
            {
                writer.Close();
                writer = null;
            }

            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
        }
    }
}
