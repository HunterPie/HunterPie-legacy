using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using HunterPie.Core;
using HunterPie.Core.Craft;
using HunterPie.Core.Definitions;
using HunterPie.Core.Enums;
using HunterPie.Core.Jobs;
using HunterPie.Core.Events;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for HeavyBowgunControl.xaml
    /// </summary>
    public partial class HeavyBowgunControl : ClassControl
    {

        Player PlayerContext { get; set; }
        HeavyBowgun Context { get; set; }

        public string AmmoText
        {
            get { return (string)GetValue(AmmoTextProperty); }
            set { SetValue(AmmoTextProperty, value); }
        }
        public static readonly DependencyProperty AmmoTextProperty =
            DependencyProperty.Register("AmmoText", typeof(string), typeof(HeavyBowgunControl));

        public double AmmoPercentage
        {
            get { return (double)GetValue(AmmoPercentageProperty); }
            set { SetValue(AmmoPercentageProperty, value); }
        }
        public static readonly DependencyProperty AmmoPercentageProperty =
            DependencyProperty.Register("AmmoPercentage", typeof(double), typeof(HeavyBowgunControl));

        public double TotalAmmoCounter
        {
            get { return (double)GetValue(TotalAmmoCounterProperty); }
            set { SetValue(TotalAmmoCounterProperty, value); }
        }
        public static readonly DependencyProperty TotalAmmoCounterProperty =
            DependencyProperty.Register("TotalAmmoCounter", typeof(double), typeof(HeavyBowgunControl));

        public int CraftCount
        {
            get { return (int)GetValue(CraftCountProperty); }
            set { SetValue(CraftCountProperty, value); }
        }
        public static readonly DependencyProperty CraftCountProperty =
            DependencyProperty.Register("CraftCount", typeof(int), typeof(HeavyBowgunControl));

        public int CraftCountChildren
        {
            get { return (int)GetValue(CraftCountChildrenProperty); }
            set { SetValue(CraftCountChildrenProperty, value); }
        }
        public static readonly DependencyProperty CraftCountChildrenProperty =
            DependencyProperty.Register("CraftCountChildren", typeof(int), typeof(HeavyBowgunControl));

        public double ZoomBarHeight
        {
            get { return (double)GetValue(ZoomBarHeightProperty); }
            set { SetValue(ZoomBarHeightProperty, value); }
        }
        public static readonly DependencyProperty ZoomBarHeightProperty =
            DependencyProperty.Register("ZoomBarHeight", typeof(double), typeof(HeavyBowgunControl));

        public string SpecialAmmoTimer
        {
            get { return (string)GetValue(SpecialAmmoTimerProperty); }
            set { SetValue(SpecialAmmoTimerProperty, value); }
        }
        public static readonly DependencyProperty SpecialAmmoTimerProperty =
            DependencyProperty.Register("SpecialAmmoTimer", typeof(string), typeof(HeavyBowgunControl));

        public float SpecialAmmoPercentage
        {
            get { return (float)GetValue(SpecialAmmoPercentageProperty); }
            set { SetValue(SpecialAmmoPercentageProperty, value); }
        }
        public static readonly DependencyProperty SpecialAmmoPercentageProperty =
            DependencyProperty.Register("SpecialAmmoPercentage", typeof(float), typeof(HeavyBowgunControl));

        public bool IsZoomEquipped
        {
            get { return (bool)GetValue(IsZoomEquippedProperty); }
            set { SetValue(IsZoomEquippedProperty, value); }
        }
        public static readonly DependencyProperty IsZoomEquippedProperty =
            DependencyProperty.Register("IsZoomEquipped", typeof(bool), typeof(HeavyBowgunControl));

        public string ZoomValue
        {
            get { return (string)GetValue(ZoomValueProperty); }
            set { SetValue(ZoomValueProperty, value); }
        }
        public static readonly DependencyProperty ZoomValueProperty =
            DependencyProperty.Register("ZoomValue", typeof(string), typeof(HeavyBowgunControl));

        public HeavyBowgunControl()
        {
            SpecialAmmoPercentage = 1;
            AmmoPercentage = 1;
            InitializeComponent();
        }

        public void SetContext(HeavyBowgun ctx, Player pCtx)
        {
            PlayerContext = pCtx;
            Context = ctx;
            HookEvents();
        }

        private void HookEvents()
        {
            Context.OnAmmoCountChange += OnAmmoCountChange;
            Context.OnEquippedAmmoChange += OnEquippedAmmoChange;
            Context.OnScopeMultiplierChange += OnScopeMultiplierChange;
            Context.OnScopeStateChange += OnScopeStateChange;
            Context.OnWyvernheartUpdate += OnWyvernheartUpdate;
            Context.OnWyvernsnipeUpdate += OnWyvernsnipeUpdate;
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
            PlayerContext.Inventory.OnInventoryUpdate += OnInventoryUpdate;
        }

        public override void UnhookEvents()
        {
            Context.OnAmmoCountChange -= OnAmmoCountChange;
            Context.OnEquippedAmmoChange -= OnEquippedAmmoChange;
            Context.OnScopeMultiplierChange -= OnScopeMultiplierChange;
            Context.OnScopeStateChange -= OnScopeStateChange;
            Context.OnWyvernheartUpdate -= OnWyvernheartUpdate;
            Context.OnWyvernsnipeUpdate -= OnWyvernsnipeUpdate;
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

        private void OnWyvernsnipeUpdate(object source, HeavyBowgunEventArgs args)
        {
            if (args.SpecialAmmoType == HBGSpecialType.Wyvernsniper)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    SpecialAmmoTimer = $"{TimeSpan.FromSeconds(args.WyvernsnipeMaxTimer - args.WyvernsnipeTimer):m\\:ss}";
                    SpecialAmmoPercentage = 1 - ((args.WyvernsnipeMaxTimer - args.WyvernsnipeTimer) / args.WyvernsnipeMaxTimer);

                    SpecialAmmoPercentage = SpecialAmmoPercentage == 0 ? 1 : SpecialAmmoPercentage;
                }));
            }
        }

        private void OnWyvernheartUpdate(object source, HeavyBowgunEventArgs args)
        {
            if (args.SpecialAmmoType == HBGSpecialType.Wyvernheart)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    SpecialAmmoTimer = $"{args.WyvernheartTimer:0}/{args.WyvernheartMaxAmmo:0}";
                    SpecialAmmoPercentage = args.WyvernheartTimer / args.WyvernheartMaxAmmo;
                }));
            }
        }

        private void OnScopeStateChange(object source, HeavyBowgunEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                IsZoomEquipped = args.HasScopeEquipped;
            }));
        }

        private void OnScopeMultiplierChange(object source, HeavyBowgunEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                ZoomBarHeight = 78 * args.ScopeZoomMultiplier;
                ZoomValue = $"{args.ScopeZoomMultiplier:0.0}x";
            }));
        }

        private void OnEquippedAmmoChange(object source, HeavyBowgunEventArgs args)
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

        private void OnAmmoCountChange(object source, HeavyBowgunEventArgs args)
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

        private void HBGControl_Loaded(object sender, RoutedEventArgs e)
        {
            var args = new HeavyBowgunEventArgs(Context);
            OnEquippedAmmoChange(this, args);
            OnWyvernheartUpdate(this, args);
            OnWyvernsnipeUpdate(this, args);
            OnScopeStateChange(this, args);
            OnScopeMultiplierChange(this, args);
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
