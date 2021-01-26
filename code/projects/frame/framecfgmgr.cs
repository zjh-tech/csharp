using System; 
using System.Xml;
using System.IO; 


public class FrameCfgMgr
{
    public static bool LoadServerCfg(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine("{0} Not Exist",path);
            return false; 
        }

        var server_cfg = GlobalDef.GServerCfg;

        XmlDocument doc = new XmlDocument();
        try
        {
            doc.Load(path);
        }
        catch(SystemException ex)
        {
            Console.WriteLine(ex.ToString());
            return false; 
        }                
        XmlNode config_node = doc.SelectSingleNode("config");
        
        server_cfg.ServerName = config_node.SelectSingleNode("servicename").InnerText;        
        server_cfg.ServerType = UInt32.Parse(config_node.SelectSingleNode("servertype").InnerText);
        server_cfg.ServerId = UInt32.Parse(config_node.SelectSingleNode("serverid").InnerText);
        server_cfg.Token = config_node.SelectSingleNode("token").InnerText;
        server_cfg.LogDir = config_node.SelectSingleNode("logdir").InnerText;
        server_cfg.LogLevel = int.Parse(config_node.SelectSingleNode("loglevel").InnerText);
        server_cfg.ThreadNum = UInt32.Parse(config_node.SelectSingleNode("threadnum").InnerText);
        server_cfg.SDConnectIp = config_node.SelectSingleNode("sdconnectip").InnerText;
        server_cfg.SDConnectPort = UInt32.Parse(config_node.SelectSingleNode("sdconnectport").InnerText);

        return true; 
    }
}