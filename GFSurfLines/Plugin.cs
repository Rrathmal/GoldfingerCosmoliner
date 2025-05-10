using Dalamud.Game.Command;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using GFSurfLines.Windows;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Threading;
using Dalamud.Interface.Animation.EasingFunctions;
using System.Collections.Generic;

namespace GFSurfLines;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;

    private const string CommandName = "/gfcl";
    public Configuration Configuration { get; init; }
    public readonly WindowSystem WindowSystem = new("GFSurfLines");
    private ConfigWindow ConfigWindow { get; init; }
    private string? soundFileWav {  get; set; }
    private string? soundFileMp3 { get; set; }
    public DirectSoundOut? SoundOut;
    private bool flag101 = false;
    private bool flying = false;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        soundFileMp3 = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "board");
        soundFileWav = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "board.wav");

        //Decompress the mp3 file if necessary
        if (!File.Exists(soundFileWav))
        {
            using (var reader = new MediaFoundationReader(soundFileMp3))
            {
                WaveFileWriter.CreateWaveFile(soundFileWav, reader);
            }
        }

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the configuration menu."
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        Log.Information($"===yo waddup===");

        Condition.ConditionChange += ConditionOnChange;
    }

    private void ConditionOnChange(ConditionFlag flag, bool value)
    {
        if (Configuration.ValidMaps.Contains(ClientState.MapId))
        {
            if (flag == ConditionFlag.Unknown101)
            {
                flag101 = value;
            }
            else if (flag101 && flag == ConditionFlag.InFlight)
            {
                switch (value)
                {
                    case true:
                        //ConditionFlag.InFlight has started
                        flying = true;
                        PlaySound();
                        break;
                    case false:
                        //ConditionFlag.InFlight has ended
                        flying = false;
                        SoundOut.Pause();
                        break;
                }
            }
        }
    }

    
    public void PlaySound()
    {
        Task.Run(() =>
        {
            try
            {
                if (SoundOut == null)
                {
                    InitAudio();
                }
                SoundOut.Play();
            }
            catch
            {
                Log.Error("Failed play audio");
                return;
            }
        });
    }

    private void SoundOut_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (flying)
        {
            //Song has ended, loop
            SoundOut.Dispose();
            SoundOut = null;
            PlaySound();
        }
    }

    private void InitAudio()
    {
        if(SoundOut != null)
        {
            TrueStop();
            SoundOut.Dispose();
        }
        WaveStream reader = new MediaFoundationReader(soundFileWav);
        var audioStream = new WaveChannel32(reader)
        {
            Volume = Configuration.Volume / 100f,
            PadWithZeroes = false
        };
        SoundOut = new DirectSoundOut();
        SoundOut.Init(audioStream);
        SoundOut.PlaybackStopped += SoundOut_PlaybackStopped;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

        Condition.ConditionChange -= ConditionOnChange;

        if (SoundOut != null)
        {
            TrueStop();
            SoundOut.Dispose();
        }
    }

    public void TrueStop()
    {
        SoundOut.PlaybackStopped -= SoundOut_PlaybackStopped;
        SoundOut.Stop();
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        switch (command.ToLower())
        {
            case CommandName:
                ToggleConfigUI();
                break;
        }
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
