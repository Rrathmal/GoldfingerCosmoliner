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
    public List<uint> ValidMaps { get; set; } = new List<uint>() { 1011 };

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
