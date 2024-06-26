<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/128554594/17.2.3%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T554282)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
[![](https://img.shields.io/badge/💬_Leave_Feedback-feecdd?style=flat-square)](#does-this-example-address-your-development-requirementsobjectives)
<!-- default badges end -->

# File Manager for ASP.NET Web Forms - Implement custom file system provider and set file/folder permissions


This example demonstrates how to create a custom file system provider and assign permissions to each folder or file. 

This example uses two tables. The first one - **FileSystemItems** - contains all the files and folders. The second one - **PermissionsSet** - contains all the permissions assigned to folders or files.

It is required only to create a custom FileSystem provider and additionally override the [GetFilePermissions](https://docs.devexpress.com/AspNet/DevExpress.Web.FileSystemProviderBase.GetFilePermissions(DevExpress.Web.FileManagerFile)) and [GetFolderPermissions](https://docs.devexpress.com/AspNet/DevExpress.Web.FileSystemProviderBase.GetFolderPermissions(DevExpress.Web.FileManagerFolder)) methods that are used to gather permissions.

## Files to Review

- [CustomFileSystemProvider.cs](./CS/App_Code/CustomFileSystemProvider.cs) (VB: [CustomFileSystemProvider.vb](./VB/App_Code/CustomFileSystemProvider.vb))
- [DataProvider.cs](./CS/App_Code/DataProvider.cs) (VB: [DataProvider.vb](./VB/App_Code/DataProvider.vb))
- [Model.cs](./CS/App_Code/Model.cs) (VB: [Model.vb](./VB/App_Code/Model.vb))
- [Default.aspx](./CS/Default.aspx) (VB: [Default.aspx](./VB/Default.aspx))
- [Default.aspx.cs](./CS/Default.aspx.cs) (VB: [Default.aspx.vb](./VB/Default.aspx.vb))

## Documentation

- [Access Rules](https://docs.devexpress.com/AspNet/119542/components/file-management/file-manager/concepts/access-control-overview/access-rules)
- [Permissions](https://docs.devexpress.com/AspNet/119543/components/file-management/file-manager/concepts/access-control-overview/permissions)
<!-- feedback -->
## Does this example address your development requirements/objectives?

[<img src="https://www.devexpress.com/support/examples/i/yes-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=asp-net-web-forms-file-manager-custom-filesystem-provider-with-permissions&~~~was_helpful=yes) [<img src="https://www.devexpress.com/support/examples/i/no-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=asp-net-web-forms-file-manager-custom-filesystem-provider-with-permissions&~~~was_helpful=no)

(you will be redirected to DevExpress.com to submit your response)
<!-- feedback end -->
