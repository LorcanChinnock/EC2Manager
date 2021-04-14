using Amazon;
using System;
using System.Text.Json;

namespace EC2Manager
{
    public class ControlValues
    {
        public string InstanceId { get; init; }
        public object Region { get; init; }
        public string AccessKey { get; init; }
        public string SecretKey { get; init; }

        public ControlValues(string instanceId, object region, string accessKey, string secretKey)
        {
            InstanceId = instanceId;
            Region = region;
            AccessKey = accessKey;
            SecretKey = secretKey;

            Validate();
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        private void Validate()
        {
            if (string.IsNullOrEmpty(InstanceId))
            {
                throw new ArgumentNullException("Instance Id is required");
            }

            if(Region == null)
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
