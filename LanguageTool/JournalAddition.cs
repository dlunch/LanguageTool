using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;

namespace LanguageTool;

internal class JournalAddition : IDisposable
{
    private unsafe delegate void JournalDetailRefresh(AddonJournalDetail* addon, uint valueCount, AtkValue* values);
    [Signature("4C 8B DC 53 41 54 41 56 48 81 EC 40 01 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4", DetourName = nameof(OnJournalDetailRefresh))]
    private readonly Hook<JournalDetailRefresh>? journalDetailOnRefresh = null!;

    private readonly IGameGui gameGui;
    private readonly IAddonLifecycle addonLifecycle;
    private readonly ExcelSheet<Quest> additionalLanguageQuests;

    public JournalAddition(IGameInteropProvider gameInteropProvider, IAddonLifecycle addonLifecycle, IGameGui gameGui, GameData additionalLanguageGameData, Configuration configuration)
    {
        gameInteropProvider.InitializeFromAttributes(this);
        this.gameGui = gameGui;
        this.addonLifecycle = addonLifecycle;

        this.additionalLanguageQuests = additionalLanguageGameData.Excel.GetSheet<Quest>(configuration.AdditionalLanguage)!;

        this.journalDetailOnRefresh?.Enable();
        addonLifecycle.RegisterListener(AddonEvent.PostSetup, "JournalAccept", OnJournalAcceptPostSetup);
    }

    public void Dispose()
    {
        this.journalDetailOnRefresh?.Dispose();
    }

    private string? getAdditionalLanguageName(uint questId)
    {
        if (questId == 0)
        {
            return null;
        }

        var exdQuestId = questId + 65536; // why?
        return additionalLanguageQuests.GetRowOrDefault(exdQuestId)?.Name.ToString();
    }

    private unsafe void OnJournalDetailRefresh(AddonJournalDetail *addon, uint valueCount, AtkValue* values)
    {
        journalDetailOnRefresh?.Original(addon, valueCount, values);

        var questId = ((uint*)addon)[140];
        var questType = ((uint*)addon)[141];
        if (questType != 1)
        {
            return;
        }

        var additionalLanguageName = getAdditionalLanguageName(questId);
        if (additionalLanguageName.IsNullOrEmpty())
        {
            return;
        }

        var text = addon->DutyNameTextNode->NodeText;
        var newText = text + $"\n[{additionalLanguageName}]";

        addon->DutyNameTextNode->SetText(newText);
    }

    private unsafe void OnJournalAcceptPostSetup(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase *)args.Addon;

        var questId = ((uint*)addon)[172];
        var additionalLanguageName = getAdditionalLanguageName(questId);
        if (additionalLanguageName.IsNullOrEmpty())
        {
            return;
        }

        var textNode = (AtkTextNode*)addon->GetNodeById(34);
        var text = textNode->NodeText;
        var newText = text + $"\n[{additionalLanguageName}]";

        textNode->SetText(newText);
    }
}
