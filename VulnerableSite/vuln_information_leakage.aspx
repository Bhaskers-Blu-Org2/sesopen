<%@ Page Language="C#" AutoEventWireup="true" CodeFile="vuln_information_leakage.aspx.cs" Inherits="VulnerableSite.vuln_information_leakage" %>

<html>
<head></head>
<body>
    Positive Leakage:<br />
    <div>
        UNC: \\server1\share\folder\file.ext<br />
        UNC: \\server2\share\folder<br />
        UNC: \\server3\share<br />
        UNC: \\server4<br />
        DOS: c:\folder\file.ext<br />
        DOS: z:\folder<br />
        DOS: J:\UPPER<br />
        IPV4: 192.168.100.100<br/>
        IPV4: 10.55.100.1/24<br/>
        IPV4: 1.2.3.4<br/>
        IPV6: 2001:0db8:0000:0000:0000:ff00:0042:8329<br/>
        IPV6: 2001:db8:0:0:0:ff00:42:8329<br />
        IPV6: 2001:db8::ff00:42:8329<br />
        IPV6: ::1<br />
    </div>
    <br />
    Negative Leakage:<br />
    <div>
        DOS: c:<br />
        HTTP: http://domain/file.ext <br/>
        HTTP: http://domain <br/>
        HTTP: https://domain/file.ext <br/>
        HTTP: https://domain <br/>
        FTP:  ftp://domain/file.ext <br/>
        FTP:  ftp://domain <br/>
        PHONE: 800.555.1212<br />        
        PHONE: 1.800.555.1212<br />        
    </div>


</body>
</html>

