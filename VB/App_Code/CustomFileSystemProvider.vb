Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Web
Imports DevExpress.Web

Public Class CustomFileSystemProvider
    Inherits FileSystemProviderBase

    Public Property Data() As DataProvider
    Private Property FolderCache() As Dictionary(Of Integer, CacheEntry)
    Private Property RootItem() As FileSystemItem

    Public Sub New(ByVal rootFolder As String)
        MyBase.New(rootFolder)
        Dim dbContext = New FilesDbContext()
        Data = New DataProvider(dbContext)
        RefreshFolderCache()
    End Sub

    Public Overrides ReadOnly Property RootFolderDisplayName() As String
        Get
            Return RootItem.Name
        End Get
    End Property

    Public Overrides Function GetFiles(ByVal folder As FileManagerFolder) As IEnumerable(Of FileManagerFile)
        Dim folderId As Integer = GetItemId(folder)
        Return Data.FileSystemItems.Join(Data.PermissionsSet, Function(i) i.Id, Function(p) p.Item.Id, Function(i, pi) New With { _
            Key .Item = i, _
            Key .Permissions = pi _
        }).Where(Function(dto) dto.Item.Parent IsNot Nothing AndAlso dto.Item.Parent.Id = folderId AndAlso Not dto.Item.IsFolder).ToList().Select(Function(dto) New FileManagerFile(Me, folder, dto.Item.Name, dto.Item.Id.ToString(), New FileManagerFileProperties With {.Permissions = GetFilePermissionsInternal(dto.Permissions)}))
    End Function
    Public Overrides Function GetFolders(ByVal parentFolder As FileManagerFolder) As IEnumerable(Of FileManagerFolder)
        Dim folderItem As FileSystemItem = FindFolderItem(parentFolder)
        Return FolderCache.Values.Where(Function(entry) entry.Item.Parent Is folderItem AndAlso entry.Item.IsFolder).Select(Function(entry) New FileManagerFolder(Me, parentFolder, entry.Item.Name, entry.Item.Id.ToString(), New FileManagerFolderProperties With {.Permissions = GetFolderPermissionsInternal(entry.Permissions)}))
    End Function
    Public Overrides Function GetFilePermissions(ByVal file As FileManagerFile) As FileManagerFilePermissions
        Dim fileId As Integer = Integer.Parse(file.Id)
        Dim permissions As Permissions = Data.PermissionsSet.FirstOrDefault(Function(p) p.Item.Id = fileId)
        If permissions Is Nothing Then
            Return FileManagerFilePermissions.Default
        End If
        Return GetFilePermissionsInternal(permissions)
    End Function
    Public Overrides Function GetFolderPermissions(ByVal folder As FileManagerFolder) As FileManagerFolderPermissions
        If String.IsNullOrEmpty(folder.RelativeName) Then
            Return FileManagerFolderPermissions.Default
        End If
        Dim folderId As Integer = Integer.Parse(folder.Id)
        Dim permissions As Permissions = Data.PermissionsSet.FirstOrDefault(Function(p) p.Item.Id = folderId)
        If permissions Is Nothing Then
            Return FileManagerFolderPermissions.Default
        End If
        Return GetFolderPermissionsInternal(permissions)
    End Function
    Private Function GetFilePermissionsInternal(ByVal permissions As Permissions) As FileManagerFilePermissions
        Return (If(permissions.Delete, FileManagerFilePermissions.Delete, FileManagerFilePermissions.Default)) Or (If(permissions.Move, FileManagerFilePermissions.Move, FileManagerFilePermissions.Default)) Or (If(permissions.Copy, FileManagerFilePermissions.Copy, FileManagerFilePermissions.Default)) Or (If(permissions.Rename, FileManagerFilePermissions.Rename, FileManagerFilePermissions.Default)) Or (If(permissions.Download, FileManagerFilePermissions.Download, FileManagerFilePermissions.Default))
    End Function
    Private Function GetFolderPermissionsInternal(ByVal permissions As Permissions) As FileManagerFolderPermissions
        Return (If(permissions.Delete, FileManagerFolderPermissions.Delete, FileManagerFolderPermissions.Default)) Or (If(permissions.Move, FileManagerFolderPermissions.Move, FileManagerFolderPermissions.Default)) Or (If(permissions.Copy, FileManagerFolderPermissions.Copy, FileManagerFolderPermissions.Default)) Or (If(permissions.Rename, FileManagerFolderPermissions.Rename, FileManagerFolderPermissions.Default)) Or (If(permissions.Create, FileManagerFolderPermissions.Create, FileManagerFolderPermissions.Default)) Or (If(permissions.Upload, FileManagerFolderPermissions.Upload, FileManagerFolderPermissions.Default)) Or (If(permissions.MoveOrCopyInto, FileManagerFolderPermissions.MoveOrCopyInto, FileManagerFolderPermissions.Default))
    End Function

    Public Overrides Function Exists(ByVal file As FileManagerFile) As Boolean
        Return FindFileItem(file) IsNot Nothing
    End Function
    Public Overrides Function Exists(ByVal folder As FileManagerFolder) As Boolean
        Return FindFolderItem(folder) IsNot Nothing
    End Function
    Public Overrides Function ReadFile(ByVal file As FileManagerFile) As Stream
        Dim fileItem As FileSystemItem = FindFileItem(file)
        Return New MemoryStream(fileItem.Content.ToArray())
    End Function
    Public Overrides Function GetLastWriteTime(ByVal file As FileManagerFile) As Date
        Dim fileItem As FileSystemItem = FindFileItem(file)
        Return fileItem.LastWriteTime
    End Function
    Public Overrides Function GetLastWriteTime(ByVal folder As FileManagerFolder) As Date
        Dim folderItem As FileSystemItem = FindFolderItem(folder)
        Return folderItem.LastWriteTime
    End Function
    Public Overrides Function GetLength(ByVal file As FileManagerFile) As Long
        Dim fileItem As FileSystemItem = FindFileItem(file)
        Return fileItem.Content.Length
    End Function

    Public Overrides Sub CreateFolder(ByVal parent As FileManagerFolder, ByVal name As String)
        UpdateAndSaveChanges(parent, Sub(parentItem) Data.CreateFolder(parentItem, name))
    End Sub
    Public Overrides Sub DeleteFile(ByVal file As FileManagerFile)
        UpdateAndSubmitChanges(file, Sub(item) Data.Remove(item))
    End Sub
    Public Overrides Sub DeleteFolder(ByVal folder As FileManagerFolder)
        UpdateAndSaveChanges(folder, Sub(item) Data.Remove(item))
    End Sub
    Public Overrides Sub MoveFile(ByVal file As FileManagerFile, ByVal newParentFolder As FileManagerFolder)
        UpdateAndSubmitChanges(file, Sub(item) item.Parent = FindFolderItem(newParentFolder))
    End Sub
    Public Overrides Sub MoveFolder(ByVal folder As FileManagerFolder, ByVal newParentFolder As FileManagerFolder)
        UpdateAndSaveChanges(folder, Sub(item) item.Parent = FindFolderItem(newParentFolder))
    End Sub
    Public Overrides Sub RenameFile(ByVal file As FileManagerFile, ByVal name As String)
        UpdateAndSubmitChanges(file, Sub(item) item.Name = name)
    End Sub
    Public Overrides Sub RenameFolder(ByVal folder As FileManagerFolder, ByVal name As String)
        UpdateAndSaveChanges(folder, Sub(item) item.Name = name)
    End Sub
    Public Overrides Sub UploadFile(ByVal folder As FileManagerFolder, ByVal fileName As String, ByVal content As Stream)
        UpdateAndSaveChanges(folder, Sub(folderItem) Data.CreateFile(folderItem, fileName, content))
    End Sub
    Public Overrides Sub CopyFile(ByVal file As FileManagerFile, ByVal newParentFolder As FileManagerFolder)
        Dim fileItem As FileSystemItem = FindFileItem(file)
        CopyCore(fileItem, newParentFolder.RelativeName, False)
    End Sub
    Public Overrides Sub CopyFolder(ByVal folder As FileManagerFolder, ByVal newParentFolder As FileManagerFolder)
        Dim folders As New List(Of FileManagerFolder)()
        FillSubFoldersList(folder, folders)
        Dim folderNameOffset As Integer = If(Not String.IsNullOrEmpty(folder.Parent.RelativeName), folder.Parent.RelativeName.Length + 1, 0)

        For Each copyingFolder As FileManagerFolder In folders
            Dim folderItem As FileSystemItem = FindFolderItem(copyingFolder)
            Dim folderPath As String = newParentFolder.RelativeName
            If copyingFolder IsNot folder Then
                folderPath = Path.Combine(folderPath, copyingFolder.Parent.RelativeName.Substring(folderNameOffset))
            End If
            CopyCore(folderItem, folderPath, True)
            For Each currentFile As FileManagerFile In copyingFolder.GetFiles()
                Dim fileItem As FileSystemItem = FindFileItem(currentFile)
                CopyCore(fileItem, Path.Combine(folderPath, copyingFolder.Name), False)
            Next currentFile
        Next copyingFolder
    End Sub
    Private Sub FillSubFoldersList(ByVal folder As FileManagerFolder, ByVal list As List(Of FileManagerFolder))
        list.Add(folder)
        For Each subFolder As FileManagerFolder In folder.GetFolders()
            FillSubFoldersList(subFolder, list)
        Next subFolder
    End Sub
    Private Sub CopyCore(ByVal item As FileSystemItem, ByVal path As String, ByVal isFolder As Boolean)
        Dim newParentFolder As New FileManagerFolder(Me, path, DirectCast(Nothing, String))

        UpdateAndSaveChanges(newParentFolder, Sub(newParentItem)
            If isFolder Then
                Data.CreateFolder(newParentItem, item.Name)
            Else
                Data.CreateFile(newParentItem, item.Name, item.Content)
            End If
        End Sub)
    End Sub
    Protected Sub UpdateAndSubmitChanges(ByVal file As FileManagerFile, ByVal update As Action(Of FileSystemItem))
        UpdateAndSubmitChangesCore(FindFileItem(file), update)
    End Sub
    Protected Sub UpdateAndSaveChanges(ByVal folder As FileManagerFolder, ByVal update As Action(Of FileSystemItem))
        UpdateAndSubmitChangesCore(FindFolderItem(folder), update)
    End Sub
    Protected Sub UpdateAndSubmitChangesCore(ByVal item As FileSystemItem, ByVal update As Action(Of FileSystemItem))
        update(item)
        Data.SaveChanges()
        RefreshFolderCache()
    End Sub
    Protected Function FindFileItem(ByVal file As FileManagerFile) As FileSystemItem
        If file.Id Is Nothing Then
            Return FindFileItemByParentFolder(file.Name, file.Folder)
        End If

        Dim fileId As Integer = GetItemId(file)
        Return Data.FindById(fileId)
    End Function
    Protected Function FindFolderItem(ByVal folder As FileManagerFolder) As FileSystemItem
        If folder.Id Is Nothing Then
            Return FindFolderItemByRelativeName(folder)
        End If

        Dim folderId As Integer = GetItemId(folder)
        Return FolderCache(folderId).Item
    End Function
    Protected Function FindFileItemByParentFolder(ByVal itemName As String, ByVal parentFolder As FileManagerFolder) As FileSystemItem
        Dim parentItem As FileSystemItem = FindFolderItemByRelativeName(parentFolder)
        Return Data.FileSystemItems.FirstOrDefault(Function(item) item.Parent IsNot Nothing AndAlso item.Parent.Id = parentItem.Id AndAlso Not item.IsFolder AndAlso item.Name = itemName)
    End Function
    Protected Function GetItemId(ByVal fileManagerItem As FileManagerItem) As Integer
        If String.IsNullOrEmpty(fileManagerItem.RelativeName) Then
            Return RootItem.Id
        End If
        Return Integer.Parse(fileManagerItem.Id)
    End Function
    Protected Function FindFolderItemByRelativeName(ByVal folder As FileManagerFolder) As FileSystemItem
        Return FolderCache.Values.Select(Function(entry) entry.Item).Where(Function(item) item.IsFolder AndAlso GetRelativeName(item) = folder.RelativeName).FirstOrDefault()
    End Function
    Protected Function GetRelativeName(ByVal folderItem As FileSystemItem) As String
        If folderItem.Parent Is Nothing Then
            Return String.Empty
        End If
        If folderItem.Parent.Parent Is Nothing Then
            Return folderItem.Name
        End If
        Dim name As String = GetRelativeName(folderItem.Parent)
        Return Path.Combine(name, folderItem.Name)
    End Function

    Protected Sub RefreshFolderCache()
        FolderCache = Data.FileSystemItems.Join(Data.PermissionsSet, Function(i) i.Id, Function(p) p.Item.Id, Function(i, pi) New With { _
            Key .Item = i, _
            Key .Permissions = pi _
        }).Where(Function(dto) dto.Item.IsFolder).ToDictionary(Function(dto) dto.Item.Id, Function(dto) New CacheEntry With { _
            .Item = dto.Item, _
            .Permissions = dto.Permissions _
        })

        RootItem = FolderCache.Values.Select(Function(entry) entry.Item).First(Function(item) item.Parent Is Nothing)
    End Sub

    Private Class CacheEntry
        Public Property Item() As FileSystemItem
        Public Property Permissions() As Permissions
    End Class
End Class