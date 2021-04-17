using Amazon;
using EC2Manager.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;

namespace EC2Manager
{
    public partial class MainWindow : Window
    {
        private EC2Accessor ec2Accessor;
        private ControlPersistence<ControlValues> controlPersistence;
        private DispatcherTimer statusTimer;

        public MainWindow()
        {
            InitializeComponent();
            Window.Dispatcher.InvokeAsync(async () =>
            {
                await WindowInitializedAsync();
            });
        }

        private async Task WindowInitializedAsync()
        {
            ec2Accessor = new EC2Accessor();
            controlPersistence = new ControlPersistence<ControlValues>();
            ConsoleOutput.SetAsConsoleOutput();
            Region.AddItems(EC2Accessor.AvailableRegions);
            await LoadControlValuesIfAvailableAsync();
            await StartStatusTimerAsync();
        }

        #region click handlers

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    var controlValues = await GetControlValuesFromControlsAsync();
                    await ec2Accessor.StartEc2InstanceAsync(controlValues);
                    await StartStatusTimerAsync();
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            }, DispatcherPriority.Normal);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StopButton.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    var controlValues = await GetControlValuesFromControlsAsync();
                    await ec2Accessor.StopEc2InstanceAsync(controlValues);
                    await StartStatusTimerAsync();
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            }, DispatcherPriority.Normal);
        }

        #endregion click handlers

        #region status polling

        private async Task StartStatusTimerAsync()
        {
            await GetStatusAndIp(this, null);
            statusTimer = new DispatcherTimer();
            statusTimer.Tick += async (s, e) => { await GetStatusAndIp(s, e); };
            statusTimer.Interval = new TimeSpan(0, 0, 30);
            statusTimer.Start();
        }

        private async Task GetStatusAndIp(object sender, EventArgs e)
        {
            var controlValues = await GetControlValuesFromControlsAsync();
            var result = await ec2Accessor.GetInstanceStateAsyncAndLog(controlValues);
            SetIndicatorColorFromInstanceState(result);
            SetLoadingSpinnerVisibility(result);

            if (result == InstanceState.Running)
            {
                await ec2Accessor.LogPublicIpAsync(controlValues);
            }
        }

        private void SetLoadingSpinnerVisibility(InstanceState result)
        {
            if (result == InstanceState.Pending || result == InstanceState.ShuttingDown || result == InstanceState.Stopping)
            {
                LoadingSpinner.Visibility = Visibility.Visible;
            }
            else
            {
                LoadingSpinner.Visibility = Visibility.Hidden;
            }
        }

        private void SetIndicatorColorFromInstanceState(InstanceState instanceState)
        {
            var brush = new SolidColorBrush
            {
                Color = instanceState switch
                {
                    InstanceState.Pending or
                    InstanceState.ShuttingDown or
                    InstanceState.Stopping => Color.FromRgb(255, 255, 0),
                    InstanceState.Running => Color.FromRgb(0, 255, 0),
                    _ => Color.FromRgb(255, 0, 0),
                }
            };
            InstanceStateIndicator.Fill = brush;
        }

        #endregion status polling

        #region control value stuff

        private void SetControlFromControlValues(ControlValues controlValues)
        {
            InstanceID.Text = controlValues.InstanceId;
            Region.SelectedItem = ec2Accessor.GetRegionBySystemName(controlValues.RegionSystemName);
            AccessKey.Text = controlValues.AccessKey;
            SecretKey.Text = controlValues.SecretKey;
        }

        private async Task<ControlValues> GetControlValuesFromControlsAsync()
        {
            var controlValues = new ControlValues
            {
                InstanceId = InstanceID.Text.Trim(),
                RegionSystemName = ((RegionEndpoint)Region.SelectedItem).SystemName,
                AccessKey = AccessKey.Text.Trim(),
                SecretKey = SecretKey.Text.Trim()
            };
            controlValues.Validate();
            await controlPersistence.Write(controlValues);
            return controlValues;
        }

        private async Task LoadControlValuesIfAvailableAsync()
        {
            var controlValues = await controlPersistence.Read();
            if (controlValues != null)
            {
                SetControlFromControlValues(controlValues);
            }
            else
            {
                Region.SelectedIndex = 0;
            }
        }

        #endregion control value stuff
    }
}