using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;

namespace LanguageTool;

internal unsafe class TooltipBase(NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
{
    private readonly StringArrayData* stringArrayData = stringArrayData;
    private readonly NumberArrayData* numberArrayData = numberArrayData;

    public SeString GetString(int field)
    {
        if (stringArrayData->AtkArrayData.Size <= field)
            throw new IndexOutOfRangeException($"Attempted to get Index#{field} ({field}) but size is only {stringArrayData->AtkArrayData.Size}");

        var stringAddress = new IntPtr(stringArrayData->StringArray[field]);
        return MemoryHelper.ReadSeStringNullTerminated(stringAddress);
    }

    public void SetString(int field, SeString seString)
    {
        var bytes = seString.EncodeWithNullTerminator();
        stringArrayData->SetValue((int)field, bytes, false);
    }
}
