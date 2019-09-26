using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterPie.Memory {
    class Address {
        // Static addresses
        public const Int64 BASE = 0x140000000;
        public const Int64 LEVEL_OFFSET = 0x03B48998;
        public const Int64 ZONE_OFFSET = 0x048EF560;
        public const Int64 MONSTER_OFFSET = 0x48DCDB8;
        public const Int64 SESSION_OFFSET = 0x048D95E0;
        public const Int64 EQUIPMENT_OFFSET = 0x03B4CD48;
        public const Int64 WEAPON_OFFSET = 0x03BEA538;
        public const Int64 PARTY_OFFSET = 0x48DDF20;

        // Consts
        public const Int64 cooldownFixed = 0x9EC;
        public const Int64 cooldownDynamic = 0x99C;
        public const Int64 timerFixed = 0xADC;
        public const Int64 timerDynamic = 0xA8C;
    }
}
