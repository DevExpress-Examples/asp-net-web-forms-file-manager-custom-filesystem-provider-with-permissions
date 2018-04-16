Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Web
Imports System.Web.Configuration
Imports System.Data.Common
Imports System.Data.Entity
Imports System.Data.Entity.SqlServer
Imports System.Runtime.Remoting.Messaging

Public Class FilesDbContext
    Inherits DbContext

    Private Const ConnectionStringName As String = "FilesConnectionString"

    Private Shared ReadOnly Property ConnectionString() As String
        Get
            Return WebConfigurationManager.ConnectionStrings(ConnectionStringName).ConnectionString
        End Get
    End Property

    Public Sub New()
        MyBase.New(ConnectionString)
        Database.SetInitializer(Of FilesDbContext)(New SampleDataInitializer())
    End Sub

    Public Property FileSystemItems() As DbSet(Of FileSystemItem)
    Public Property PermissionsSet() As DbSet(Of Permissions)

    Protected Overrides Sub OnModelCreating(ByVal modelBuilder As DbModelBuilder)
        Dim itemConfig = modelBuilder.Entity(Of FileSystemItem)()
        itemConfig.HasKey(Function(i) i.Id)
        itemConfig.Property(Function(i) i.Name).IsRequired()
        itemConfig.Property(Function(i) i.IsFolder).IsRequired()
        itemConfig.Property(Function(i) i.LastWriteTime).IsRequired()
        itemConfig.Property(Function(i) i.Size).IsRequired()
        itemConfig.Property(Function(i) i.Content).IsOptional()
        itemConfig.HasOptional(Function(i) i.Parent)

        Dim permissionConfig = modelBuilder.Entity(Of Permissions)()
        permissionConfig.HasKey(Function(p) p.Id)
        permissionConfig.HasRequired(Function(p) p.Item)
        permissionConfig.Property(Function(p) p.Rename).IsRequired()
        permissionConfig.Property(Function(p) p.Move).IsRequired()
        permissionConfig.Property(Function(p) p.Copy).IsRequired()
        permissionConfig.Property(Function(p) p.Delete).IsRequired()
        permissionConfig.Property(Function(p) p.Download).IsRequired()
        permissionConfig.Property(Function(p) p.Create).IsRequired()
        permissionConfig.Property(Function(p) p.MoveOrCopyInto).IsRequired()

        MyBase.OnModelCreating(modelBuilder)
    End Sub
End Class

