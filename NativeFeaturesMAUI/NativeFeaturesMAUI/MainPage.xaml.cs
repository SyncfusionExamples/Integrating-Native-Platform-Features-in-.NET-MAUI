using System.Collections.ObjectModel;

namespace NativeFeaturesMAUI
{
    public partial class MainPage : ContentPage
    {
        private readonly IToastService? _toastService;
        private readonly ObservableCollection<string> _eventLog = new();
        private bool _isAccelerometerActive;
        private bool _isCompassActive;

        /// <summary>
        /// Initializes UI components, resolves services, toggles platform-specific UI,
        /// binds the events list, sets initial state, hides chips/cards, and refreshes network status.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            _toastService = Application.Current?.Handler?.MauiContext?.Services.GetService<IToastService>();
            bool isMobile = DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS;

            SetVisible(AccelCard, isMobile);
            SetVisible(CompassCard, isMobile);

            if (EventsList is not null)
            {
                EventsList.ItemsSource = _eventLog;
            }

            SetState("Ready", "App started");
            SetVisible(PhotoCard, false);
            SetVisible(FileChip, false);
            SetVisible(LocationChip, false);
            RefreshNetworkStatus();
        }

        /// <summary>
        /// Updates chips (state/action/time), message text, and activity log with an optional detail.
        /// Maintains a bounded list of the latest 8 entries.
        /// </summary>
        /// <param name="state">Current state label (e.g., Success, Error).</param>
        /// <param name="action">Action name (e.g., Take Photo, Vibrate).</param>
        /// <param name="detail">Optional detail text displayed in the Output label.</param>
        private void SetState(string state, string action, string? detail = null)
        {
            SetText(ChipState, state);
            SetText(ChipLastAction, action);
            SetText(ChipTime, DateTime.Now.ToString("HH:mm:ss"));

            if (!string.IsNullOrWhiteSpace(detail))
            {
                SetText(Output, detail!);
            }

            var entry = $"{DateTime.Now:HH:mm:ss} • {action} → {state}{(string.IsNullOrEmpty(detail) ? "" : $" · {detail}")}";
            _eventLog.Insert(0, entry);

            while (_eventLog.Count > 8)
            {
                _eventLog.RemoveAt(_eventLog.Count - 1);
            }
        }

