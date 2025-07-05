using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;


[Serializable]
public class BlockDataListWrapper
{
    public List<BlockData> blocks = new List<BlockData>();

    public bool fill(Page page)
    {
        if (page == null) { Debug.LogError("no page"); return false; }

        foreach (Block block in page.blockList)
        {
            blocks.Add(block.toBlockData());
        }
        return true;
    }

    // assigns unique generated fileNames to previous filePaths stored in SongBlockData
    public void assignFileNames(Dictionary<string, string> fileNameMap)
    {
        foreach (SongBlockData songBlock in blocks.OfType<SongBlockData>())
        {
            // originally it stores the filePath in the fileName field
            string filePath = songBlock.fileName;

            // assigning unique generated fileName to filePath
            if (filePath == null) { return; }
            songBlock.fileName = fileNameMap[filePath];
        }
    }
}

public class LoadResult
{
    public BlockDataListWrapper blockDataListWrapper;
    public string extractPath;
}

public static class SaveData
{
    private static string cachePath = Application.persistentDataPath + "/cache";

    public static void deleteCache()
    {
        if (Directory.Exists(cachePath) == false) { return; }

        string[] subdirectories = Directory.GetDirectories(cachePath);
        foreach (string subdirectory in subdirectories)
        {
            Directory.Delete(subdirectory, recursive: true);
        }
    }

    public static void save(Page page, string path)
    {
        List<string> filePaths = getAudioFilePaths(page);
        Dictionary<string, string> fileNameMap = generateAudioFileNames(filePaths);

        // create a data representation of the whole page
        BlockDataListWrapper list = new BlockDataListWrapper();
        list.fill(page);

        list.assignFileNames(fileNameMap);

        // generate a temp json file
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
        };
        string json = JsonConvert.SerializeObject(list, settings);

        // write the generated json string into a file in /temp directory
        string tempDirectoryPath = Application.persistentDataPath + "/temp";
        string jsonPath = tempDirectoryPath + "/config.json";
        Directory.CreateDirectory(tempDirectoryPath);
        File.WriteAllText(jsonPath, json);

        // pack all the audio files and config.json to single .zip
        packToZip(filePaths, fileNameMap, jsonPath, path);

        // remove temporary config.json and /temp directory
        File.Delete(jsonPath);
        Directory.Delete(tempDirectoryPath);
        Debug.Log("success??");
    }

    public static async Task<LoadResult> load(string zipPath)
    {
        using (var zip = ZipFile.OpenRead(zipPath))
        {
            if (zip.Entries.Count == 0)
            {
                Debug.LogWarning("Attempt to open an empty file");
                return null;
            }
        }

        // extract the zip file
        string projectName = Path.GetFileNameWithoutExtension(zipPath);
        string extractPath = cachePath + $"/{projectName}";
        if (Directory.Exists(extractPath))
        {
            // find OverwriteDialogMenu reference
            OverwriteDialogMenu menu = Resources.FindObjectsOfTypeAll<OverwriteDialogMenu>().FirstOrDefault();
            if (menu == null)
            {
                Debug.LogError("Couldnt find OverwriteDialogMenu reference on the scene");
                return null;
            }
            bool overwrite = await menu.askForOverwrite();

            if (overwrite)
            {
                Directory.Delete(extractPath, recursive: true);
                Page page = Page.findByName(projectName);
                if (page != null) { page.destroy(); }
            }
            else { return null; }
        }
        else
        {
            Directory.CreateDirectory(extractPath);
        }
        ZipFile.ExtractToDirectory(zipPath, extractPath, overwriteFiles: true);

        // deserialize the json config
        string jsonPath = extractPath + "/config.json";
        string json = File.ReadAllText(jsonPath);
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = { new BlockConverter() }
        };

        LoadResult loadResult = new LoadResult();
        loadResult.blockDataListWrapper = JsonConvert.DeserializeObject<BlockDataListWrapper>(json, settings);
        loadResult.extractPath = extractPath;

        return loadResult;
    }

    // stores all audio file paths used in songBlocks in a single ordered List
    private static List<string> getAudioFilePaths(Page page)
    {
        var filePaths = new List<string>();
        var checkedFilePaths = new HashSet<string>();

        foreach (SongBlock songBlock in page.blockList.OfType<SongBlock>())
        {
            string filePath = songBlock.filePath;

            if (filePath == null) { continue; }
            if (checkedFilePaths.Contains(filePath)) { continue; }

            checkedFilePaths.Add(filePath);
            filePaths.Add(filePath);
        }

        return filePaths;
    }

    // creates a dictionary with unique fileNames that each corespond to its filePath
    private static Dictionary<string, string> generateAudioFileNames(List<string> filePaths)
    {
        var fileNameMap = new Dictionary<string, string>();
        var usedFileNames = new HashSet<string>();

        foreach (string filePath in filePaths)
        {
            if (filePath == null) { continue; }

            string fileName = Path.GetFileName(filePath);
            string uniqueName = fileName;
            int index = 1;
            while (usedFileNames.Contains(uniqueName))
            {
                uniqueName = $"{fileName}_{index++}";
            }

            usedFileNames.Add(uniqueName);
            fileNameMap[filePath] = uniqueName;
        }

        return fileNameMap;
    }

    private static void packToZip(List<string> filePaths, Dictionary<string, string> fileNameMap, string jsonPath, string zipPath)
    {
        // allows for overwriting the .zip
        if (File.Exists(zipPath)) { File.Delete(zipPath); }

        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (string filePath in filePaths)
            {
                string fileName;
                if (fileNameMap.TryGetValue(filePath, out fileName) == false)
                {
                    Debug.LogError($"Key for fileNameMap not found: {filePath} - skipping this file in zip: {zipPath}");
                    continue;
                }
                
                zip.CreateEntryFromFile(filePath, fileName, System.IO.Compression.CompressionLevel.Fastest);
            }

            zip.CreateEntryFromFile(jsonPath, "config.json", System.IO.Compression.CompressionLevel.Fastest);
        }
    }
}

public class BlockConverter : JsonConverter
{
    // public override bool CanConvert(Type objectType) => objectType == typeof(BlockData);
    public override bool CanConvert(Type objectType)
    {
        return typeof(BlockData).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        string type = jo["type"]?.ToString();

        BlockData block = type switch
        {
            "SongBlock" => new SongBlockData(),
            "WaitBlock" => new WaitBlockData(),
            "WaitUntilBlock" => new WaitUntilBlockData(),

            _ => throw new Exception($"Unknown block type in deserialisation: {type}")
        };

        serializer.Populate(jo.CreateReader(), block);
        return block;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