Public Class DataProvider
    Private Shared Property Random() As Random

    Public Property DbContext() As FilesDbContext

    Public ReadOnly Property FileSystemItems() As IQueryable(Of FileSystemItem)
        Get
            Return DbContext.FileSystemItems
        End Get
    End Property
    Public ReadOnly Property PermissionsSet() As IQueryable(Of Permissions)
        Get
            Return DbContext.PermissionsSet
        End Get
    End Property

    Public Sub New(ByVal dbContext As FilesDbContext)
        Me.DbContext = dbContext
    End Sub

    Public Sub SaveChanges()
        DbContext.SaveChanges()
    End Sub

    Public Function FindById(ByVal id As Integer) As FileSystemItem
        Return DbContext.FileSystemItems.Find(id)
    End Function

    Public Function CreateFolder(ByVal parent As FileSystemItem, ByVal name As String) As FileSystemItem
        Dim folder = New FileSystemItem With { _
            .Name = name, _
            .Parent = parent, _
            .IsFolder = True, _
            .LastWriteTime = Date.UtcNow _
        }
        DbContext.FileSystemItems.Add(folder)
        CreateRandomFolderPermissions(folder)
        Return folder
    End Function

    'with permissions
    Public Function CreateFolder(ByVal parent As FileSystemItem, ByVal name As String, ByVal permissions As Permissions) As FileSystemItem
        Dim folder = New FileSystemItem With { _
            .Name = name, _
            .Parent = parent, _
            .IsFolder = True, _
            .LastWriteTime = Date.UtcNow _
        }
        DbContext.FileSystemItems.Add(folder)
        CreateFolderPermissions(folder, permissions)
        Return folder
    End Function
    Public Function CreateFile(ByVal parent As FileSystemItem, ByVal name As String, ByVal content As String, ByVal permissions As Permissions) As FileSystemItem
        Dim bytes() As Byte = System.Text.Encoding.Default.GetBytes(content)
        Return CreateFile(parent, name, bytes, permissions)
    End Function
    Public Function CreateFile(ByVal parent As FileSystemItem, ByVal name As String, ByVal content As Stream, ByVal permissions As Permissions) As FileSystemItem
        Dim bytes() As Byte = ReadAllBytes(content)
        Return CreateFile(parent, name, bytes, permissions)
    End Function
    Public Function CreateFile(ByVal parent As FileSystemItem, ByVal name As String, ByVal content() As Byte, ByVal permissions As Permissions) As FileSystemItem
        Dim file = New FileSystemItem With { _
            .Name = name, _
            .Parent = parent, _
            .LastWriteTime = Date.UtcNow _
        }
        file.Content = content
        file.Size = file.Content.Length
        DbContext.FileSystemItems.Add(file)
        CreateFilePermissions(file, permissions)
        Return file
    End Function
    'without permissions
    Public Function CreateFile(ByVal parent As FileSystemItem, ByVal name As String, ByVal content As String) As FileSystemItem
        Dim bytes() As Byte = System.Text.Encoding.Default.GetBytes(content)
        Return CreateFile(parent, name, bytes)
    End Function
    Public Function CreateFile(ByVal parent As FileSystemItem, ByVal name As String, ByVal content As Stream) As FileSystemItem
        Dim bytes() As Byte = ReadAllBytes(content)
        Return CreateFile(parent, name, bytes)
    End Function
    Public Function CreateFile(ByVal parent As FileSystemItem, ByVal name As String, ByVal content() As Byte) As FileSystemItem
        Dim file = New FileSystemItem With { _
            .Name = name, _
            .Parent = parent, _
            .LastWriteTime = Date.UtcNow _
        }
        file.Content = content
        file.Size = file.Content.Length
        DbContext.FileSystemItems.Add(file)
        CreateRandomFilePermissions(file)
        Return file
    End Function

    Public Sub Remove(ByVal item As FileSystemItem)
        DbContext.FileSystemItems.Remove(item)
    End Sub
    'with permissions
    Private Sub CreateFolderPermissions(ByVal folder As FileSystemItem, ByVal permissions As Permissions)
        permissions.Item = folder
        DbContext.PermissionsSet.Add(permissions)
    End Sub
    'without permissions and random permissions
    Private Sub CreateRandomFolderPermissions(ByVal folder As FileSystemItem)
        Dim permissions = New Permissions With { _
            .Rename = GetRandomBoolean(), _
            .Move = GetRandomBoolean(), _
            .Copy = GetRandomBoolean(), _
            .Delete = GetRandomBoolean(), _
            .Create = GetRandomBoolean(), _
            .Upload = GetRandomBoolean(), _
            .MoveOrCopyInto = GetRandomBoolean() _
        }
        permissions.Item = folder
        DbContext.PermissionsSet.Add(permissions)
    End Sub
    'with permissions
    Private Sub CreateFilePermissions(ByVal file As FileSystemItem, ByVal permissions As Permissions)
        permissions.Item = file
        DbContext.PermissionsSet.Add(permissions)
    End Sub
    'without permissions
    Private Sub CreateRandomFilePermissions(ByVal file As FileSystemItem)
        Dim permissions = New Permissions With { _
            .Rename = GetRandomBoolean(), _
            .Move = GetRandomBoolean(), _
            .Copy = GetRandomBoolean(), _
            .Delete = GetRandomBoolean(), _
            .Download = GetRandomBoolean() _
        }
        permissions.Item = file
        DbContext.PermissionsSet.Add(permissions)
    End Sub

    Private Function ReadAllBytes(ByVal stream As Stream) As Byte()
        Using ms As New MemoryStream()
            stream.CopyTo(ms)
            Return ms.ToArray()
        End Using
    End Function

    Private Function GetRandomBoolean() As Boolean
        If Random Is Nothing Then
            Random = New Random()
        End If
        Return Random.NextDouble() > 0.5
    End Function
End Class

