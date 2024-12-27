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

    public TooltipAdditions(TooltipHook tooltipHook, GameData additionalGameData, Configuration configuration)
    {
        this.tooltipHook = tooltipHook;

        this.additionalLanguageItems = additionalGameData.Excel.GetSheetRaw("Item", configuration.AdditionalLanguage)!;

        tooltipHook.OnItemTooltip += this.OnItemTooltip;
    }

    public void Dispose()
    {
        tooltipHook.OnItemTooltip -= this.OnItemTooltip;
    }

    public void OnItemTooltip(ItemTooltip itemTooltip)
    {
        var additionalLanguageName = additionalLanguageItems.GetRow(itemTooltip.Item.ItemId)!.ReadColumn<string>(9)!;

        ItemTooltip.ItemTooltipField targetField;
        var newString = new SeString();
        if (ItemTooltip.IsFieldVisible(ItemTooltip.ItemTooltipFields.Description))
        {
            targetField = ItemTooltip.ItemTooltipField.Description;
            var rawString = itemTooltip.GetString(targetField);
            newString.Append($"[{additionalLanguageName}]");
            newString.Append(NewLinePayload.Payload);
            newString.Append(rawString);
        }
        else if (ItemTooltip.IsFieldVisible(ItemTooltip.ItemTooltipFields.Effects))
        {
            targetField = ItemTooltip.ItemTooltipField.Effects;
            var rawString = itemTooltip.GetString(targetField);
            newString.Append($"[{additionalLanguageName}]");
            newString.Append(NewLinePayload.Payload);
            newString.Append(rawString);
        }
        else if (ItemTooltip.IsFieldVisible(ItemTooltip.ItemTooltipFields.Levels))
        {
            targetField = ItemTooltip.ItemTooltipField.EquipLevel;
            var rawString = itemTooltip.GetString(targetField);
            newString.Append(rawString);
            newString.Append(NewLinePayload.Payload);
            newString.Append(new UIForegroundPayload(1));
            newString.Append($"[{additionalLanguageName}]");
        }
        else
        {
            return;
        }


        itemTooltip.SetString(targetField, newString);
    }
}
