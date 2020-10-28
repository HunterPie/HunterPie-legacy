using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using HunterPie.Core;
using HunterPie.Core.Craft;
using HunterPie.Core.Definitions;
using HunterPie.Core.Jobs;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for LightBowgunControl.xaml
    /// </summary>
    public partial class LightBowgunControl : ClassControl
    {

        Player PlayerContext { get; set; }
        LightBowgun Context { get; set; }

        public string AmmoText
        {
            get { return (string)GetValue(AmmoTextProperty); }
            set { SetValue(AmmoTextProperty, value); }
        }
        public static readonly DependencyProperty AmmoTextProperty =
            DependencyProperty.Register("AmmoText", typeof(string), typeof(LightBowgunControl));

        public double AmmoPercentage
        {
            get { return (double)GetValue(AmmoPercentageProperty); }
            set { SetValue(AmmoPercentageProperty, value); }
        }
        public static readonly DependencyProperty AmmoPercentageProperty =
            DependencyProperty.Register("AmmoPercentage", typeof(double), typeof(LightBowgunControl));

        public double TotalAmmoCounter
        {
            get { return (double)GetValue(TotalAmmoCounterProperty); }
            set { SetValue(TotalAmmoCounterProperty, value); }
        }
        public static readonly DependencyProperty TotalAmmoCounterProperty =
            DependencyProperty.Register("TotalAmmoCounter", typeof(double), typeof(LightBowgunControl));

        public int CraftCount
        {
            get { return (int)GetValue(CraftCountProperty); }
            set { SetValue(CraftCountProperty, value); }
        }
        public static readonly DependencyProperty CraftCountProperty =
            DependencyProperty.Register("CraftCount", typeof(int), typeof(LightBowgunControl));

        public int CraftCountChildren
        {
            get { return (int)GetValue(CraftCountChildrenProperty); }
            set { SetValue(CraftCountChildrenProperty, value); }
        }
        public static readonly DependencyProperty CraftCountChildrenProperty =
            DependencyProperty.Register("CraftCountChildren", typeof(int), typeof(LightBowgunControl));

        public double ZoomBarHeight
        {
            get { return (double)GetValue(ZoomBarHeightProperty); }
            set { SetValue(ZoomBarHeightProperty, value); }
        }
        public static readonly DependencyProperty ZoomBarHeightProperty =
            DependencyProperty.Register("ZoomBarHeight", typeof(double), typeof(LightBowgunControl));

        public int GroundAmmoCounter
        {
            get { return (int)GetValue(GroundAmmoCounterProperty); }
            set { SetValue(GroundAmmoCounterProperty, value); }
        }
        public static readonly DependencyProperty GroundAmmoCounterProperty =
            DependencyProperty.Register("GroundAmmoCounter", typeof(int), typeof(LightBowgunControl));

        public string SpecialAmmoTimer
        {
            get { return (string)GetValue(SpecialAmmoTimerProperty); }
            set { SetValue(SpecialAmmoTimerProperty, value); }
        }
        public static readonly DependencyProperty SpecialAmmoTimerProperty =
            DependencyProperty.Register("SpecialAmmoTimer", typeof(string), typeof(LightBowgunControl));

        public float SpecialAmmoPercentage
        {
            get { return (float)GetValue(SpecialAmmoPercentageProperty); }
            set { SetValue(SpecialAmmoPercentageProperty, value); }
        }
        public static readonly DependencyProperty SpecialAmmoPercentageProperty =
            DependencyProperty.Register("SpecialAmmoPercentage", typeof(float), typeof(LightBowgunControl));

        public LightBowgunControl()
        {
            SpecialAmmoPercentage = 1;
            AmmoPercentage = 1;
            InitializeComponent();
        }

        public void SetContext(LightBowgun ctx, Player pCtx)
        {
            PlayerContext = pCtx;
            Context = ctx;
            HookEvents();
        }

        private void HookEvents()
        {
            Context.OnSpecialAmmoRegenUpdate += OnSpecialAmmoRegenUpdate;
            Context.OnGroundAmmoCountChange += OnGroundAmmoCountChange;
            Context.OnAmmoCountChange += OnAmmoCountChange;
            Context.OnEquippedAmmoChange += OnEquippedAmmoChange;
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
            PlayerContext.Inventory.OnInventoryUpdate += OnInventoryUpdate;
        }

        public override void UnhookEvents()
        {
            Context.OnSpecialAmmoRegenUpdate -= OnSpecialAmmoRegenUpdate;
            Context.OnGroundAmmoCountChange -= OnGroundAmmoCountChange;
            Context.OnAmmoCountChange -= OnAmmoCountChange;
            Context.OnEquippedAmmoChange -= OnEquippedAmmoChange;
            Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
            PlayerContext.Inventory.OnInventoryUpdate -= OnInventoryUpdate;
            base.UnhookEvents();
        }

        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                HasSafiBuff = args.SafijiivaRegenCounter != -1;
                SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
            }));
        }


        private void OnGroundAmmoCountChange(object source, LightBowgunEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                GroundAmmoCounter = args.GroundAmmo;
            }));
        }

        private void OnSpecialAmmoRegenUpdate(object source, LightBowgunEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                SpecialAmmoTimer = TimeSpan.FromSeconds(LightBowgun.GroundAmmoMaxTimer - args.SpecialAmmoRegen).ToString("mm\\:ss");
                SpecialAmmoPercentage = args.SpecialAmmoRegen / LightBowgun.GroundAmmoMaxTimer;
            }));
        }

        private void OnEquippedAmmoChange(object source, LightBowgunEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                sEquippedAmmo ammo = args.EquippedAmmo;
                sAmmo ammoInfo = args.Ammos.ElementAtOrDefault(ammo.index);
                AmmoText = $"{ammoInfo.Ammo}/{ammoInfo.Maximum}";
                AmmoPercentage = (double)ammoInfo.Ammo / (double)ammoInfo.Maximum;
                TotalAmmoCounter = ammo.ItemId == 137 ? double.PositiveInfinity : ammoInfo.Total;
                CalculateCrafting(ammo.ItemId);
            }));
        }

        private void OnAmmoCountChange(object source, LightBowgunEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                sEquippedAmmo ammo = args.EquippedAmmo;
                sAmmo ammoInfo = args.Ammos.ElementAtOrDefault(ammo.index);
                AmmoText = $"{ammoInfo.Ammo}/{ammoInfo.Maximum}";
                AmmoPercentage = (double)ammoInfo.Ammo / (double)ammoInfo.Maximum;
                TotalAmmoCounter = ammo.ItemId == 137 ? double.PositiveInfinity : ammoInfo.Total;
                CalculateCrafting(ammo.ItemId);
            }));
        }


        private void OnInventoryUpdate(object sender, Core.Events.InventoryUpdatedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                sEquippedAmmo ammo = Context.EquippedAmmo;
                sAmmo ammoInfo = Context.Ammos.ElementAtOrDefault(ammo.index);
                TotalAmmoCounter = ammo.ItemId == 137 ? double.PositiveInfinity : ammoInfo.Total;
                CalculateCrafting(ammo.ItemId);
            }));
        }

        private void LBGControl_Loaded(object sender, RoutedEventArgs e)
        {
            var args = new LightBowgunEventArgs(Context);
            OnSpecialAmmoRegenUpdate(this, args);
            OnGroundAmmoCountChange(this, args);
            OnAmmoCountChange(this, args);
            OnEquippedAmmoChange(this, args);
            OnSafijiivaCounterUpdate(this, new JobEventArgs(Context));
        }

        private void CalculateCrafting(int itemId)
        {
            Recipe ammoRecipe = Recipes.FindRecipe(itemId);

            if (ammoRecipe is null)
            {
                CraftCount = 0;
                CraftCountChildren = 0;
                return;
            }
            HashSet<int> itemIds = ammoRecipe.MaterialsNeeded.Select(m => m.ItemId).ToHashSet();

            sItem[] playerInventory = PlayerContext.Inventory.FindItemsAndAmmos(itemIds);
            
            CraftCount = Crafting.CalculateTotal(playerInventory, ammoRecipe);

            // Calculate craft from required items
            sItem[] ammosRequired = PlayerContext.Inventory.FindAmmos(itemIds);
            foreach (sItem ammoReq in ammosRequired)
            {
                Recipe ammoReqRecipe = Recipes.FindRecipe(ammoReq.ItemId);
                if (ammoReqRecipe is null)
                {
                    CraftCountChildren = 0;
                    continue;
                }
                HashSet<int> inv = ammoReqRecipe.MaterialsNeeded.Select(m => m.ItemId).ToHashSet();
                for (int i = 0; i < playerInventory.Length; i++)
                {
                    if (playerInventory[i].ItemId == ammoReq.ItemId)
                    {
                        playerInventory[i].Amount += Crafting.CalculateTotal(PlayerContext.Inventory.FindItemsAndAmmos(inv), ammoReqRecipe);
                        break;
                    }
                }
            }
            CraftCountChildren = Crafting.CalculateTotal(playerInventory, ammoRecipe) - CraftCount;
        }
    }
}
