using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace BoostApp.Shared
{
    public static class X509CertHelper
    {
        /// <summary>
        /// Fetch a X.509 certificate from the current user's certificate store.
        /// </summary>
        /// <param name="certificateName">Name of certificate</param>
        public static X509Certificate2 FromCurrentUser(string certificateName)
        {
            if (string.IsNullOrWhiteSpace(certificateName))
                throw new ArgumentException("CertificateName was empty.", "certificateName");

            X509Certificate2 cert = null;

            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = store.Certificates;

                // Find unexpired certificates.
                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                // From the collection of unexpired certificates, find the ones with the correct name.
                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certificateName, false);

                // Return the first certificate in the collection, has the right name and is current.
                cert = signingCert.OfType<X509Certificate2>().OrderByDescending(c => c.NotBefore).FirstOrDefault();
            }
            return cert;
        }
    }
}
