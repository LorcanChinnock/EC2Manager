using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EC2Manager
{
    public class EC2Accessor
    {
        public static IEnumerable<RegionEndpoint> AvailableRegions => RegionEndpoint.EnumerableAllRegions;

        private string lastConsoleMessage;

        public async Task StartEc2InstanceAsync(ControlValues controlValues)
        {
            using var ec2Client = GetEc2ClientWithControlValues(controlValues);
            await ec2Client.StartInstancesAsync(new StartInstancesRequest
            {
                InstanceIds = new List<string> { controlValues.InstanceId }
            });
        }

        public async Task StopEc2InstanceAsync(ControlValues controlValues)
        {
            using var ec2Client = GetEc2ClientWithControlValues(controlValues);
            await ec2Client.StopInstancesAsync(new StopInstancesRequest
            {
                InstanceIds = new List<string> { controlValues.InstanceId }
            });
        }

        public async Task<InstanceState> GetInstanceStateAsyncAndLog(ControlValues controlValues)
        {
            var instanceState = await GetInstanceStateAsync(controlValues);
            LogIfDifferent($"Instance {controlValues.InstanceId} state: {instanceState}");
            return instanceState;
        }

        public async Task LogPublicIpAsync(ControlValues controlValues)
        {
            await GetPublicIpAsync(controlValues).ContinueWith(async ip =>
            {
                LogIfDifferent($"Public ip is: {await ip}");
            });
        }

        private async Task<string> GetPublicIpAsync(ControlValues controlValues)
        {
            var networkInterfaceId = await GetFirstNetworkInterfaceOfInstanceAsync(controlValues);
            var ip = await GetPublicIpv4OfNetworkInterfaceAsync(controlValues, networkInterfaceId);
            return ip;
        }

        private async Task<InstanceState> GetInstanceStateAsync(ControlValues controlValues)
        {
            using var ec2Client = GetEc2ClientWithControlValues(controlValues);
            var request = new DescribeInstanceStatusRequest
            {
                IncludeAllInstances = true,
                InstanceIds = new List<string> { controlValues.InstanceId }
            };
            var result = await ec2Client.DescribeInstanceStatusAsync(request);
            var instanceState = (InstanceState)result.InstanceStatuses[0].InstanceState.Code;
            return instanceState;
        }

        private async Task<string> GetPublicIpv4OfNetworkInterfaceAsync(ControlValues controlValues, string networkInterfaceId)
        {
            using var ec2Client = GetEc2ClientWithControlValues(controlValues);
            var result = await ec2Client.DescribeNetworkInterfacesAsync(new DescribeNetworkInterfacesRequest
            {
                NetworkInterfaceIds = new List<string> { networkInterfaceId }
            });

            var ip = result.NetworkInterfaces[0].Association.PublicIp;
            return ip;
        }

        private async Task<string> GetFirstNetworkInterfaceOfInstanceAsync(ControlValues controlValues)
        {
            using var ec2Client = GetEc2ClientWithControlValues(controlValues);
            var instance = await ec2Client.DescribeInstancesAsync(new DescribeInstancesRequest
            {
                InstanceIds = new List<string> { controlValues.InstanceId }
            });
            var networkInterfaceId = instance.Reservations[0].Instances[0].NetworkInterfaces[0].NetworkInterfaceId;
            return networkInterfaceId;
        }

        public RegionEndpoint GetRegionBySystemName(string regionSystemName) =>
            RegionEndpoint.GetBySystemName(regionSystemName);

        private AmazonEC2Client GetEc2ClientWithControlValues(ControlValues controlValues) =>
            new(controlValues.AccessKey, controlValues.SecretKey, GetRegionBySystemName(controlValues.RegionSystemName));

        private void LogIfDifferent(string message)
        {
            if (lastConsoleMessage != message)
            {
                Console.WriteLine(message);
                lastConsoleMessage = message;
            }
        }
    }
}