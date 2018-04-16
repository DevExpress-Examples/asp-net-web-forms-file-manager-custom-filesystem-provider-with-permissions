using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Runtime.Remoting.Messaging;

public class FilesDbContext : DbContext {
    const string ConnectionStringName = "FilesConnectionString";

    static string ConnectionString {
        get {
            return WebConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
        }
    }

    public FilesDbContext()
        : base(ConnectionString) {
        Database.SetInitializer<FilesDbContext>(new SampleDataInitializer());
    }

    public DbSet<FileSystemItem> FileSystemItems { get; set; }
    public DbSet<Permissions> PermissionsSet { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder) {
        var itemConfig = modelBuilder.Entity<FileSystemItem>();
        itemConfig.HasKey(i => i.Id);
        itemConfig.Property(i => i.Name).IsRequired();
        itemConfig.Property(i => i.IsFolder).IsRequired();
        itemConfig.Property(i => i.LastWriteTime).IsRequired();
        itemConfig.Property(i => i.Size).IsRequired();
        itemConfig.Property(i => i.Content).IsOptional();
        itemConfig.HasOptional(i => i.Parent);

        var permissionConfig = modelBuilder.Entity<Permissions>();
        permissionConfig.HasKey(p => p.Id);
        permissionConfig.HasRequired(p => p.Item);
        permissionConfig.Property(p => p.Rename).IsRequired();
        permissionConfig.Property(p => p.Move).IsRequired();
        permissionConfig.Property(p => p.Copy).IsRequired();
        permissionConfig.Property(p => p.Delete).IsRequired();
        permissionConfig.Property(p => p.Download).IsRequired();
        permissionConfig.Property(p => p.Create).IsRequired();
        permissionConfig.Property(p => p.MoveOrCopyInto).IsRequired();

        base.OnModelCreating(modelBuilder);
    }
}

public class DataProvider {
    static Random Random { get; set; }

    public FilesDbContext DbContext { get; set; }

    public IQueryable<FileSystemItem> FileSystemItems { get { return DbContext.FileSystemItems; } }
    public IQueryable<Permissions> PermissionsSet { get { return DbContext.PermissionsSet; } }

    public DataProvider(FilesDbContext dbContext) {
        DbContext = dbContext;
    }

    public void SaveChanges() {
        DbContext.SaveChanges();
    }

    public FileSystemItem FindById(int id) {
        return DbContext.FileSystemItems.Find(id);
    }

    public FileSystemItem CreateFolder(FileSystemItem parent, string name) {
        var folder = new FileSystemItem { Name = name, Parent = parent, IsFolder = true, LastWriteTime = DateTime.UtcNow };
        DbContext.FileSystemItems.Add(folder);
        CreateRandomFolderPermissions(folder);
        return folder;
    }

    //with permissions
    public FileSystemItem CreateFolder(FileSystemItem parent, string name, Permissions permissions) {
        var folder = new FileSystemItem { Name = name, Parent = parent, IsFolder = true, LastWriteTime = DateTime.UtcNow };
        DbContext.FileSystemItems.Add(folder);
        CreateFolderPermissions(folder, permissions);
        return folder;
    }
    public FileSystemItem CreateFile(FileSystemItem parent, string name, string content, Permissions permissions) {
        byte[] bytes = System.Text.Encoding.Default.GetBytes(content);
        return CreateFile(parent, name, bytes, permissions);
    }
    public FileSystemItem CreateFile(FileSystemItem parent, string name, Stream content, Permissions permissions) {
        byte[] bytes = ReadAllBytes(content);
        return CreateFile(parent, name, bytes, permissions);
    }
    public FileSystemItem CreateFile(FileSystemItem parent, string name, byte[] content, Permissions permissions) {
        var file = new FileSystemItem { Name = name, Parent = parent, LastWriteTime = DateTime.UtcNow };
        file.Content = content;
        file.Size = file.Content.Length;
        DbContext.FileSystemItems.Add(file);
        CreateFilePermissions(file, permissions);
        return file;
    }
    //without permissions
    public FileSystemItem CreateFile(FileSystemItem parent, string name, string content) {
        byte[] bytes = System.Text.Encoding.Default.GetBytes(content);
        return CreateFile(parent, name, bytes);
    }
    public FileSystemItem CreateFile(FileSystemItem parent, string name, Stream content) {
        byte[] bytes = ReadAllBytes(content);
        return CreateFile(parent, name, bytes);
    }
    public FileSystemItem CreateFile(FileSystemItem parent, string name, byte[] content) {
        var file = new FileSystemItem { Name = name, Parent = parent, LastWriteTime = DateTime.UtcNow };
        file.Content = content;
        file.Size = file.Content.Length;
        DbContext.FileSystemItems.Add(file);
        CreateRandomFilePermissions(file);
        return file;
    }

