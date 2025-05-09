using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace GFSurfLines;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public int Volume { get; set; } = 100;
    public bool Enabled { get; set; } = true;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
