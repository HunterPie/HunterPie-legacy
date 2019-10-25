using System;

namespace HunterPie.Memory {
    class Address {

        public const string GAME_VERSION = "167898";

        // Static addresses
        public const Int64 BASE = 0x140000000;
        public const Int64 LEVEL_OFFSET = 0x03B4A278;
        public const Int64 ZONE_OFFSET = 0x048F0E20;
        public const Int64 MONSTER_OFFSET = 0x48DE698;
        public const Int64 SESSION_OFFSET = 0x048D95E0;
        public const Int64 EQUIPMENT_OFFSET = 0x03BE71E0;
        public const Int64 WEAPON_OFFSET = 0x03BEBE18;
        public const Int64 PARTY_OFFSET = 0x48DF800;

        // Consts
        public const Int64 cooldownFixed = 0x9EC;
        public const Int64 cooldownDynamic = 0x99C;
        public const Int64 timerFixed = 0xADC;
        public const Int64 timerDynamic = 0xA8C;
    }
}
