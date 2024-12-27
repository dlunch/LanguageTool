using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Dalamud.Plugin.Services;

namespace LanguageTool;

internal class TooltipHook : IDisposable
{
    private unsafe delegate byte ItemHoveredDelegate(IntPtr a1, IntPtr* a2, int* containerId, ushort* slotId, IntPtr a5, uint slotIdInt, IntPtr a7);
    [Signature("E8 ?? ?? ?? ?? 84 C0 0F 84 ?? ?? ?? ?? 48 89 9C 24 ?? ?? ?? ?? 48 89 B4 24", DetourName = nameof(ItemHoveredDetour))]
        private readonly Hook<ItemHoveredDelegate>? itemHoveredHook = null!;

    private unsafe delegate void* GenerateItemTooltip(AtkUnitBase* addonItemDetail, NumberArrayData* numberArrayData, StringArrayData* stringArrayData);
    [Signature("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC ?? 48 8B 42 ?? 4C 8B EA", DetourName = nameof(GenerateItemTooltipDetour))]
    private readonly Hook<GenerateItemTooltip>? generateItemTooltipHook = null!;

    private InventoryItem hoveredItem;
    private readonly IGameGui gameGui;

    public unsafe delegate void ItemTooltipDelegate(ItemTooltip itemTooltip);
    public event ItemTooltipDelegate? OnItemTooltip;

    private ulong lastItem;
    private bool blockItemTooltip;

    public TooltipHook(IGameInteropProvider gameInteropProvider, IGameGui gameGui)
    {
        gameInteropProvider.InitializeFromAttributes(this);

        itemHoveredHook?.Enable();
        generateItemTooltipHook?.Enable();

        this.gameGui = gameGui;
        gameGui.HoveredItemChanged += GuiOnHoveredItemChanged;
    }

    public void Dispose()
    {
        gameGui.HoveredItemChanged -= GuiOnHoveredItemChanged;
        itemHoveredHook?.Dispose();
        generateItemTooltipHook?.Dispose();
    }

    // prevent tooltip hook being called twice
    private void GuiOnHoveredItemChanged(object? sender, ulong e)
    {
        if (lastItem == 0 && e != 0)
        {
            blockItemTooltip = true;
            lastItem = e;
        }
        else if (lastItem != 0 && e == 0)
        {
            blockItemTooltip = true;
            lastItem = e;
        }
        else
        {
            blockItemTooltip = false;
            lastItem = e;
        }
    }


    private unsafe byte ItemHoveredDetour(IntPtr a1, IntPtr* a2, int* containerid, ushort* slotid, IntPtr a5, uint slotidint, IntPtr a7)
    {
        var returnValue = itemHoveredHook!.Original(a1, a2, containerid, slotid, a5, slotidint, a7);
        hoveredItem = *(InventoryItem*)(a7);
        return returnValue;
    }

    public unsafe void* GenerateItemTooltipDetour(AtkUnitBase* addonItemDetail, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (!blockItemTooltip)
        {
            this.OnItemTooltip?.Invoke(new ItemTooltip(hoveredItem, stringArrayData));
        }
        else
        {
            blockItemTooltip = false;
        }

        return generateItemTooltipHook!.Original(addonItemDetail, numberArrayData, stringArrayData);
    }
}
