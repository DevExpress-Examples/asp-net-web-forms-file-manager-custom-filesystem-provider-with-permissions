using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using DevExpress.Web;

public class CustomFileSystemProvider : FileSystemProviderBase {
    public DataProvider Data { get; set; }
    Dictionary<int, CacheEntry> FolderCache { get; set; }
    FileSystemItem RootItem { get; set; }

    public CustomFileSystemProvider(string rootFolder)
        : base(rootFolder) {
        var dbContext = new FilesDbContext();
        Data = new DataProvider(dbContext);
        RefreshFolderCache();
    }

    public override string RootFolderDisplayName { get { return RootItem.Name; } }

    public override IEnumerable<FileManagerFile> GetFiles(FileManagerFolder folder) {
        int folderId = GetItemId(folder);
        return Data.FileSystemItems.
            Join(Data.PermissionsSet, i => i.Id, p => p.Item.Id, (i, pi) => new {
                Item = i,
                Permissions = pi
            }).
            Where(dto => dto.Item.Parent != null && dto.Item.Parent.Id == folderId && !dto.Item.IsFolder).
            ToList().
            Select(dto => new FileManagerFile(this, folder, dto.Item.Name, dto.Item.Id.ToString(), new FileManagerFileProperties {
                Permissions = GetFilePermissionsInternal(dto.Permissions)
            }));
    }
    public override IEnumerable<FileManagerFolder> GetFolders(FileManagerFolder parentFolder) {
        FileSystemItem folderItem = FindFolderItem(parentFolder);
        return FolderCache.Values.
            Where(entry => entry.Item.Parent == folderItem && entry.Item.IsFolder).
            Select(entry => new FileManagerFolder(this, parentFolder, entry.Item.Name, entry.Item.Id.ToString(),
                new FileManagerFolderProperties {
                    Permissions = GetFolderPermissionsInternal(entry.Permissions)
                }));
    }
    public override FileManagerFilePermissions GetFilePermissions(FileManagerFile file) {
        int fileId = int.Parse(file.Id);
        Permissions permissions = Data.PermissionsSet.FirstOrDefault(p => p.Item.Id == fileId);
        if(permissions == null)
            return FileManagerFilePermissions.Default;
        return GetFilePermissionsInternal(permissions);
    }
    public override FileManagerFolderPermissions GetFolderPermissions(FileManagerFolder folder) {
        if(string.IsNullOrEmpty(folder.RelativeName))
            return FileManagerFolderPermissions.Default;
        int folderId = int.Parse(folder.Id);
        Permissions permissions = Data.PermissionsSet.FirstOrDefault(p => p.Item.Id == folderId);
        if(permissions == null)
            return FileManagerFolderPermissions.Default;
        return GetFolderPermissionsInternal(permissions);
    }
    FileManagerFilePermissions GetFilePermissionsInternal(Permissions permissions) {
        return (permissions.Delete ? FileManagerFilePermissions.Delete : FileManagerFilePermissions.Default)
            | (permissions.Move ? FileManagerFilePermissions.Move : FileManagerFilePermissions.Default)
            | (permissions.Copy ? FileManagerFilePermissions.Copy : FileManagerFilePermissions.Default)
            | (permissions.Rename ? FileManagerFilePermissions.Rename : FileManagerFilePermissions.Default)
            | (permissions.Download ? FileManagerFilePermissions.Download : FileManagerFilePermissions.Default);
    }
    FileManagerFolderPermissions GetFolderPermissionsInternal(Permissions permissions) {
        return (permissions.Delete ? FileManagerFolderPermissions.Delete : FileManagerFolderPermissions.Default)
            | (permissions.Move ? FileManagerFolderPermissions.Move : FileManagerFolderPermissions.Default)
            | (permissions.Copy ? FileManagerFolderPermissions.Copy : FileManagerFolderPermissions.Default)
            | (permissions.Rename ? FileManagerFolderPermissions.Rename : FileManagerFolderPermissions.Default)
            | (permissions.Create ? FileManagerFolderPermissions.Create : FileManagerFolderPermissions.Default)
            | (permissions.Upload ? FileManagerFolderPermissions.Upload : FileManagerFolderPermissions.Default)
            | (permissions.MoveOrCopyInto ? FileManagerFolderPermissions.MoveOrCopyInto : FileManagerFolderPermissions.Default);
    }

    public override bool Exists(FileManagerFile file) {
        return FindFileItem(file) != null;
    }
    public override bool Exists(FileManagerFolder folder) {
        return FindFolderItem(folder) != null;
    }
    public override Stream ReadFile(FileManagerFile file) {
        FileSystemItem fileItem = FindFileItem(file);
        return new MemoryStream(fileItem.Content.ToArray());
    }
    public override DateTime GetLastWriteTime(FileManagerFile file) {
        FileSystemItem fileItem = FindFileItem(file);
        return fileItem.LastWriteTime;
    }
    public override DateTime GetLastWriteTime(FileManagerFolder folder) {
        FileSystemItem folderItem = FindFolderItem(folder);
        return folderItem.LastWriteTime;
    }
    public override long GetLength(FileManagerFile file) {
        FileSystemItem fileItem = FindFileItem(file);
        return fileItem.Content.Length;
    }

