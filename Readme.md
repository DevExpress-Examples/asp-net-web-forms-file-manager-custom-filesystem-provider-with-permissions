<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/128554594/17.2.3%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T554282)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
<!-- default badges end -->
<!-- default file list -->
*Files to look at*:

* [CustomFileSystemProvider.cs](./CS/App_Code/CustomFileSystemProvider.cs) (VB: [CustomFileSystemProvider.vb](./VB/App_Code/CustomFileSystemProvider.vb))
* [DataProvider.cs](./CS/App_Code/DataProvider.cs) (VB: [DataProvider.vb](./VB/App_Code/DataProvider.vb))
* [Model.cs](./CS/App_Code/Model.cs) (VB: [Model.vb](./VB/App_Code/Model.vb))
* [Default.aspx](./CS/Default.aspx) (VB: [Default.aspx](./VB/Default.aspx))
* [Default.aspx.cs](./CS/Default.aspx.cs) (VB: [Default.aspx.vb](./VB/Default.aspx.vb))
<!-- default file list end -->
# ASPxFileManager - Implement custom FileSystem provider and set file/folder permissions
<!-- run online -->
**[[Run Online]](https://codecentral.devexpress.com/t554282/)**
<!-- run online end -->


<p>This example demonstrates how to create a custom FileSystem provider and assign permissions to each folder or file. This example uses two tables. <br>The first one - FileSystemItems - contains all the files and folders.<br>The second one - PermissionsSet - contains all the permissions assigned to folders or files.<br>It is required only to create a custom FileSystem provider and additionally override the GetFilePermissions and GetFolderPermissions methods that are used to gather permissions.</p>
<p><strong>See also</strong>: <a href="http://help.devexpress.com/#AspNet/CustomDocument119543">ASPxFileManager Permissions</a></p>

<br/>


