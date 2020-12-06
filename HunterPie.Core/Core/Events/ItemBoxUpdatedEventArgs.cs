using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using HunterPie.Core.Definitions;

namespace HunterPie.Core.Events
{
    public class ItemBoxUpdatedEventArgs : EventArgs
    {
        public IReadOnlyDictionary<int, sItem> consumables { get; private set; }
        public IReadOnlyDictionary<int, sItem> ammo { get; private set; }
        public IReadOnlyDictionary<int, sItem> materials { get; private set; }
        public IReadOnlyDictionary<int, sItem> decorations { get; private set; }

        public ItemBoxUpdatedEventArgs(ItemBox box)
        {
            consumables = box.consumables;
            ammo = box.ammo;
            materials = box.materials;
            decorations = box.decorations;
        }
    }
}
