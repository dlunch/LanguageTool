using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using LanguageTool.Windows;
using Lumina;

namespace LanguageTool;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;

    public Configuration Configuration { get; init; } = null!;
    private ConfigWindow ConfigWindow { get; init; } = null!;
    private TooltipHook TooltipHook { get; init; } = null!;
    private TooltipAdditions TooltipAdditions { get; init; } = null!;

    public readonly WindowSystem WindowSystem = new("LanguageTool");

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        ConfigWindow = new ConfigWindow(Configuration);

        WindowSystem.AddWindow(ConfigWindow);

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        TooltipHook = new TooltipHook(GameInteropProvider, GameGui);

        var globalGameData = new GameData(Configuration.GlobalGamePath);

        TooltipAdditions = new TooltipAdditions(TooltipHook, globalGameData, Configuration);
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();

        TooltipHook.Dispose();
        TooltipAdditions.Dispose();
    }

    private void DrawUI() => WindowSystem.Draw();

    private void ToggleConfigUI() => ConfigWindow.Toggle();
}
