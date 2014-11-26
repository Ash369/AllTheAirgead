using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;
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
    /// <summary>
    /// Main page class
    /// </summary>
    public sealed partial class PivotPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        // List of categories populated by database
        private List<Category> Categories;
        // List of user expenses populated by the database
        private List<Expense> Expenses = new List<Expense>();
        // Collection to transform the priority level number to user readable strings
        private static readonly IList<string> PriorityLevels = new ReadOnlyCollection<string>
            (new List<string> { "High", "Medium", "Low" });

        // Holds the minimum date displayed on the chart
        DateTime MinExpenseDate = DateTime.Now.Date;

        // Data object for expense chart item
        public class DateValueItem
        {
            public DateTime Date { get; set; }
            public int Value { get; set; }
            public int Id { get; set; }
        }

        // Constants for chart display
        private const double MaxChartXScale = 4.0f;
        private const double MinChartXScale = 0.1f;
        private const double ChartXScaleIntervalThresh = 3.0f;
        private const double MaxChartYScale = 4.0f;
        private const double MinChartYScale = 1.0f;

        // Current X and Y chart scale
        private double ChartXScale = 1.0f;
        private double ChartYScale = 1.0f;

        // Constructor to iniatize components and setup event handles
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
            PriorityBox.DataContext = PriorityLevels;

            // Get the categories from the database if not already received
            if (Categories == null)
                await GetCategoryData();
            // Get the first week back of expenses for the chart if none have been found
            if (Expenses.Count == 0)
                await UpdateExpenseChart();
            // Otherwise refresh the items
            else
                await RefreshExpenseChart();
            // Set the expense chart datapoint style
            (ExpenseChart.Series[0] as LineSeries).DataPointStyle = StyleService.LargeDataPoint(StyleService.Colours[0]);
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
        
        /// <summary>
        /// Gets the categories from the database and fills the list. Also builds chart settings with all the categories
        /// </summary>
        /// <returns></returns>
        private async Task GetCategoryData()
        {
            // Display the progress wheel when getting data
            ProgressRing LoginProgress = new ProgressRing();
            LoginProgress.HorizontalAlignment = HorizontalAlignment.Center;
            LoginProgress.VerticalAlignment = VerticalAlignment.Center;
            ContentRoot.Children.Add(LoginProgress);
            LoginProgress.IsActive = true;

            // Populate category list from database
            IMobileServiceTable<Category> CategoryTable = App.alltheairgeadClient.GetTable<Category>();
            Categories = await CategoryTable.ToListAsync();
            // Set the category box source to use the category list
            CategoryBox.ItemsSource = Categories;

            // Clear the Chart settings list
            ChartSettings.Items.Clear();

            // Build a new stack panel in the setting menu to hold the name and colour box to show for the line data
            StackPanel TotalStack = new StackPanel();
            TotalStack.Orientation = Orientation.Horizontal;

            // Build a text block and rectangle coloured for the all expenses line. Line is permanent so not using toggle switch here
            TextBlock TotalText = new TextBlock();
            TotalText.Text = "All Expenses";
            TotalText.FontSize = 24;
            TotalText.Width = 300;
            TotalText.Margin = new Thickness(20, 5, 20, 5);

            Rectangle TotalRectangle = new Rectangle();
            TotalRectangle.Width = 50;
            TotalRectangle.Height = 10;
            TotalRectangle.HorizontalAlignment = HorizontalAlignment.Right;
            TotalRectangle.Margin = new Thickness(20, 5, 20, 5);
            TotalRectangle.Fill = new SolidColorBrush(Colors.Blue);

            // Add the text block and rectangle to the stack panel
            TotalStack.Children.Add(TotalText);
            TotalStack.Children.Add(TotalRectangle);
            // Add the stack panel to the settings list
            ChartSettings.Items.Add(TotalStack);

            // For each of the categories, add the name and a box to toggle the line on or off
            foreach (Category i in Categories)
            {
                // Build the stack panel for holding the two objects
                StackPanel CategoryStack = new StackPanel();
                CategoryStack.Orientation = Orientation.Horizontal;

                // Build the toggle switch allowing the category to be turned on or off
                ToggleSwitch CategoryToggle = new ToggleSwitch();
                CategoryToggle.OnContent = i.id;
                CategoryToggle.OffContent = i.id;
                CategoryToggle.Width = 300;
                CategoryToggle.Margin = new Thickness(20, 5, 20, 5);
                // Set the event handler for a toggle
                CategoryToggle.Toggled += new RoutedEventHandler(CategoryToggle_Toggled);

                // Build the rectangle for didplaying the line colour
                Rectangle CategoryRectangle = new Rectangle();
                CategoryRectangle.Width = 50;
                CategoryRectangle.Height = 10;
                CategoryRectangle.HorizontalAlignment = HorizontalAlignment.Right;
                CategoryRectangle.Margin = new Thickness(20, 5, 20, 5);
                CategoryRectangle.Fill = new SolidColorBrush(Colors.Gray);

                // Add the items to the stack and add the stack to the list
                CategoryStack.Children.Add(CategoryToggle);
                CategoryStack.Children.Add(CategoryRectangle);
                ChartSettings.Items.Add(CategoryStack);
            }

            // Disable the progress wheel
            LoginProgress.IsActive = false;
            ContentRoot.Children.Remove(LoginProgress);
        }

        /// <summary>
        /// Gets at least a week back for displaying on the chart. Keeps searching back week by week if there is still data to find
        /// </summary>
        /// <returns></returns>
        private async Task UpdateExpenseChart()
        {
            // Update if the chart is scrolled all the way back
            if (ChartScroll.HorizontalOffset == ChartScroll.ScrollableWidth)
            {
                // Display the progress wheel when getting data
                ProgressRing LoginProgress = new ProgressRing();
                LoginProgress.HorizontalAlignment = HorizontalAlignment.Center;
                LoginProgress.VerticalAlignment = VerticalAlignment.Center;
                ContentRoot.Children.Add(LoginProgress);
                LoginProgress.IsActive = true;

                List<Expense> TempExpenses = new List<Expense>();

                // Query the database to see how many expenses there are in the database
                IMobileServiceTable<Expense> ExpenseTable = App.alltheairgeadClient.GetTable<Expense>();
                var TotalCount = (await ExpenseTable.Take(0).IncludeTotalCount().ToListAsync() as IQueryResultEnumerable<Expense>).TotalCount;
                // If all the expense  have been found, don't searchfor more
                if (Expenses.Count >= TotalCount)
                {
                    // Disable the progress wheel and exit
                    LoginProgress.IsActive = false;
                    ContentRoot.Children.Remove(LoginProgress);
                    return;
                }

                // Keep track of how many weeks have been checked in this update
                int weekssearched = 0;
                do
                {
                    // Query for the next week back of expenses
                    MinExpenseDate = MinExpenseDate.AddDays(-7);
                    TempExpenses = await ExpenseTable.Where(a => (a.Date > MinExpenseDate) & (a.Date <= MinExpenseDate.AddDays(7))).OrderByDescending(a => a.Date).ToListAsync();
                    weekssearched++;
                } while (TempExpenses.Count == 0);

                // If some are found, add them to the list of expenses
                if (TempExpenses.Count != 0)
                {
                    // Add the found expenses to the list of total expenses
                    Expenses.AddRange(TempExpenses);

                    // Expand the chart by a fixed amount for every week
                    ExpenseChart.Width += 750*ChartXScale * weekssearched;

                    // Go through each series of data and update them with the latest data
                    foreach (LineSeries i in ExpenseChart.Series)
                    {
                        // Create temporary series to hold new data for series
                        List<Expense> SeriesExpenses = new List<Expense>();
                        List<DateValueItem> TempItems = new List<DateValueItem>();
                        // Take only the data relevent to the current series
                        if (i.Name == "TotalExpenses")
                            SeriesExpenses = TempExpenses;
                        else
                            SeriesExpenses = TempExpenses.FindAll(a => a.Category == (string)i.Title);

                        // Get the current list of items in the series
                        TempItems = (List<DateValueItem>)i.ItemsSource;
                        if (TempItems == null)
                            TempItems = new List<DateValueItem>();

                        // Add the expenses to the current list
                        foreach (Expense j in SeriesExpenses)
                            TempItems.Add(new DateValueItem { Date = (j.Date + j.Time.TimeOfDay), Value = (int)j.Price, Id = j.Id });

                        // Set the source of the series to the updated list
                        i.ItemsSource = TempItems;

                        // If the series is the total expenses series, update the axis
                        if (ExpenseChart.Series.IndexOf(i) == 0)
                        {
                            i.IndependentAxis =
                                new DateTimeAxis
                                {
                                    Minimum = MinExpenseDate,
                                    Maximum = DateTime.Now,
                                    Location = AxisLocation.Bottom,
                                    Orientation = AxisOrientation.X,
                                    IntervalType = (ChartXScale > ChartXScaleIntervalThresh ? DateTimeIntervalType.Hours : DateTimeIntervalType.Days),
                                };
                            i.DependentRangeAxis =
                                new LinearAxis
                                {
                                    Orientation = AxisOrientation.Y,
                                    ShowGridLines = true,
                                    Location = AxisLocation.Right,
                                    Minimum = 0,
                                };
                        }
                        // Otherwise update the others to the match the total expenses one
                        else
                        {
                            i.IndependentAxis = ((ExpenseChart.Series[0] as LineSeries).IndependentAxis as DateTimeAxis);
                            i.DependentRangeAxis = ((ExpenseChart.Series[0] as LineSeries).DependentRangeAxis as LinearAxis);
                        }
                        i.Refresh();
                    }
                }
                // Disable the progress wheel
                LoginProgress.IsActive = false;
                ContentRoot.Children.Remove(LoginProgress);
            }
        }

        /// <summary>
        /// Updates the data in the expense chart with the latest expenses
        /// </summary>
        /// <returns></returns>
        private async Task RefreshExpenseChart()
        {
            // Display the progress wheel when getting data
            ProgressRing LoginProgress = new ProgressRing();
            LoginProgress.HorizontalAlignment = HorizontalAlignment.Center;
            LoginProgress.VerticalAlignment = VerticalAlignment.Center;
            ContentRoot.Children.Add(LoginProgress);
            LoginProgress.IsActive = true;

            // Get a handle to the expense table in the database
            IMobileServiceTable<Expense> ExpenseTable = App.alltheairgeadClient.GetTable<Expense>();

            // Query the database for all the expenses after the minimum displayed date
            Expenses = await ExpenseTable.Where(a => (a.Date > MinExpenseDate) && (a.Date <= DateTime.Now)).OrderByDescending(a => a.Date).ToListAsync();
            // Update every series in the chart
            foreach (LineSeries i in ExpenseChart.Series)
            {
                List<DateValueItem> Items = new List<DateValueItem>();
                foreach (Expense j in Expenses)
                    // Add expenses for the current series only
                    if (j.Category == (string)i.Title || ExpenseChart.Series.IndexOf(i) == 0)
                        Items.Add(new DateValueItem { Date = (j.Date + j.Time.TimeOfDay), Value = (int)j.Price, Id = j.Id });

                i.ItemsSource = Items;
                i.Refresh();
            }

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
            await GetCategoryData();
        }

        /// <summary>
        /// Called when the setting button is pressed. Shows the settings flyout dialog
        /// </summary>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.ShowAt(SettingsButton);
        }

        /// <summary>
        /// Logs the user out when the logout button is pressed in the app bar menu
        /// </summary>
        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            CustomAccountService.Logout();

            // Clear all the data from the previous user
            MinExpenseDate = DateTime.Now.Date;
            Expenses.Clear();
            foreach (LineSeries i in ExpenseChart.Series)
            {
                i.ItemsSource = new List<DateValueItem>();
                i.Refresh();
            }
            // Navigate back to the login page
            if (!Frame.Navigate(typeof(LoginPage)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
            // Show amessage to the user
            MessageDialog Dialog = new MessageDialog("Logged out");
            await Dialog.ShowAsync();
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

            if (CategoryBox.SelectedIndex < 0)
                await new MessageDialog("Category must be specified").ShowAsync();
            else if (String.IsNullOrWhiteSpace(PriceBox.Text))
                await new MessageDialog("Price must be specified").ShowAsync();
            else
            {
                PriorityBox.SelectedIndex = (CategoryBox.SelectedItem as Category).DefaultPriority;
                Expense ExpenseData = new Expense(CategoryBox.SelectedValue.ToString(), Decimal.Parse(PriceBox.Text), DateBox.Date, new DateTime(TimeBox.Time.Ticks), (byte?)(PriorityBox.SelectedIndex + 1), MoreInfoBox.Text);

                // Display the progress wheel when getting data
                ProgressRing LoginProgress = new ProgressRing();
                LoginProgress.HorizontalAlignment = HorizontalAlignment.Center;
                LoginProgress.VerticalAlignment = VerticalAlignment.Center;
                ContentRoot.Children.Add(LoginProgress);
                LoginProgress.IsActive = true;

                // Send the data to the mobile service
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

            // Get the selected item as a DateValueItem
            DateValueItem SelectedItem = (sender as LineSeries).SelectedItem as DateValueItem;

            // Find the selected item in the expenses list using the stored Id 
            Expense NewExpense = Expenses.Find(a => a.Id == SelectedItem.Id);

            // Set the header to show the category name of the expense
            TextBlock ExpenseHeader = new TextBlock();
            ExpenseHeader.Text = NewExpense.Category;
            ExpenseHeader.FontSize = 32;
            ExpenseHeader.Margin = new Thickness(30, 20, 0, 20);

            // Show the expense in the popup
            TextBlock ExpenseTip = new TextBlock();
            ExpenseTip.Text = NewExpense.Display();
            ExpenseTip.FontSize = 20;
            ExpenseTip.Margin = new Thickness(40, 10, 0, 20);

            // Create an edit expense button to link to the item page
            Button EditExpenseButton = new Button();
            EditExpenseButton.Content = "Edit Expense";
            EditExpenseButton.Click += new RoutedEventHandler(EditExpenseButton_Click);
            EditExpenseButton.Margin = new Thickness(40, 0, 0, 0);

            // Add the header, the information and the button to the popup children
            DataPointInfo.Children.Add(ExpenseHeader);
            DataPointInfo.Children.Add(ExpenseTip);
            DataPointInfo.Children.Add(EditExpenseButton);
            // Show the popup
            DataPointPop.ShowAt(sender as LineSeries);
        }

        /// <summary>
        /// Unselects the point and clears the flyout when it is closed
        /// </summary>
        private void DataPointPop_Closed(object sender, object e)
        {
            // Clear the items from the popup and deselect the item
            DataPointInfo.Children.Clear();
            (ExpenseChart.Series[0] as LineSeries).SelectedItem = null;
        }

        /// <summary>
        /// Navigates to the edit expense page when the edit expense button is pressed in the flyout
        /// </summary>
        private void EditExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            Expense NewExpense = Expenses.Find(a => a.Id == ((ExpenseChart.Series[0] as LineSeries).SelectedItem as DateValueItem).Id);
            // Write data to global variable to pass to new page.
            (App.Current as App).ExpenseEditData = NewExpense;

            // Navigate to the item page
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
            // Check that the view is not still changing
            if (e.IsIntermediate == false)
                await UpdateExpenseChart();

        }
        
        /// <summary>
        /// Called on ManipulationDelta event detected on X axis rectangle. Used to change axis scale easily
        /// </summary>
        private void ChartScroll_ManipulationXDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            // Make sure that the scale has changed by a sufficient amount to change the chart scale
            if (Math.Abs(e.Delta.Scale - 1.0) >= 0.01)
            {
                // Make sure that the scale is within the bounds
                if (ChartXScale <= MaxChartXScale && ChartXScale >= MinChartXScale)
                {
                    // Scale proprtional to the amount the manipulation has scaled
                    ChartXScale *= 1.0 + 0.5 * (e.Delta.Scale - 1.0);
                    // Make sure the scale doesn't jump outside the bounds
                    if (ChartXScale < MinChartXScale)
                        ChartXScale = MinChartXScale;
                    if (ChartXScale > MaxChartXScale)
                        ChartXScale = MaxChartXScale;

                    // Change the width of the chart to match the new scale
                    ExpenseChart.Width = 750 * ChartXScale * ((DateTime.Now.Date - MinExpenseDate).Days / 7);

                    // Adhust the axis to a more appropriate scale
                    if (ChartXScale > ChartXScaleIntervalThresh)
                        ((ExpenseChart.Series[0] as LineSeries).IndependentAxis as DateTimeAxis).IntervalType = DateTimeIntervalType.Hours;
                    else
                        ((ExpenseChart.Series[0] as LineSeries).IndependentAxis as DateTimeAxis).IntervalType = DateTimeIntervalType.Days;
                }
                // If the scale is outside the bounds, scale back
                else if (ChartXScale < MinChartXScale)
                {
                    ChartXScale = MinChartXScale;
                    ExpenseChart.Width = 750 * ChartXScale * ((DateTime.Now.Date - MinExpenseDate).Days / 7);
                }
                else if (ChartXScale > MaxChartXScale)
                {
                    ChartXScale = MaxChartXScale;
                    ExpenseChart.Width = 750 * ChartXScale * ((DateTime.Now.Date - MinExpenseDate).Days / 7);
                }
            }

            // Adjust the position to the allow for dragging in the axis
            double NewPosition = ChartScroll.HorizontalOffset * ChartXScale + e.Delta.Translation.X;
            ChartScroll.ChangeView(NewPosition, ChartScroll.VerticalOffset, ChartScroll.ZoomFactor, true);
        }

        /// <summary>
        /// Called on ManipulationDelta event detected on Y axis rectangle. Used to change axis scale easily
        /// </summary>
        private void ChartScroll_ManipulationYDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            // Make sure that the scale has changed by a sufficient amount to change the chart scale
            if (Math.Abs(e.Delta.Scale - 1.0) >= 0.01)
            {
                // Make sure that the scale is within the bounds
                if (ChartYScale <= MaxChartYScale && ChartYScale >= MinChartYScale)
                {
                    ChartYScale *= 1.0 + 0.5 * (e.Delta.Scale - 1.0);
                    // Make sure the scale doesn't jump outside the bounds
                    if (ChartYScale < MinChartYScale)
                        ChartYScale = MinChartYScale;
                    if (ChartYScale > MaxChartYScale)
                        ChartYScale = MaxChartYScale;

                    // Change the height if the chart to match the new scale
                    ExpenseChart.Height = PivotItem2.ActualHeight * ChartYScale;
                }
                // If the scale is outside the bounds, scale back
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

            // Adjust the position to the allow for dragging in the axis
            double NewPosition = ChartScroll.VerticalOffset * ChartYScale + e.Delta.Translation.Y;
            ChartScroll.ChangeView(ChartScroll.HorizontalOffset, NewPosition, ChartScroll.ZoomFactor, true);
        }

        /// <summary>
        /// Called when the category setting is toggled. Adds or removes a series from the chart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CategoryToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // Get the name of the catefory from the toggle switch
            string CategoryName = ((sender as ToggleSwitch).OnContent as string);

            // Check to see if it was toggled on or off
            if ((sender as ToggleSwitch).IsOn)
            {
                List<Expense> TempItems;
                List<DateValueItem> TempChartData = new List<DateValueItem>();
                LineSeries CategorySeries = new LineSeries();
                Binding CategorySeriesIVBinding = new Binding();
                Binding CategorySeriesDVBinding = new Binding();
                Style CategoryYAxisStyle = new Style(typeof(NumericAxisLabel));
                Style CategoryXAxisStyle = new Style(typeof(DateTimeAxisLabel));
                Setter Hide = new Setter(VisibilityProperty, Visibility.Collapsed);

                // Set the axis stule for the series
                CategoryYAxisStyle.Setters.Add(Hide);
                CategoryXAxisStyle.Setters.Add(Hide);

                // Set the path bindings for the X and Y axis data.
                CategorySeriesIVBinding.Path = new PropertyPath("Date");
                CategorySeriesDVBinding.Path = new PropertyPath("Value");

                // Find all the expenses for the category
                if (CategoryName == "Total Expenses")
                    TempItems = Expenses;
                else
                    TempItems = Expenses.FindAll(a => a.Category == CategoryName);

                // Add all the expenses to a list for displaying
                foreach (Expense i in TempItems)
                    TempChartData.Add(new DateValueItem { Date = (i.Date + i.Time.TimeOfDay), Value = (int)i.Price, Id = i.Id });

                // Setup the series
                CategorySeries.Name = CategoryName + "Series";
                CategorySeries.Title = CategoryName;
                CategorySeries.ItemsSource = TempChartData;
                CategorySeries.IndependentValueBinding = CategorySeriesIVBinding;
                CategorySeries.DependentValueBinding = CategorySeriesDVBinding;
                CategorySeries.IsSelectionEnabled = true;
                CategorySeries.SelectionChanged += DataPointTapped;
                CategorySeries.IndependentAxis = ((ExpenseChart.Series[0] as LineSeries).IndependentAxis as DateTimeAxis);
                CategorySeries.DependentRangeAxis = ((ExpenseChart.Series[0] as LineSeries).DependentRangeAxis as LinearAxis);
                CategorySeries.DataPointStyle = StyleService.LargeDataPoint(StyleService.Colours[ExpenseChart.Series.Count]);

                // Get the rectangle for the category to colour to match the series colour
                foreach (StackPanel i in ChartSettings.Items)
                {
                    if ((i.Children[0] as ToggleSwitch) == (sender as ToggleSwitch))
                        (i.Children[1] as Rectangle).Fill = new SolidColorBrush(StyleService.Colours[ExpenseChart.Series.Count]);
                }

                // Add the series to the chart
                ExpenseChart.Series.Add(CategorySeries);
                CategorySeries.Refresh();
            }
            // If the switch is turned off, remove the series
            else
            {
                // Find the series for the toggle by category name and remove it
                foreach (LineSeries i in ExpenseChart.Series)
                {
                    if ((string)i.Title == CategoryName)
                    {
                        ExpenseChart.Series.Remove(i);
                        break;
                    }
                }
                //  Find the ractangle for the category and colour it gray
                foreach (StackPanel i in ChartSettings.Items)
                {
                    if ((i.Children[0] as ToggleSwitch) == (sender as ToggleSwitch))
                    {
                        (i.Children[1] as Rectangle).Fill = new SolidColorBrush(Colors.Gray);
                        break;
                    }
                }

                // Recolour all the remaining line series to be the colour for they're position in the series list
                foreach (LineSeries i in ExpenseChart.Series)
                {
                    i.DataPointStyle = StyleService.LargeDataPoint(StyleService.Colours[ExpenseChart.Series.IndexOf(i)]);
                    foreach (StackPanel j in ChartSettings.Items)
                    {
                        if (ChartSettings.Items.IndexOf(j) > 0)
                        {
                            if (((j.Children[0] as ToggleSwitch).OnContent as string) == (i.Title as string))
                            {
                                (j.Children[1] as Rectangle).Fill = new SolidColorBrush(StyleService.Colours[ExpenseChart.Series.IndexOf(i)]);
                            }
                        }
                    }
                }
            }
        }

    }
}
