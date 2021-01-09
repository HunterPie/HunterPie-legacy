using System.Runtime.InteropServices;
using HunterPie.Core.Definitions;
using HunterPie.Memory;

namespace HunterPie.Core.Input
{
    public static class PlayerKeyboard
    {
        public static sKeyConfig Get(MenuControls key)
        {
            int offset = Marshal.SizeOf<sKeyConfig>();

            long ptr = Kernel.Read<long>(Address.GetAddress("BASE") + Address.GetAddress("SETTINGS_KEYBOARD"));
            return Kernel.ReadStructure<sKeyConfig>(ptr + 0xA50 + (offset * (int)key));
        }
    }

    public enum MenuControls : int
    {
        Confirm,
        Back,
        PageLeft,
        PageRight,
        CategoryLeft,
        CategoryRight,
        Unknown01,
        MenuLeft,
        MenuRight,
        MenuUp,
        MenuDown,
        Unknown02, // Empty
        Unknown03, // A
        Unknown04, // D
        Unknown05, // W
        Unknown06, // S
        RotateLeft,
        RotateRight,
        Unknown07, // Empty
        Unknown08, // Empty
        RegisterLoadout,
        Unknown09, // 0x11
        Unknown10, // 0x10
        Unknown11, // 0x09
        Unknown12, // F
        Unknown13, // Empty
        Restock,
        ToggleWildlife,
        Unknown14, // Empty
        Unknown15, // 0x10


    }
}
