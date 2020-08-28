<%@ Page Language="vb" AutoEventWireup="true" CodeFile="Default.aspx.vb" Inherits="_Default" %>

<%@ Register Assembly="DevExpress.Web.v17.2, Version=17.2.15.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.ASPxTreeList.v17.2, Version=17.2.15.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxTreeList" TagPrefix="dx" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script>
        function onFocusedNodeChanged(s, e) {
            grid.PerformCallback(s.GetFocusedNodeKey());
        }
		var command;
        function onBeginCallback(s, e) {
            if (e.command === "UPDATEEDIT")
                command = e.command;
            else
                command = undefined;
        }
        function onEndCallback(s, e) {
            if (command === "UPDATEEDIT")
				fileManger.Refresh();
			if (s.cp_SetVisible == true)
				s.SetVisible(true);
			else
				s.SetVisible(false);
		}
		function onFileUploaded(s, e) {
			RefreshTreeAndGrid();
		}
		function onFolderCreated(s, e) {
			RefreshTreeAndGrid();
		}
		function onItemCopied(s, e) {
			RefreshTreeAndGrid();
		}
		function onItemDeleted(s, e) {
			RefreshTreeAndGrid();
		}
		function onItemMoved(s, e) {
			RefreshTreeAndGrid();
		}
		function onItemRenamed(s, e) {
			RefreshTreeAndGrid();
		}
		function RefreshTreeAndGrid() {
			treeList.PerformCallback();
			grid.Refresh();
		}
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <dx:ASPxFileManager runat="server" ID="FileManager" ClientInstanceName="fileManger" 
            CustomFileSystemProviderTypeName="CustomFileSystemProvider" Height="340"
            OnFileUploading="ASPxFileManager1_FileUploading"
            OnFolderCreating="ASPxFileManager1_FolderCreating"
            OnItemDeleting="ASPxFileManager1_ItemDeleting"
            OnItemMoving="ASPxFileManager1_ItemMoving"
            OnItemRenaming="ASPxFileManager1_ItemRenaming"
            OnItemCopying="ASPxFileManager1_ItemCopying">
            <Settings ThumbnailFolder="~/Thumb" />
            <SettingsEditing AllowCopy="true" AllowMove="true" AllowRename="true" AllowDelete="true"
                AllowCreate="true" AllowDownload="true" />
            <SettingsUpload Enabled="true" />
            <SettingsPermissions>
                <AccessRules>
                    <dx:FileManagerFolderAccessRule Edit="Deny" />
                    <dx:FileManagerFileAccessRule Path="*" Download="Deny" />
                </AccessRules>
            </SettingsPermissions>
			<ClientSideEvents FileUploaded="onFileUploaded" FolderCreated="onFolderCreated" ItemCopied="onItemCopied"
				ItemDeleted="onItemDeleted" ItemMoved="onItemMoved" ItemRenamed="onItemRenamed" />
        </dx:ASPxFileManager>
        <br />
        Use this treelist and grid to change permissions of files and folders on the fly
        <br />
        <br />
        <table>
            <tr>
                <td>
                    <dx:ASPxTreeList ID="tree" runat="server" DataSourceID="ds" KeyFieldName="Id" ClientInstanceName="treeList"
                         ParentFieldName="Parent.Id"
                        AutoGenerateColumns="false" OnDataBound="tree_DataBound" Width="450" OnCustomCallback="tree_CustomCallback">
                        <Columns>
                            <dx:TreeListDataColumn FieldName="Name"></dx:TreeListDataColumn>
                            <dx:TreeListDataColumn FieldName="IsFolder"></dx:TreeListDataColumn>
                        </Columns>
                        <SettingsBehavior AllowFocusedNode="true" />
                        <ClientSideEvents FocusedNodeChanged="onFocusedNodeChanged" />
                    </dx:ASPxTreeList>

                </td>
                <td>&nbsp;
                    <br />
                </td>
                <td>
                    <dx:ASPxGridView ID="grid" runat="server" DataSourceID="dsPermissions" ClientInstanceName="grid" OnCustomCallback="grid_CustomCallback"
                        KeyFieldName="Id" AutoGenerateColumns="true" OnDataBound="grid_DataBound" OnBatchUpdate="grid_BatchUpdate" OnRowValidating="grid_RowValidating"
						ClientVisible="false">
                        <SettingsEditing Mode="Batch"></SettingsEditing>
                        <ClientSideEvents BeginCallback="onBeginCallback" EndCallback="onEndCallback" />
                    </dx:ASPxGridView>
                </td>
            </tr>
        </table>
        <ef:EntityDataSource ID="ds" runat="server" ContextTypeName="FilesDbContext" EntitySetName="FileSystemItems"></ef:EntityDataSource>
        <ef:EntityDataSource ID="dsPermissions" runat="server" ContextTypeName="FilesDbContext" 
            EntitySetName="PermissionsSet" Where="it.Item.Id = @IdParam" EnableUpdate="true">
            <WhereParameters>
                <asp:SessionParameter Name="IdParam" SessionField="IdParam" Type="Int32" DefaultValue="1" />
            </WhereParameters>
        </ef:EntityDataSource>
    </form>
</body>
</html>