using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UWP_BackgroundBlinkIssue.Agenda
{
    public sealed partial class AgendaControl : UserControl
    {
        public AgendaControl()
        {
            this.InitializeComponent();

            Pager.SelectedIndexChanged += Pager_SelectedIndexChanged;
        }

        #region private members

        private readonly TimeSpan _duration = TimeSpan.FromMilliseconds(250);
        private List<List<ViewModelBase>> _pages;
        private List<UIElement> _oldItems;
        private Storyboard _transitionStoryboard = new Storyboard();

        #endregion

        #region DP ItemsSource

        public IEnumerable<ViewModelBase> ItemsSource
        {
            get
            {
                return (IEnumerable<ViewModelBase>)GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
                DependencyProperty.Register("ItemsSource",
                typeof(IEnumerable<ViewModelBase>),
                typeof(AgendaControl),
                new PropertyMetadata(null, ItemsSourcePropertyChanged));

        private static void ItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var agandaControl = (AgendaControl)d;
            var oldCollection = e.OldValue as INotifyCollectionChanged;
            var newCollection = e.NewValue as INotifyCollectionChanged;

            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= agandaControl.OnItemsCollectionChanged;
            }

            if (newCollection != null)
            {
                newCollection.CollectionChanged += agandaControl.OnItemsCollectionChanged;
            }

            agandaControl.UpdateView();
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateView();
        }

        #endregion

        #region DP Switch time

        public int SwitchTime
        {
            get { return (int)GetValue(SwitchTimeProperty); }
            set { SetValue(SwitchTimeProperty, value); }
        }

        public static readonly DependencyProperty SwitchTimeProperty =
                DependencyProperty.Register("SwitchTime",
                typeof(int),
                typeof(AgendaControl), new PropertyMetadata(0));

        #endregion

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ItemsHost.Children.Clear();
            UpdateView();
        }

        private void UpdateView()
        {
            // Skip if Layout isn't rendered yet
            if (ItemsSource == null || LayoutRoot.ActualWidth <= 0 || LayoutRoot.ActualHeight <= 0)
            {
                return;
            }

            // Calculate how many items fit on one page and how many pages are needed for all items
            var maxHeight = LayoutRoot.ActualHeight;

            // Split items into pages
            int pageId = 0;
            double itemsPerPageHeight = 0;
            _pages = new List<List<ViewModelBase>>();
            _pages.Add(new List<ViewModelBase>());
            foreach (var item in ItemsSource)
            {
                var itemHeight = GetItemHeight(item);

                // We can only move an item to the next page if it's not the first item.
                // If it's the first element on the page (i.e. itemsPerPageHeight == 0),
                // we must add it to the page regardless of how much space an item will take.
                if (itemsPerPageHeight != 0)
                {
                    // Time to create new page.
                    if (itemsPerPageHeight + itemHeight > maxHeight)
                    {
                        pageId++;
                        itemsPerPageHeight = 0;
                        _pages.Add(new List<ViewModelBase>());
                    }
                }

                _pages[pageId].Add(item);
                itemsPerPageHeight += itemHeight;
            }

            // Configure pager
            Pager.Count = _pages.Count;
            Pager.SelectedIndex = -1;
            Pager.Duration = TimeSpan.FromSeconds(SwitchTime);
            Pager.StopShow();
            Pager.StartShow();
        }

        private void Pager_SelectedIndexChanged(object sender, int e)
        {
            LoadItems(_pages[e]);
        }

        private void LoadItems(List<ViewModelBase> items)
        {
            StopAnimation();
            // #1 Hide old items
            if (ItemsHost.Children.Count > 0)
            {
                _oldItems = ItemsHost.Children.ToList();
                for (int i = 0; i < ItemsHost.Children.Count; i++)
                {
                    var oldItem = ItemsHost.Children[i] as FrameworkElement;
                    _transitionStoryboard.Children.Add(RotateItem(oldItem, i * (_duration / 2), 0, 90, EasingMode.EaseIn));
                }
            }

            double topOffset = 0;
            // #2 Show new items
            for (int i = 0; i < items.Count; i++)
            {
                var ai = new AgendaItemControl();
                ai.DataContext = items[i];
                ai.Width = ItemsHost.ActualWidth;
                ai.Projection = new PlaneProjection()
                {
                    RotationX = -90
                };
                Canvas.SetTop(ai, topOffset);
                double height = GetItemHeight(items[i]);

                // Item is too big, cut it off.
                if (height > LayoutRoot.ActualHeight)
                {
                    ai.Height = LayoutRoot.ActualHeight;
                }

                ItemsHost.Children.Add(ai);

                // Calculate next item offset.
                _transitionStoryboard.Children.Add(RotateItem(ai, (i + 1) * (_duration / 2), -90, 0, EasingMode.EaseIn));
                topOffset += height;
            }

            _transitionStoryboard.Begin();
        }

        private void StopAnimation()
        {
            _transitionStoryboard.Stop();
            _transitionStoryboard.Children.Clear();
            ClearOldItems();
        }

        private void ClearOldItems()
        {
            if (_oldItems == null || _oldItems.Count == 0)
            {
                return;
            }

            foreach (var item in _oldItems)
            {
                ItemsHost.Children.Remove(item);
            }
            _oldItems = null;
        }

        private DoubleAnimationUsingKeyFrames RotateItem(FrameworkElement fe, TimeSpan offset, double from, double to, EasingMode mode)
        {
            var da = new DoubleAnimationUsingKeyFrames();
            da.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(offset),
                Value = from,
            });
            da.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(offset + _duration),
                Value = to,
                EasingFunction = new PowerEase()
                {
                    EasingMode = mode
                }
            });

            Storyboard.SetTarget(da, fe);
            Storyboard.SetTargetProperty(da, "(UIElement.Projection).(PlaneProjection.RotationX)");

            return da;
        }

        private double GetItemHeight(ViewModelBase viewModel)
        {
            var ai = new AgendaItemControl();
            ai.Width = ItemsHost.ActualWidth;
            ai.DataContext = viewModel;
            ai.Measure(new Size(int.MaxValue, int.MaxValue));
            return ai.DesiredSize.Height;
        }
    }
}
