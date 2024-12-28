using Dalamud.Game.Gui;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace LanguageTool.Hooks.Tooltip;

internal unsafe class ActionTooltip(HoveredAction action, NumberArrayData* numberArrayData, StringArrayData* stringArrayData) : TooltipBase(numberArrayData, stringArrayData)
{
    public HoveredAction Action { get; } = action;

    public enum ActionTooltipField
    {
        ActionName,
        ActionKind,
        Unknown02,
        RangeText,
        RangeValue,
        RadiusText,
        RadiusValue,
        MPCostText,
        MPCostValue,
        RecastText,
        RecastValue,
        CastText,
        CastValue,
        Description,
        Level,
        ClassJobAbbr,
    }
}
