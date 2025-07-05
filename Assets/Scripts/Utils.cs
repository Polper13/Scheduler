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

    public static GameObject getChildWithComponent<T>(GameObject parent) where T : Component
    {
        foreach (Transform child in parent.transform)
        {
            T component = child.GetComponent<T>();
            if (component != null) { return child.gameObject; }
        }

        Debug.LogError("Couldnt find a child with component on parent: " + parent);
        return null;
    }
}
