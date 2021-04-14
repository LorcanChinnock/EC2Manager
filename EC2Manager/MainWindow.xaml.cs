using Amazon;
using System;
using System.Linq;
using System.Windows;

namespace EC2Manager
{
    public partial class MainWindow : Window
    {
        private EC2Accessor ec2Accessor;
        private ControlPersistence<ControlValues> controlPersistence;

        public MainWindow()
        {
            ec2Accessor = new EC2Accessor();
            controlPersistence = new ControlPersistence<ControlValues>();
            InitializeComponent();
            SetConsoleOutputter();
            SetRegionControlOptions();
            LoadControlValuesIfAvailable();
        }

        private void SetConsoleOutputter()
        {
            var outputter = new TextBoxOutputter(ConsoleOutput);
            Console.SetOut(outputter);
        }

        #region click handlers
        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var controlValues = GetControlValuesFromControls();
                await ec2Accessor.StartEc2InstanceAndLog(controlValues);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }

        private async void Stop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var controlValues = GetControlValuesFromControls();
                await ec2Accessor.StopEc2InstanceAndLog(controlValues);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }

        private async void Restart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var controlValues = GetControlValuesFromControls();
                await ec2Accessor.RestartEc2InstanceAndLog(controlValues);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
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