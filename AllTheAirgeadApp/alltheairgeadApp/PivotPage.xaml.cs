using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Data;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Input;
using Microsoft.WindowsAzure.MobileServices;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;
using alltheairgeadApp.Common;
using alltheairgeadApp.Services;
using alltheairgeadApp.DataObjects;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace alltheairgeadApp
{
    public sealed partial class PivotPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private List<Category> Categories;
        private static readonly IList<string> PriorityLevels = new ReadOnlyCollection<string>
            (new List<string> { "High", "Medium", "Low" });

        DateTime MinExpenseDate = DateTime.Now.Date;
        List<NameValueItem> items = new List<NameValueItem>();

        public class NameValueItem
        {
            public DateTime Date { get; set; }
            public int Value { get; set; }
            public int Id { get; set; }
        }

        private const double MaxChartXScale = 8.0f;
        private const double MinChartXScale = 0.1f;
        private const double ChartXScaleIntervalThresh = 6.0f;

        private const double MaxChartYScale = 4.0f;
        private const double MinChartYScale = 1.0f;

        private double ChartXScale = 1.0f;
        private double ChartYScale = 1.0f;

        public PivotPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            PriorityBox.DataContext = PriorityLevels;
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
        private void SecondPivot_Loaded(object sender, RoutedEventArgs e)
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

            await GetCategoryData();
            await UpdateExpenseChart();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
        
        public async Task GetCategoryData()
        {
            List<string> CategoryNames = new List<string>();
            IMobileServiceTable<Category> CategoryTable = App.alltheairgeadClient.GetTable<Category>();
            Categories = await CategoryTable.ToListAsync();
            foreach (Category i in Categories)
                CategoryNames.Add(i.id);
            CategoryBox.ItemsSource = CategoryNames;
        }

        public async Task UpdateExpenseChart()
        {
            if (ChartScroll.HorizontalOffset == ChartScroll.ScrollableWidth)
            {
                List<NameValueItem> TempItems = new List<NameValueItem>();
                List<Expense> Expenses;
                IMobileServiceTable<Expense> ExpenseTable = App.alltheairgeadClient.GetTable<Expense>();
                var TotalCount = (await ExpenseTable.Take(0).IncludeTotalCount().ToListAsync() as IQueryResultEnumerable<Expense>).TotalCount;
                if (items.Count >= TotalCount)
                    return;

                // Keep track of how many weeks have been checked in the query
                int weekssearched = 0;
                do
                {
                    MinExpenseDate = MinExpenseDate.AddDays(-7);
                    Expenses = await ExpenseTable.Where(a => (a.Date > MinExpenseDate) & (a.Date <= MinExpenseDate.AddDays(7))).OrderByDescending(a => a.Date).ToListAsync();
                    weekssearched++;
                } while (Expenses.Count == 0);
                if (Expenses.Count != 0)
                {
                    ExpenseChart.Width += 750*ChartXScale * weekssearched;
                    foreach (Expense i in Expenses)
                        TempItems.Add(new NameValueItem { Date = (i.Date + i.Time.TimeOfDay), Value = (int)i.Price, Id = i.Id });

                    items.AddRange(TempItems);
                    ((LineSeries)ExpenseChart.Series[0]).ItemsSource = items;
                    ((LineSeries)ExpenseChart.Series[0]).IndependentAxis =
                        new DateTimeAxis
                        {
                            Minimum = MinExpenseDate,
                            Maximum = DateTime.Now,
                            Orientation = AxisOrientation.X,
                            IntervalType = (ChartXScale > ChartXScaleIntervalThresh ? DateTimeIntervalType.Days : DateTimeIntervalType.Hours
                            )

                        };
                    ((LineSeries)ExpenseChart.Series[0]).DependentRangeAxis =
                        new LinearAxis
                        {
                            Orientation = AxisOrientation.Y,
                            ShowGridLines = true,
                            Location = AxisLocation.Right,
                            Minimum = 0,
                        };
                    ((LineSeries)ExpenseChart.Series[0]).Refresh();
                }
            }
        }

        private void CategoryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        
        public void DataPointTapped(Object sender, SelectionChangedEventArgs e)
        {
            //IMobileServiceTable<Expense> ExpenseTable = App.alltheairgeadClient.GetTable<Expense>();
            int Id = ((sender as LineSeries).SelectedItem as NameValueItem).Id;
            //Expense NewExpense = await ExpenseTable.LookupAsync(Id);
        }

        public async void ChartScroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e.IsIntermediate == false)
                await UpdateExpenseChart();

        }
        
        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable Submit to prevent multiple entries
            string message;
            SubmitButton.IsEnabled = false;
            ExpenseService ExpenseService = new ExpenseService();
            try
            {
                {
                    if (CategoryBox.SelectedIndex < 0)
                        throw new Exception("Category must be specified");
                    if (new DateTime((DateBox.Date.Date + TimeBox.Time).Ticks) > DateTime.Now)
                    {
                        DateBox.Date = DateTime.Now;
                        TimeBox.Time = DateTime.Now.TimeOfDay;
                        throw new Exception("Date and time must not be in the future");
                    }
                    if (String.IsNullOrWhiteSpace(PriceBox.Text) || Decimal.Parse(PriceBox.Text) < 0)
                        throw new Exception("Price must be positive");
                }

                Expense ExpenseData = new Expense(CategoryBox.SelectedValue.ToString(), Decimal.Parse(PriceBox.Text), DateBox.Date, new DateTime(TimeBox.Time.Ticks), MoreInfoBox.Text);
                if(await ExpenseService.AddExpense(ExpenseData))
                {
                    message = "Expense Saved";
                    CategoryBox.SelectedIndex = -1;
                    PriceBox.Text = "";
                    DateBox.Date = DateTime.Now;
                    TimeBox.Time = DateTime.Now.TimeOfDay;
                    PriorityBox.SelectedIndex = -1;
                    MoreInfoBox.Text = "";
                }
                else
                {
                    message = "Saving Failed";
                }
            }
            catch (Exception ex)
            {
                message = "Saving Failed\n"+ex.Message;
            }
            await new MessageDialog(message).ShowAsync();

            // Reenable when finished
            SubmitButton.IsEnabled = true;
        }
        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            CustomAccountService AccountService = new CustomAccountService();
            AccountService.Logout();

            MinExpenseDate = DateTime.Now.Date;
            items.Clear();
            ((LineSeries)ExpenseChart.Series[0]).Refresh();
            if (!Frame.Navigate(typeof(LoginPage)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
            MessageDialog Dialog = new MessageDialog("Logged out");
            await Dialog.ShowAsync();
        }

        
        // Called on ManipulationDelta event detected on X axis rectangle. Used to change axis scale easily
        private void ChartScroll_ManipulationXDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (Math.Abs(e.Delta.Scale - 1.0) >= 0.01)
            {
                if (ChartXScale <= MaxChartXScale && ChartXScale >= MinChartXScale)
                {
                    ChartXScale *= e.Delta.Scale;
                    ExpenseChart.Width = 750 * ChartXScale * ((DateTime.Now.Date - MinExpenseDate).Days / 7);

                    if (ChartXScale > ChartXScaleIntervalThresh)
                        ((ExpenseChart.Series[0] as LineSeries).IndependentAxis as DateTimeAxis).IntervalType = DateTimeIntervalType.Hours;
                    else
                        ((ExpenseChart.Series[0] as LineSeries).IndependentAxis as DateTimeAxis).IntervalType = DateTimeIntervalType.Days;
                }
                else if (ChartXScale < MinChartXScale)
                {
                    ChartXScale = MinChartXScale;
                }
                else if (ChartXScale > MaxChartXScale)
                {
                    ChartXScale = MaxChartXScale;
                }
            }

            double NewPosition = ChartScroll.HorizontalOffset * ChartXScale + e.Delta.Translation.X;
            ChartScroll.ChangeView(NewPosition, ChartScroll.VerticalOffset, ChartScroll.ZoomFactor, true);
        }

        private void ChartScroll_ManipulationYDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (Math.Abs(e.Delta.Scale - 1.0) >= 0.01)
            {
                if (ChartYScale <= MaxChartYScale && ChartYScale >= MinChartYScale)
                {
                    ChartYScale *= 1.0 + 0.5 * (e.Delta.Scale - 1.0);
                    // Make sure the scale doesn't jump outside the bounds
                    if (ChartYScale < MinChartYScale)
                        ChartYScale = MinChartYScale;
                    if (ChartYScale > MaxChartYScale)
                        ChartYScale = MaxChartYScale;

                    ExpenseChart.Height = PivotItem2.ActualHeight * ChartYScale;
                }
                else if (ChartYScale < MinChartYScale)
                {
                    ChartYScale = MinChartYScale;
                    ExpenseChart.Height = PivotItem2.ActualHeight * ChartYScale;
                }
                else if (ChartYScale > MaxChartYScale)
                {
                    ChartYScale = MaxChartYScale;
                    ExpenseChart.Height = PivotItem2.ActualHeight * ChartYScale;
                }
            }

            double NewPosition = ChartScroll.VerticalOffset * ChartYScale + e.Delta.Translation.Y;
            ChartScroll.ChangeView(ChartScroll.HorizontalOffset, NewPosition, ChartScroll.ZoomFactor, true);
        }
        
    }
}
