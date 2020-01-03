using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
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
using Microsoft.Toolkit.Uwp.UI.Animations;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UWP_BackgroundBlinkIssue.Agenda
{
    public sealed partial class PagerControl : UserControl
    {
        public PagerControl()
        {
            this.InitializeComponent();
        }

        #region private members

        private CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region DP Count

        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Count.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(int), typeof(PagerControl), new PropertyMetadata(0, CountPropertyChanged));

        private static void CountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pager = d as PagerControl;
            if (pager.Count != 0)
            {
                pager.UpdateUI();
            }
        }

        #endregion

        #region DP SelectedIndex

        /// Fires event, when the index changes. But only if it's valid index (i.e. not -1).
        /// </summary>
        public event EventHandler<int> SelectedIndexChanged;

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        /// Starts with -1, which means no page is selected.
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(PagerControl), new PropertyMetadata(-1, SelectedIndexPropertyChanged));

        private static void SelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pager = (PagerControl)d;

            if (pager.SelectedIndex != -1)
            {
                pager.SelectedIndexChanged?.Invoke(pager, (int)e.NewValue);
            }
        }

        #endregion

        #region DP Duration

        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Duration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(PagerControl), new PropertyMetadata(null));

        #endregion

        private void UpdateUI()
        {
            // Remove old progress bars.
            var itemsToRemove = new List<ProgressBar>();
            foreach (var item in LayoutRoot.Children)
            {
                if (item is ProgressBar)
                {
                    itemsToRemove.Add((ProgressBar)item);
                }
            }
            foreach (var item in itemsToRemove)
            {
                LayoutRoot.Children.Remove(item);
            }

            // Create new progress bars
            var defaultMarging = 5;
            var availableWidth = ActualWidth - defaultMarging * 2 * Count; // * 2 because marging from right and left.
            var progressBarWidth = availableWidth / Count;
            for (int i = 0; i < Count; i++)
            {
                var progressBar = new ProgressBar()
                {
                    Background = new SolidColorBrush(Colors.Silver),
                    Foreground = new SolidColorBrush(Colors.Red),
                    Width = progressBarWidth,
                    Height = 10,
                    Margin = new Thickness(5, 0, 5, 0),
                    Minimum = 0,
                    Maximum = 100
                };
                LayoutRoot.Children.Add(progressBar);
            }
        }

        private void SelectNextItem()
        {
            if (SelectedIndex < Count - 1 || SelectedIndex == -1)
            {
                SelectedIndex++;
            }
            else
            {
                SelectedIndex = 0;
            }
        }

        private async Task AnimateProgress()
        {
            // Find out which progress bar should be animated.
            var progressBars = LayoutRoot.Children.OfType<ProgressBar>().ToList();
            var bar = progressBars[SelectedIndex];

            // Create progress animation.
            var storyBoard = new Storyboard();
            var doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0;
            doubleAnimation.To = 100;
            doubleAnimation.Duration = new Duration(Duration);
            doubleAnimation.EnableDependentAnimation = true;

            storyBoard.Children.Add(doubleAnimation);
            Storyboard.SetTarget(doubleAnimation, bar);
            Storyboard.SetTargetProperty(doubleAnimation, "Value");

            await storyBoard.BeginAsync();
        }

        public async void StartShow()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            while (true)
            {
                SelectNextItem();
                await AnimateProgress();
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        public void StopShow()
        {
            if (_cancellationTokenSource is null)
            {
                return;
            }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }
}
