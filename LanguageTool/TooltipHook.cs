using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Dalamud.Plugin.Services;
using Dalamud.Game.Gui;

namespace LanguageTool;

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
    }

    public void Dispose()
    {
        generateItemTooltipHook?.Dispose();
        generateActionTooltipHook?.Dispose();
    }

    public unsafe void* GenerateItemTooltipDetour(AtkUnitBase* addonItemDetail, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (lastItem != this.gameGui.HoveredItem)
        {
            this.OnItemTooltip?.Invoke(new ItemTooltip(this.gameGui.HoveredItem, numberArrayData, stringArrayData));
            lastItem = this.gameGui.HoveredItem;
        }

        return generateItemTooltipHook!.Original(addonItemDetail, numberArrayData, stringArrayData);
    }

    public unsafe void* GenerateActionTooltipDetour(AtkUnitBase* addonItemDetail, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (lastActionId != this.gameGui.HoveredAction.ActionID)
        {
            this.OnActionTooltip?.Invoke(new ActionTooltip(this.gameGui.HoveredAction, numberArrayData, stringArrayData));
            lastActionId = this.gameGui.HoveredAction.ActionID;
        }

        return generateActionTooltipHook.Original(addonItemDetail, numberArrayData, stringArrayData);
    }

}
