using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using System;
using System.Collections.Generic;

namespace EC2Manager
{
    internal class EC2Accessor
    {
        public IEnumerable<RegionEndpoint> availableRegions;

        public EC2Accessor()
        {
            availableRegions = RegionEndpoint.EnumerableAllRegions;
        }

        public void StartEc2InstanceAndLog(ControlValues controlValues)
        {
            var ec2Client = GetEc2ClientWithControlValues(controlValues);
            Console.WriteLine($"Trying to start EC2 instance {controlValues.InstanceId}");
            StartEc2Instance(ec2Client, controlValues.InstanceId);
            Console.WriteLine($"Instance {controlValues.InstanceId} started successfully");
        }

        public void StopEc2InstanceAndLog(ControlValues controlValues)
        {
            var ec2Client = GetEc2ClientWithControlValues(controlValues);
            Console.WriteLine($"Trying to stop EC2 instance {controlValues.InstanceId}");
            StopEc2Instance(ec2Client, controlValues.InstanceId);
            Console.WriteLine($"Instance {controlValues.InstanceId} stopped successfully");
        }

        public InstanceState GetInstanceState(ControlValues controlValues)
        {
            var ec2Client = GetEc2ClientWithControlValues(controlValues);
            Console.WriteLine($"Trying to get EC2 instance {controlValues.InstanceId} state");
            var request = new DescribeInstanceStatusRequest
            {
                IncludeAllInstances = true,
                InstanceIds = new List<string> { controlValues.InstanceId }
            };
            var result = ec2Client.DescribeInstanceStatusAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
            var instanceState = (InstanceState)result.InstanceStatuses[0].InstanceState.Code;
            Console.WriteLine($"Instance {controlValues.InstanceId} state: {instanceState}");
            return instanceState;
        }

        private void StartEc2Instance(AmazonEC2Client client, string instanceId)
        {
            client.StartInstancesAsync(new StartInstancesRequest
            {
                InstanceIds = new List<string> { instanceId }
            }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public RegionEndpoint GetRegionBySystemName(string regionSystemName) => RegionEndpoint.GetBySystemName(regionSystemName);

        private void StopEc2Instance(AmazonEC2Client client, string instanceId)
        {
            client.StopInstancesAsync(new StopInstancesRequest
            {
                InstanceIds = new List<string> { instanceId }
            }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private AmazonEC2Client GetEc2ClientWithControlValues(ControlValues controlValues) => new AmazonEC2Client(controlValues.AccessKey, controlValues.SecretKey, GetRegionBySystemName(controlValues.RegionSystemName));
    }
}