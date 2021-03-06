using System;

using CMS.PortalControls;
using CMS.CMSHelper;

public partial class CMSWebParts_Community_Friends_FriendsList : CMSAbstractWebPart
{
    #region "Stop processing"

    /// <summary>
    /// Returns true if the control processing should be stopped.
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
            lstFriends.StopProcessing = value;
        }
    }

    #endregion


    public override void OnContentLoaded()
    {
        base.OnContentLoaded();
        SetupControl();
    }


    public override void ReloadData()
    {
        base.ReloadData();
        SetupControl();
    }


    /// <summary>
    /// Initializes the control properties.
    /// </summary>
    protected void SetupControl()
    {
        if (StopProcessing)
        {
            // Do nothing
            lstFriends.StopProcessing = true;
        }
        else
        {
            lstFriends.RedirectToAccessDeniedPage = false;
            lstFriends.UserID = CMSContext.CurrentUser.UserID;
        }
    }


    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        Visible = (CMSContext.CurrentUser.IsAuthenticated() && lstFriends.HasData());
    }
}
