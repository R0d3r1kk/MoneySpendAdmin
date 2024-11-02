using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
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
using System.Threading.Tasks;
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
        ObservableCollection<CalendarItem> Items { get; set; }

        CalendarItem SelectionModel { get; set; }

        private MovementRepository moveRepo { get; set; }

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

            init();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var global = e.Parameter as Global;
            if (global != null)
            {
                App.MAIN.loading(true);
                this.moveRepo = new MovementRepository(global.dataAccess);
                await prepareDates();
                App.MAIN.loading(false);
                App.MAIN.NotifyUser($"{moves.Count} Transacciones", InfoBarSeverity.Informational);
            }
            init();
            
            base.OnNavigatedTo(e);
        }

        public void init()
        {
            calendarDP.SelectedDate = DateTime.Now;
            calendarTypeSelector.SelectedItem = calendarTypeSelector.Items.FirstOrDefault();
            Items = GetDates(calendarDP.SelectedDate.Value.Year, calendarDP.SelectedDate.Value.Month, calendarTypeSelector.SelectedItem.Text);
            calendarGrid.ItemsSource = Items;
            CmbWeek.Visibility = Visibility.Collapsed;
        }

        public ObservableCollection<CalendarItem> GetDates(int year, int month, string type, int multi = 1)
        {
            var random = new Random();
            var idx = random.Next(0, hoverColors.Count - 1);
            var days = 0; 
            switch (type)
            {
                case "Mes":
                    days = DateTime.DaysInMonth(year, month);
                    CmbWeek.Visibility = Visibility.Collapsed;
                    break;
                case "Semana":
                    days = 7 * multi;
                    CmbWeek.Visibility = Visibility.Visible;
                    break;
            }

            return new ObservableCollection<CalendarItem>(Enumerable.Range(1, days)  // Days: 1, 2 ... 31 etc.
                             .Select(day => new CalendarItem(day, new DateTime(year, month, day).ToLongDateString().ToUpperInvariant(), unhoverColors[idx])) // Map each day to a date
                             .ToList()); // Load dates into a list
        }


        public async Task prepareDates()
        {
            moves = new ObservableCollection<Movement>(await this.moveRepo.GetAllAsync());
            foreach (var m in moves)
            {
                var item = Items.FirstOrDefault(i => DateTime.Parse(i.Date) == formatDate(m.fecha));
                if (item != null)
                {
                    item.movements.Add(m);
                }
            }
        }

        public DateTime formatDate(string fecha)
        {
            var split = fecha.Split(' ');
            var meses = new List<string>() { "ENE", "FEB", "AMR", "ABR", "MAY", "JUN", "JUL", "AGO", "SEP", "OCT", "NOV", "DIC" };

            var exist = meses.IndexOf(split[1]);
            if (exist > -1)
            {
                split[1] = $"{exist + 1}";
            }

            return DateTime.Parse($"{split[0]}/{split[1]}/{split[2]}");
        }

        public SolidColorBrush getBrushFromHex(string hex)
        {
            byte A = Convert.ToByte(hex.Substring(1, 2), 16);
            byte R = Convert.ToByte(hex.Substring(3, 2), 16);
            byte G = Convert.ToByte(hex.Substring(5, 2), 16);
            byte B = Convert.ToByte(hex.Substring(7, 2), 16);
            return new SolidColorBrush(Color.FromArgb(A, R, G, B));
        }

        public async Task updateState(int year, int month, string text, int multi = 1)
        {
            Items = GetDates(year, month, text, multi);
            calendarGrid.ItemsSource = Items;
            await prepareDates();
        }

        public bool isMonthSelected()
        {
            return calendarTypeSelector.SelectedItem.Text == "Mes";
        }
        #region Calendar
        private void calendarGrid_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            var target = args.Element as RelativePanel;
            var clr = target.Background as SolidColorBrush;
            var strHex = clr.Color.ToString();
            var colorIdx = unhoverColors.FindIndex(c => c == strHex);
            if (colorIdx > -1) 
            {
                var lblMoves = target.Children.FirstOrDefault(c => (c as TextBlock).Name == "lblMoves") as TextBlock;
                if (lblMoves != null)
                {
                    lblMoves.Foreground = getBrushFromHex(hoverColors[colorIdx]);
                }
            }
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
            {
                target.Background = getBrushFromHex(hoverColors[colorIdx]);

                var lblMoves = target.Children.FirstOrDefault(c => (c as TextBlock).Name == "lblMoves") as TextBlock;
                if (lblMoves != null)
                {
                    lblMoves.Foreground = getBrushFromHex(unhoverColors[colorIdx]);
                }
            }
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
            {
                target.Background = getBrushFromHex(unhoverColors[colorIdx]);

                var lblMoves = target.Children.FirstOrDefault(c => (c as TextBlock).Name == "lblMoves") as TextBlock;
                if (lblMoves != null)
                {
                    lblMoves.Foreground = getBrushFromHex(hoverColors[colorIdx]);
                }
            }
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

        private async void calendarDP_SelectedDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs args)
        {
            var target = CmbWeek.SelectedItem as ComboBoxItem;
            if (calendarDP.SelectedDate != null)
            {
                await updateState(args.NewDate.Value.Year, args.NewDate.Value.Month, calendarTypeSelector.SelectedItem.Text,  isMonthSelected() ? 1 : int.Parse(target.Content.ToString()));
            }
        }

        private async void calendarTypeSelector_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            if (this.moveRepo != null)
            {
                await updateState(calendarDP.SelectedDate.Value.Year, calendarDP.SelectedDate.Value.Month, selectedItem.Text);
            }
        }

        private async void CmbWeek_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var target = CmbWeek.SelectedItem as ComboBoxItem;
            if (CmbWeek.SelectedItem != null && calendarDP != null && calendarTypeSelector.SelectedItem != null)
            {
                await updateState(calendarDP.SelectedDate.Value.Year, calendarDP.SelectedDate.Value.Month, calendarTypeSelector.SelectedItem.Text, int.Parse(target.Content.ToString()));
            }
        }
    }
}
