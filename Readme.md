# ASPxFileManager - Implement custom FileSystem provider and set file/folder permissions


<p>This example demonstrates how to create a custom FileSystem provider and assign permissions to each folder or file. This example uses two tables. <br>The first one - FileSystemItems - contains all the files and folders.<br>The second one - PermissionsSet - contains all the permissions assigned to folders or files.<br>It is required only to create a custom FileSystem provider and additionally override the GetFilePermissions and GetFolderPermissions methods that are used to gather permissions.</p>
<p><strong>See also</strong>: <a href="http://help.devexpress.com/#AspNet/CustomDocument119543">ASPxFileManager Permissions</a></p>

<br/>


