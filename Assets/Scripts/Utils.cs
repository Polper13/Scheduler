using UnityEngine;
using System.IO.Compression;

public static class Utils
{
    public static bool ZipContainsFile(string zipPath, string fileName)
    {
        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName == "config.json") { return true; }
            }
            return false;
        }
    }
}
