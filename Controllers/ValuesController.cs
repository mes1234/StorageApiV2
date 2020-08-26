using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage;
using Azure.Storage.Sas;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Logging;

namespace StorageApiV2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {


        private ILogger _logger = null;

        public ValuesController(ILogger<ValuesController> logger)
        {
            this._logger = logger;
        }
        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {

            this._logger.LogInformation("Hello World!");
            Console.WriteLine("Hello World!");

            // Construct the blob endpoint from the account name.
            string accName = "wk12345storage";
            string blobEndpoint = string.Format("https://{0}.blob.core.windows.net", accName);
            this._logger.LogInformation("Setup a blob endpoint");
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();//("RunAs = App");
            this._logger.LogInformation("Created Token Provider");
            KeyVaultClient kvc = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            this._logger.LogInformation("Created KeyValutAccess");
            string kvadd = "https://mykeyswitek123456.vault.azure.net/";

            var secret = await kvc.GetSecretAsync(kvadd, "primaryKey");
            this._logger.LogInformation("Got Key");

            StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(accName, secret.Value);

            // Create a SAS token that's valid for one hour.
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = "test",
                BlobName = "hello.txt",
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddHours(-60),
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(60)
            };

            // Specify read permissions for the SAS.
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Use the key to get the SAS token.
            string sasToken = sasBuilder.ToSasQueryParameters(storageCredentials).ToString();
            //string sasToken = sasBuilder.ToSasQueryParameters(sas,accName).ToString();

            // Construct the full URI, including the SAS token.
            UriBuilder fullUri = new UriBuilder()
            {
                Scheme = "https",
                Host = string.Format("{0}.blob.core.windows.net", accName),
                Path = string.Format("{0}/{1}", "test", "hello.txt"),
                Query = sasToken
            };
            this._logger .LogInformation($"Your uri is {fullUri}");
            return Ok(fullUri.ToString());
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
