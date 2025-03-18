using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using LanguageTool.Hooks.Tooltip;
using Lumina;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Text.RegularExpressions;

namespace LanguageTool;

internal class TooltipAdditions : IDisposable
{
    private readonly TooltipHook tooltipHook;
    private readonly ExcelSheet<Item> additionalLanguageItems;
    private readonly ExcelSheet<Lumina.Excel.Sheets.Action> additionalLanguageActions;
    private readonly ExcelSheet<Trait> additionalLanguageTraits;
    private readonly ExcelSheet<ExtraCommand> additionalLanguageExtraCommand;
    private readonly ExcelSheet<MainCommand> additionalLanguageMainCommand;
    private readonly ExcelSheet<GeneralAction> additionalLanguageGeneralAction;
    private readonly ExcelSheet<PetAction> additionalLanguagePetAction;
    private readonly ExcelSheet<BuddyAction> additionalLanguageBuddyAction;


    public TooltipAdditions(TooltipHook tooltipHook, GameData additionalLanguageGameData, Configuration configuration)
    {
        this.tooltipHook = tooltipHook;

        this.additionalLanguageItems = additionalLanguageGameData.Excel.GetSheet<Item>(configuration.AdditionalLanguage)!;
        this.additionalLanguageActions = additionalLanguageGameData.Excel.GetSheet<Lumina.Excel.Sheets.Action>(configuration.AdditionalLanguage)!;
        this.additionalLanguageTraits = additionalLanguageGameData.Excel.GetSheet<Trait>(configuration.AdditionalLanguage)!;
        this.additionalLanguageExtraCommand = additionalLanguageGameData.Excel.GetSheet<ExtraCommand>(configuration.AdditionalLanguage)!;
        this.additionalLanguageMainCommand = additionalLanguageGameData.Excel.GetSheet<MainCommand>(configuration.AdditionalLanguage)!;
        this.additionalLanguageGeneralAction = additionalLanguageGameData.Excel.GetSheet<GeneralAction>(configuration.AdditionalLanguage)!;
        this.additionalLanguagePetAction = additionalLanguageGameData.Excel.GetSheet<PetAction>(configuration.AdditionalLanguage)!;
        this.additionalLanguageBuddyAction = additionalLanguageGameData.Excel.GetSheet<BuddyAction>(configuration.AdditionalLanguage)!;

        tooltipHook.OnItemTooltip += this.OnItemTooltip;
        tooltipHook.OnActionTooltip += this.OnActionTooltip;
    }

    public void Dispose()
    {
        tooltipHook.OnItemTooltip -= this.OnItemTooltip;
        tooltipHook.OnActionTooltip -= this.OnActionTooltip;
    }

    private void OnItemTooltip(ItemTooltip tooltip)
    {
        var additionalLanguageName = additionalLanguageItems.GetRowOrDefault((uint)tooltip.ItemId)?.Name.ToString();
        if (additionalLanguageName == null)
        {
            return;
        }

        ItemTooltip.ItemTooltipField targetField;
        var newString = new SeString();
        if (ItemTooltip.IsFieldVisible(ItemTooltip.ItemTooltipFields.Description))
        {
            targetField = ItemTooltip.ItemTooltipField.Description;
            var rawString = tooltip.GetString((int)targetField);
            newString.Append($"[{additionalLanguageName}]");
            newString.Append(NewLinePayload.Payload);
            newString.Append(rawString);
        }
        else if (ItemTooltip.IsFieldVisible(ItemTooltip.ItemTooltipFields.Effects))
        {
            targetField = ItemTooltip.ItemTooltipField.Effects;
            var rawString = tooltip.GetString((int)targetField);
            newString.Append($"[{additionalLanguageName}]");
            newString.Append(NewLinePayload.Payload);
            newString.Append(rawString);
        }
        else if (ItemTooltip.IsFieldVisible(ItemTooltip.ItemTooltipFields.Levels))
        {
            targetField = ItemTooltip.ItemTooltipField.EquipLevel;
            var rawString = tooltip.GetString((int)targetField);
            newString.Append(rawString);
            newString.Append(NewLinePayload.Payload);
            newString.Append(new UIForegroundPayload(1));
            newString.Append($"[{additionalLanguageName}]");
        }
        else
        {
            return;
        }


        tooltip.SetString((int)targetField, newString);
    }

    private void OnActionTooltip(ActionTooltip tooltip)
    {
        var additionalLanguageName = tooltip.Action.ActionKind switch
        {
            HoverActionKind.Action =>
                additionalLanguageActions.GetRowOrDefault(tooltip.Action.ActionID)?.Name.ToString(),
            HoverActionKind.Trait =>
                additionalLanguageTraits.GetRowOrDefault(tooltip.Action.ActionID)?.Name.ToString(),
            HoverActionKind.ExtraCommand =>
                additionalLanguageExtraCommand.GetRowOrDefault(tooltip.Action.ActionID)?.Name.ToString(),
            HoverActionKind.MainCommand =>
                additionalLanguageMainCommand.GetRowOrDefault(tooltip.Action.ActionID)?.Name.ToString(),
            HoverActionKind.GeneralAction =>
                additionalLanguageGeneralAction.GetRowOrDefault(tooltip.Action.ActionID)?.Name.ToString(),
            HoverActionKind.PetOrder =>
                additionalLanguagePetAction.GetRowOrDefault(tooltip.Action.ActionID)?.Name.ToString(),
            HoverActionKind.CompanionOrder =>
                additionalLanguageBuddyAction.GetRowOrDefault(tooltip.Action.ActionID)?.Name.ToString(),
            _ => null,
        };


        if (additionalLanguageName == null)
        {
            return;
        }

        var rawString = tooltip.GetString((int)ActionTooltip.ActionTooltipField.Description);
        var newString = new SeString();
        newString.Append($"[{additionalLanguageName}]");
        newString.Append(NewLinePayload.Payload);
        newString.Append(rawString);

        tooltip.SetString((int)ActionTooltip.ActionTooltipField.Description, newString);
    }
}
