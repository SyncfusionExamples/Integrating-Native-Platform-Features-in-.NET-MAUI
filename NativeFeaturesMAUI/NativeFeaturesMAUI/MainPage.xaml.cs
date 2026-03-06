using System.Collections.ObjectModel;

namespace NativeFeaturesMAUI
{
    public partial class MainPage : ContentPage
    {
        private MainPageViewModel? _viewModel;

        /// <summary>
        /// Initializes UI components, resolves services, toggles platform-specific UI,
        /// binds the events list, sets initial state, hides chips/cards, and refreshes network status.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            var services = Application.Current?.Handler?.MauiContext?.Services;
            _viewModel = services?.GetService<MainPageViewModel>();

            bool isMobile = DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS;
            SetVisible(AccelCard, isMobile);
            SetVisible(CompassCard, isMobile);

            if (_viewModel is not null)
            {
                BindingContext = _viewModel;
                EventsList.ItemsSource = _viewModel.EventLog;

                _viewModel.StateChanged += (s, a, d) => MainThread.BeginInvokeOnMainThread(() => SetState(s, a, d));
                _viewModel.PhotoCaptured += (stream, name) => MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (Photo is not null)
                        Photo.Source = ImageSource.FromStream(() => stream);
                    SetVisible(PhotoCard, true);
                    SetVisible(FileChip, false);
                    SetVisible(LocationChip, false);
                });

                _viewModel.FilePicked += name => MainThread.BeginInvokeOnMainThread(() =>
                {
                    SetText(FileNameLabel, name);
                    SetVisible(FileChip, true);
                    SetVisible(PhotoCard, false);
                    SetVisible(LocationChip, false);
                });

                _viewModel.LocationUpdated += text => MainThread.BeginInvokeOnMainThread(() =>
                {
                    SetText(LocationLabel, text);
                    SetVisible(LocationChip, true);
                    SetVisible(PhotoCard, false);
                    SetVisible(FileChip, false);
                });

                _viewModel.NetworkUpdated += text => MainThread.BeginInvokeOnMainThread(() => SetText(NetworkLabel, text));

                _viewModel.AccelerometerReadingUpdated += text => MainThread.BeginInvokeOnMainThread(() => SetText(AccelLabel, text));
                _viewModel.ShakeDetectedEvent += () => MainThread.BeginInvokeOnMainThread(() =>
                {
                    SetText(ShakeLabel, "Shake: detected");
                    SetState("Success", "Shake", "User shook the device");
                });

                _viewModel.CompassHeadingUpdated += text => MainThread.BeginInvokeOnMainThread(() => SetText(CompassLabel, text));
            }

            SetState("Ready", "App started");
            SetVisible(PhotoCard, false);
            SetVisible(FileChip, false);
            SetVisible(LocationChip, false);
            _viewModel?.NetworkAsync();
        }

        /// <summary>
        /// Updates chips (state/action/time), message text, and activity log with an optional detail.
        /// Maintains a bounded list of the latest 8 entries.
        /// </summary>
        private void SetState(string state, string action, string? detail = null)
        {
            SetText(ChipState, state);
            SetText(ChipLastAction, action);
            SetText(ChipTime, DateTime.Now.ToString("HH:mm:ss"));

            if (!string.IsNullOrWhiteSpace(detail))
            {
                SetText(Output, detail!);
            }

            // Event logging is handled by the view model; avoid adding duplicate entries here.
        }

        /// <summary>
        /// Subscribes to connectivity changes and refreshes network status when the page appears.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel?.OnAppearing();
        }

        /// <summary>
        /// Unsubscribes from connectivity changes and stops active sensors to save battery when the page disappears.
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel?.OnDisappearing();
        }

        /// <summary>
        /// Sets the visibility of the specified visual element.
        /// </summary>
        private static void SetVisible(VisualElement? element, bool isVisible)
        {
            if (element is not null) element.IsVisible = isVisible;
        }

        /// <summary>
        /// Sets the text of the specified label, if it is not null.
        /// </summary>
        private static void SetText(Label? label, string text)
        {
            if (label is not null) label.Text = text;
        }
    }
}
