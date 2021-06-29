using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

public class PlayerData
{
    public SaveFile SaveFile;
    public int Skin;
    public string Username;

    public void Save()
    {
        if (!Directory.Exists(Server.SavePath + "/Users/" + Username))
        {
            Directory.CreateDirectory(Server.SavePath + "/Users/" + Username);
            Directory.CreateDirectory(Server.SavePath + "/Users/" + Username + "/Logs");
        }

        byte[] data = 
    }

    public void Load()
    {

    }
}
