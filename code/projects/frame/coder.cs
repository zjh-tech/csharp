using System;
using System.IO;
using Framework.ETcp;


class MsgHeader
{
    public bool EncodeFlag = false;
    public bool ZipFlag = false;
    public UInt32 BodyLen = 0;
}

public class Coder : ICoder
{
    public UInt32 GetHeaderLen()
    {
        return PackageHeaderLen;
    }

    public bool GetBodyLen(byte[] datas, out UInt32 body_len)
    {
        MemoryStream memstream = new MemoryStream(datas);
        BinaryReader reader = new BinaryReader(memstream);
        msg_header.EncodeFlag = reader.ReadBoolean();
        msg_header.ZipFlag = reader.ReadBoolean();
        byte[] body_len_bytes =  reader.ReadBytes(4);
        Array.Reverse(body_len_bytes);
        msg_header.BodyLen = BitConverter.ToUInt32(body_len_bytes);
        body_len = msg_header.BodyLen;
        return true;
    }

    public bool EncodeBody(byte[] content, out byte[] out_content)
    {
        out_content = content;
        return false;
    }

    public void DecodeBody(byte[] content, out byte[] out_content)
    {
        if (!msg_header.EncodeFlag)
        {
            out_content = content;
            return; 
        }

        out_content = null;         
    }

    public bool ZipBody(byte[] content, out byte[] out_content)
    {
        out_content = content;
        return false; 
    }

    public bool UnzipBody(byte[] content, out byte[] out_content)
    {        
        if (!msg_header.ZipFlag)
        {
            out_content = content;
            return true; 
        }

        out_content = null; 
        return false;
    }

    public void FillNetStream(byte[] datas, out byte[] out_content)
    {
        byte[] encode_datas = null;
        bool encode_flag = EncodeBody(datas, out encode_datas);
        byte[] zip_datas = null;
        bool zip_flag = ZipBody(encode_datas, out zip_datas);

        MemoryStream memstream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(memstream);
        writer.Write(encode_flag);
        writer.Write(zip_flag);
        byte[] body_len_bytes = System.BitConverter.GetBytes((UInt32)zip_datas.Length);
        Array.Reverse(body_len_bytes);
        writer.Write(body_len_bytes);
        writer.Write(zip_datas);
        out_content = memstream.ToArray();
    }

    private MsgHeader msg_header = new MsgHeader();
    private const UInt32 PackageHeaderLen = 6;        //MsgHeader(bool + bool + uint32)
}

