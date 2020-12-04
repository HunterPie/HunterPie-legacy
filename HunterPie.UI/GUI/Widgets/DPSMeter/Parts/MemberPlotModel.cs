using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.Core.Enums;
using OxyPlot;
using OxyPlot.Wpf;

namespace HunterPie.GUI.Widgets.DPSMeter.Parts
{
    public class MemberPlotModel
    {
        private readonly Member Member;

        /// <summary>
        /// All data points samples. This is exact data (same ref) that is used to display damage total plot.
        /// This will be updated regardless of current mode.
        /// </summary>
        private readonly ObservableCollection<DataPoint> DamagePoints = new ObservableCollection<DataPoint>();

        /// <summary>
        /// Reference to collection that is used by Series as source.
        /// </summary>
        private ObservableCollection<DataPoint> Data;

        public AreaSeries Series { get; }
        public bool HasData => DamagePoints.Any(dp => dp.Y > 0);
        public DamagePlotMode Mode { get; private set; }

        public MemberPlotModel(Member member, string color, DamagePlotMode mode)
        {
            Member = member;
            Series = new AreaSeries();

            ChangeMode(mode);
            ChangeColor(color);
        }

        public void ChangeColor(string hexColor)
        {
            Series.Color = (Color)ColorConverter.ConvertFromString(hexColor);
        }

        public void ChangeMode(DamagePlotMode mode)
        {
            if (Mode == mode && Series.ItemsSource != null) return;

            switch (mode)
            {
                case DamagePlotMode.Dps:
                    Data = new ObservableCollection<DataPoint>(DamageToDps(DamagePoints));
                    Series.ItemsSource = Data;
                    break;

                case DamagePlotMode.CumulativeTotal:
                    Series.ItemsSource = Data = DamagePoints;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            Mode = mode;
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator - we're operating with int damage values, data loss is impossible here
        private static IEnumerable<DataPoint> DamageToDps(IList<DataPoint> damagePoints)
        {
            for (int i = 0; i < damagePoints.Count; i++)
            {
                DataPoint currentPoint = damagePoints[i];

                // skipping all but last of sequential points with same exact time.
                bool isSynthetic = i < damagePoints.Count - 1 && currentPoint.X == damagePoints[i + 1].X;
                if (isSynthetic && i != 0) continue;

                yield return DmgToDps(currentPoint);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DataPoint DmgToDps(DataPoint dmgPoint) => dmgPoint.X == 0
            // point with zero time should not be possible under normal conditions, but just to be safe...
            ? dmgPoint
            // using DPS * 1000, since we don't show any scales on UI. That way we can use integers instead of pesky doubles
            : new DataPoint(dmgPoint.X, (int)(dmgPoint.Y / dmgPoint.X * 1000 * 1000));


        // ReSharper disable CompareOfFloatsByEqualityOperator - we're operating with int damage values, data loss is impossible here
        public void UpdateDamage(int now)
        {
            if (!Member.IsInParty)
            {
                // member isn't present, don't update plot
                return;
            }

            int dmg = Member.Damage;
            if (DamagePoints.Count == 0)
            {
                // first entry must start from bottom of the plot, otherwise area will not be enclosed
                DamagePoints.Add(new DataPoint(now, 0));
            }
            else if (DamagePoints.Count > 1)
            {
                DataPoint last = DamagePoints[DamagePoints.Count - 1];
                DataPoint beforeLast = DamagePoints[DamagePoints.Count - 2];
                if (last.Y == dmg && beforeLast.Y == dmg)
                {
                    // To avoid spawning a lot of nodes with same damage value, we'll just update last one
                    DamagePoints.RemoveAt(DamagePoints.Count - 1);
                }
                else
                {
                    // In order for plot to have pillar lines instead of slopes, we'll add synthetic data point.
                    // This is point B' from representation below:
                    //         -B--  >          B--
                    //       /       >          |
                    //  --A/         >    --A---B'
                    DamagePoints.Add(new DataPoint(now, last.Y));

                    // DPS plot can use slopes, since it is continuous function of time, so no additional points are needed.
                }
            }

            DataPoint newPoint = new DataPoint(now, dmg);
            DamagePoints.Add(newPoint);

            if (Mode == DamagePlotMode.Dps)
            {
                UpdateDpsData(now, ref newPoint);
            }
        }

        private void UpdateDpsData(int now, ref DataPoint newPoint)
        {
            if (Data.Count == 0)
            {
                // first entry must start from bottom of the plot, otherwise area will not be enclosed
                Data.Add(new DataPoint(now, 0));
            }

            var dpsPoint = DmgToDps(newPoint);
            if (Data[Data.Count - 1].Y == dpsPoint.Y)
            {
                // To avoid spawning a lot of nodes with same damage value, we'll just update last one
                Data.RemoveAt(Data.Count - 1);
            }
            Data.Add(dpsPoint);
        }
    }
}
