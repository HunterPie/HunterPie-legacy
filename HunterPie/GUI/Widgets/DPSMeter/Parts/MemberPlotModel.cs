using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using HunterPie.Core;
using OxyPlot;
using OxyPlot.Wpf;

namespace HunterPie.GUI.Widgets.DPSMeter.Parts
{
    public class MemberPlotModel
    {
        private readonly Member Member;
        private readonly ObservableCollection<DataPoint> DataPoints = new ObservableCollection<DataPoint>();
        public AreaSeries Series { get; }
        public bool HasData => DataPoints.Any(dp => dp.Y > 0);

        public MemberPlotModel(Member member, string color)
        {
            Member = member;
            Series = new AreaSeries {ItemsSource = DataPoints};
            ChangeColor(color);
        }

        public void ChangeColor(string hexColor)
        {
            Series.Color = (Color)ColorConverter.ConvertFromString(hexColor);
        }


        // ReSharper disable CompareOfFloatsByEqualityOperator - we're operating with int damage values, data loss is impossible here
        public void UpdateDamage(int now)
        {
            if (!Member.IsInParty)
            {
                // member isn't present, don't update chart
                return;
            }

            int dmg = Member.Damage;
            if (DataPoints.Count == 0)
            {
                // first entry must start from bottom of the chart, otherwise area will not be enclosed
                DataPoints.Add(new DataPoint(now, 0));
            }
            else if (DataPoints.Count > 1)
            {
                DataPoint last = DataPoints[DataPoints.Count - 1];
                DataPoint beforeLast = DataPoints[DataPoints.Count - 2];
                if (last.Y == dmg && beforeLast.Y == dmg)
                {
                    // To avoid spawning a lot of nodes with same damage value, we'll just update last one
                    DataPoints.RemoveAt(DataPoints.Count - 1);
                }
                else
                {
                    // In order for plot to have pillar lines instead of slopes, we'll add synthetic data point.
                    // This is point B' from representation below:
                    //         -B--  >          B--
                    //       /       >          |
                    //  --A/         >    --A---B'
                    DataPoints.Add(new DataPoint(now, last.Y));
                }
            }

            DataPoints.Add(new DataPoint(now, dmg));
        }
    }
}
