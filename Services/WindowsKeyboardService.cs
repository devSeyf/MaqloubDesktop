using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Maqloub.Services;

public sealed class WindowsKeyboardService
{
    private const uint InputKeyboard = 1;
    private const ushort VkControl = 0x11;
    private const ushort VkC = 0x43;
    private const uint KeyEventKeyUp = 0x0002;
 
    private const ushort VkV = 0x56;  // This prevent Virtualkey for "V" 



    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(
        uint inputCount,
        Input[] inputs,
        int inputSize);

    public bool SendCopyShortcut()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        var inputs = new[]
        {
            CreateKeyboardInput(VkControl, 0),
            CreateKeyboardInput(VkC, 0),
            CreateKeyboardInput(VkC, KeyEventKeyUp),
            CreateKeyboardInput(VkControl, KeyEventKeyUp)
        };

        var sentCount = SendInput(
            (uint)inputs.Length,
            inputs,
            Marshal.SizeOf<Input>());

        Debug.WriteLine($"Copy input events sent: {sentCount}");

        return sentCount == (uint)inputs.Length;
    }

    private static Input CreateKeyboardInput( ushort virtualKey,uint flags)
    {
        return new Input
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    VirtualKey = virtualKey,
                    Flags = flags
                }
            }
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MouseInput Mouse;

        [FieldOffset(0)]
        public KeyboardInput Keyboard;

        [FieldOffset(0)]
        public HardwareInput Hardware;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HardwareInput
    {
        public uint Message;
        public ushort ParameterLow;
        public ushort ParameterHigh;
    }




    // Method to send the Ctrl+V paste shortcut
    public bool SendPasteShortcut()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        var inputs = new[]
        {
        CreateKeyboardInput(VkControl, 0),
        CreateKeyboardInput(VkV, 0),
        CreateKeyboardInput(VkV, KeyEventKeyUp),
        CreateKeyboardInput(VkControl, KeyEventKeyUp)
    };

        var sentCount = SendInput(
            (uint)inputs.Length,
            inputs,
            Marshal.SizeOf<Input>());

        Debug.WriteLine($"Paste input events sent: {sentCount}");

        return sentCount == (uint)inputs.Length;
    }


}