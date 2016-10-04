# WebSec vulnerability detectors
##### Please provide feedback at sesopen@microsoft.com

WebSec vulnerability detectors are used by Bing and other Microsoft online services and sites.

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

	Alternatively, run msbuild on the command line:
	
	```
      msbuild WebSec.sln
    ```

### Library dependencies
All dependencies will be downloaded automatically on build time through the NuGet.
 * Selenium
 * Fiddler
 * HtmlAgilityPack
 * Microsoft Unity Pattern

### Software dependencies
 * [Chrome browser (version >= 52.0.2)](https://www.google.com/chrome/browser/desktop/)
 * [Visual Studio 2015](https://www.visualstudio.com/downloads/)

### Projects
 
 * Common is a library containing helper classes
 * Library is containing the WebSec engine
 * Plugins is a library containing the attacks
 * Library.Tests are the unit tests for the WebSec engine
 * Plugins.Tests are the unit tests for the attacks (plugins)
 * Worker is a command line for encapsulation the WebSec engine
 * VulnerableSite is a sample site containing vulnerabilities
 
### Run
 * Go to 
    ```
      Run cmd
    ```

	```
	  cd [sesopen]/distribution/[configuration build]/worker
	```

    ```
      Execute Worker.exe -u http://www.bing.com/search?q=test or 
    ```

    ```
      Execute Worker.exe -u FilePath(containing a list of urls added line by line) 
    ```  

 * Results will be verbose and output to Worker.Info.log which can be found under the same path after the scan is running

### Code quality tools
* Stylecop
* Code analysis

Disclaimer
----

We do not take responsibility for the way in which any one uses this application (WebSec). We have made the purposes of the application clear and it should not be used maliciously.

License
----

Code licensed under the [MIT License](https://github.com/Microsoft/sesopen/blob/master/WebSec/LICENSE.txt).