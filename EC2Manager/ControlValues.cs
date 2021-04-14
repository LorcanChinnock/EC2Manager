using System;
using System.Text.Json;

namespace EC2Manager
{
    [Serializable]
    public class ControlValues
    {
        public string InstanceId { get; init; }
        public string RegionSystemName { get; init; }
        public string AccessKey { get; init; }
        public string SecretKey { get; init; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(InstanceId))
            {
                throw new ArgumentNullException("Instance Id is required");
            }

            if (string.IsNullOrEmpty(RegionSystemName))
            {
                throw new ArgumentNullException("Region is required");
            }

            if (string.IsNullOrEmpty(AccessKey))
            {
                throw new ArgumentNullException("Access Key is required");
            }

            if (string.IsNullOrEmpty(SecretKey))
            {
                throw new ArgumentNullException("Secret Key is required");
            }
        }
    }
}