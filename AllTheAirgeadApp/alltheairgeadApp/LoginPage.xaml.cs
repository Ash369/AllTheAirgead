using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media;
using Windows.Phone.UI.Input;
using Windows.ApplicationModel.Resources;
using EmailValidation;
using alltheairgeadApp.Common;
using alltheairgeadApp.Services;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace alltheairgeadApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private TextBlock EmailErrorText;
        private TextBlock EmailTakenText;
        private TextBlock PasswordErrorText;
        private TextBlock PasswordShortText;

        public LoginPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            HardwareButtons.BackPressed += this.HardwareButtons_BackPressed;
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
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
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

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable the button after press to stop multiple presses
            LoginButton.IsEnabled = false;

            // Login the user and then load data from the mobile service.
            Boolean LoginVerbose = true;

            // Display the progress wheel when logging in
            ProgressRing LoginProgress = new ProgressRing();
            LoginProgress.HorizontalAlignment = HorizontalAlignment.Center;
            LoginProgress.VerticalAlignment = VerticalAlignment.Center;
            ContentRoot.Children.Add(LoginProgress);
            LoginProgress.IsActive = true;

            if (await CustomAccountService.Login(EmailLogin.Text, PasswordLogin.Password, LoginVerbose))
            {
                if (!Frame.Navigate(typeof(PivotPage)))
                {
                    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                }
            }
            // Disable the progress wheel
            LoginProgress.IsActive = false;
            ContentRoot.Children.Remove(LoginProgress);

            // Reenable the button to prevent the user from getting stuck for any reason
            LoginButton.IsEnabled = true;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable the button to prevent multiple attempts
            RegisterButton.IsEnabled = false;

            // Display the progress wheel when regstering
            ProgressRing LoginProgress = new ProgressRing();
            LoginProgress.HorizontalAlignment = HorizontalAlignment.Center;
            LoginProgress.VerticalAlignment = VerticalAlignment.Center;
            ContentRoot.Children.Add(LoginProgress);
            LoginProgress.IsActive = true;

            if (await CustomAccountService.Register(EmailRegister.Text, PasswordRegister.Password))
            {
                if (!Frame.Navigate(typeof(PivotPage)))
                {
                    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                }
            }
            // Disable the progress wheel
            LoginProgress.IsActive = false;
            ContentRoot.Children.Remove(LoginProgress);

            // Reenable the button when finished
            RegisterButton.IsEnabled = true;
        }

        // Called when text in the email registration box is changed. Check that the input is valid and inform the user.
        private async void EmailRegister_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Check that the email is a valid format
            if (EmailValidator.Validate(EmailRegister.Text))
            {
                // Check that the email has not already been registered
                if (await CustomAccountService.EmailAvailableCheck(EmailRegister.Text))
                {
                    // Turn on the light telling the user that the entry is valid
                    EmailValidLight.Fill = new SolidColorBrush(Windows.UI.Colors.LawnGreen);
                    // Remove the error messages if they were active
                    if (RegisterMessages.Children.Contains(EmailErrorText))
                        RegisterMessages.Children.Remove(EmailErrorText);
                    if (RegisterMessages.Children.Contains(EmailTakenText))
                        RegisterMessages.Children.Remove(EmailTakenText);

                    // Enable the register button when everything is valid
                    if (PasswordRegister.Password.Length >= 6 && PasswordRegister.Password == PasswordConfirmationRegister.Password)
                        RegisterButton.IsEnabled = true;
                }
                // If the email is already registered tell the user
                else
                {
                    // Turn off the valid light
                    EmailValidLight.Fill = new SolidColorBrush(Windows.UI.Colors.Gray);
                    // Display the error message
                    if (!RegisterMessages.Children.Contains(EmailTakenText))
                    {
                        EmailTakenText = new TextBlock();
                        EmailTakenText.Text = "Email address already registered";
                        EmailTakenText.FontSize = 16;
                        RegisterMessages.Children.Add(EmailTakenText);
                        // Disable the register button
                        RegisterButton.IsEnabled = false;
                    }
                }
            }
            // If the input is not a valid email format, inform the user by displaying a message.
            else
            {
                var a = resourceLoader;
                // Turn off the green light if it was on
                EmailValidLight.Fill = new SolidColorBrush(Windows.UI.Colors.Gray);
                // Display a message if it is not already displayed
                if (!RegisterMessages.Children.Contains(EmailErrorText))
                {
                    // Create text block to display
                    EmailErrorText = new TextBlock();
                    EmailErrorText.Text = "Email Address invalid";
                    EmailErrorText.FontSize = 16;
                    // Add it to the Stack Panel for registration messages
                    RegisterMessages.Children.Add(EmailErrorText);
                    // Disable the register button if data isn't valid
                    RegisterButton.IsEnabled = false;
                }
            }
        }

        // Called by a change in the password registration box. Check that the password is at least the minimum password length
        private void PasswordRegister_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Check that the password is the required length
            if (PasswordRegister.Password.Length >= 6)
            {
                // Turn on the light signalling that the password is acceptable
                PasswordValidLight.Fill = new SolidColorBrush(Windows.UI.Colors.LawnGreen);
                // Remove the error message if it has been displayed
                if (RegisterMessages.Children.Contains(PasswordShortText))
                    RegisterMessages.Children.Remove(PasswordShortText);

                // Enable the register button if all else has been satisfied
                if (EmailValidator.Validate(EmailRegister.Text) && PasswordRegister.Password == PasswordConfirmationRegister.Password)
                    RegisterButton.IsEnabled = true;
            }
            // If the password is not valid, inform the user with a helpful message
            else
            {
                // Turn off the valid light
                PasswordValidLight.Fill = new SolidColorBrush(Windows.UI.Colors.Gray);
                // Display a message if it was not already displayed
                if (!RegisterMessages.Children.Contains(PasswordShortText))
                {
                    // Create the text block
                    PasswordShortText = new TextBlock();
                    PasswordShortText.Text = "Password must be at least 6 characters";
                    PasswordShortText.FontSize = 16;
                    // Add it to the Stack Panel provided for messages
                    RegisterMessages.Children.Add(PasswordShortText);
                    // Disable the register button if data isn't valid
                    RegisterButton.IsEnabled = false;
                }
            }

        }

        // Called by a change in the password confirmation field. Check that the two passwords match
        private void PasswordConfirmationRegister_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Check that the passowrds match
            if (PasswordRegister.Password == PasswordConfirmationRegister.Password)
            {
                // Turn on the password confirmation valid light to inform the user that the password is correct
                PasswordConfirmValidLight.Fill = new SolidColorBrush(Windows.UI.Colors.LawnGreen);
                // Remove the displayed error messages
                if (RegisterMessages.Children.Contains(PasswordErrorText))
                    RegisterMessages.Children.Remove(PasswordErrorText);

                // Enable the register button when all data is valid
                if (EmailValidator.Validate(EmailRegister.Text) && PasswordRegister.Password.Length >= 6)
                    RegisterButton.IsEnabled = true;
            }
            // If the passwords don't match, inform the user
            else
            {
                // Show an error message if not already displayed
                if (!RegisterMessages.Children.Contains(PasswordErrorText))
                {
                    // Create the error message
                    PasswordErrorText = new TextBlock();
                    PasswordErrorText.Text = "Passwords do not match";
                    PasswordErrorText.FontSize = 16;
                    // Add it to the stack panel for display
                    RegisterMessages.Children.Add(PasswordErrorText);
                    // Disable the register button if data isn't valid
                    RegisterButton.IsEnabled = false;
                }
            }


        }

        /// <summary>
        /// Closes the app when the back button is pressed to avoid going back to a logged in screen
        /// </summary>
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            // Set the event to handled to prevent bubbling up to higher levels
            e.Handled = true;
            // Exit the app
            App.Current.Exit();
        }
    }
}
