using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;

namespace LanguageTool;

internal unsafe class ItemTooltip(InventoryItem item, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
{
    public InventoryItem Item { get; set; } = item;

    private readonly StringArrayData* stringArrayData = stringArrayData;
    private readonly NumberArrayData* numberArrayData = numberArrayData;

    public SeString GetString(ItemTooltipField field)
    {
        if (stringArrayData->AtkArrayData.Size <= (int)field)
            throw new IndexOutOfRangeException($"Attempted to get Index#{(int)field} ({field}) but size is only {stringArrayData->AtkArrayData.Size}");

        var stringAddress = new IntPtr(stringArrayData->StringArray[(int)field]);
        return MemoryHelper.ReadSeStringNullTerminated(stringAddress);
    }

    public void SetString(ItemTooltipField field, SeString seString)
    {
        var bytes = seString.EncodeWithNullTerminator();
        stringArrayData->SetValue((int)field, bytes, false);
    }

    public static unsafe bool IsFieldVisible(ItemTooltipFields tooltipField)
    {
        var flags = (ItemTooltipFields)RaptureAtkModule.Instance()->AtkArrayDataHolder.GetNumberArrayData(29)->IntArray[5];
        return flags.HasFlag(tooltipField);
    }

    public enum ItemTooltipField : byte
    {
        Name = 0,
        GlamourName = 1,
        Type = 2,
        Stat1Label = 4,
        Stat2Label = 5,
        Stat3Label = 6,
        Stat1 = 7,
        Stat2 = 8,
        Stat3 = 9,
        Stat1Delta = 10,
        Stat2Delta = 11,
        Stat3Delta = 12,
        Description = 13,
        Quantity = 14,
        EffectsLabel = 15,
        Effects = 16,
        EquipJobs = 22,
        EquipLevel = 23,
        VendorSellPrice = 25,
        Crafter = 26,
        Level = 27,
        Condition = 28,
        SpiritbondLabel = 29,
        Spiritbond = 30,
        RepairLevel = 31,
        Materials = 32,
        QuickRepairs = 33,
        MateriaMelding = 34,
        Capabilities = 35,
        BonusesLabel = 36,
        Bonus1 = 37,
        Bonus2 = 38,
        Bonus3 = 39,
        Bonus4 = 40,
        MateriaLabel = 52,
        Materia1 = 53,
        Materia2 = 54,
        Materia3 = 55,
        Materia4 = 56,
        Materia5 = 57,
        Materia1Effect = 58,
        Materia2Effect = 59,
        Materia3Effect = 60,
        Materia4Effect = 61,
        Materia5Effect = 62,
        ShopSellingPrice = 63,
        ControllerControls = 64,
    }

    [Flags]
    public enum ItemTooltipFields
    {
        Crafter = 1 << 0,
        Description = 1 << 1,
        VendorSellPrice = 1 << 2,
        Unknown3 = 1 << 3,
        Bonuses = 1 << 4,
        Materia = 1 << 5,
        CraftingAndRepairs = 1 << 6,
        Effects = 1 << 8,
        DyeableIndicator = 1 << 10,
        Stat1 = 1 << 11,
        Stat2 = 1 << 12,
        Stat3 = 1 << 13,

        Levels = 1 << 15,
        GlamourIndicator = 1 << 16,
        Unknown19 = 1 << 19,
    }
}
