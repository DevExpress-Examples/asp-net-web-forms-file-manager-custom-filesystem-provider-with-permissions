using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevExpress.Web;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;

public partial class _Default : System.Web.UI.Page {
	protected void Page_Init(object sender, EventArgs e) {

	}
    protected void tree_DataBound(object sender, EventArgs e) {
        tree.ExpandAll();
    }
    bool IsItemIdFocused = false;
    protected void grid_CustomCallback(object sender, ASPxGridViewCustomCallbackEventArgs e) {
        Session["IdParam"] = e.Parameters;
        grid.DataBind();
    }

    protected void grid_DataBound(object sender, EventArgs e) {
        grid.Columns["Id"].Visible = false;
        grid.Columns["Item.Id"].Visible = false;
        grid.Columns["Item"].Visible = false;

        if((bool)tree.FocusedNode["IsFolder"]) {
            grid.Columns["Download"].Visible = false;
            grid.Columns["Create"].Visible = true;
            grid.Columns["Upload"].Visible = true;
            grid.Columns["MoveOrCopyInto"].Visible = true;
        }
        else {
            grid.Columns["Download"].Visible = true;
            grid.Columns["Create"].Visible = false;
            grid.Columns["Upload"].Visible = false;
            grid.Columns["MoveOrCopyInto"].Visible = false;
        }
		if (Session["IdParam"] == null || Session["IdParam"].ToString() == "1")
			grid.JSProperties["cp_SetVisible"] = false;
		else
			grid.JSProperties["cp_SetVisible"] = true;
	}


	//the following operations are cancelled - to allow them, remove the ValidateSiteMode method
	protected void ASPxFileManager1_ItemDeleting(object source, DevExpress.Web.FileManagerItemDeleteEventArgs e) {
		ValidateSiteMode(e);
    }
    protected void ASPxFileManager1_ItemMoving(object source, DevExpress.Web.FileManagerItemMoveEventArgs e) {
		ValidateSiteMode(e);
    }
    protected void ASPxFileManager1_ItemRenaming(object source, DevExpress.Web.FileManagerItemRenameEventArgs e) {
		ValidateSiteMode(e);
    }
    protected void ASPxFileManager1_FolderCreating(object source, DevExpress.Web.FileManagerFolderCreateEventArgs e) {
		ValidateSiteMode(e);
    }
    protected void ASPxFileManager1_FileUploading(object source, DevExpress.Web.FileManagerFileUploadEventArgs e) {
		ValidateSiteMode(e);

    }
    protected void ASPxFileManager1_ItemCopying(object source, FileManagerItemCopyEventArgs e) {
		ValidateSiteMode(e);
    }
    protected void ValidateSiteMode(FileManagerActionEventArgsBase e) {
        e.Cancel = true;
        e.ErrorText = "Data modifications are not allowed in the example.";
    }

    protected void grid_BatchUpdate(object sender, DevExpress.Web.Data.ASPxDataBatchUpdateEventArgs e) {
		//data modifications are not allowed in examples - to enable them, remove the following code
		e.Handled = true;
    }

    protected void grid_RowValidating(object sender, DevExpress.Web.Data.ASPxDataValidationEventArgs e) {
        e.RowError = "Data modifications are not allowed in examples";
    }

	protected void tree_CustomCallback(object sender, DevExpress.Web.ASPxTreeList.TreeListCustomCallbackEventArgs e) {
		tree.Nodes[0].Focus();
		tree.DataBind();
	}
}