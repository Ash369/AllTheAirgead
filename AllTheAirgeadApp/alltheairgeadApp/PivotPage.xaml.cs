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
using Windows.Storage;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace alltheairgeadApp
{
    public sealed partial class PivotPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private List<Category> Categories;
        private List<Expense> Expenses = new List<Expense>();
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

            //(ExpenseChart.Series[0] as LineSeries).DataPointStyle.Setters.Add(new Setter(LineDataPoint.WidthProperty, 20));
            //(ExpenseChart.Series[0] as LineSeries).DataPointStyle.Setters.Add(new Setter(LineDataPoint.HeightProperty, 20));
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
            PriorityBox.DataContext = PriorityLevels;

            if (Categories == null)
                await GetCategoryData();
            if (items.Count == 0)
                await UpdateExpenseChart();
            else
                await RefreshExpenseChart();
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
            // Display the progress wheel when getting data
            ProgressRing LoginProgress = new ProgressRing();
            LoginProgress.HorizontalAlignment = HorizontalAlignment.Center;
            LoginProgress.VerticalAlignment = VerticalAlignment.Center;
            ContentRoot.Children.Add(LoginProgress);
            LoginProgress.IsActive = true;

            IMobileServiceTable<Category> CategoryTable = App.alltheairgeadClient.GetTable<Category>();
            Categories = await CategoryTable.ToListAsync();
            foreach (Category i in Categories)
            CategoryBox.ItemsSource = Categories;

            // Disable the progress wheel
            LoginProgress.IsActive = false;
            ContentRoot.Children.Remove(LoginProgress);
        }

        private async Task UpdateExpenseChart()
        {
            if (ChartScroll.HorizontalOffset == ChartScroll.ScrollableWidth)
            {
                // Display the progress wheel when getting data
                ProgressRing LoginProgress = new ProgressRing();
                LoginProgress.HorizontalAlignment = HorizontalAlignment.Center;
                LoginProgress.VerticalAlignment = VerticalAlignment.Center;
                ContentRoot.Children.Add(LoginProgress);
                LoginProgress.IsActive = true;

                List<NameValueItem> TempItems = new List<NameValueItem>();
                List<Expense> TempExpenses = new List<Expense>();
                IMobileServiceTable<Expense> ExpenseTable = App.alltheairgeadClient.GetTable<Expense>();
                var TotalCount = (await ExpenseTable.Take(0).IncludeTotalCount().ToListAsync() as IQueryResultEnumerable<Expense>).TotalCount;
                if (items.Count >= TotalCount)
                    return;

                // Keep track of how many weeks have been checked in the query
                int weekssearched = 0;
                do
                {
                    MinExpenseDate = MinExpenseDate.AddDays(-7);
                    TempExpenses = await ExpenseTable.Where(a => (a.Date > MinExpenseDate) & (a.Date <= MinExpenseDate.AddDays(7))).OrderByDescending(a => a.Date).ToListAsync();
                    weekssearched++;
                } while (TempExpenses.Count == 0);
                if (TempExpenses.Count != 0)
                {
                    ExpenseChart.Width += 750*ChartXScale * weekssearched;
                    foreach (Expense i in TempExpenses)
                        TempItems.Add(new NameValueItem { Date = (i.Date + i.Time.TimeOfDay), Value = (int)i.Price, Id = i.Id });

                    items.AddRange(TempItems);
                    Expenses.AddRange(TempExpenses);
                    ((LineSeries)ExpenseChart.Series[0]).ItemsSource = items;
                    ((LineSeries)ExpenseChart.Series[0]).IndependentAxis =
                        new DateTimeAxis
                        {
                            Minimum = MinExpenseDate,
                            Maximum = DateTime.Now,
                            Orientation = AxisOrientation.X,
                            IntervalType = (ChartXScale > ChartXScaleIntervalThresh ? DateTimeIntervalType.Hours : DateTimeIntervalType.Days
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
                // Disable the progress wheel
                LoginProgress.IsActive = false;
                ContentRoot.Children.Remove(LoginProgress);
            }
        }

        private async Task RefreshExpenseChart()
        {
            // Display the progress wheel when getting data
            ProgressRing LoginProgress = new ProgressRing();
            LoginProgress.HorizontalAlignment = HorizontalAlignment.Center;
            LoginProgress.VerticalAlignment = VerticalAlignment.Center;
            ContentRoot.Children.Add(LoginProgress);
            LoginProgress.IsActive = true;

            IMobileServiceTable<Expense> ExpenseTable = App.alltheairgeadClient.GetTable<Expense>();
            
            items.Clear();
            Expenses = await ExpenseTable.Where(a => (a.Date > MinExpenseDate) && (a.Date <= DateTime.Now)).OrderByDescending(a => a.Date).ToListAsync();
            foreach (Expense i in Expenses)
                items.Add(new NameValueItem { Date = (i.Date + i.Time.TimeOfDay), Value = (int)i.Price, Id = i.Id });

            ((LineSeries)ExpenseChart.Series[0]).Refresh();

            // Disable the progress wheel
            LoginProgress.IsActive = false;
            ContentRoot.Children.Remove(LoginProgress);
        }

        /// <summary>
        /// Refreshes the Expense chart and category data when the app bar button is pressed.
        /// </summary>
        private async void RefreshAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshExpenseChart();
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
        /// Changes the text displayed in the priority level box to the default for the category when 
        /// the category is chosen.
        /// </summary>
        private void CategoryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PriorityBox.SelectedIndex < 0)
                PriorityBox.PlaceholderText = PriorityLevels[(CategoryBox.SelectedItem as Category).DefaultPriority];
        }

        /// <summary>
        /// Sends the entered expense to the mobile service when the sibmit button is pressed
        /// </summary>
        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable Submit to prevent multiple entries
            SubmitButton.IsEnabled = false;

            PriorityBox.SelectedIndex = (CategoryBox.SelectedItem as Category).DefaultPriority;

            if (CategoryBox.SelectedIndex < 0)
                await new MessageDialog("Category must be specified").ShowAsync();
            else if (String.IsNullOrWhiteSpace(PriceBox.Text))
                await new MessageDialog("Price must be specified").ShowAsync();
            else
            {
                Expense ExpenseData = new Expense(CategoryBox.SelectedValue.ToString(), Decimal.Parse(PriceBox.Text), DateBox.Date, new DateTime(TimeBox.Time.Ticks), (byte?)(PriorityBox.SelectedIndex + 1), MoreInfoBox.Text);

                // Display the progress wheel when getting data
                ProgressRing LoginProgress = new ProgressRing();
                LoginProgress.HorizontalAlignment = HorizontalAlignment.Center;
                LoginProgress.VerticalAlignment = VerticalAlignment.Center;
                ContentRoot.Children.Add(LoginProgress);
                LoginProgress.IsActive = true;

                if (await ExpenseService.AddExpense(ExpenseData))
                {
                    // Clear input data
                    CategoryBox.SelectedIndex = -1;
                    PriceBox.Text = "";
                    DateBox.Date = DateTime.Now;
                    TimeBox.Time = DateTime.Now.TimeOfDay;
                    PriorityBox.SelectedIndex = -1;
                    PriorityBox.PlaceholderText = "Priority";
                    MoreInfoBox.Text = "";

                    // Refresh the chart with the new data
                    await RefreshExpenseChart();
                }

                // Disable the progress wheel
                LoginProgress.IsActive = false;
                ContentRoot.Children.Remove(LoginProgress);
            }
            // Reenable when finished
            SubmitButton.IsEnabled = true;
        }
        
        /// <summary>
        /// Opens a popup flyout displaying information about the relevent expense when a data point
        /// on the chart is tapped
        /// </summary>
        public void DataPointTapped(Object sender, SelectionChangedEventArgs e)
        {
            if ((sender as LineSeries).SelectedItem == null)
                return;

            NameValueItem SelectedItem = (sender as LineSeries).SelectedItem as NameValueItem;

            Expense NewExpense = Expenses.Find(a => a.Id == SelectedItem.Id);

            TextBlock ExpenseHeader = new TextBlock();
            ExpenseHeader.Text = NewExpense.Category;
            ExpenseHeader.FontSize = 32;
            ExpenseHeader.Margin = new Thickness(30, 20, 0, 20);

            TextBlock ExpenseTip = new TextBlock();
            ExpenseTip.Text = NewExpense.Display();
            ExpenseTip.FontSize = 20;
            ExpenseTip.Margin = new Thickness(40, 10, 0, 20);

            Button EditExpenseButton = new Button();
            EditExpenseButton.Content = "Edit Expense";
            EditExpenseButton.Click += new RoutedEventHandler(EditExpenseButton_Click);
            EditExpenseButton.Margin = new Thickness(40, 0, 0, 0);

            DataPointInfo.Children.Add(ExpenseHeader);
            DataPointInfo.Children.Add(ExpenseTip);
            DataPointInfo.Children.Add(EditExpenseButton);
            DataPointPop.ShowAt(sender as LineSeries);
        }

        /// <summary>
        /// Unselects the point and clears the flyout when it is closed
        /// </summary>
        private void DataPointPop_Closed(object sender, object e)
        {
            DataPointInfo.Children.Clear();
            (ExpenseChart.Series[0] as LineSeries).SelectedItem = null;
        }

        /// <summary>
        /// Navigates to the edit expense page when the edit expense button is pressed in the flyout
        /// </summary>
        private void EditExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            Expense NewExpense = Expenses.Find(a => a.Id == ((ExpenseChart.Series[0] as LineSeries).SelectedItem as NameValueItem).Id);
            // Write data to global variable to pass to new page.
            (App.Current as App).ExpenseEditData = NewExpense;

            if (!Frame.Navigate(typeof(ItemPage)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        /// <summary>
        /// Calls the UpdateExpenseChart() method when the scroll viewer reaches the edge of the chart
        /// </summary>
        private async void ChartScroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e.IsIntermediate == false)
                await UpdateExpenseChart();

        }

        /// <summary>
        /// Logs the user out when the logout button is pressed in the app bar menu
        /// </summary>
        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            CustomAccountService.Logout();

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
        
        /// <summary>
        /// Called on ManipulationDelta event detected on X axis rectangle. Used to change axis scale easily
        /// </summary>
        private void ChartScroll_ManipulationXDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (Math.Abs(e.Delta.Scale - 1.0) >= 0.01)
            {
                if (ChartXScale <= MaxChartXScale && ChartXScale >= MinChartXScale)
                {
                    ChartXScale *= 1.0 + 0.5 * (e.Delta.Scale - 1.0);
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

        /// <summary>
        /// Called on ManipulationDelta event detected on Y axis rectangle. Used to change axis scale easily
        /// </summary>
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
