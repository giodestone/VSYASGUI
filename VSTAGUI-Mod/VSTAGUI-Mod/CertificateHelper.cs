using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace VSYASGUI_Mod
{
    /// <summary>
    /// Provides helper classes to deal with HTTPS certificates (<see cref="X509Certificate2"/>).
    /// </summary>
    internal static class CertificateHelper
    {
            
        public static bool DoesPrivateCertificateExist(Config config)
        {
            return File.Exists(config.HttpsPrivateCertificateFileName);
        }

        public static bool DoesPublicCertificateExist(Config config)
        {
            return File.Exists(config.HttpsPublicCertificateFileName);
        }

        public static bool IsCertificateExpired(X509Certificate2 certificate, int daysToSubtract=0)
        {
            return  DateTime.Now - TimeSpan.FromDays(-daysToSubtract) >= certificate.NotAfter;
        }

        public static X509Certificate2 LoadPublicCertificate(Config config)
        {
            return new X509Certificate2(config.HttpsPublicCertificateFileName);
        }

        public static void RenamePreviousCertificates(Config config)
        {
            if (DoesPrivateCertificateExist(config))
                File.Move(config.HttpsPrivateCertificateFileName, config.HttpsPrivateCertificateFileName + "_expired_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
            if (DoesPublicCertificateExist(config))
                File.Move(config.HttpsPublicCertificateFileName, config.HttpsPublicCertificateFileName + "_expired_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
        }

        public static X509Certificate2 MakeAndSaveCert(Config config)
        {
            const string CRT_HEADER = "-----BEGIN CERTIFICATE-----\n";
            const string CRT_FOOTER = "\n-----END CERTIFICATE-----";

            const string KEY_HEADER = "-----BEGIN RSA PRIVATE KEY-----\n";
            const string KEY_FOOTER = "\n-----END RSA PRIVATE KEY-----";

            using var rsa = RSA.Create();
            var certRequest = new CertificateRequest("cn=test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            // We're just going to create a temporary certificate, that won't be valid for long
            var certificate = certRequest.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(config.HttpsDefaultKeyDurationDays));

            // export the private key
            var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks);

            File.WriteAllText(config.HttpsPrivateCertificateFileName, KEY_HEADER + privateKey + KEY_FOOTER);

            // Export the certificate
            var exportData = certificate.Export(X509ContentType.Cert);

            var crt = Convert.ToBase64String(exportData, Base64FormattingOptions.InsertLineBreaks);
            File.WriteAllText(config.HttpsPublicCertificateFileName, CRT_HEADER + crt + CRT_FOOTER);

            return certificate;
        }
    }
}
