using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using LanguageTool.Hooks.Tooltip;
using Lumina;
using Lumina.Excel;
using System;
using System.Text.RegularExpressions;

namespace LanguageTool;

internal class TooltipAdditions : IDisposable
{
    private readonly TooltipHook tooltipHook;
    private readonly RawExcelSheet additionalLanguageItems;
    private readonly RawExcelSheet additionalLanguageActions;
    private readonly RawExcelSheet additionalLanguageTraits;
    private readonly RawExcelSheet additionalLanguageExtraCommand;
    private readonly RawExcelSheet additionalLanguageMainCommand;
    private readonly RawExcelSheet additionalLanguageGeneralAction;
    private readonly RawExcelSheet additionalLanguagePetAction;


    public TooltipAdditions(TooltipHook tooltipHook, GameData additionalLanguageGameData, Configuration configuration)
    {
        this.tooltipHook = tooltipHook;

        this.additionalLanguageItems = additionalLanguageGameData.Excel.GetSheetRaw("Item", configuration.AdditionalLanguage)!;
        this.additionalLanguageActions = additionalLanguageGameData.Excel.GetSheetRaw("Action", configuration.AdditionalLanguage)!;
        this.additionalLanguageTraits = additionalLanguageGameData.Excel.GetSheetRaw("Trait", configuration.AdditionalLanguage)!;
        this.additionalLanguageExtraCommand = additionalLanguageGameData.Excel.GetSheetRaw("ExtraCommand", configuration.AdditionalLanguage)!;
        this.additionalLanguageMainCommand = additionalLanguageGameData.Excel.GetSheetRaw("MainCommand", configuration.AdditionalLanguage)!;
        this.additionalLanguageGeneralAction = additionalLanguageGameData.Excel.GetSheetRaw("GeneralAction", configuration.AdditionalLanguage)!;
        this.additionalLanguagePetAction = additionalLanguageGameData.Excel.GetSheetRaw("PetAction", configuration.AdditionalLanguage)!;

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
        var additionalLanguageName = additionalLanguageItems.GetRow((uint)tooltip.ItemId)?.ReadColumn<string>(9);
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
                additionalLanguageActions.GetRow(tooltip.Action.ActionID)?.ReadColumn<string>(0),
            HoverActionKind.Trait =>
                additionalLanguageTraits.GetRow(tooltip.Action.ActionID)?.ReadColumn<string>(0),
            HoverActionKind.ExtraCommand =>
                additionalLanguageExtraCommand.GetRow(tooltip.Action.ActionID)?.ReadColumn<string>(0),
            HoverActionKind.MainCommand =>
                additionalLanguageMainCommand.GetRow(tooltip.Action.ActionID)?.ReadColumn<string>(5),
            HoverActionKind.GeneralAction =>
                additionalLanguageGeneralAction.GetRow(tooltip.Action.ActionID)?.ReadColumn<string>(0),
            HoverActionKind.PetOrder =>
                additionalLanguagePetAction.GetRow(tooltip.Action.ActionID)?.ReadColumn<string>(0),
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
