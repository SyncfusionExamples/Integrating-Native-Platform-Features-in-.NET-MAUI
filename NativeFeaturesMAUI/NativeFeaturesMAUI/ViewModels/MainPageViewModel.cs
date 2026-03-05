using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Networking;
using Microsoft.Maui.ApplicationModel;
using NativeFeaturesMAUI.Helpers;
using NativeFeaturesMAUI.Services;

namespace NativeFeaturesMAUI
{
    public class MainPageViewModel
    {
        private readonly IToastService _toast;
        private readonly ILocationService _locationService;
        private bool _isAccelerometerActive;
        private bool _isCompassActive;

        public ObservableCollection<string> EventLog { get; } = new();

        public ICommand TakePhotoCommand { get; }
        public ICommand PickFileCommand { get; }
        public ICommand GetLocationCommand { get; }
        public ICommand VibrateCommand { get; }
        public ICommand ToastCommand { get; }
        public ICommand DeviceModelCommand { get; }
        public ICommand OrientationCommand { get; }
        public ICommand NetworkCommand { get; }
        public ICommand AccelerometerToggleCommand { get; }
        public ICommand CompassToggleCommand { get; }

        public event Action<string, string, string?>? StateChanged;
        public event Action<Stream, string>? PhotoCaptured;
        public event Action<string>? FilePicked;
        public event Action<string>? LocationUpdated;
        public event Action<string>? NetworkUpdated;
        public event Action<string>? AccelerometerReadingUpdated;
        public event Action? ShakeDetectedEvent;
        public event Action<string>? CompassHeadingUpdated;

        public MainPageViewModel(IToastService toast, ILocationService locationService)
        {
            _toast = toast ?? throw new ArgumentNullException(nameof(toast));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));

            TakePhotoCommand = new AsyncCommand(TakePhotoAsync);
            PickFileCommand = new AsyncCommand(PickFileAsync);
            GetLocationCommand = new AsyncCommand(GetLocationAsync);
            VibrateCommand = new AsyncCommand(VibrateAsync);
            ToastCommand = new AsyncCommand(ShowToastAsync);
            DeviceModelCommand = new AsyncCommand(DeviceModelAsync);
            OrientationCommand = new AsyncCommand(OrientationAsync);
            NetworkCommand = new AsyncCommand(NetworkAsync);
            AccelerometerToggleCommand = new AsyncCommand(() =>
            {
                if (!_isAccelerometerActive)
                    StartAccelerometer();
                else
                    StopAccelerometer();
                return Task.CompletedTask;
            });