    public void Remove(FileSystemItem item) {
        DbContext.FileSystemItems.Remove(item);
    }
    //with permissions
    void CreateFolderPermissions(FileSystemItem folder, Permissions permissions) {
        permissions.Item = folder;
        DbContext.PermissionsSet.Add(permissions);
    }
    //without permissions and random permissions
    void CreateRandomFolderPermissions(FileSystemItem folder) {
        var permissions = new Permissions {
            Rename = GetRandomBoolean(),
            Move = GetRandomBoolean(),
            Copy = GetRandomBoolean(),
            Delete = GetRandomBoolean(),
            Create = GetRandomBoolean(),
            Upload = GetRandomBoolean(),
            MoveOrCopyInto = GetRandomBoolean()
        };
        permissions.Item = folder;
        DbContext.PermissionsSet.Add(permissions);
    }
    //with permissions
    void CreateFilePermissions(FileSystemItem file, Permissions permissions) {
        permissions.Item = file;
        DbContext.PermissionsSet.Add(permissions);
    }
    //without permissions
    void CreateRandomFilePermissions(FileSystemItem file) {
        var permissions = new Permissions {
            Rename = GetRandomBoolean(),
            Move = GetRandomBoolean(),
            Copy = GetRandomBoolean(),
            Delete = GetRandomBoolean(),
            Download = GetRandomBoolean()
        };
        permissions.Item = file;
        DbContext.PermissionsSet.Add(permissions);
    }

    byte[] ReadAllBytes(Stream stream) {
        using(MemoryStream ms = new MemoryStream()) {
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }

    bool GetRandomBoolean() {
        if(Random == null)
            Random = new Random();
        return Random.NextDouble() > 0.5;
    }
}

public class SampleDataInitializer : CreateDatabaseIfNotExists<FilesDbContext> {
    protected override void Seed(FilesDbContext context) {
        base.Seed(context);

        var data = new DataProvider(context);

        FileSystemItem root = data.CreateFolder(null, "Root",
            new Permissions() {
                Rename = true,
                Move = true,
                Copy = false,
                Delete = false,
                Download = false,
                Create = false,
                Upload = false,
                MoveOrCopyInto = true
            });
        FileSystemItem folder1 = data.CreateFolder(root, "Folder 1",
                        new Permissions() {
                            Rename = true,
                            Move = true,
                            Copy = false,
                            Delete = true,
                            Download = false,
                            Create = false,
                            Upload = false,
                            MoveOrCopyInto = true
                        });
        FileSystemItem folder2 = data.CreateFolder(root, "Folder 2",
            new Permissions() {
                Rename = false,
                Move = false,
                Copy = false,
                Delete = false,
                Download = false,
                Create = true,
                Upload = true,
                MoveOrCopyInto = true
            });
        FileSystemItem subFolder1 = data.CreateFolder(folder1, "Subfolder 1",
                        new Permissions() {
                            Rename = false,
                            Move = false,
                            Copy = false,
                            Delete = true,
                            Download = false,
                            Create = true,
                            Upload = true,
                            MoveOrCopyInto = true
                        });

        data.CreateFile(folder1, "File 1.txt", "Content 1",
                        new Permissions() {
                            Rename = false,
                            Move = false,
                            Copy = false,
                            Delete = true,
                            Download = true,
                            Create = false,
                            Upload = false,
                            MoveOrCopyInto = false
                        });
        data.CreateFile(folder1, "File 2.txt", "Content 2",
                        new Permissions() {
                            Rename = true,
                            Move = true,
                            Copy = false,
                            Delete = false,
                            Download = false,
                            Create = false,
                            Upload = false,
                            MoveOrCopyInto = true
                        });
        data.CreateFile(folder1, "File 3.txt", "Content 3",
                                    new Permissions() {
                                        Rename = true,
                                        Move = true,
                                        Copy = false,
                                        Delete = true,
                                        Download = true,
                                        Create = false,
                                        Upload = false,
                                        MoveOrCopyInto = false
                                    });

        data.CreateFile(folder2, "Project 1.txt", "Content 1",
                                    new Permissions() {
                                        Rename = false,
                                        Move = false,
                                        Copy = true,
                                        Delete = true,
                                        Download = true,
                                        Create = false,
                                        Upload = false,
                                        MoveOrCopyInto = false
                                    });
        data.CreateFile(folder2, "Project 2.txt", "Content 2",
                                     new Permissions() {
                                         Rename = true,
                                         Move = false,
                                         Copy = true,
                                         Delete = false,
                                         Download = true,
                                         Create = false,
                                         Upload = false,
                                         MoveOrCopyInto = false
                                     });
        data.CreateFile(folder2, "Project 3.txt", "Content 3",
                                    new Permissions() {
                                        Rename = false,
                                        Move = false,
                                        Copy = false,
                                        Delete = true,
                                        Download = true,
                                        Create = false,
                                        Upload = false,
                                        MoveOrCopyInto = false
                                    });

        data.CreateFile(subFolder1, "Doc 1.txt", "Content 1",
                                    new Permissions() {
                                        Rename = true,
                                        Move = false,
                                        Copy = true,
                                        Delete = true,
                                        Download = false,
                                        Create = false,
                                        Upload = false,
                                        MoveOrCopyInto = false
                                    });
        data.CreateFile(subFolder1, "Doc 2.txt", "Content 2",
                                    new Permissions() {
                                        Rename = false,
                                        Move = false,
                                        Copy = false,
                                        Delete = false,
                                        Download = true,
                                        Create = false,
                                        Upload = false,
                                        MoveOrCopyInto = false
                                    });
        data.CreateFile(subFolder1, "Doc 3.txt", "Content 3",
                                    new Permissions() {
                                        Rename = true,
                                        Move = false,
                                        Copy = false,
                                        Delete = false,
                                        Download = false,
                                        Create = false,
                                        Upload = false,
                                        MoveOrCopyInto = false
                                    });

        data.SaveChanges();
    }
}