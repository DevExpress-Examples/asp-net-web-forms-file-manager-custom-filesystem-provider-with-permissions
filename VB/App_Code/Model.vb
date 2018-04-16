Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class FileSystemItem
    Public Property Id() As Integer
    Public Property Name() As String
    Public Property IsFolder() As Boolean
    Public Property LastWriteTime() As Date
    Public Property Size() As Long
    Public Property Content() As Byte()
    Public Property Parent() As FileSystemItem
End Class

Public Class Permissions
    Public Property Id() As Integer
    Public Overridable Property Item() As FileSystemItem
    Public Property Rename() As Boolean
    Public Property Move() As Boolean
    Public Property Copy() As Boolean
    Public Property Delete() As Boolean
    Public Property Download() As Boolean
    Public Property Create() As Boolean
    Public Property Upload() As Boolean
    Public Property MoveOrCopyInto() As Boolean
End Class