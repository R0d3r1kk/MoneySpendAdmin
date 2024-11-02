using Microsoft.UI;
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
using System.Data;
using System.Linq;
using System.Reflection;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MoneySpendAdmin.Views
{
    public sealed partial class HomePage : Page
    {
        public ObservableCollection<Balance> balanceList { get; set; }
        private BalanceRepository balRepo { get; set; }

        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var global = e.Parameter as Global;
            if (global != null)
            {
                this.balRepo = new BalanceRepository(global.dataAccess);
                this.balanceList = new ObservableCollection<Balance>(await this.balRepo.GetAllAsync());

            }
            base.OnNavigatedTo(e);
        }
    }
}
