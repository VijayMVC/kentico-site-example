using System;
using System.Text;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.CMSHelper;
using CMS.GlobalHelper;
using CMS.UIControls;
using CMS.OnlineMarketing;
using CMS.SettingsProvider;
using CMS.WebAnalytics;
using CMS.ExtendedControls;

public partial class CMSModules_ContactManagement_Controls_UI_Activity_List : CMSAdminListControl, ICallbackEventHandler
{
    #region "Variables"

    private Hashtable mParameters;
    private int mPageSize = -1;
    private bool modifyPermission = false;
    private int siteId = CMSContext.CurrentSiteID;
    private int ActivityID = 0;

    /// <summary>
    /// Available actions in mass action selector.
    /// </summary>
    protected enum Action
    {
        SelectAction = 0,
        Delete = 1
    }


    /// <summary>
    /// Selected objects in mass action selector.
    /// </summary>
    protected enum What
    {
        Selected = 0,
        All = 1
    }


    /// <summary>
    /// URL of the page for contact deletion.
    /// </summary>
    protected const string DELETE_PAGE = "~/CMSModules/ContactManagement/Pages/Tools/Activities/Activity/Delete.aspx";

    #endregion


    #region "Properties"

    /// <summary>
    /// Get or sets additional WHERE condition.
    /// </summary>
    public string WhereCondition
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets ORDER BY.
    /// </summary>
    public string OrderBy
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets page size.
    /// </summary>
    public int PageSize
    {
        get { return mPageSize; }
        set { mPageSize = value; }
    }


    /// <summary>
    /// Indicates if the control should perform the operations.
    /// </summary>
    public override bool StopProcessing
    {
        get
        {
            return base.StopProcessing;
        }
        set
        {
            base.StopProcessing = value;
            this.gridElem.StopProcessing = value;
        }
    }


    /// <summary>
    /// Indicates if the control is used on the live site.
    /// </summary>
    public override bool IsLiveSite
    {
        get
        {
            return base.IsLiveSite;
        }
        set
        {
            base.IsLiveSite = value;
            gridElem.IsLiveSite = value;
        }
    }


    /// <summary>
    /// True if site name column is visible.
    /// </summary>
    public bool ShowSiteNameColumn
    {
        get;
        set;
    }


    /// <summary>
    /// True if contact name is visible.
    /// </summary>
    public bool ShowContactNameColumn
    {
        get;
        set;
    }


    /// <summary>
    /// True if IP address column is visible.
    /// </summary>
    public bool ShowIPAddressColumn
    {
        get;
        set;
    }


    /// <summary>
    /// True if remove action button is visible.
    /// </summary>
    public bool ShowRemoveButton
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets contact ID
    /// </summary>
    public int ContactID
    {
        get;
        set;
    }


    /// <summary>
    /// True if contact (specified by ContactID) is merged.
    /// </summary>
    public bool IsMergedContact
    {
        get;
        set;
    }


    /// <summary>
    /// True if contact (specified by ContactID) is global.
    /// </summary>
    public bool IsGlobalContact
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets info text for empty list.
    /// </summary>
    public string ZeroRowsText
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets the callback argument.
    /// </summary>
    private string CallbackArgument
    {
        get;
        set;
    }


    /// <summary>
    /// Site ID retrieved from dialog parameters.
    /// </summary>
    public int SiteID
    {
        get
        {
            return siteId;
        }
        set
        {
            siteId = value;
        }
    }


    /// <summary>
    /// Dialog control identifier.
    /// </summary>
    private string Identifier
    {
        get
        {
            string identifier = hdnIdentificator.Value;
            if (string.IsNullOrEmpty(identifier))
            {
                identifier = Guid.NewGuid().ToString();
                hdnIdentificator.Value = identifier;
            }

            return identifier;
        }
    }

    #endregion


    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        if (PageSize >= 0)
        {
            gridElem.Pager.DefaultPageSize = PageSize;
        }
        gridElem.OrderBy = OrderBy;
        gridElem.WhereCondition = WhereCondition;
        gridElem.OnExternalDataBound += new OnExternalDataBoundEventHandler(gridElem_OnExternalDataBound);

        // Set gridelement empty list info text either to set property or default value
        gridElem.ZeroRowsText = ((ZeroRowsText == null) ? GetString("om.activities.noactivities") : GetString(ZeroRowsText));

        if (ContactID > 0)
        {
            gridElem.ObjectType = "om.activitycontactlist";
            if (IsMergedContact)
            {
                gridElem.ObjectType = "om.activitycontactmergedlist";
            }
            if (IsGlobalContact)
            {
                gridElem.ObjectType = "om.activitycontactgloballist";
            }

            QueryDataParameters parameters = new QueryDataParameters();
            parameters.AddId("@ContactID", ContactID);

            gridElem.QueryParameters = parameters;
        }

