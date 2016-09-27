//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common.TestInfrastructure
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;

    /// <summary>
    /// A test setup helpers.
    /// </summary>
    public static class TestSetupHelpers
    {
        /// <summary>
        /// Cleans the selenium driver.
        /// </summary>
        public static void CleanSeleniumDriver()
        {
            Process[] chromedriver = Process.GetProcessesByName("chromedriver");

            foreach (var process in chromedriver)
            {
                process.Kill();
                process.WaitForExit();
            }
        }

        /// <summary>
        /// Starts the IIS Express Server for the VulnerableSite project.
        /// </summary>
        public static void StartIisExpress()
        {
            var exePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                @"IIS Express\IISExpress.EXE");

            if (CheckAvailableServerPort(Constants.VulnerabilitiesSitePort))
            {
                StartSite(
                    Path.Combine(
                        new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName,
                        "VulnerableSite"),
                    exePath,
                    Constants.VulnerabilitiesSitePort);
            }

            if (CheckAvailableServerPort(Constants.VulnerabilitiesSiteSslPort))
            {
                StartSite(
                    Path.Combine(
                        new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName,
                        "VulnerableSite"),
                    exePath,
                    Constants.VulnerabilitiesSiteSslPort);
            }
        }

        /// <summary>
        /// Check available server port.
        /// </summary>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private static bool CheckAvailableServerPort(int port)
        {
            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties globalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpConnInfoArray = globalProperties.GetActiveTcpListeners();

            return tcpConnInfoArray.All(endpoint => endpoint.Port != port);
        }

        /// <summary>
        /// Starts a site.
        /// </summary>
        /// <param name="sitePath">
        /// Full pathname of the site file.
        /// </param>
        /// <param name="exePath">
        /// Full pathname of the ii s executable file.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        private static void StartSite(string sitePath, string exePath, int port)
        {
            // create a new process to start the IIS Express Server
            using (var process = new Process
            {
                StartInfo =
                {
                    FileName = exePath,
                    Arguments = "/path:{0} /port:{1}".FormatIc(sitePath, port),
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            })
            {
                // start the web site
                process.Start();
            }
        }
    }
}