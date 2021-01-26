using System;

namespace Framework.ETcp
{
    public interface IAttachParas
    {
        byte[] FillNetStream();
    }

    public interface ICoder
    {
        UInt32 GetHeaderLen();

        bool GetBodyLen(byte[] datas, out UInt32 body_len);

        bool EncodeBody(byte[] content, out byte[] out_content);

        void DecodeBody(byte[] content, out byte[] out_content);

        bool ZipBody(byte[] content, out byte[] out_content);

        bool UnzipBody(byte[] content, out byte[] out_content);

        void FillNetStream(byte[] datas, out byte[] out_content);
    }

}
