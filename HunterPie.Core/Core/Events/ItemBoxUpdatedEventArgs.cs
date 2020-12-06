using System;
using System.Collections.Generic;

namespace HunterPie.Core.Events
{
    public class ItemBoxUpdatedEventArgs : EventArgs
    {
        public IReadOnlyDictionary<int, int> consumables { get; }
        public IReadOnlyDictionary<int, int> ammo { get; }
        public IReadOnlyDictionary<int, int> materials { get; }
        public IReadOnlyDictionary<int, int> decorations { get; }

        public ItemBoxUpdatedEventArgs(ItemBox box)
        {
            consumables = box.Consumables;
            ammo = box.Ammo;
            materials = box.Materials;
            decorations = box.Decorations;
        }
    }
}
