using System;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using NAudio.Wave;

namespace GFSurfLines.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private Plugin parent;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Configuration###GFCosmoline")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 90);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
        parent = plugin;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        
    }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        var configValue = Configuration.Enabled;
        if (ImGui.Checkbox("Enabled", ref configValue))
        {
            Configuration.Enabled = configValue;
            //Disable the feature
            if (!configValue)
            {
                if (parent.SoundOut != null)
                {
                    parent.SoundOut.Stop();
                    parent.SoundOut.Dispose();
                    parent.SoundOut = null;
                }
            }
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            Configuration.Save();
        }

        var movable = Configuration.Volume;
        if(ImGui.DragInt("Volume: ", ref movable, 1f, 1,100))
        {
            Configuration.Volume = movable;
            if(parent.SoundOut != null)
            {
                bool wasRunning = false;
                if(parent.SoundOut.PlaybackState == PlaybackState.Playing)
                {
                    parent.SoundOut.Stop();
                    wasRunning = true;
                }
                parent.SoundOut.Dispose();
                parent.SoundOut = null;
                if (wasRunning)
                {
                    parent.PlaySound();
                }
            }
            Configuration.Save();
        }
    }
}
