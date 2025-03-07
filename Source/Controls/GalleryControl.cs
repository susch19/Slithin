﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace Slithin.Controls
{
    public class GalleryControl : ItemsControl
    {
        public static StyledProperty<bool> AreButtonsVisibleProperty =
           AvaloniaProperty.Register<GalleryControl, bool>(nameof(AreButtonsVisible), true);

        public static StyledProperty<ObservableCollection<Indicator>> IndicatorsProperty =
                    AvaloniaProperty.Register<GalleryControl, ObservableCollection<Indicator>>(nameof(Indicators), new());

        public bool AreButtonsVisible
        {
            get { return GetValue(AreButtonsVisibleProperty); }
            set { SetValue(AreButtonsVisibleProperty, value); }
        }

        public ObservableCollection<Indicator> Indicators
        {
            get { return GetValue(IndicatorsProperty); }
            set { SetValue(IndicatorsProperty, value); }
        }

        protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Indicators.Add(new());
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            var part_left = e.NameScope.Find<Button>("PART_LEFT");
            var part_carousel = e.NameScope.Find<Carousel>("PART_CAROUSEL");
            var part_right = e.NameScope.Find<Button>("PART_RIGHT");

            var timer = new Timer();
            timer.Elapsed += async (s, e) =>
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (part_carousel.SelectedIndex < part_carousel.ItemCount - 1)
                    {
                        part_carousel.SelectedIndex++;
                    }
                    else
                    {
                        timer.Stop();
                    }

                    part_right.IsEnabled = part_carousel.SelectedIndex < part_carousel.ItemCount - 1;
                    part_left.IsEnabled = true;
                });
            };
            timer.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;

            part_left.Click += (s, e) =>
            {
                if (part_carousel.SelectedIndex != 0)
                {
                    part_carousel.SelectedIndex--;
                }

                part_left.IsEnabled = part_carousel.SelectedIndex > 0;
                part_right.IsEnabled = true;
            };
            part_right.Click += (s, e) =>
            {
                if (part_carousel.SelectedIndex < part_carousel.ItemCount - 1)
                {
                    part_carousel.SelectedIndex++;
                }

                part_right.IsEnabled = part_carousel.SelectedIndex < part_carousel.ItemCount - 1;
                part_left.IsEnabled = true;
            };

            timer.Start();
        }
    }

    public class Indicator
    {
    }
}
