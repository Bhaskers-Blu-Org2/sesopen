# WebSec vulnerability detectors
##### Please provide feedback at sesopen@microsoft.com

WebSec is a highly scalable web application security scanner that powers Bing and other Microsoft domains.

### Version
0.1.0.0 (Beta)

### Building WebSec

You can build WebSec on Windows 7 SP1 or above, and Windows Server 2008 R2 or above, with Visual Studio 2015. Once you have Visual Studio installed:

  * Clone WebSec through 

    ```
      git clone https://github.com/Microsoft/sesopen.git
    ```
  * Open "[sesopen]\WebSec\WebSec.sln" in Visual Studio
  * Build Solution
  * The output will be placed under the "[sesopen]/distribution" folder

### Dependencies
All dependencies will be downloaded automatically on build time through the NuGet.
 * Selenium
 * Fiddler
 * HtmlAgilityPack
 * Microsoft Unity Pattern

### Software dependencies
 * Chrome browser (version >= 52.0.2)

### Projects
 
 * Common is a library containing helper classes
 * Library is containing the WebSec engine
 * Plugins is a library containing the attacks
 * Library.Tests are the unit tests for the WebSec engine
 * Plugins.Tests are the unit tests for the attacks (plugins)
 * Worker is a command line for encapsulation the WebSec engine
 * VulnerableSite is a sample site containing vulnerabilities
 
### Run
 * Go to [sesopen]/distribution/[configuration build]/worker
 * Run cmd
 * Execute Worker.exe -u http://www.bing.com/search?q=test or 
 * Execute Worker.exe -u FilePath(containing a list of urls added line by line) 
 * Results will be verbose and output to Worker.Info.log which can be found under the same path after the scan is running

### WebSec code quality tools
* Stylecop
* Code analysis

License
----

Code licensed under the [MIT License](https://github.com/Microsoft/sesopen/blob/master/WebSec/LICENSE.txt).