        // Check permission modify for current site only. To be able to display other sites user must be global admin = no need to check permission.
        modifyPermission = ActivityHelper.AuthorizedManageActivity(CMSContext.CurrentSiteID, false);

        ScriptHelper.RegisterDialogScript(this.Page);

        string scriptBlock = string.Format(@"
            function ViewDetails(id) {{ modalDialog('{0}' + id, 'ActivityDetails', '900px', '700px'); return false; }}",
            ResolveUrl(@"~/CMSModules/ContactManagement/Pages/Tools/Activities/Activity/Activity_Details.aspx"));

        ScriptHelper.RegisterClientScriptBlock(this, GetType(), "Actions", scriptBlock, true);
    }


    protected void Page_PreRender(object sender, EventArgs e)
    {
        gridElem.NamedColumns["sitename"].Visible = this.ShowSiteNameColumn;
        gridElem.NamedColumns["contactname"].Visible = this.ShowContactNameColumn;
        gridElem.NamedColumns["ipaddress"].Visible = this.ShowIPAddressColumn;

        // Create mass actions dropdowns
        if (gridElem.RowsCount > 0 && ShowRemoveButton && modifyPermission)
        {
            InitMassActionDDs();
            pnlFooter.Visible = true;
        }
    }


    /// <summary>
    /// Inits the mass action dropdown lists.
    /// </summary>
    private void InitMassActionDDs()
    {
        drpAction.Items.Clear();
        drpWhat.Items.Clear();
        drpAction.Items.Add(new ListItem(GetString("general." + Action.SelectAction), Convert.ToInt32(Action.SelectAction).ToString()));
        drpAction.Items.Add(new ListItem(GetString("general.delete"), Convert.ToInt32(Action.Delete).ToString()));
        drpWhat.Items.Add(new ListItem(GetString("om.activity." + What.Selected), Convert.ToInt32(What.Selected).ToString()));
        drpWhat.Items.Add(new ListItem(GetString("om.activity." + What.All), Convert.ToInt32(What.All).ToString()));
        RegisterScripts();
    }


    /// <summary>
    /// Registers JS.
    /// </summary>
    private void RegisterScripts()
    {
        ScriptHelper.RegisterDialogScript(this.Page);
        StringBuilder script = new StringBuilder();

        // Register script to open dialogs
        script.Append(@"
function Delete(queryParameters)
{
    document.location.href = '" + ResolveUrl(DELETE_PAGE) + @"' + queryParameters;
}
function Refresh()
{
    __doPostBack('" + pnlUpdate.ClientID + @"', '');
}
");
        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "GridActions", ScriptHelper.GetScript(script.ToString()));
        ScriptHelper.RegisterClientScriptBlock(this, GetType(), "ContactStatus_" + ClientID, ScriptHelper.GetScript("var dialogParams_" + ClientID + " = '';"));

        // Register script for mass actions
        script = new StringBuilder();
        script.Append(
@"
function PerformAction(selectionFunction, selectionField, actionId, actionLabel, whatId) {
    var confirmation = null;
    var label = document.getElementById(actionLabel);
    var action = document.getElementById(actionId).value;
    var whatDrp = document.getElementById(whatId);
    var selectionFieldElem = document.getElementById(selectionField);
    label.innerHTML = '';
    if (action == '", (int)Action.SelectAction, @"') {
        label.innerHTML = '", GetString("MassAction.SelectSomeAction"), @"'
    }
    else if (eval(selectionFunction) && (whatDrp.value == '", (int)What.Selected, @"')) {
        label.innerHTML = '", GetString("om.activity.massaction.select"), @"';
    }
    else {
        var param = 'massaction;' + whatDrp.value;
        if (whatDrp.value == '", (int)What.Selected, @"') {
            param = param + '#' + selectionFieldElem.value;
        }
        switch(action) {
            case '", (int)Action.Delete, @"':
                dialogParams_", ClientID, @" = param + ';", (int)Action.Delete, @"';", Page.ClientScript.GetCallbackEventReference(this, "dialogParams_" + ClientID, "Delete", null), @";
                break;
            default:
                confirmation = null;
                break;
        }
        if (confirmation != null) {
            return confirm(confirmation)
        }
    }
    return false;
}
function SelectValue_" + this.ClientID + @"(valueID) {
    document.getElementById('" + hdnIdentificator.ClientID + @"').value = valueID;" +
    ControlsHelper.GetPostBackEventReference(btnOk, null) + @";
}
");

        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "MassActions", ScriptHelper.GetScript(script.ToString()));

        // Add action to button
        btnOk.OnClientClick = "return PerformAction('" + gridElem.GetCheckSelectionScript() + "','" + gridElem.GetSelectionFieldClientID() + "','" + drpAction.ClientID + "','" + lblInfo.ClientID + "','" + drpWhat.ClientID + "');";
    }


    protected object gridElem_OnExternalDataBound(object sender, string sourceName, object parameter)
    {
        switch (sourceName)
        {
            case "view":
                ImageButton viewBtn = (ImageButton)sender;
                viewBtn.OnClientClick = string.Format("dialogParams_{0} = '{1}';{2};return false;",
                                                      ClientID,
                                                      viewBtn.CommandArgument, Page.ClientScript.GetCallbackEventReference(this, "dialogParams_" + ClientID, "ViewDetails", null));
                break;
            case "delete":
                if (!ShowRemoveButton || !modifyPermission)
                {
                    ImageButton deleteBtn = (ImageButton)sender;
                    deleteBtn.Enabled = false;
                    deleteBtn.Attributes.Add("src", GetImageUrl("Design/Controls/UniGrid/Actions/DeleteDisabled.png"));

                }
                break;
            case "acttype":
            case "acttypedesc":
                string codeName = ValidationHelper.GetString(parameter, null);
                string value = null;
                ActivityTypeInfo ati = ActivityTypeInfoProvider.GetActivityTypeInfo(codeName);
                if (ati != null)
                {
                    if (sourceName == "acttype")
                    {
                        value = ResHelper.LocalizeString(ati.ActivityTypeDisplayName);
                    }
                    else
                    {
                        value = ResHelper.LocalizeString(ati.ActivityTypeDescription);
                    }
                    return HTMLHelper.HTMLEncode(value);
                }
                return HTMLHelper.HTMLEncode(codeName);
        }
        return null;
    }

    #endregion

    /// <summary>
    /// Gets callback result.
    /// </summary>
    public string GetCallbackResult()
    {
        mParameters = new Hashtable();
        string queryString = null;

        // ...for mass action
        if (CallbackArgument.StartsWith("massaction;", StringComparison.InvariantCultureIgnoreCase))
        {
            // Get values of callback argument
            string[] selection = CallbackArgument.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            if (selection.Length != 3)
            {
                return null;
            }

            Action action = (Action)ValidationHelper.GetInteger(selection[2], 0);
            switch (action)
            {
                case Action.Delete:
                    mParameters["where"] = GetWhereCondition(selection[1]);
                    mParameters["issitemanager"] = ContactHelper.IsSiteManager;
                    break;
                default:
                    return null;
            }

            mParameters["siteid"] = SiteID;

            WindowHelper.Add(Identifier, mParameters);
            queryString = "?params=" + Identifier;
            queryString = URLHelper.AddParameterToUrl(queryString, "hash", QueryHelper.GetHash(queryString));

            return queryString;
        }
        else
        {
            mParameters["where"] = gridElem.WhereCondition;
            string sortDirection = gridElem.SortDirect;
            if (String.IsNullOrEmpty(sortDirection))
            {
                sortDirection = gridElem.OrderBy;
            }
            mParameters["orderby"] = sortDirection;
            mParameters["contactid"] = ContactID;
            mParameters["ismerged"] = IsMergedContact;
            mParameters["isglobal"] = IsGlobalContact;

            WindowHelper.Add(Identifier, mParameters);

            queryString = "?params=" + Identifier;
            queryString = URLHelper.AddParameterToUrl(queryString, "hash", QueryHelper.GetHash(queryString));
            queryString = URLHelper.AddParameterToUrl(queryString, "activityid", ActivityID.ToString());

            return queryString;
        }
    }


    /// <summary>
    /// Returns where condition depending on mass action selection.
    /// </summary>
    /// <param name="whatValue">Value of What dd-list; if the value is 'selected' it also contains selected items</param>
    private string GetWhereCondition(string whatValue)
    {
        string where = string.Empty;

        if (!string.IsNullOrEmpty(whatValue))
        {
            string selectedItems = null;
            string whatAction = null;

            if (whatValue.Contains("#"))
            {
                // Char '#' devides what-value and selected items
                whatAction = whatValue.Substring(0, whatValue.IndexOf("#"));
                selectedItems = whatValue.Substring(whatValue.IndexOf("#") + 1);
            }
            else
            {
                whatAction = whatValue;
            }

            What what = (What)ValidationHelper.GetInteger(whatAction, 0);

            switch (what)
            {
                case What.All:
                    // For all items get where condition from grid setting
                    where = gridElem.WhereCondition;
                    break;
                case What.Selected:
                    // Convert array to integer values to make sure no sql injection is possible (via string values)
                    string[] items = selectedItems.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    items = ContactHelper.GetSafeArray(items);
                    where = SqlHelperClass.GetWhereCondition<int>("ActivityID", items, false);
                    break;
            }
        }

        return where;
    }


    /// <summary>
    /// Raise callback method.
    /// </summary>
    public void RaiseCallbackEvent(string eventArgument)
    {
        CallbackArgument = eventArgument;
        ActivityID = ValidationHelper.GetInteger(eventArgument, 0);
    }
}