            CompassToggleCommand = new AsyncCommand(() =>
            {
                if (!_isCompassActive)
                    StartCompass();
                else
                    StopCompass();
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Logs a state change event and records the action, state, and optional detail in the event log.
        /// </summary>
        private void Log(string state, string action, string? detail = null)
        {
            StateChanged?.Invoke(state, action, detail);

            var entry = $"{DateTime.Now:HH:mm:ss} • {action} → {state}{(string.IsNullOrEmpty(detail) ? "" : $" · {detail}")}";
            EventLog.Insert(0, entry);
            while (EventLog.Count > 8)
                EventLog.RemoveAt(EventLog.Count - 1);
        }

        /// <summary>
        /// Takes a photo using the device camera. Handles permissions, unsupported features, and cancellation gracefully.
        /// </summary>
        /// <returns></returns>
        public async Task TakePhotoAsync()
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.MacCatalyst || DeviceInfo.DeviceType == DeviceType.Virtual)
                {
                    Log("Unsupported", "Take Photo", "Capture not supported on this device.");
                    return;
                }

                var status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    Log("Denied", "Take Photo", "Camera permission not granted.");
                    return;
                }

                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo is null)
                {
                    Log("Cancelled", "Take Photo");
                    return;
                }

                await using var read = await photo.OpenReadAsync();
                var ms = new MemoryStream();
                await read.CopyToAsync(ms);
                ms.Position = 0;

                PhotoCaptured?.Invoke(ms, photo.FileName ?? "photo.jpg");
                Log("Success", "Take Photo", $"Captured: {photo.FileName}");
            }
            catch (FeatureNotSupportedException)
            {
                Log("Unsupported", "Take Photo", "Capture not supported on this device.");
            }
            catch (Exception ex)
            {
                Log("Error", "Take Photo", ex.Message);
            }
        }

        /// <summary>
        /// Picks a file from the device storage. Handles cancellation and errors gracefully, and logs the picked file name on success.
        /// </summary>
        /// <returns></returns>
        public async Task PickFileAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Select a file" });
                if (result is null)
                {
                    Log("Cancelled", "Pick File");
                    return;
                }

                FilePicked?.Invoke(result.FileName);
                Log("Success", "Pick File", $"Picked: {result.FileName}");
            }
            catch (Exception ex)
            {
                Log("Error", "Pick File", ex.Message);
            }
        }

        /// <summary>
        /// Get the current location of the device. Handles permissions and errors gracefully, and logs the latitude and longitude on success.
        /// </summary>
        /// <returns></returns>
        public async Task GetLocationAsync()
        {
            try
            {
                var loc = await _locationService.GetLocationAsync();
                if (loc is null)
                {
                    Log("Unavailable", "Get Location", "No location returned or permission denied.");
                    return;
                }

                var text = $"Lat {loc.Latitude:F5}, Lon {loc.Longitude:F5} · Acc {loc.Accuracy} m";
                LocationUpdated?.Invoke(text);
                Log("Success", "Get Location", $"Lat {loc.Latitude:F5}, Lon {loc.Longitude:F5}");
            }
            catch (Exception ex)
            {
                Log("Error", "Get Location", ex.Message);
            }
        }

        /// <summary>
        /// Vibrates the device for a short duration. Handles unsupported features gracefully and logs the action.
        /// </summary>
        /// <returns></returns>
        public Task VibrateAsync()
        {
            try
            {
                Vibration.Default.Vibrate(TimeSpan.FromSeconds(1));
                Log("Success", "Vibrate", "Haptic feedback triggered.");
            }
            catch (FeatureNotSupportedException)
            {
                Log("Unsupported", "Vibrate", "Vibration not supported.");
            }
            catch (Exception ex)
            {
                Log("Error", "Vibrate", ex.Message);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Shows a native toast notification with a predefined title and the provided message. Handles errors gracefully and logs the outcome.
        /// </summary>
        /// <returns></returns>
        public Task ShowToastAsync()
        {
            try
            {
                _toast.Show("Hello from native UI!"); // fire-and-forget
                Log("Success", "Toast / Alert", "System notification posted.");
            }
            catch (Exception ex)
            {
                Log("Error", "Toast / Alert", ex.Message);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Detects the device model using a platform-specific service. Logs the detected model or any errors encountered during detection.
        /// </summary>
        /// <returns></returns>
        public Task DeviceModelAsync()
        {
            var model = DeviceModelService.Model();
            Log("Success", "Device Model", model);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Orientation: Detects the current device orientation using a platform-specific service. Logs the detected orientation or any errors encountered during detection.
        /// </summary>
        /// <returns></returns>
        public Task OrientationAsync()
        {
            var value = PlatformOrientationService.GetOrientation();
            Log("Success", "Orientation", value);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Network: Detects the current network access and connection profiles using Microsoft.Maui.Networking. Logs the network status and connection type, and updates the UI via the NetworkUpdated event.
        /// </summary>
        /// <returns></returns>
        public Task NetworkAsync()
        {
            var access = Connectivity.Current.NetworkAccess;
            var profiles = Connectivity.Current.ConnectionProfiles;

            string profileText = profiles.Contains(ConnectionProfile.WiFi) ? " · Wi‑Fi" :
                                 profiles.Contains(ConnectionProfile.Cellular) ? " · Cellular" : string.Empty;

            string status = access switch
            {
                NetworkAccess.Internet => "Online",
                NetworkAccess.ConstrainedInternet => "Captive portal",
                NetworkAccess.Local => "Local only",
                NetworkAccess.None => "Offline",
                _ => "Unknown"
            };

            var text = status + (status == "Online" ? profileText : string.Empty);
            NetworkUpdated?.Invoke(text);
            Log("Info", "Network", text);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Initializes network connectivity monitoring when the view appears.
        /// </summary>
        public void OnAppearing()
        {
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
            RefreshNetworkStatus();
        }

        /// <summary>
        /// Handles cleanup operations when the associated view or component is about to disappear.
        /// </summary>
        public void OnDisappearing()
        {
            Connectivity.ConnectivityChanged -= OnConnectivityChanged;
            StopAccelerometer();
            StopCompass();
        }

        /// <summary>
        /// Refreshes the current network status and notifies subscribers of any changes.
        /// </summary>
        private void RefreshNetworkStatus()
        {
            var access = Connectivity.Current.NetworkAccess;
            var profiles = Connectivity.Current.ConnectionProfiles;

            string profileText = profiles.Contains(ConnectionProfile.WiFi) ? " · Wi‑Fi" :
                                 profiles.Contains(ConnectionProfile.Cellular) ? " · Cellular" : string.Empty;

            string status = access switch
            {
                NetworkAccess.Internet => "Online",
                NetworkAccess.ConstrainedInternet => "Captive portal",
                NetworkAccess.Local => "Local only",
                NetworkAccess.None => "Offline",
                _ => "Unknown"
            };

            var text = status + (status == "Online" ? profileText : string.Empty);
            NetworkUpdated?.Invoke(text);
            Log("Info", "Network", text);
        }

        /// <summary>
        /// On connectivity changes, refreshes the network status and logs the new status. Notifies subscribers of the change via the StateChanged event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            RefreshNetworkStatus();
            StateChanged?.Invoke(e.NetworkAccess == NetworkAccess.Internet ? "Online" : "Offline", "Network", null);
        }

        /// <summary>
        /// Starts listening for accelerometer sensor data and shake events on the device.
        /// </summary>
        public void StartAccelerometer()
        {
            if (!Accelerometer.Default.IsSupported)
            {
                Log("Unsupported", "Accelerometer", "Not supported on this device.");
                return;
            }

            Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
            Accelerometer.Default.ShakeDetected += OnShakeDetected;
            Accelerometer.Default.Start(SensorSpeed.UI);
            _isAccelerometerActive = true;
            Log("Listening", "Accelerometer", "Streaming X/Y/Z");
        }

        /// <summary>
        /// Stops the accelerometer and unsubscribes from related event handlers.
        /// </summary>
        public void StopAccelerometer()
        {
            if (!_isAccelerometerActive)
                return;

            Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
            Accelerometer.Default.ShakeDetected -= OnShakeDetected;
            Accelerometer.Default.Stop();
            _isAccelerometerActive = false;
            Log("Stopped", "Accelerometer");
        }

        /// <summary>
        /// On accelerometer reading changes, formats the X/Y/Z acceleration values and notifies subscribers via the AccelerometerReadingUpdated event. Logs the new readings in the event log.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            var a = e.Reading.Acceleration;
            var text = $"X {a.X:F2}  Y {a.Y:F2}  Z {a.Z:F2}";
            AccelerometerReadingUpdated?.Invoke(text);
        }

        /// <summary>
        /// On shake detected, notifies subscribers via the ShakeDetectedEvent and logs the shake event in the event log. This provides feedback that a shake gesture was recognized by the accelerometer sensor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShakeDetected(object? sender, EventArgs e)
        {
            ShakeDetectedEvent?.Invoke();
            Log("Success", "Shake", "User shook the device");
        }

        /// <summary>
        /// Starts listening for compass sensor data
        /// </summary>
        public void StartCompass()
        {
            if (!Compass.Default.IsSupported)
            {
                Log("Unsupported", "Compass", "Not supported on this device.");
                return;
            }

            Compass.Default.ReadingChanged += OnCompassReadingChanged;
            Compass.Default.Start(SensorSpeed.UI);
            _isCompassActive = true;
            Log("Listening", "Compass", "Heading streaming");
        }

        /// <summary>
        /// Stops the compass sensor and unsubscribes from compass reading updates.
        /// </summary>
        public void StopCompass()
        {
            if (!_isCompassActive)
                return;

            Compass.Default.ReadingChanged -= OnCompassReadingChanged;
            Compass.Default.Stop();
            _isCompassActive = false;
            Log("Stopped", "Compass");
        }

        /// <summary>
        /// On compass reading changes, formats the magnetic heading and notifies subscribers via the CompassHeadingUpdated event. Logs the new heading in the event log. This provides real-time feedback of the device's orientation relative to magnetic north.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCompassReadingChanged(object? sender, CompassChangedEventArgs e)
        {
            var heading = e.Reading.HeadingMagneticNorth;
            CompassHeadingUpdated?.Invoke($"Heading: {heading:F0}°");
        }
    }
}
