using Dalamud.Configuration;

namespace LanguageTool;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public string GlobalGamePath { get; set; } = string.Empty;
    public Lumina.Data.Language GlobalLanguage { get; set; } = Lumina.Data.Language.English;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