Public Class SampleDataInitializer
    Inherits CreateDatabaseIfNotExists(Of FilesDbContext)

    Protected Overrides Sub Seed(ByVal context As FilesDbContext)
        MyBase.Seed(context)

        Dim data = New DataProvider(context)

        Dim root As FileSystemItem = data.CreateFolder(Nothing, "Root", New Permissions() With { _
            .Rename = True, _
            .Move = True, _
            .Copy = False, _
            .Delete = False, _
            .Download = False, _
            .Create = False, _
            .Upload = False, _
            .MoveOrCopyInto = True _
        })
        Dim folder1 As FileSystemItem = data.CreateFolder(root, "Folder 1", New Permissions() With { _
            .Rename = True, _
            .Move = True, _
            .Copy = False, _
            .Delete = True, _
            .Download = False, _
            .Create = False, _
            .Upload = False, _
            .MoveOrCopyInto = True _
        })
        Dim folder2 As FileSystemItem = data.CreateFolder(root, "Folder 2", New Permissions() With { _
            .Rename = False, _
            .Move = False, _
            .Copy = False, _
            .Delete = False, _
            .Download = False, _
            .Create = True, _
            .Upload = True, _
            .MoveOrCopyInto = True _
        })
        Dim subFolder1 As FileSystemItem = data.CreateFolder(folder1, "Subfolder 1", New Permissions() With { _
            .Rename = False, _
            .Move = False, _
            .Copy = False, _
            .Delete = True, _
            .Download = False, _
            .Create = True, _
            .Upload = True, _
            .MoveOrCopyInto = True _
        })

        data.CreateFile(folder1, "File 1.txt", "Content 1", New Permissions() With { _
            .Rename = False, _
            .Move = False, _
            .Copy = False, _
            .Delete = True, _
            .Download = True, _
            .Create = False, _
            .Upload = False, _
            .MoveOrCopyInto = False _
        })
        data.CreateFile(folder1, "File 2.txt", "Content 2", New Permissions() With { _
            .Rename = True, _
            .Move = True, _
            .Copy = False, _
            .Delete = False, _
            .Download = False, _
            .Create = False, _
            .Upload = False, _
            .MoveOrCopyInto = True _
        })
        data.CreateFile(folder1, "File 3.txt", "Content 3", New Permissions() With { _
            .Rename = True, _
            .Move = True, _
            .Copy = False, _
            .Delete = True, _
            .Download = True, _
            .Create = False, _
            .Upload = False, _
            .MoveOrCopyInto = False _
        })

        data.CreateFile(folder2, "Project 1.txt", "Content 1", New Permissions() With { _
            .Rename = False, _
            .Move = False, _
            .Copy = True, _
            .Delete = True, _
            .Download = True, _
            .Create = False, _
            .Upload = False, _
            .MoveOrCopyInto = False _
        })
        data.CreateFile(folder2, "Project 2.txt", "Content 2", New Permissions() With { _
            .Rename = True, _
            .Move = False, _
            .Copy = True, _
            .Delete = False, _
            .Download = True, _
            .Create = False, _
            .Upload = False, _
            .MoveOrCopyInto = False _
        })
        data.CreateFile(folder2, "Project 3.txt", "Content 3", New Permissions() With { _
            .Rename = False, _
            .Move = False, _
            .Copy = False, _
            .Delete = True, _
            .Download = True, _
            .Create = False, _
            .Upload = False, _
            .MoveOrCopyInto = False _
        })

        data.CreateFile(subFolder1, "Doc 1.txt", "Content 1", New Permissions() With { _
            .Rename = True, _
            .Move = False, _
            .Copy = True, _
            .Delete = True, _
            .Download = False, _
            .Create = False, _
            .Upload = False, _
            .MoveOrCopyInto = False _
        })
        data.CreateFile(subFolder1, "Doc 2.txt", "Content 2", New Permissions() With { _
            .Rename = False, _
            .Move = False, _
            .Copy = False, _
            .Delete = False, _
            .Download = True, _
            .Create = False, _
            .Upload = False, _
            .MoveOrCopyInto = False _
        })
        data.CreateFile(subFolder1, "Doc 3.txt", "Content 3", New Permissions() With { _
            .Rename = True, _
            .Move = False, _
            .Copy = False, _
            .Delete = False, _
            .Download = False, _
            .Create = False, _
            .Upload = False, _
            .MoveOrCopyInto = False _
        })

        data.SaveChanges()
    End Sub
End Class