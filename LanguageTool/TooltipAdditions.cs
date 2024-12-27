using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina;
using Lumina.Excel;
using System;

namespace LanguageTool;

internal class TooltipAdditions : IDisposable
{
    readonly TooltipHook tooltipHook;
    RawExcelSheet additionalLanguageItems;
    RawExcelSheet additionalLanguageActions;

    public TooltipAdditions(TooltipHook tooltipHook, GameData additionalGameData, Configuration configuration)
    {
        this.tooltipHook = tooltipHook;

        this.additionalLanguageItems = additionalGameData.Excel.GetSheetRaw("Item", configuration.AdditionalLanguage)!;
        this.additionalLanguageActions = additionalGameData.Excel.GetSheetRaw("Action", configuration.AdditionalLanguage)!;

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
        var additionalLanguageName = additionalLanguageItems.GetRow((uint)tooltip.ItemId)!.ReadColumn<string>(9)!;

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
        var additionalLanguageName = additionalLanguageActions.GetRow((uint)tooltip.Action.ActionID)!.ReadColumn<string>(0)!;

        var rawString = tooltip.GetString((int)ActionTooltip.ActionTooltipField.Description);
        var newString = new SeString();
        newString.Append($"[{additionalLanguageName}]");
        newString.Append(NewLinePayload.Payload);
        newString.Append(rawString);

        tooltip.SetString((int)ActionTooltip.ActionTooltipField.Description, newString);
    }
}
