using System.Collections.ObjectModel;

namespace NativeFeaturesMAUI
{
    public partial class MainPage : ContentPage
    {
        private readonly IToastService? _toast;
        private readonly ObservableCollection<string> _events = new();
        private bool _accelActive = false;
        private bool _compassActive = false;

        /// <summary>
        /// Initializes UI components, resolves services, toggles platform-specific UI,
        /// binds the events list, sets initial state, hides chips/cards, and refreshes network status.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            _toast = Application.Current?.Handler?.MauiContext?.Services.GetService<IToastService>();
            // Enable accelerometer/compass UI only on mobile platforms
            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            {
                if (AccelCard != null)
                {
                    AccelCard.IsVisible = true;
                }
                if (CompassCard != null)
                {
                    CompassCard.IsVisible = true;
                }
            }
            else
            {
                if (AccelCard != null)
                {
                    AccelCard.IsVisible = false;
                }
                if (CompassCard != null)
                {
                    CompassCard.IsVisible = false;
                }
            }

            if (EventsList != null)
            {
                EventsList.ItemsSource = _events;
            }

            SetState("Ready", "App started");
            if (PhotoCard != null)
            {
                PhotoCard.IsVisible = false;
            }
            if (FileChip != null)
            {
                FileChip.IsVisible = false;
            }
            if (LocationChip != null)
            {
                LocationChip.IsVisible = false;
            }

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
            if (ChipState != null)
            {
                ChipState.Text = state;
            }
            if (ChipLastAction != null)
            {
                ChipLastAction.Text = action;
            }
            if (ChipTime != null)
            {
                ChipTime.Text = DateTime.Now.ToString("HH:mm:ss");
            }

            if (!string.IsNullOrWhiteSpace(detail) && Output != null)
            {
                Output.Text = detail;
            }

            var entry = $"{DateTime.Now:HH:mm:ss} • {action} → {state}{(string.IsNullOrEmpty(detail) ? "" : $" · {detail}")}";
            _events.Insert(0, entry);
            while (_events.Count > 8)
            {
                _events.RemoveAt(_events.Count - 1);
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
                // If running on MacCatalyst or a virtual device, capture isn't supported
                if (DeviceInfo.Platform == DevicePlatform.MacCatalyst || DeviceInfo.DeviceType == DeviceType.Virtual)
                {
                    SetState("Unsupported", "Take Photo", "Capture not supported on this device.");
                    if (PhotoCard != null)
                    {
                        PhotoCard.IsVisible = false;
                    }
                    return;
                }

                var status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    SetState("Denied", "Take Photo", "Camera permission not granted.");
                    if (PhotoCard != null)
                    {
                        PhotoCard.IsVisible = false;
                    }
                    return;
                }

                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo is null)
                {
                    SetState("Cancelled", "Take Photo");
                    if (PhotoCard != null)
                    {
                        PhotoCard.IsVisible = false;
                    }
                    return;
                }

                await using var read = await photo.OpenReadAsync();
                var ms = new MemoryStream();
                await read.CopyToAsync(ms);
                ms.Position = 0;

                if (Photo != null)
                {
                    Photo.Source = ImageSource.FromStream(() => ms);
                }
                if (PhotoCard != null)
                {
                    PhotoCard.IsVisible = true;
                }

                SetState("Success", "Take Photo", $"Captured: {photo.FileName}");

