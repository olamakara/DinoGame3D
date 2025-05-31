using System;
using System.IO;
class DirectoryUtils
{
    public static string PathToDir = null;
    public static string[] GetFilesFromDir(string myPath)
    {
        return Directory.GetFiles(myPath);
    }
    public static string[] GetDirectoriesFromDir(string myPath)
    {
        return Directory.GetDirectories(myPath);
    }
    public static string[] GetFilesFromDir()
    {
        return Directory.GetFiles(PathToDir);
    }
    public static void RemoveFIlesFromDir(string myPath)
    {
        System.IO.DirectoryInfo di = new DirectoryInfo(myPath);
        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete(); 
        }
    }
    public static void RemoveFIlesFromDir()
    {
        RemoveFIlesFromDir(PathToDir);
    }
}
