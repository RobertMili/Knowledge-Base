using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//namespace MembershipAPI.Services
//{
//    public class KeyVaultService
//    {
//        public static async Task<string> GetSecretAsync(string secretName)
//        {
//            try
//            {
//                var azureServiceTokenProvider = new AzureServiceTokenProvider();
//                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
//                var secret = await keyVaultClient.GetSecretAsync($"https://BoostAppKeyVault.vault.azure.net/secrets/{secretName}").ConfigureAwait(false);

//                return secret.Value;
//            }
//            catch (KeyVaultErrorException e)
//            {
//                throw new Exception(e.Message);
//            }
//        }
//        public static string GetSecret(string secretName)
//        {
//            Task<string> task = Task.Run(async () =>
//            {
//                try
//                {
//                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
//                    var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
//                    var secret = await keyVaultClient.GetSecretAsync($"https://BoostAppKeyVault.vault.azure.net/secrets/{secretName}").ConfigureAwait(false);

//                    return secret.Value;
//                }
//                catch (KeyVaultErrorException e)
//                {
//                    throw new Exception(e.Message);
//                }
//            });
//            return task.Result;
//        }
//    }
//}
