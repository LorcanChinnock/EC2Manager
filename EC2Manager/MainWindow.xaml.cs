using Amazon;
using System;
using System.Drawing;
using System.Linq;
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

        private delegate void StartDelegate();
        private delegate void StopDelegate();
        private delegate void RestartDelegate();

        public MainWindow()
        {
            ec2Accessor = new EC2Accessor();
            controlPersistence = new ControlPersistence<ControlValues>();
            InitializeComponent();
            SetConsoleOutputter();
            SetRegionControlOptions();
            LoadControlValuesIfAvailable();
            SetupStatusTimer();
        }

        private void SetConsoleOutputter()
        {
            var outputter = new TextBoxOutputter(ConsoleOutput);
            Console.SetOut(outputter);
        }

        #region click handlers
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Dispatcher.BeginInvoke(DispatcherPriority.Normal,new StartDelegate(() => {
                try
                {
                    var controlValues = GetControlValuesFromControls();
                    ec2Accessor.StartEc2InstanceAndLog(controlValues);
                    StatusDispatchAction(this, null);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            }));
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StopButton.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new StopDelegate(() => {
                try
                {
                    var controlValues = GetControlValuesFromControls();
                    ec2Accessor.StopEc2InstanceAndLog(controlValues);
                    StatusDispatchAction(this, null);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            }));
        }
        #endregion

        #region status polling
        private void SetupStatusTimer()
        {
            statusTimer = new DispatcherTimer();
            statusTimer.Tick += new EventHandler(StatusDispatchAction);
            statusTimer.Interval = new TimeSpan(0, 0, 10);
            statusTimer.Start();
        }

        private void StatusDispatchAction(object sender, EventArgs e)
        {
            var controlValues = GetControlValuesFromControls();
            LoadingSpinner.Visibility = Visibility.Visible;
            var result = ec2Accessor.GetInstanceState(controlValues);
            LoadingSpinner.Visibility = Visibility.Hidden;
            SetIndicatorColorFromInstanceState(result);
        }

        private void SetIndicatorColorFromInstanceState(InstanceState instanceState)
        {
            var brush = new SolidColorBrush();
            brush.Color = instanceState switch
            {
                InstanceState.Pending or 
                InstanceState.ShuttingDown or 
                InstanceState.Stopping => Color.FromRgb(255, 255, 0),
                InstanceState.Running => Color.FromRgb(0, 255, 0),
                _ => Color.FromRgb(255, 0, 0),
            };
            InstanceStateIndicator.Fill = brush;
        }
        #endregion

        #region control value stuff
        private void SetControlFromControlValues(ControlValues controlValues)
        {
            InstanceID.Text = controlValues.InstanceId;
            Region.SelectedItem = ec2Accessor.GetRegionBySystemName(controlValues.RegionSystemName);
            AccessKey.Text = controlValues.AccessKey;
            SecretKey.Text = controlValues.SecretKey;
        }

        private ControlValues GetControlValuesFromControls()
        {
            var controlValues = new ControlValues
            {
                InstanceId = InstanceID.Text.Trim(),
                RegionSystemName = ((RegionEndpoint)Region.SelectedItem).SystemName,
                AccessKey = AccessKey.Text.Trim(),
                SecretKey = SecretKey.Text.Trim()
            };
            controlValues.Validate();
            controlPersistence.Write(controlValues);
            return controlValues;
        }

        private void LoadControlValuesIfAvailable()
        {
            var controlValues = controlPersistence.Read();
            if (controlValues != null)
            {
                SetControlFromControlValues(controlValues);
            }
            else
            {
                Region.SelectedIndex = 0;
            }
        }

        private void SetRegionControlOptions()
        {
            var allAvailableRegions = ec2Accessor.availableRegions;
            allAvailableRegions.ToList().ForEach(region =>
            {
                Region.Items.Add(region);
            });
        }
        #endregion
    }
}