    public override void CreateFolder(FileManagerFolder parent, string name) {
        UpdateAndSaveChanges(parent, parentItem => Data.CreateFolder(parentItem, name));
    }
    public override void DeleteFile(FileManagerFile file) {
        UpdateAndSubmitChanges(file, item => Data.Remove(item));
    }
    public override void DeleteFolder(FileManagerFolder folder) {
        UpdateAndSaveChanges(folder, item => Data.Remove(item));
    }
    public override void MoveFile(FileManagerFile file, FileManagerFolder newParentFolder) {
        UpdateAndSubmitChanges(file, item => item.Parent = FindFolderItem(newParentFolder));
    }
    public override void MoveFolder(FileManagerFolder folder, FileManagerFolder newParentFolder) {
        UpdateAndSaveChanges(folder, item => item.Parent = FindFolderItem(newParentFolder));
    }
    public override void RenameFile(FileManagerFile file, string name) {
        UpdateAndSubmitChanges(file, item => item.Name = name);
    }
    public override void RenameFolder(FileManagerFolder folder, string name) {
        UpdateAndSaveChanges(folder, item => item.Name = name);
    }
    public override void UploadFile(FileManagerFolder folder, string fileName, Stream content) {
        UpdateAndSaveChanges(folder, folderItem => Data.CreateFile(folderItem, fileName, content));
    }
    public override void CopyFile(FileManagerFile file, FileManagerFolder newParentFolder) {
        FileSystemItem fileItem = FindFileItem(file);
        CopyCore(fileItem, newParentFolder.RelativeName, false);
    }
    public override void CopyFolder(FileManagerFolder folder, FileManagerFolder newParentFolder) {
        List<FileManagerFolder> folders = new List<FileManagerFolder>();
        FillSubFoldersList(folder, folders);
        int folderNameOffset = !string.IsNullOrEmpty(folder.Parent.RelativeName) ? folder.Parent.RelativeName.Length + 1 : 0;

        foreach(FileManagerFolder copyingFolder in folders) {
            FileSystemItem folderItem = FindFolderItem(copyingFolder);
            string folderPath = newParentFolder.RelativeName;
            if(copyingFolder != folder)
                folderPath = Path.Combine(folderPath, copyingFolder.Parent.RelativeName.Substring(folderNameOffset));
            CopyCore(folderItem, folderPath, true);
            foreach(FileManagerFile currentFile in copyingFolder.GetFiles()) {
                FileSystemItem fileItem = FindFileItem(currentFile);
                CopyCore(fileItem, Path.Combine(folderPath, copyingFolder.Name), false);
            }
        }
    }
    void FillSubFoldersList(FileManagerFolder folder, List<FileManagerFolder> list) {
        list.Add(folder);
        foreach(FileManagerFolder subFolder in folder.GetFolders())
            FillSubFoldersList(subFolder, list);
    }
    void CopyCore(FileSystemItem item, string path, bool isFolder) {
        FileManagerFolder newParentFolder = new FileManagerFolder(this, path, (string)null);

        UpdateAndSaveChanges(newParentFolder,
            newParentItem => {
                if(isFolder)
                    Data.CreateFolder(newParentItem, item.Name);
                else
                    Data.CreateFile(newParentItem, item.Name, item.Content);
            });
    }
    protected void UpdateAndSubmitChanges(FileManagerFile file, Action<FileSystemItem> update) {
        UpdateAndSubmitChangesCore(FindFileItem(file), update);
    }
    protected void UpdateAndSaveChanges(FileManagerFolder folder, Action<FileSystemItem> update) {
        UpdateAndSubmitChangesCore(FindFolderItem(folder), update);
    }
    protected void UpdateAndSubmitChangesCore(FileSystemItem item, Action<FileSystemItem> update) {
        update(item);
        Data.SaveChanges();
        RefreshFolderCache();
    }
    protected FileSystemItem FindFileItem(FileManagerFile file) {
        if(file.Id == null)
            return FindFileItemByParentFolder(file.Name, file.Folder);

        int fileId = GetItemId(file);
        return Data.FindById(fileId);
    }
    protected FileSystemItem FindFolderItem(FileManagerFolder folder) {
        if(folder.Id == null)
            return FindFolderItemByRelativeName(folder);

        int folderId = GetItemId(folder);
        return FolderCache[folderId].Item;
    }
    protected FileSystemItem FindFileItemByParentFolder(string itemName, FileManagerFolder parentFolder) {
        FileSystemItem parentItem = FindFolderItemByRelativeName(parentFolder);
        return Data.FileSystemItems.FirstOrDefault(
            item => item.Parent != null && item.Parent.Id == parentItem.Id && !item.IsFolder && item.Name == itemName);
    }
    protected int GetItemId(FileManagerItem fileManagerItem) {
        if(string.IsNullOrEmpty(fileManagerItem.RelativeName))
            return RootItem.Id;
        return int.Parse(fileManagerItem.Id);
    }
    protected FileSystemItem FindFolderItemByRelativeName(FileManagerFolder folder) {
        return FolderCache.Values.
            Select(entry => entry.Item).
            Where(item => item.IsFolder && GetRelativeName(item) == folder.RelativeName).
            FirstOrDefault();
    }
    protected string GetRelativeName(FileSystemItem folderItem) {
        if(folderItem.Parent == null)
            return string.Empty;
        if(folderItem.Parent.Parent == null)
            return folderItem.Name;
        string name = GetRelativeName(folderItem.Parent);
        return Path.Combine(name, folderItem.Name);
    }

    protected void RefreshFolderCache() {
        FolderCache = Data.FileSystemItems.
            Join(Data.PermissionsSet, i => i.Id, p => p.Item.Id, (i, pi) => new {
                Item = i,
                Permissions = pi
            }).
            Where(dto => dto.Item.IsFolder).
            ToDictionary(dto => dto.Item.Id, dto => new CacheEntry {
                Item = dto.Item,
                Permissions = dto.Permissions
            });

        RootItem = FolderCache.Values.
            Select(entry => entry.Item).
            First(item => item.Parent == null);
    }

    class CacheEntry {
        public FileSystemItem Item { get; set; }
        public Permissions Permissions { get; set; }
    }
}