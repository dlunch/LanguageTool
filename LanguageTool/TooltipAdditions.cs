using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina;
using Lumina.Excel;
using System;

namespace LanguageTool;

internal class TooltipAdditions : IDisposable
{
    readonly TooltipHook tooltipHook;
    RawExcelSheet globalItem;

    public TooltipAdditions(TooltipHook tooltipHook, GameData globalGameData, Configuration configuration)
    {
        this.tooltipHook = tooltipHook;

        this.globalItem = globalGameData.Excel.GetSheetRaw("Item", configuration.GlobalLanguage)!;

        tooltipHook.OnItemTooltip += this.OnItemTooltip;
    }

    public void Dispose()
    {
        tooltipHook.OnItemTooltip -= this.OnItemTooltip;
    }

    public void OnItemTooltip(ItemTooltip itemTooltip)
    {
        var globalName = globalItem.GetRow(itemTooltip.Item.ItemId)!.ReadColumn<string>(9)!;

        ItemTooltip.ItemTooltipField targetField;
        var newString = new SeString();
        if (ItemTooltip.IsFieldVisible(ItemTooltip.ItemTooltipFields.Description))
        {
            targetField = ItemTooltip.ItemTooltipField.Description;
            var rawString = itemTooltip.GetString(targetField);
            newString.Append($"[{globalName}]");
            newString.Append(NewLinePayload.Payload);
            newString.Append(rawString);
        }
        else if (ItemTooltip.IsFieldVisible(ItemTooltip.ItemTooltipFields.Effects))
        {
            targetField = ItemTooltip.ItemTooltipField.Effects;
            var rawString = itemTooltip.GetString(targetField);
            newString.Append($"[{globalName}]");
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
            newString.Append($"[{globalName}]");
        }
        else
        {
            return;
        }


        itemTooltip.SetString(targetField, newString);
    }
}
