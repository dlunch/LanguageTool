using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina;
using Lumina.Excel;
using System;

namespace LanguageTool;

internal class JournalAddition : IDisposable
{
    private unsafe delegate void JournalDetailRefresh(AddonJournalDetail* addon, uint valueCount, AtkValue* values);
    [Signature("4C 8B DC 53 41 54 41 56 48 81 EC 40 01 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4", DetourName = nameof(OnJournalDetailRefresh))]
    private readonly Hook<JournalDetailRefresh>? journalDetailOnRefresh = null!;

    private readonly IGameGui gameGui;
    private readonly RawExcelSheet additionalLanguageQuests;

    public JournalAddition(IGameInteropProvider gameInteropProvider, IGameGui gameGui, GameData additionalLanguageGameData, Configuration configuration)
    {
        gameInteropProvider.InitializeFromAttributes(this);
        this.gameGui = gameGui;

        this.additionalLanguageQuests = additionalLanguageGameData.Excel.GetSheetRaw("Quest", configuration.AdditionalLanguage)!;

        this.journalDetailOnRefresh?.Enable();
    }

    public void Dispose()
    {
        this.journalDetailOnRefresh?.Disable();
    }

    private unsafe void OnJournalDetailRefresh(AddonJournalDetail *addon, uint valueCount, AtkValue* values)
    {
        journalDetailOnRefresh?.Original(addon, valueCount, values);

        var agentQuestJournal = AgentQuestJournal.Instance();

        var text = addon->DutyNameTextNode->NodeText;
        var questId = agentQuestJournal->SelectedQuestId;
        var exdQuestId = questId + 65535; // why?

        var additionalLanguageName = additionalLanguageQuests.GetRow(exdQuestId)?.ReadColumn<string>(0);
        if (additionalLanguageName.IsNullOrEmpty())
        {
            return;
        }

        var newText = text + $"\n[{additionalLanguageName}]";

        addon->DutyNameTextNode->SetText(newText);
    }
}
