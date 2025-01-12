using System;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;

namespace StarPointApi.Shared
{
    public class AzureKeyVault
    {
        public static async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient =
                    new KeyVaultClient(
                        new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secret = await keyVaultClient
                    .GetSecretAsync($"https://BoostAppKeyVault.vault.azure.net/secrets/{secretName}")
                    .ConfigureAwait(false);

                return secret.Value;
            }
            catch (KeyVaultErrorException e)
            {
                throw new Exception(e.Message);
            }
        }

        public static string GetSecret(string secretName)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    return await GetSecretAsync(secretName);
                }
                catch (KeyVaultErrorException e)
                {
                    throw new Exception(e.Message);
                }
            });
            return task.Result;
        }
    }
}