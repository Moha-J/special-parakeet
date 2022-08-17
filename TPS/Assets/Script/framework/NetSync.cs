using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NetSync
{
    private NetSync() { }
    private static NetSync Singleton;
    public static NetSync GetSingleton()
    {
        if(Singleton == null)
        {
            Singleton = new NetSync();
        }
        return Singleton;
    }


}

