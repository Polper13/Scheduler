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

    public static AudioClip normalize(this AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("no clip to normalize: clip null");
            return null;
        }

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        float maxAmp = 0f;
        for (int i = 0; i < samples.Length; i++)
            maxAmp = Mathf.Max(maxAmp, Mathf.Abs(samples[i]));

        if (maxAmp > 0.0001f)  // Avoid div-by-zero nonsense
        {
            for (int i = 0; i < samples.Length; i++)
                samples[i] /= maxAmp;  // Normalize to peak 1.0
        }

        AudioClip normalized = AudioClip.Create
        (
            "normalized_" + clip.name,
            clip.samples,
            clip.channels,
            clip.frequency,
            false
        );
        normalized.SetData(samples, 0);
        return normalized;  // Fresh clip—original stays pure
    }
}
