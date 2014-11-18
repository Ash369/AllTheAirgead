using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Microsoft.WindowsAzure.MobileServices;
using alltheairgeadApp.Common;
using alltheairgeadApp.Services;
using alltheairgeadApp.DataObjects;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace alltheairgeadApp
{
    public sealed partial class PivotPage : Page
    {
        private const string FirstGroupName = "FirstGroup";
        private const string SecondGroupName = "SecondGroup";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public PivotPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
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
        /// session. The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            /*
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroup = await SampleDataSource.GetGroupAsync("Group-1");
            this.DefaultViewModel[FirstGroupName] = sampleDataGroup;
             */
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        /// <summary>
        /// Adds an item to the list when the app bar button is clicked.
        /// </summary>
        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {/*
            string groupName = this.pivot.SelectedIndex == 0 ? FirstGroupName : SecondGroupName;
            var group = this.DefaultViewModel[groupName] as SampleDataGroup;
            var nextItemId = group.Items.Count + 1;
            var newItem = new SampleDataItem(
                string.Format(CultureInfo.InvariantCulture, "Group-{0}-Item-{1}", this.pivot.SelectedIndex + 1, nextItemId),
                string.Format(CultureInfo.CurrentCulture, this.resourceLoader.GetString("NewItemTitle"), nextItemId),
                string.Empty,
                string.Empty,
                this.resourceLoader.GetString("NewItemDescription"),
                string.Empty);

            group.Items.Add(newItem);

            // Scroll the new item into view.
            var container = this.pivot.ContainerFromIndex(this.pivot.SelectedIndex) as ContentControl;
            var listView = container.ContentTemplateRoot as ListView;
            listView.ScrollIntoView(newItem, ScrollIntoViewAlignment.Leading);
        */
        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {/*
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
            if (!Frame.Navigate(typeof(ItemPage), itemId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
          */
        }

        /// <summary>
        /// Loads the content for the second pivot item when it is scrolled into view.
        /// </summary>
        private async void SecondPivot_Loaded(object sender, RoutedEventArgs e)
        {/*
            var sampleDataGroup = await SampleDataSource.GetGroupAsync("Group-2");
            this.DefaultViewModel[SecondGroupName] = sampleDataGroup;
          */
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
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            await UpdateExpenseChart();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        public class NameValueItem
        {
            public DateTime Date { get; set; }
            public int Value { get; set; }
            public int Id { get; set; }
        }

        DateTime MinDisplayedDate = DateTime.Now.Date;
        List<NameValueItem> items = new List<NameValueItem>();
        public async Task UpdateExpenseChart()
        {
            if (ChartScroll.HorizontalOffset == ChartScroll.ScrollableWidth)
            {
                List<NameValueItem> TempItems = new List<NameValueItem>();
                List<Expense> Expenses;
                IMobileServiceTable<Expense> ExpenseTable = App.alltheairgeadClient.GetTable<Expense>();
                int its = 0;
                do
                {
                    MinDisplayedDate = MinDisplayedDate.AddDays(-7);
                    Expenses = await ExpenseTable.Where(a => (a.Date > MinDisplayedDate) & (a.Date <= MinDisplayedDate.AddDays(7))).OrderByDescending(a => a.Date).ToListAsync();
                    its++;
                } while (Expenses.Count == 0 || its >= 5);
                if (Expenses.Count != 0)
                {
                    ExpenseChart.Width += 1000*its;
                    foreach (Expense i in Expenses)
                        TempItems.Add(new NameValueItem { Date = (i.Date + i.Time.TimeOfDay), Value = (int)i.Price, Id = i.Id });

                    items.AddRange(TempItems);
                    ((LineSeries)ExpenseChart.Series[0]).ItemsSource = items;
                    ((LineSeries)ExpenseChart.Series[0]).IndependentAxis =
                        new DateTimeAxis
                        {
                            Minimum = MinDisplayedDate,
                            Maximum = DateTime.Now,
                            Orientation = AxisOrientation.X,
                            Interval = 1 / ChartScroll.ZoomFactor,
                            IntervalType = DateTimeIntervalType.Days,

                        };
                    ((LineSeries)ExpenseChart.Series[0]).DependentRangeAxis =
                        new LinearAxis
                        {
                            Orientation = AxisOrientation.Y,
                            ShowGridLines = true,
                            Location = AxisLocation.Right,
                        };
                }
                ((LineSeries)ExpenseChart.Series[0]).Refresh();
                UpdateExpenseAxis();
            }
            else
                (((LineSeries)ExpenseChart.Series[0]).IndependentAxis as DateTimeAxis).Interval = 1 / ChartScroll.ZoomFactor;
        }

        public void UpdateExpenseAxis()
        {/*
            List<NameValueItem> TempItems = items;
            TempItems.Sort(delegate(NameValueItem a, NameValueItem b)
            {
                if (a.Value > b.Value)
                    return 1;
                else if (a.Value < b.Value)
                    return -1;
                else
                    return 0;
            });
            ((LineSeries)FixedAxisChart.Series[0]).DependentRangeAxis =
                new LinearAxis
                {
                    Minimum = TempItems[0].Value,
                    Maximum = TempItems[TempItems.Count - 1].Value,
                    Orientation = AxisOrientation.Y,
                    Interval = (TempItems[TempItems.Count - 1].Value - TempItems[0].Value) / 5,
                };*/
        }

        public async void DataPointTapped(Object sender, SelectionChangedEventArgs e)
        {
            IMobileServiceTable<Expense> ExpenseTable = App.alltheairgeadClient.GetTable<Expense>();
            int Id = ((sender as LineSeries).SelectedItem as NameValueItem).Id;
            Expense NewExpense = await ExpenseTable.LookupAsync(Id);
        }

        public async void ChartScroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e.IsIntermediate == false)
                await UpdateExpenseChart();

        }

        #endregion

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            ExpenseService ExpenseService = new ExpenseService();
            Expense ExpenseData = new Expense((Category.SelectedValue as ComboBoxItem).Content.ToString(), Decimal.Parse(Price.Text), Date.Date, new DateTime(Time.Time.Ticks), MoreInfo.Text);
            if(await ExpenseService.AddExpense(ExpenseData))
            {
                MessageDialog Dialog = new MessageDialog("Expense saved");
                await Dialog.ShowAsync();
                Category.SelectedIndex = -1;
                Price.Text = "";
                Date.Date = DateTime.Now;
                Time.Time = DateTime.Now.TimeOfDay;
                MoreInfo.Text = "";
            }
            else
            {
                MessageDialog Dialog = new MessageDialog("Saving Failed");
                await Dialog.ShowAsync();
            }


        }
    }
}
