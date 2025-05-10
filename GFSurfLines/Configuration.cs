using Dalamud.Configuration;
using System;
using System.Collections.Generic;

namespace GFSurfLines;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public int Volume { get; set; } = 50;
    public bool Enabled { get; set; } = true;
    public List<uint> ValidMaps { get; set; } = new List<uint>() { 1007, 1008, 1009, 1010, 1011, 1012, 1013, 1014, 1015, 1016, 1017, 1018, 1019, 1020, 1021, 1022, 1023, 1024, 1025, 1026 };

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
