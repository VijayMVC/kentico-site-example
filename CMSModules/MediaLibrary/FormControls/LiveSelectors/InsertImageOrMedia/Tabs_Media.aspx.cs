using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.UIControls;
using CMS.GlobalHelper;
using CMS.ExtendedControls;
using CMS.CMSHelper;
using CMS.SiteProvider;
using CMS.SettingsProvider;

public partial class CMSModules_MediaLibrary_FormControls_LiveSelectors_InsertImageOrMedia_Tabs_Media : CMSLiveModalPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (QueryHelper.ValidateHash("hash"))
        {
            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.MediaLibrary", CMSContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.MediaLibrary");
            }

            string output = QueryHelper.GetString("output", "");

            bool checkUI = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CKEditor:PersonalizeToolbarOnLiveSite"], false);
            if ((output == "copy") || (output == "move") || (output == "relationship") || (output == "selectpath"))
            {
                checkUI = false;
            }

            if (checkUI)
            {
                string errorMessage = "";

                OutputFormatEnum outputFormat = CMSDialogHelper.GetOutputFormat(output, QueryHelper.GetBoolean("link", false));
                if ((outputFormat == OutputFormatEnum.HTMLLink) && !CMSContext.CurrentUser.IsAuthorizedPerUIElement("CMS.WYSIWYGEditor", "InsertLink"))
                {
                    errorMessage = "InsertLink";
                }
                else if ((outputFormat == OutputFormatEnum.HTMLMedia) && !CMSContext.CurrentUser.IsAuthorizedPerUIElement("CMS.WYSIWYGEditor", "InsertImageOrMedia"))
                {
                    errorMessage = "InsertImageOrMedia";
                }

                if (errorMessage != "")
                {
                    RedirectToCMSDeskUIElementAccessDenied("CMS.WYSIWYGEditor", errorMessage);
                }

                if (!CMSContext.CurrentUser.IsAuthorizedPerUIElement("CMS.MediaDialog", "MediaLibrariesTab"))
                {
                    errorMessage = "MediaLibrariesTab";
                }

                if (errorMessage != "")
                {
                    RedirectToCMSDeskUIElementAccessDenied("CMS.MediaDialog", errorMessage);
                }
            }

            ScriptHelper.RegisterJQuery(this.Page);
            CMSDialogHelper.RegisterDialogHelper(this.Page);

            this.linkMedia.InitFromQueryString();
        }
        else
        {
            this.linkMedia.StopProcessing = true;
        }
    }
}
