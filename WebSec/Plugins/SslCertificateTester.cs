//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using Common;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.Logger;
    using Library.PluginBase;

    /// <summary>
    /// Ssl certificate tester. Finding issues in certificates.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Test, Name = "SSL certificate",
        Description =
            "Verifies the SSL certificate presented by the server is not about to expire, has not been revoked," + 
             " was issued by an approved root CA and was delivered using a strong algorithm.")]
    public sealed class SslCertificateTester : PluginBaseAbstract
    {
        /// <summary>
        /// The minimum number of bits for an acceptable key exchange.
        /// </summary>
        public const int MinimumKeyExchangeStrength = 2048;

        /// <summary>
        /// The minimum number of bits for an acceptable cipher strength.
        /// </summary>
        public const int MinimumCipherStrength = 128;

        /// <summary>
        /// The number of days until a certificate expires.
        /// </summary>
        public const int CertificateExpirationThresholdInDays = 21;

        /// <summary>
        /// Certificates should not expire beyond this number of years in the future.
        /// </summary>
        public const int MaximumCertifcateExpiryPeriodInYears = 5;

        /// <summary>
        /// Non compliant cipher algorithms for SSL usage.
        /// </summary>
        public readonly IList<CipherAlgorithmType> CipherAlgorithmBlacklist =
            new ReadOnlyCollection<CipherAlgorithmType>(new[]
            {
                CipherAlgorithmType.None,
                CipherAlgorithmType.Des,
                CipherAlgorithmType.Rc2,
                CipherAlgorithmType.Null
            });

        /// <summary>
        /// Accepted cipher algorithms for SSL usage.
        /// </summary>
        public readonly IList<CipherAlgorithmType> CipherAlgorithmWhitelist;

        /// <summary>
        /// Accepted root CAs.
        /// </summary>
        public readonly string[] RootCertificateAuthorityWhitelist =
        {
            "VeriSign Class 3 Public Primary Certification Authority - G5",
            "Baltimore CyberTrust Root",
            "AddTrust External CA Root" // COMODO
        };

        /// <summary>
        /// Accepted cipher algorithms for SSL usage.
        /// </summary>
        public readonly IList<SslProtocols> SslProtocolWhitelist = new ReadOnlyCollection<SslProtocols>(new[]
        {
            SslProtocols.Ssl3,
            SslProtocols.Tls
        });

        /// <summary>
        /// Initializes a new instance of the SslCertificateTester class.
        /// </summary>
        public SslCertificateTester()
        {
            // Automatically add new algos as .Net makes them available.
            this.CipherAlgorithmWhitelist =
                new ReadOnlyCollection<CipherAlgorithmType>(
                    Enum.GetValues(typeof(CipherAlgorithmType))
                        .Cast<CipherAlgorithmType>()
                        .Except(this.CipherAlgorithmBlacklist).ToList());
        }

        /// <inheritdoc/>
        public override void Init(IContext currentcontext, ITarget target)
        {
            base.Init(currentcontext, target);
        }

        /// <inheritdoc/>
        public override void DoTests()
        {
            Logger.WriteInfo(
                "Starting SSL tests for {0} on port {1}.", 
                HostNameFormatted(this.TestTarget),
                TestTarget.Uri.Port);

            try
            {
                if (TestTarget.Uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase))
                {
                    string server = TestTarget.Uri.DnsSafeHost;

                    var tcp = new TcpClient(server, TestTarget.Uri.Port);

                    try
                    {
                        using (
                            var ssl = new SslStream(
                                tcp.GetStream(),
                                false,
                                this.UserCertificateValidationCallback,
                                null,
                                EncryptionPolicy.AllowNoEncryption))
                        {
                            ssl.AuthenticateAsClient(
                                server,
                                null,
                                SslProtocols.Default,
                                checkCertificateRevocation: true);

                            this.VerifyChannelSecurity(ssl);
                        }
                    }
                    catch (AuthenticationException ex)
                    {
                        Logger.WriteError(ex);
                    }
                    catch (IOException ex)
                    {
                        Logger.WriteError(ex);
                    }

                    this.VerifySslProtocolBlacklist(server, TestTarget.Uri.Port);
                }
                else
                {
                    Logger.WriteInfo(
                        "Host {0} does not use https and will be ignored.",
                        HostNameFormatted(this.TestTarget));
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex);
            }
        }

        /// <summary>
        /// Host name formatted.
        /// </summary>
        /// <param name="target">
        ///     Target for the.
        /// </param>
        /// <returns>
        /// A string.
        /// </returns>
        private static string HostNameFormatted(ITarget target)
        {
            return $"{target.Uri.Scheme}://{target.Uri.DnsSafeHost}";
        }

        /// <summary>
        /// Gets common name.
        /// </summary>
        /// <param name="distinquishedName">
        ///     Name of the distinguished.
        /// </param>
        /// <returns>
        /// The common name.
        /// </returns>
        private static string GetCommonName(string distinquishedName)
        {
            const string Cn = "CN=";

            if (distinquishedName.StartsWith(Cn, StringComparison.InvariantCultureIgnoreCase) &&
                distinquishedName.IndexOf(',') >= 3)
            {
                return distinquishedName.Substring(Cn.Length, distinquishedName.IndexOf(',') - Cn.Length);
            }

            return distinquishedName;
        }

        /// <summary>
        /// Verify channel security.
        /// </summary>
        /// <param name="ssl">
        ///     The ssl.
        /// </param>
        private void VerifyChannelSecurity(SslStream ssl)
        {
            Logger.WriteInfo("Verifying channel security for {0}.", HostNameFormatted(this.TestTarget));

            // must be SSL3 or TLS1
            if (!this.SslProtocolWhitelist.Contains(ssl.SslProtocol))
            {
                string allowedProtocols = string.Join(
                    ", ",
                    this.SslProtocolWhitelist.Select(s => s.ToString()).ToArray());

                this.CreateVuln(
                    $"SSL protocol reports to be {ssl.SslProtocol}, acceptable values are {allowedProtocols}.");
            }

            // data must be encrypted
            if (!ssl.IsEncrypted)
            {
                this.CreateVuln("SSL channel is not encrypting data.");
            }

            // data must be signed
            if (!ssl.IsSigned)
            {
                this.CreateVuln("SSL channel is not signing data.");
            }

            // Weak cipher strength
            if (ssl.CipherStrength < MinimumCipherStrength)
            {
                this.CreateVuln(
                    $"SSL cipher strength is {ssl.KeyExchangeStrength} bits, the minimum accepted size is {MinimumCipherStrength} bits.");
            }

            // Weak cipher algo
            if (!this.CipherAlgorithmWhitelist.Contains(ssl.CipherAlgorithm))
            {
                string allowedCiphers = string.Join(
                    ", ",
                    this.CipherAlgorithmWhitelist.Select(s => s.ToString()).ToArray());

                this.CreateVuln(
                    $"SSL cipher algorithm is {ssl.KeyExchangeStrength}, acceptable values are {allowedCiphers}.");
            }
        }

        /// <summary>
        /// Callback, called when the user certificate validation.
        /// </summary>
        /// <param name="sender">
        ///     Source of the event.
        /// </param>
        /// <param name="certificate">
        ///     The certificate.
        /// </param>
        /// <param name="chain">
        ///     The chain.
        /// </param>
        /// <param name="sslPolicyErrors">
        ///     The ssl policy errors.
        /// </param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private bool UserCertificateValidationCallback(
            object sender, 
            X509Certificate certificate, 
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            Logger.WriteInfo("Verifying certificate chain for {0}.", HostNameFormatted(this.TestTarget));

            var dateNow = DateTime.Now;

            var certificate2 = certificate as X509Certificate2;
            if (certificate2 != null)
            {
                // shipping a private key
                if (certificate2.HasPrivateKey)
                {
                    this.CreateVuln($"certificate with thumbprint '{certificate2.Thumbprint}' contains a private key!");
                }

                // certificate expires soon
                var expiresInDays = certificate2.NotAfter.Subtract(dateNow).Days;
                if (expiresInDays >= 0 && expiresInDays <= CertificateExpirationThresholdInDays)
                {
                    this.CreateVuln(
                        $"certificate expires in {expiresInDays} days on {certificate2.NotAfter.ToShortDateString()}");
                }

                // certificate already expired
                if (expiresInDays < 0)
                {
                    this.CreateVuln($"certificate expired on {certificate2.NotAfter.ToShortDateString()}");
                }

                // certificate expiry too far in the future
                if (expiresInDays >= MaximumCertifcateExpiryPeriodInYears * dateNow.DaysInYear())
                {
                    this.CreateVuln(
                        "certificate expiry is greater than {1} years out ({0})".FormatIc(
                            certificate2.NotAfter.ToShortDateString(), MaximumCertifcateExpiryPeriodInYears));
                }

                // certificate is not valid for use yet based on date
                if (certificate2.NotBefore > dateNow)
                {
                    this.CreateVuln(
                        "certificate cannot be used before {0}".FormatIc(certificate2.NotBefore.ToShortDateString()));
                }

                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) ==
                    SslPolicyErrors.RemoteCertificateNameMismatch)
                {
                    this.CreateVuln("certificate name does not match the site that supplied the certificate.");
                }

                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) ==
                    SslPolicyErrors.RemoteCertificateNotAvailable)
                {
                    this.CreateVuln("certificate not available.");
                }

                // root CA is not recognized
                var rootAuthority = chain.ChainElements[chain.ChainElements.Count - 1];
                if (rootAuthority != null && rootAuthority.Certificate != null &&
                    rootAuthority.Certificate.IssuerName.Name != null)
                {
                    string cn = GetCommonName(rootAuthority.Certificate.IssuerName.Name);

                    if (!this.RootCertificateAuthorityWhitelist.Any(cn.StartsWithOi))
                    {
                        this.CreateVuln(
                            "root certificate authority \"{0}\" is not on the approved list.".FormatIc(
                                cn));
                    }
                }

                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) ==
                    SslPolicyErrors.RemoteCertificateChainErrors)
                {
                    // Look for certificate chain problems.
                    foreach (X509ChainElement chainElement in chain.ChainElements as IEnumerable)
                    {
                        // this will report X509ChainStatusFlags.Revoked for CRL revocation, among other issues.
                        foreach (
                            var status in
                                chainElement.ChainElementStatus.Where(s => s.Status != X509ChainStatusFlags.NoError))
                        {
                            this.CreateVuln(
                                "Certificate in chain with thumbprint {0} has a problem - ({1}) {2}".FormatIc(
                                    chainElement.Certificate.Thumbprint,
                                    status.Status,
                                    status.StatusInformation));
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Verify ssl protocol blacklist.
        /// </summary>
        /// <param name="server">
        ///     The server.
        /// </param>
        /// <param name="port">
        ///     The port.
        /// </param>
        private void VerifySslProtocolBlacklist(string server, int port)
        {
            // Explicitly check for non compliant SSL protocols
            try
            {
                var tcp = new TcpClient(server, port);

                using (
                    var ssl = new SslStream(tcp.GetStream(), false, null, null, EncryptionPolicy.AllowNoEncryption))
                {
                    ssl.AuthenticateAsClient(server, null, SslProtocols.Ssl2, false);

                    this.CreateVuln("SSL2 connections supported");
                }
            }
            catch (AuthenticationException)
            {
                // connection request should have failed.
            }
        }

        /// <summary>
        /// Creates a vulnerability.
        /// </summary>
        /// <param name="reason">
        ///     The reason.
        /// </param>
        private void CreateVuln(string reason)
        {
            this.AddVulnerability(
                this.Name,
                string.Empty,
                string.Empty,
                reason,
                new HttpWebResponseHolder
                {
                    RequestAbsolutUri = TestTarget.Uri.OriginalString,
                    StatusCode = HttpStatusCode.OK
                },
                null,
                VulnerabilityLevelEnum.Medium);
        }
    }
}