using System;
using System.Windows;
using System.Linq;

namespace EC2Manager
{
    public partial class MainWindow : Window
    {
        private EC2Accessor ec2Accessor;
        public MainWindow()
        {
            InitializeComponent();
            ec2Accessor = new EC2Accessor();

            SetConsoleOutputter();
            SetRegionOptions();
        }

        private void SetConsoleOutputter()
        {
            var outputter = new TextBoxOutputter(ConsoleOutput);
            Console.SetOut(outputter);
        }

        private void SetRegionOptions()
        {
            var allAvailableRegions = ec2Accessor.availableRegions;
            allAvailableRegions.ToList().ForEach(region =>
            {
                Region.Items.Add(region);
            });

            Region.SelectedIndex = 0;
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var controlValues = GetControlValues();
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
                var controlValues = GetControlValues();
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
                var controlValues = GetControlValues();
                await ec2Accessor.RestartEc2InstanceAndLog(controlValues);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }

        private ControlValues GetControlValues()
        {
            var controlValues = new ControlValues(InstanceID.Text.Trim(),
                Region.SelectedItem,
                AccessKey.Text.Trim(),
                SecretKey.Text.Trim());
            return controlValues;
        }
    }
}
