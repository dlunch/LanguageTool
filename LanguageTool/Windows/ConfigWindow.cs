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
        var globalGamePath = this.configuration.GlobalGamePath;
        if (ImGui.InputText("Global Game Path", ref globalGamePath, 255))
        {
            this.configuration.GlobalGamePath = globalGamePath;
            this.configuration.Save();
        }
    }
}