        /// <summary>
        /// Handles the Take Photo action: checks platform support, requests permission,
        /// captures a photo using the system camera, and displays the result in the UI.
        /// Manages success/cancel/error states and toggles related UI chips/cards.
        /// </summary>
        private async void OnTakePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.MacCatalyst || DeviceInfo.DeviceType == DeviceType.Virtual)
                {
                    SetState("Unsupported", "Take Photo", "Capture not supported on this device.");
                    SetVisible(PhotoCard, false);
                    return;
                }

                var status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    SetState("Denied", "Take Photo", "Camera permission not granted.");
                    SetVisible(PhotoCard, false);
                    return;
                }

                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo is null)
                {
                    SetState("Cancelled", "Take Photo");
                    SetVisible(PhotoCard, false);
                    return;
                }

                await using var read = await photo.OpenReadAsync();
                var ms = new System.IO.MemoryStream();
                await read.CopyToAsync(ms);
                ms.Position = 0;

                if (Photo is not null)
                    Photo.Source = ImageSource.FromStream(() => ms);

                SetVisible(PhotoCard, true);
                SetState("Success", "Take Photo", $"Captured: {photo.FileName}");

                SetVisible(FileChip, false);
                SetVisible(LocationChip, false);
            }
            catch (FeatureNotSupportedException)
            {
                SetVisible(PhotoCard, false);
                SetState("Unsupported", "Take Photo", "Capture not supported on this device.");
            }
            catch (Exception ex)
            {
                SetVisible(PhotoCard, false);
                SetState("Error", "Take Photo", ex.Message);
            }
        }

        /// <summary>
        /// Opens the platform file picker with a title and displays the picked file name.
        /// Updates UI chips and handles cancel/error states.
        /// </summary>
        private async void OnPickFileClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Select a file" });
                if (result is null)
                {
                    SetVisible(FileChip, false);
                    SetState("Cancelled", "Pick File");
                    return;
                }

                SetText(FileNameLabel, result.FileName);
                SetVisible(FileChip, true);
                SetState("Success", "Pick File", $"Picked: {result.FileName}");
                SetVisible(PhotoCard, false);
                SetVisible(LocationChip, false);
            }
            catch (Exception ex)
            {
                SetVisible(FileChip, false);
                SetState("Error", "Pick File", ex.Message);
            }
        }

        /// <summary>
        /// Requests location permission, retrieves a medium-accuracy location with a timeout,
        /// updates the UI (label + chip), and reports errors/denials/cancellations.
        /// </summary>
        private async void OnGetLocationClicked(object sender, EventArgs e)
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    SetVisible(LocationChip, false);
                    SetState("Denied", "Get Location", "Location permission not granted.");
                    return;
                }

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request);

                if (location is null)
                {
                    SetVisible(LocationChip, false);
                    SetState("Unavailable", "Get Location", "No location returned.");
                    return;
                }

                SetText(
                    LocationLabel,
                    $"Lat {location.Latitude:F5}, Lon {location.Longitude:F5} · Acc {location.Accuracy} m"
                );

                SetVisible(LocationChip, true);
                SetState("Success", "Get Location", $"Lat {location.Latitude:F5}, Lon {location.Longitude:F5}");

                SetVisible(PhotoCard, false);
                SetVisible(FileChip, false);
            }
            catch (Exception ex)
            {
                SetVisible(LocationChip, false);
                SetState("Error", "Get Location", ex.Message);
            }
        }

        /// <summary>
        /// Triggers a 1-second vibration (haptic feedback) if supported and updates state.
        /// Handles unsupported or error cases gracefully.
        /// </summary>
        private void OnVibrateClicked(object sender, EventArgs e)
        {
            try
            {
                Vibration.Default.Vibrate(TimeSpan.FromSeconds(1));
                SetState("Success", "Vibrate", "Haptic feedback triggered.");
                SetVisible(PhotoCard, false);
                SetVisible(FileChip, false);
                SetVisible(LocationChip, false);
            }
            catch (FeatureNotSupportedException)
            {
                SetState("Unsupported", "Vibrate", "Vibration not supported.");
            }
            catch (Exception ex)
            {
                SetState("Error", "Vibrate", ex.Message);
            }
        }

        /// <summary>
        /// Attempts to show a toast/notification using the resolved IToastService.
        /// Logs an error if the service could not be resolved.
        /// </summary>
        private async void OnToastClicked(object sender, EventArgs e)
        {
            try
            {
                if (_toastService is null)
                {
                    SetState("Error", "Toast / Alert", "IToastService not resolved. Check DI registrations.");
                    return;
                }

                await _toastService.ShowAsync("Hello from native UI!");
                SetState("Success", "Toast / Alert", "System notification posted.");
            }
            catch (Exception ex)
            {
                SetState("Error", "Toast / Alert", ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a user-friendly device model string and logs it to the activity feed.
        /// Also hides other content chips/cards for a clean UI.
        /// </summary>
        private void OnDeviceModelClicked(object sender, EventArgs e)
        {
            var model = DeviceModelService.Model();
            SetState("Success", "Device Model", model);
            SetVisible(PhotoCard, false);
            SetVisible(FileChip, false);
            SetVisible(LocationChip, false);
        }

        /// <summary>
        /// Detects and displays the current platform orientation using PlatformOrientationService.
        /// Resets other chips/cards to keep the UI focused on this result.
        /// </summary>
        private void OnOrientationClicked(object sender, EventArgs e)
        {
            var value = PlatformOrientationService.GetOrientation();
            SetState("Success", "Orientation", value);
            SetVisible(PhotoCard, false);
            SetVisible(FileChip, false);
            SetVisible(LocationChip, false);
        }

        /// <summary>
        /// Manually refreshes the network status and logs an informational state with the label text.
        /// </summary>
        private void OnNetworkClicked(object sender, EventArgs e)
        {
            RefreshNetworkStatus();
            SetState("Info", "Network", NetworkLabel?.Text);
        }

        /// <summary>
        /// Reads current network access and connection profiles, maps them to a human-friendly label,
        /// and updates the NetworkLabel. Prioritizes Wi-Fi and Cellular for chip display.
        /// </summary>
        private void RefreshNetworkStatus()
        {
            var access = Connectivity.Current.NetworkAccess;
            var profiles = Connectivity.Current.ConnectionProfiles;

            string profileText =
                profiles.Contains(ConnectionProfile.WiFi) ? " · Wi‑Fi" :
                profiles.Contains(ConnectionProfile.Cellular) ? " · Cellular" : string.Empty;

            string status = access switch
            {
                NetworkAccess.Internet => "Online",
                NetworkAccess.ConstrainedInternet => "Captive portal",
                NetworkAccess.Local => "Local only",
                NetworkAccess.None => "Offline",
                _ => "Unknown"
            };

            SetText(NetworkLabel, status + (status == "Online" ? profileText : string.Empty));
        }

        /// <summary>
        /// Handles runtime connectivity changes by refreshing the status label and
        /// logging a state (Online/Offline) along with the current label text.
        /// </summary>
        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            RefreshNetworkStatus();
            SetState(e.NetworkAccess == NetworkAccess.Internet ? "Online" : "Offline", "Network", NetworkLabel?.Text);
        }

        /// <summary>
        /// Toggles the accelerometer: starts streaming readings and shake detection if inactive;
        /// otherwise stops listening and hides the chip.
        /// </summary>
        private void OnAccelerometerClicked(object sender, EventArgs e)
        {
            if (!_isAccelerometerActive)
                StartAccelerometer();
            else
                StopAccelerometer();
        }

        /// <summary>
        /// Starts accelerometer updates at UI speed, subscribes to reading and shake events,
        /// sets internal active flag, shows the chip, and logs a listening state.
        /// </summary>
        private void StartAccelerometer()
        {
            if (!Accelerometer.Default.IsSupported)
            {
                SetState("Unsupported", "Accelerometer", "Not supported on this device.");
                return;
            }

            Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
            Accelerometer.Default.ShakeDetected += OnShakeDetected;
            Accelerometer.Default.Start(SensorSpeed.UI);
            _isAccelerometerActive = true;

            SetVisible(AccelChip, true);
            SetState("Listening", "Accelerometer", "Streaming X/Y/Z");
        }

        /// <summary>
        /// Stops accelerometer updates and unsubscribes from events if active.
        /// Hides the chip and logs a stopped state.
        /// </summary>
        private void StopAccelerometer()
        {
            if (!_isAccelerometerActive)
                return;

            Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
            Accelerometer.Default.ShakeDetected -= OnShakeDetected;
            Accelerometer.Default.Stop();
            _isAccelerometerActive = false;

            SetVisible(AccelChip, false);
            SetState("Stopped", "Accelerometer");
        }

        /// <summary>
        /// Updates the accelerometer label with formatted X/Y/Z values on the UI thread.
        /// </summary>
        private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            var a = e.Reading.Acceleration;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SetText(AccelLabel, $"X {a.X:F2}  Y {a.Y:F2}  Z {a.Z:F2}");
            });
        }

        /// <summary>
        /// Handles shake detection: updates the UI label and logs a success state for the shake action.
        /// </summary>
        private void OnShakeDetected(object? sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SetText(ShakeLabel, "Shake: detected");
                SetState("Success", "Shake", "User shook the device");
            });
        }

        /// <summary>
        /// Toggles the compass: starts heading updates if inactive, otherwise stops them.
        /// </summary>
        private void OnCompassClicked(object sender, EventArgs e)
        {
            if (!_isCompassActive)
                StartCompass();
            else
                StopCompass();
        }

        /// <summary>
        /// Starts compass updates at UI speed, subscribes to ReadingChanged,
        /// shows the chip, sets the active flag, and logs a listening state.
        /// </summary>
        private void StartCompass()
        {
            if (!Compass.Default.IsSupported)
            {
                SetState("Unsupported", "Compass", "Not supported on this device.");
                return;
            }

            Compass.Default.ReadingChanged += OnCompassReadingChanged;
            Compass.Default.Start(SensorSpeed.UI);
            _isCompassActive = true;

            SetVisible(CompassChip, true);
            SetState("Listening", "Compass", "Heading streaming");
        }

        /// <summary>
        /// Stops compass updates and unsubscribes from events if active.
        /// Hides the chip and logs a stopped state.
        /// </summary>
        private void StopCompass()
        {
            if (!_isCompassActive)
                return;

            Compass.Default.ReadingChanged -= OnCompassReadingChanged;
            Compass.Default.Stop();
            _isCompassActive = false;

            SetVisible(CompassChip, false);
            SetState("Stopped", "Compass");
        }

        /// <summary>
        /// Updates the compass label with the current magnetic heading (integer degrees) on the UI thread.
        /// </summary>
        private void OnCompassReadingChanged(object? sender, CompassChangedEventArgs e)
        {
            var heading = e.Reading.HeadingMagneticNorth;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SetText(CompassLabel, $"Heading: {heading:F0}°");
            });
        }

        /// <summary>
        /// Subscribes to connectivity changes and refreshes network status when the page appears.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshNetworkStatus();
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
        }

        /// <summary>
        /// Unsubscribes from connectivity changes and stops active sensors to save battery when the page disappears.
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= OnConnectivityChanged;
            StopAccelerometer();
            StopCompass();
        }

        // Helpers
        private static void SetVisible(VisualElement? element, bool isVisible)
        {
            if (element is not null) element.IsVisible = isVisible;
        }

        private static void SetText(Label? label, string text)
        {
            if (label is not null) label.Text = text;
        }
    }

    /// <summary>
    /// Abstraction for lightweight toast/toast-like notifications. Implement per platform and register in DI.
    /// </summary>
    public interface IToastService
    {
        Task ShowAsync(string message);
    }
}