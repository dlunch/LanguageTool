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
        var description = itemTooltip.GetString(ItemTooltip.ItemTooltipField.Description);

        var newString = new SeString();
        newString.Append($"[{globalName}]");
        newString.Append(NewLinePayload.Payload);
        newString.Append(description);

        itemTooltip.SetString(ItemTooltip.ItemTooltipField.Description, newString);
    }
}
