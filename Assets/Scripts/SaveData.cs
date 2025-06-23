using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
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

public static class SaveData
{
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

    public static BlockDataListWrapper load(string zipPath, out string extractPath)
    {
        // extract the zip file
        string projectName = Path.GetFileNameWithoutExtension(zipPath);
        extractPath = Application.persistentDataPath + $"/{projectName}";
        if (Directory.Exists(extractPath))
        {
            // TODO handle overwrites
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
        return JsonConvert.DeserializeObject<BlockDataListWrapper>(json, settings);
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
