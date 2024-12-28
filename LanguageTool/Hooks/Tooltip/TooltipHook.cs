using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Dalamud.Plugin.Services;
using Dalamud.Game.Gui;

namespace LanguageTool.Hooks.Tooltip;

internal class TooltipHook : IDisposable
{
    private unsafe delegate void* GenerateItemTooltip(AtkUnitBase* addonItemDetail, NumberArrayData* numberArrayData, StringArrayData* stringArrayData);
    [Signature("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC ?? 48 8B 42 ?? 4C 8B EA", DetourName = nameof(GenerateItemTooltipDetour))]
    private readonly Hook<GenerateItemTooltip>? generateItemTooltipHook = null!;

    private unsafe delegate void* GenerateActionTooltip(AtkUnitBase* addonActionDetail, NumberArrayData* numberArrayData, StringArrayData* stringArrayData);
    [Signature("E8 ?? ?? ?? ?? 48 8B 43 ?? 48 8B 9F", DetourName = nameof(GenerateActionTooltipDetour))]
    private readonly Hook<GenerateActionTooltip> generateActionTooltipHook = null!;

    private readonly IGameGui gameGui;

    public unsafe delegate void ItemTooltipDelegate(ItemTooltip tooltip);
    public event ItemTooltipDelegate? OnItemTooltip;

    public unsafe delegate void ActionTooltipDelegate(ActionTooltip tooltip);
    public event ActionTooltipDelegate? OnActionTooltip;

    private ulong lastItem = 0;
    private ulong lastActionId = 0;

    public TooltipHook(IGameInteropProvider gameInteropProvider, IGameGui gameGui)
    {
        gameInteropProvider.InitializeFromAttributes(this);

        generateItemTooltipHook?.Enable();
        generateActionTooltipHook?.Enable();

        this.gameGui = gameGui;
        this.gameGui.HoveredItemChanged += OnHoveredItemChanged!;
        this.gameGui.HoveredActionChanged += OnHoveredActionChanged!;
    }

    public void Dispose()
    {
        gameGui.HoveredActionChanged -= OnHoveredActionChanged!;
        gameGui.HoveredItemChanged -= OnHoveredItemChanged!;
        generateItemTooltipHook?.Dispose();
        generateActionTooltipHook?.Dispose();
    }

    private void OnHoveredItemChanged(object sender, ulong itemId)
    {
        if (itemId == 0)
        {
            lastItem = 0;
        }
    }

    private void OnHoveredActionChanged(object sender, HoveredAction action)
    {
        if (action.ActionID == 0)
        {
            lastActionId = 0;
        }
    }

    private unsafe void* GenerateItemTooltipDetour(AtkUnitBase* addonItemDetail, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (lastItem != gameGui.HoveredItem)
        {
            OnItemTooltip?.Invoke(new ItemTooltip(gameGui.HoveredItem, numberArrayData, stringArrayData));
            lastItem = gameGui.HoveredItem;
        }

        return generateItemTooltipHook!.Original(addonItemDetail, numberArrayData, stringArrayData);
    }

    private unsafe void* GenerateActionTooltipDetour(AtkUnitBase* addonItemDetail, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (lastActionId != gameGui.HoveredAction.ActionID)
        {
            OnActionTooltip?.Invoke(new ActionTooltip(gameGui.HoveredAction, numberArrayData, stringArrayData));
            lastActionId = gameGui.HoveredAction.ActionID;
        }

        return generateActionTooltipHook.Original(addonItemDetail, numberArrayData, stringArrayData);
    }

}
