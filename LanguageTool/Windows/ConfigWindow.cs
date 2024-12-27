using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace LanguageTool.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;

    internal ConfigWindow(Configuration configuration) : base("LanguageToolConfig")
    {
        Size = new Vector2(600, 600);
        SizeCondition = ImGuiCond.FirstUseEver;

        this.configuration = configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var additionalGameData = this.configuration.AdditionalGamePath;
        if (ImGui.InputText("Additional Game Path", ref additionalGameData, 255))
        {
            this.configuration.AdditionalGamePath = additionalGameData;
            this.configuration.Save();
        }
    }
}
