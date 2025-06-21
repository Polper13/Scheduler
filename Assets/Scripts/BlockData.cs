using System;
using Newtonsoft.Json;

[Serializable]
public abstract class BlockData
{
    [JsonProperty(Order = 1)] public string type;
}

[Serializable]
public class WaitBlockData : BlockData
{
    [JsonProperty(Order = 2)] public int value;
}

[Serializable]
public class WaitUntilBlockData : BlockData
{
    [JsonProperty(Order = 2)] public string value;
}

[Serializable]
public class SongBlockData : BlockData
{
    [JsonProperty(Order = 2)] public string fileName;
    [JsonProperty(Order = 3)] public SongBlockSettings settings;
}