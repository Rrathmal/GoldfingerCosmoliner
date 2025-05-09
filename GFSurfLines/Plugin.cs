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
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");

        Condition.ConditionChange += ConditionOnChange;
    }

    private void FrameworkOnUpdate(IFramework framework)
    {
        
    }

    private void ConditionOnChange(ConditionFlag flag, bool value)
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
                    //Unknown 101 has started
                    PlaySound();
                    break;
                case false:
                    //Unknown 101 has ended
                    SoundOut.Pause();
                    break;
            }
        }
    }

    public void PlaySound()
    {
        Task.Run(() =>
        {
            WaveStream reader;

            try
            {
                reader = new MediaFoundationReader(soundFileWav);
            }
            catch
            {
                Log.Error("Failed read sound file", soundFileWav);
                return;
            }

            var audioStream = new WaveChannel32(reader)
            {
                Volume = Configuration.Volume / 100f
            };
            using (reader)
            {
                try
                {
                    if (SoundOut == null)
                    {
                        SoundOut = new DirectSoundOut();
                        SoundOut.Init(audioStream);
                    }
                    SoundOut.Play();
                }
                catch
                {
                    Log.Error("Failed play sound");
                    return;
                }
            }
        });
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

        Condition.ConditionChange -= ConditionOnChange;
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
