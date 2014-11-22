using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.WindowsAzure.MobileServices;
using alltheairgeadApp.Common;
using alltheairgeadApp.DataObjects;
using alltheairgeadApp.Services;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace alltheairgeadApp
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ItemPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        Expense Expense;
        private List<Category> Categories;
        private static readonly IList<string> PriorityLevels = new ReadOnlyCollection<string>
            (new List<string> { "High", "Medium", "Low" });

        public ItemPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        } 

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // Get the expense to show
            Expense = (App.Current as App).ExpenseEditData;
            if (Expense == null)
            {
                await new MessageDialog("Expense Data not found").ShowAsync();
                navigationHelper.GoBack();
            }

            PriorityBox.DataContext = PriorityLevels;

            // Update the category information
            await GetCategoryData();

            CategoryBox.SelectedIndex = Categories.FindIndex(a => a.id == Expense.Category);
            PriceBox.Text = Expense.Price.ToString();
            PriorityBox.SelectedIndex = (int)Expense.Priority - 1;
            DateBox.Date = Expense.Date.Date;
            TimeBox.Time = Expense.Time.TimeOfDay;
            MoreInfoBox.Text = Expense.MoreInfo;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async Task GetCategoryData()
        {
            List<string> CategoryNames = new List<string>();
            IMobileServiceTable<Category> CategoryTable = App.alltheairgeadClient.GetTable<Category>();
            Categories = await CategoryTable.ToListAsync();
            foreach (Category i in Categories)
                CategoryNames.Add(i.id);
            CategoryBox.ItemsSource = CategoryNames;
        }

        private void DeleteButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ConfirmDeletePop.ShowAt(DeleteButton);
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable Submit to prevent multiple entries
            UpdateButton.IsEnabled = false;

            if (CategoryBox.SelectedIndex < 0)
                throw new Exception("Category must be specified");
            else if (String.IsNullOrWhiteSpace(PriceBox.Text))
                throw new Exception("Price must be specified");
            else
            {
                Expense NewExpense = new Expense(CategoryBox.SelectedValue.ToString(), Decimal.Parse(PriceBox.Text), DateBox.Date, new DateTime(TimeBox.Time.Ticks), (byte?)(PriorityBox.SelectedIndex + 1), MoreInfoBox.Text);
                if (await ExpenseService.UpdateExpense(Expense, NewExpense))
                {
                    // Reenable the update button
                    UpdateButton.IsEnabled = true;
                    // Return to the previous page if successful
                    navigationHelper.GoBack();
                }
            }

            // Reenable when finished
            UpdateButton.IsEnabled = true;
        }

        private async void ConfirmDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable the button to prevent multiple presses
            ConfirmDeleteButton.IsEnabled = false;
            if (await ExpenseService.DeleteExpense(Expense))
            {
                navigationHelper.GoBack();
            }
            // Reenable the button after finished
            ConfirmDeleteButton.IsEnabled = true;
        }
    }
}