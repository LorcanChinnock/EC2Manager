using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EC2Manager
{
    internal class EC2Accessor
    {
        public IEnumerable<RegionEndpoint> availableRegions;

        public EC2Accessor()
        {
            availableRegions = RegionEndpoint.EnumerableAllRegions;
        }

        public async Task StartEc2InstanceAndLog(ControlValues controlValues)
        {
            var ec2Client = GetEc2ClientWithControlValues(controlValues);
            Console.WriteLine($"Trying to start EC2 instance with params:\n{controlValues}");
            await StartEc2Instance(ec2Client, controlValues.InstanceId);
            Console.WriteLine("Instance started successfully");
        }

        public async Task StopEc2InstanceAndLog(ControlValues controlValues)
        {
            var ec2Client = GetEc2ClientWithControlValues(controlValues);
            Console.WriteLine($"Trying to stop EC2 instance with params:\n{controlValues}");
            await StopEc2Instance(ec2Client, controlValues.InstanceId);
            Console.WriteLine("Instance stopped successfully");
        }

        public async Task RestartEc2InstanceAndLog(ControlValues controlValues)
        {
            var ec2Client = GetEc2ClientWithControlValues(controlValues);
            Console.WriteLine($"Trying to restart EC2 instance with params:\n{controlValues}");
            await StopEc2Instance(ec2Client, controlValues.InstanceId).ContinueWith(_ => StartEc2Instance(ec2Client, controlValues.InstanceId));
            Console.WriteLine("Instance restarted successfully");
        }

        private async Task StartEc2Instance(AmazonEC2Client client, string instanceId)
        {
            await client.StartInstancesAsync(new StartInstancesRequest
            {
                InstanceIds = new List<string> { instanceId }
            });
        }

        public RegionEndpoint GetRegionBySystemName(string regionSystemName) => RegionEndpoint.GetBySystemName(regionSystemName);

        private async Task StopEc2Instance(AmazonEC2Client client, string instanceId)
        {
            await client.StopInstancesAsync(new StopInstancesRequest
            {
                InstanceIds = new List<string> { instanceId }
            });
        }

        private AmazonEC2Client GetEc2ClientWithControlValues(ControlValues controlValues) => new AmazonEC2Client(controlValues.AccessKey, controlValues.SecretKey, GetRegionBySystemName(controlValues.RegionSystemName));
    }
}