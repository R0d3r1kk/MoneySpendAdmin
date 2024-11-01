using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MoneySpendAdmin.DAL;
using MoneySpendAdmin.DAL.Entities;
using MoneySpendAdmin.DAL.Repository;
using MoneySpendAdmin.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MoneySpendAdmin.Views
{
    public class CalendarItem
    {
        public int Day { get; private set; }
        public string Date { get; private set; }
        public string BgColor { get; private set; }
        public string badgeColor { get; private set; }
        public ObservableCollection<Movement> movements { get; set; }

        public CalendarItem(int day, string date, string bgColor)
        {
            this.Day = day;
            this.Date = date;
            BgColor = bgColor;
            movements = new ObservableCollection<Movement>();
        }

        public void setMovements(List<Movement> moves)
        {
            this.movements = new ObservableCollection<Movement>(moves);
        }
    }

    public sealed partial class CalendarPage : Page
    {

        private readonly Compositor compositor = CompositionTarget.GetCompositorForCurrentThread();
        private SpringVector3NaturalMotionAnimation springAnimation;

        ObservableCollection<CalendarItem> Items { get; set; }
        CalendarItem SelectionModel { get; set; }

        private MovementRepository moveRepo;

        ObservableCollection<Movement> moves;

        List<string> unhoverColors = new List<string>()
        {
            "#FFCBD5E0",
            "#FFFC8181",
            "#FFF6AD55",
            "#FF68D391",
            "#FF4FD1C5",
            "#FF63B3ED",
            "#FF76E4F7",
            "#FFB794F4",
            "#FFF687B3"
        };

        List<string> hoverColors = new List<string>()
        {
            "#FF1A202C",
            "#FF9B2C2C",
            "#FF9C4221",
            "#FF285E61",
            "#FF2C5282",
            "#FF0987A0",
            "#FF553C9A",
            "#FF97266D"
        };

        public CalendarPage()
        {
            this.InitializeComponent();

            Items = GetDates(DateTime.Now.Year, DateTime.Now.Month);
            calendarGrid.ItemsSource = Items;
            calendarDP.Date = DateTime.Now;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var global = e.Parameter as Global;
            if (global != null)
            {
                this.moveRepo = new MovementRepository(global.dataAccess);
                moves = new ObservableCollection<Movement>(await this.moveRepo.GetAllAsync());
                prepareDates();
                App.MAIN.NotifyUser($"{moves.Count} Transacciones", InfoBarSeverity.Informational);
            }
            base.OnNavigatedTo(e);
        }

        public ObservableCollection<CalendarItem> GetDates(int year, int month)
        {
            var random = new Random();
            var idx = random.Next(0, hoverColors.Count - 1);

            return new ObservableCollection<CalendarItem>(Enumerable.Range(1, DateTime.DaysInMonth(year, month))  // Days: 1, 2 ... 31 etc.
                             .Select(day => new CalendarItem(day, new DateTime(year, month, day).ToLongDateString().ToUpperInvariant(), unhoverColors[idx])) // Map each day to a date
                             .ToList()); // Load dates into a list
        }

        public void prepareDates()
        {
            foreach(var m in moves)
            {
                var item = Items.FirstOrDefault(i => DateTime.Parse(i.Date) == DateTime.Parse(m.fecha));
                if(item != null)
                {
                    item.movements.Add(m);
                }
            }
        }

        public SolidColorBrush getBrushFromHex(string hex)
        {
            byte A = Convert.ToByte(hex.Substring(1, 2), 16);
            byte R = Convert.ToByte(hex.Substring(3, 2), 16);
            byte G = Convert.ToByte(hex.Substring(5, 2), 16);
            byte B = Convert.ToByte(hex.Substring(7, 2), 16);
            return new SolidColorBrush(Color.FromArgb(A, R, G, B));
        }

        private void CreateOrUpdateSpringAnimation(float finalValue)
        {
            if (springAnimation == null)
            {
                springAnimation = compositor.CreateSpringVector3Animation();
                springAnimation.Target = "Scale";
            }

            springAnimation.FinalValue = new Vector3(finalValue);
        }

        #region Calendar
        private void calendarGrid_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            var selectionIndex = args.Index;
            var item = ElementCompositionPreview.GetElementVisual(args.Element);

        }

        private void calendarGrid_ElementIndexChanged(ItemsRepeater sender, ItemsRepeaterElementIndexChangedEventArgs args)
        {
            var selectionIndex = args.NewIndex;
        }

        private void calendarGrid_ElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
        {

        }

        private void RelativePanel_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var target = sender as RelativePanel;
            var clr = target.Background as SolidColorBrush;
            var strHex = clr.Color.ToString();
            var colorIdx = unhoverColors.FindIndex(c => c == strHex);

            if (colorIdx > -1)
                target.Background = getBrushFromHex(hoverColors[colorIdx]);

            //CreateOrUpdateSpringAnimation(1.1f);
            //target.CenterPoint = new Vector3((float)((sender as Button).ActualWidth / 2.0), (float)((sender as Button).ActualHeight / 2.0), 1f);
            //((UIElement)sender).StartAnimation(springAnimation);
        }

        private void RelativePanel_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var target = sender as RelativePanel;
            var clr = target.Background as SolidColorBrush;
            var strHex = clr.Color.ToString();
            var colorIdx = hoverColors.FindIndex(c => c == strHex);

            if (colorIdx > -1)
                target.Background = getBrushFromHex(unhoverColors[colorIdx]);

            //CreateOrUpdateSpringAnimation(1.0f);
            //target.CenterPoint = new Vector3((float)((sender as Button).ActualWidth / 2.0), (float)((sender as Button).ActualHeight / 2.0), 1f);
            //((UIElement)sender).StartAnimation(springAnimation);
        }

        private void RelativePanel_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var target = sender as RelativePanel;
        }

        private void RelativePanel_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var item = sender as FrameworkElement;
            // When the clicked item has been recieved, bring it to the middle of the viewport.
            item.StartBringIntoView(new BringIntoViewOptions()
            {
                VerticalAlignmentRatio = 0.5,
                AnimationDesired = true,
            });

        }

        private void RelativePanel_GotFocus(object sender, RoutedEventArgs e)
        {
            var item = sender as FrameworkElement;
            // When the clicked item has been recieved, bring it to the middle of the viewport.
            item.StartBringIntoView(new BringIntoViewOptions()
            {
                VerticalAlignmentRatio = 0.5,
                AnimationDesired = true,
            });

        }
        #endregion

        private void calendarDP_SelectedDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs args)
        {
            if(calendarDP.SelectedDate != null)
            {
                Items = GetDates(args.NewDate.Value.Year, args.NewDate.Value.Month);
                calendarGrid.ItemsSource = Items;
                prepareDates();
            }
        }
    }
}
