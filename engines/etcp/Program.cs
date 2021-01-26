using Framework.ETcp; 
class Program
{
    static void Main(string[] args)
    {
        Net.Instance.Init();
        Net.Instance.Run(100);
    }
}