                if (FileChip != null)
                {
                    FileChip.IsVisible = false;
                }
                if (LocationChip != null)
                {
                    LocationChip.IsVisible = false;
                }
            }
            catch (FeatureNotSupportedException)
            {
                if (PhotoCard != null)
                {
                    PhotoCard.IsVisible = false;
                }
                SetState("Unsupported", "Take Photo", "Capture not supported on this device.");
            }
            catch (Exception ex)
            {
                if (PhotoCard != null)
                {
                    PhotoCard.IsVisible = false;
                }
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
                    if (FileChip != null)
                    {
                        FileChip.IsVisible = false;
                    }
                    SetState("Cancelled", "Pick File");
                    return;
                }

                if (FileNameLabel != null)
                {
                    FileNameLabel.Text = result.FileName;
                }
                if (FileChip != null)
                {
                    FileChip.IsVisible = true;
                }

                SetState("Success", "Pick File", $"Picked: {result.FileName}");

                if (PhotoCard != null)
                {
                    PhotoCard.IsVisible = false;
                }
                if (LocationChip != null)
                {
                    LocationChip.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                if (FileChip != null)
                {
                    FileChip.IsVisible = false;
                }
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
                    if (LocationChip != null)
                    {
                        LocationChip.IsVisible = false;
                    }
                    SetState("Denied", "Get Location", "Location permission not granted.");
                    return;
                }

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request);

                if (location is null)
                {
                    if (LocationChip != null)
                    {
                        LocationChip.IsVisible = false;
                    }
                    SetState("Unavailable", "Get Location", "No location returned.");
                    return;
                }

                if (LocationLabel != null)
                {
                    LocationLabel.Text = $"Lat {location.Latitude:F5}, Lon {location.Longitude:F5} · Acc {location.Accuracy} m";
                }

                if (LocationChip != null)
                {
                    LocationChip.IsVisible = true;
                }

                SetState("Success", "Get Location", $"Lat {location.Latitude:F5}, Lon {location.Longitude:F5}");

                if (PhotoCard != null)
                {
                    PhotoCard.IsVisible = false;
                }
                if (FileChip != null)
                {
                    FileChip.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                if (LocationChip != null)
                {
                    LocationChip.IsVisible = false;
                }
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
                if (PhotoCard != null)
                {
                    PhotoCard.IsVisible = false;
                }
                if (FileChip != null)
                {
                    FileChip.IsVisible = false;
                }
                if (LocationChip != null)
                {
                    LocationChip.IsVisible = false;
                }
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
                if (_toast is null)
                {
                    SetState("Error", "Toast / Alert", "IToastService not resolved. Check DI registrations.");
                    return;
                }

                await _toast.ShowAsync("Hello from native UI!");
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

            if (PhotoCard != null)
            {
                PhotoCard.IsVisible = false;
            }
            if (FileChip != null)
            {
                FileChip.IsVisible = false;
            }
            if (LocationChip != null)
            {
                LocationChip.IsVisible = false;
            }
        }

        /// <summary>
        /// Detects and displays the current platform orientation using PlatformOrientationService.
        /// Resets other chips/cards to keep the UI focused on this result.
        /// </summary>
        private void OnOrientationClicked(object sender, EventArgs e)
        {
            var value = PlatformOrientationService.GetOrientation();
            SetState("Success", "Orientation", value);

            if (PhotoCard != null)
            {
                PhotoCard.IsVisible = false;
            }
            if (FileChip != null)
            {
                FileChip.IsVisible = false;
            }
            if (LocationChip != null)
            {
                LocationChip.IsVisible = false;
            }
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

            string profileText = string.Empty;
            if (profiles.Contains(ConnectionProfile.WiFi))
            {
                profileText = " · Wi‑Fi";
            }
            else if (profiles.Contains(ConnectionProfile.Cellular))
            {
                profileText = " · Cellular";
            }

            string status = access switch
            {
                NetworkAccess.Internet => "Online",
                NetworkAccess.ConstrainedInternet => "Captive portal",
                NetworkAccess.Local => "Local only",
                NetworkAccess.None => "Offline",
                _ => "Unknown"
            };

            if (NetworkLabel != null)
            {
                NetworkLabel.Text = status + ((status == "Online") ? profileText : string.Empty);
            }
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
            if (!_accelActive)
            {
                StartAccelerometer();
            }
            else
            {
                StopAccelerometer();
            }
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

            Accelerometer.Default.ReadingChanged += OnAccelReadingChanged;
            Accelerometer.Default.ShakeDetected += OnShakeDetected;
            Accelerometer.Default.Start(SensorSpeed.UI);
            _accelActive = true;

            if (AccelChip != null)
            {
                AccelChip.IsVisible = true;
            }
            SetState("Listening", "Accelerometer", "Streaming X/Y/Z");
        }

        /// <summary>
        /// Stops accelerometer updates and unsubscribes from events if active.
        /// Hides the chip and logs a stopped state.
        /// </summary>
        private void StopAccelerometer()
        {
            if (!_accelActive)
            {
                return;
            }

            Accelerometer.Default.ReadingChanged -= OnAccelReadingChanged;
            Accelerometer.Default.ShakeDetected -= OnShakeDetected;
            Accelerometer.Default.Stop();
            _accelActive = false;

            if (AccelChip != null)
            {
                AccelChip.IsVisible = false;
            }
            SetState("Stopped", "Accelerometer");
        }

        /// <summary>
        /// Updates the accelerometer label with formatted X/Y/Z values on the UI thread.
        /// </summary>
        private void OnAccelReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            var a = e.Reading.Acceleration;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (AccelLabel != null)
                {
                    AccelLabel.Text = $"X {a.X:F2}  Y {a.Y:F2}  Z {a.Z:F2}";
                }
            });
        }

        /// <summary>
        /// Handles shake detection: updates the UI label and logs a success state for the shake action.
        /// </summary>
        private void OnShakeDetected(object? sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (ShakeLabel != null)
                {
                    ShakeLabel.Text = "Shake: detected";
                }
                SetState("Success", "Shake", "User shook the device");
            });
        }

        /// <summary>
        /// Toggles the compass: starts heading updates if inactive, otherwise stops them.
        /// </summary>
        private void OnCompassClicked(object sender, EventArgs e)
        {
            if (!_compassActive)
            {
                StartCompass();
            }
            else
            {
                StopCompass();
            }
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
            _compassActive = true;

            if (CompassChip != null)
            {
                CompassChip.IsVisible = true;
            }
            SetState("Listening", "Compass", "Heading streaming");
        }

        /// <summary>
        /// Stops compass updates and unsubscribes from events if active.
        /// Hides the chip and logs a stopped state.
        /// </summary>
        private void StopCompass()
        {
            if (!_compassActive)
            {
                return;
            }

            Compass.Default.ReadingChanged -= OnCompassReadingChanged;
            Compass.Default.Stop();
            _compassActive = false;

            if (CompassChip != null)
            {
                CompassChip.IsVisible = false;
            }
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
                if (CompassLabel != null)
                {
                    CompassLabel.Text = $"Heading: {heading:F0}°";
                }
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
    }

    /// <summary>
    /// Abstraction for lightweight toast/toast-like notifications. Implement per platform and register in DI.
    /// </summary>
    public interface IToastService
    {
        Task ShowAsync(string message);
    }
}