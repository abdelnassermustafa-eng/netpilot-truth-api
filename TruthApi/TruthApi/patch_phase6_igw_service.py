from pathlib import Path

svc = Path("services/AwsEc2Service.cs")
text = svc.read_text()

if "GetInternetGatewayResourceRowsAsync" in text:
    print("IGW methods already exist. No changes made.")
    exit()

insert_block = """
        // ===============================
        // Internet Gateways
        // ===============================
        public async Task<List<InternetGateway>> GetInternetGatewaysAsync()
        {
            var response = await _ec2Client.DescribeInternetGatewaysAsync(
                new DescribeInternetGatewaysRequest());

            return response.InternetGateways ?? new List<InternetGateway>();
        }

        public async Task<List<InternetGatewayResourceRow>> GetInternetGatewayResourceRowsAsync()
        {
            var igws = await GetInternetGatewaysAsync();
            var rows = new List<InternetGatewayResourceRow>();

            foreach (var igw in igws)
            {
                var attachedVpcs = igw.Attachments
                    .Where(a => !string.IsNullOrEmpty(a.VpcId))
                    .Select(a => a.VpcId);

                var state = igw.Attachments.FirstOrDefault()?.State?.Value ?? "detached";

                rows.Add(new InternetGatewayResourceRow
                {
                    IgwId = igw.InternetGatewayId ?? "",
                    AttachedVpcIds = string.Join(",", attachedVpcs),
                    AttachmentState = state,
                    Region = Region
                });
            }

            return rows;
        }
"""

# insert before final closing brace of class
idx = text.rfind("}")
new_text = text[:idx] + insert_block + "\n" + text[idx:]

svc.write_text(new_text)
print("IGW service methods added.")
