using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HunterPie.Core.Definitions;

namespace HunterPie.Core.Events
{
    public class InventoryUpdatedEventArgs : EventArgs
    {
        public IReadOnlyDictionary<int, sItem> Items;
        public IReadOnlyDictionary<int, sItem> Ammos;

        public InventoryUpdatedEventArgs(Inventory inventory)
        {
            Items = inventory.Items;
            Ammos = inventory.Ammo;
        }
    }
}
