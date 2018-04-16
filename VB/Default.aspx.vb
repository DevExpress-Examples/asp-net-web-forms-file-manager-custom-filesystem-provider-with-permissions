Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports DevExpress.Web
Imports System.Data.SqlClient
Imports System.Web.Configuration
Imports System.Data

Partial Public Class _Default
    Inherits System.Web.UI.Page

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As EventArgs)

    End Sub
    Protected Sub tree_DataBound(ByVal sender As Object, ByVal e As EventArgs)
        tree.ExpandAll()
    End Sub
    Private IsItemIdFocused As Boolean = False
    Protected Sub grid_CustomCallback(ByVal sender As Object, ByVal e As ASPxGridViewCustomCallbackEventArgs)
        Session("IdParam") = e.Parameters
        grid.DataBind()
    End Sub

    Protected Sub grid_DataBound(ByVal sender As Object, ByVal e As EventArgs)
        grid.Columns("Id").Visible = False
        grid.Columns("Item.Id").Visible = False
        grid.Columns("Item").Visible = False

        If CBool(tree.FocusedNode("IsFolder")) Then
            grid.Columns("Download").Visible = False
            grid.Columns("Create").Visible = True
            grid.Columns("Upload").Visible = True
            grid.Columns("MoveOrCopyInto").Visible = True
        Else
            grid.Columns("Download").Visible = True
            grid.Columns("Create").Visible = False
            grid.Columns("Upload").Visible = False
            grid.Columns("MoveOrCopyInto").Visible = False
        End If
        If Session("IdParam") Is Nothing OrElse Session("IdParam").ToString() = "1" Then
            grid.JSProperties("cp_SetVisible") = False
        Else
            grid.JSProperties("cp_SetVisible") = True
        End If
    End Sub


    'the following operations are cancelled - to allow them, remove the ValidateSiteMode method
    Protected Sub ASPxFileManager1_ItemDeleting(ByVal source As Object, ByVal e As DevExpress.Web.FileManagerItemDeleteEventArgs)
        ValidateSiteMode(e)
    End Sub
    Protected Sub ASPxFileManager1_ItemMoving(ByVal source As Object, ByVal e As DevExpress.Web.FileManagerItemMoveEventArgs)
        ValidateSiteMode(e)
    End Sub
    Protected Sub ASPxFileManager1_ItemRenaming(ByVal source As Object, ByVal e As DevExpress.Web.FileManagerItemRenameEventArgs)
        ValidateSiteMode(e)
    End Sub
    Protected Sub ASPxFileManager1_FolderCreating(ByVal source As Object, ByVal e As DevExpress.Web.FileManagerFolderCreateEventArgs)
        ValidateSiteMode(e)
    End Sub
    Protected Sub ASPxFileManager1_FileUploading(ByVal source As Object, ByVal e As DevExpress.Web.FileManagerFileUploadEventArgs)
        ValidateSiteMode(e)

    End Sub
    Protected Sub ASPxFileManager1_ItemCopying(ByVal source As Object, ByVal e As FileManagerItemCopyEventArgs)
        ValidateSiteMode(e)
    End Sub
    Protected Sub ValidateSiteMode(ByVal e As FileManagerActionEventArgsBase)
        e.Cancel = True
        e.ErrorText = "Data modifications are not allowed in the example."
    End Sub

    Protected Sub grid_BatchUpdate(ByVal sender As Object, ByVal e As DevExpress.Web.Data.ASPxDataBatchUpdateEventArgs)
        'data modifications are not allowed in examples - to enable them, remove the following code
        e.Handled = True
    End Sub

    Protected Sub grid_RowValidating(ByVal sender As Object, ByVal e As DevExpress.Web.Data.ASPxDataValidationEventArgs)
        e.RowError = "Data modifications are not allowed in examples"
    End Sub

    Protected Sub tree_CustomCallback(ByVal sender As Object, ByVal e As DevExpress.Web.ASPxTreeList.TreeListCustomCallbackEventArgs)
        tree.Nodes(0).Focus()
        tree.DataBind()
    End Sub
End Class