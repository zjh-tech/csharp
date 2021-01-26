using System;
using Google.Protobuf;
using Config;
using System.IO;
using Framework.ELog;
                     
public class StaticDataMgr
{
   public static StaticDataMgr Instance = new StaticDataMgr();
                     
   public ErrorString LoadAllCfg(string prefix_path)
   {
      string tip_path = prefix_path + "/tip.pb";
      tip_cfg = (tipcfg)load_cfg<tipcfg>(tip_path);
      if (tip_cfg == null)
      {
         return new ErrorString("Load Tip File Error...");
      }
      Log.Infof("[StaticDataMgr] Tip={0}", tip_cfg.ToString());
                     
      return null;
   }
                     
   private IMessage load_cfg<T>(string path) where T : IMessage<T> ,new()
   {
      FileStream file_stream = null;
      try
      {
         file_stream = File.Open(path, FileMode.Open);
      }
      catch(SystemException)
      {
         Log.Infof("[StaticDataMgr] Path = {0} Open File Error", path); 
         return null;
      }
                     
      if (file_stream != null)
      {
         BinaryReader reader = new BinaryReader(file_stream);
         if(reader != null)
         {
            byte[] datas = reader.ReadBytes((int)file_stream.Length);
            MessageParser<T> parser = new MessageParser<T>(() => new T());
            return parser.ParseFrom(datas);
         }
      }
                     
      return null;
   }
                     
   public tipcfg  GetTipCfg()
   {
      return tip_cfg;
   }
                     
   public tip  GetTipByID(UInt32 id)
   {
      if (tip_cfg.Datas.ContainsKey(id))
      {
         return tip_cfg.Datas[id];
      }
                     
      return null;
   }
                     
   private tipcfg tip_cfg = null;
                     
}
                     
