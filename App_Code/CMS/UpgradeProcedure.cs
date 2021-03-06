using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Threading;

using CMS.GlobalHelper;
using CMS.CMSHelper;
using CMS.SettingsProvider;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.SiteProvider;
using CMS.PortalEngine;
using CMS.FormEngine;
using CMS.IO;


/// <summary>
/// Class carying the code to perform the upgrade procedure.
/// </summary>
public static class UpgradeProcedure
{
    #region "Variables"

    /// <summary>
    /// Users dataset.
    /// </summary>
    public static DataSet ds = null;

    // Path to images
    private static string mAvatarsPath = null;
    // path to upgrade package
    private static string mUpgradePackagePath = null;

    private static string mWebsitePath = null;

    #endregion


    #region "Methods"

    /// <summary>
    /// Runs the update procedure.
    /// </summary>
    /// <param name="conn">Connection to use</param>
    public static void Update(GeneralConnection conn)
    {
        if (SqlHelper.IsDatabaseAvailable)
        {
            try
            {
                string version = SettingsKeyProvider.GetStringValue("CMSDataVersion");
                switch (version.ToLower())
                {
                    case "3.0":
                        Update31();
                        break;

                    case "3.1a":
                        Update40();
                        break;

                    case "4.1":
                        Update50();
                        break;

                    case "5.0":
                        Update55();
                        break;
                }
            }
            catch
            {
            }
        }
    }

    #endregion


    #region "Update 6.0"

    public static void Update60()
    {
        #region "UI Personalization"

        // Creates new UI Elements under CMS.Content module (New page personalization)
        ResourceInfo ri = ResourceInfoProvider.GetResourceInfo("CMS.Content");
        if (ri != null)
        {

            UIElementInfo content = UIElementInfoProvider.GetUIElementInfo(ri.ResourceId, "CMSContent");
            if (content != null)
            {
                int parentId = content.ElementID;

                UIElementInfo uiNew = new UIElementInfo();

                uiNew.ElementIsCustom = false;
                uiNew.ElementParentID = parentId;
                uiNew.ElementID = 0;
                uiNew.ElementDisplayName = "{$content.ui.new$}";
                uiNew.ElementName = "New";
                UIElementInfoProvider.SetUIElementInfo(uiNew);

                parentId = uiNew.ElementID;

                uiNew.ElementParentID = parentId;
                uiNew.ElementID = 0;
                uiNew.ElementDisplayName = "{$content.ui.linkexistingdoc$}";
                uiNew.ElementName = "New.LinkExistingDocument";
                UIElementInfoProvider.SetUIElementInfo(uiNew);

                uiNew.ElementParentID = parentId;
                uiNew.ElementID = 0;
                uiNew.ElementDisplayName = "{$content.ui.selecttemplate$}";
                uiNew.ElementName = "New.SelectTemplate";
                UIElementInfoProvider.SetUIElementInfo(uiNew);

                parentId = uiNew.ElementID;

                uiNew.ElementParentID = parentId;
                uiNew.ElementID = 0;
                uiNew.ElementDisplayName = "{$content.ui.usetemplate$}";
                uiNew.ElementName = "SelectTemplate.UseTemplate";
                UIElementInfoProvider.SetUIElementInfo(uiNew);

                uiNew.ElementParentID = parentId;
                uiNew.ElementID = 0;
                uiNew.ElementDisplayName = "{$content.ui.inheritfromparent$}";
                uiNew.ElementName = "SelectTemplate.InheritFromParent";
                UIElementInfoProvider.SetUIElementInfo(uiNew);

                uiNew.ElementParentID = parentId;
                uiNew.ElementID = 0;
                uiNew.ElementDisplayName = "{$content.ui.createblank$}";
                uiNew.ElementName = "SelectTemplate.CreateBlank";
                UIElementInfoProvider.SetUIElementInfo(uiNew);

            }
        }
        #endregion
    }

    #endregion


    #region "Update 5.5"

    public static void Update55()
    {
        DataClassInfo dci = null;

        #region "CMS.UserSettings"

        try
        {
            dci = DataClassInfoProvider.GetDataClass("cms.usersettings");
            if (dci != null)
            {
                CMS.FormEngine.FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    FormFieldInfo ffi = new FormFieldInfo();
                    ffi.Name = "UserFacebookID";
                    ffi.DataType = FormFieldDataTypeEnum.Text;
                    ffi.Size = 100;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.LabelControl;
                    ffi.Visible = false;
                    ffi.Caption = "UserFacebookID";
                    ffi.DefaultValue = String.Empty;
                    ffi.Description = String.Empty;
                    ffi.RegularExpression = String.Empty;

                    fi.AddFormField(ffi);

                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = CMS.DataEngine.TableManager.GetXmlSchema("CMS_UserSettings");

                    DataClassInfoProvider.SetDataClass(dci);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, false);
                }
            }

        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent("CMS.UserSettings - Update", "Update", ex);
        }

        #endregion

        #region "CMS.User"

        try
        {
            dci = DataClassInfoProvider.GetDataClass("cms.user");
            if (dci != null)
            {
                CMS.FormEngine.FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    FormFieldInfo ffi = new FormFieldInfo();
                    ffi.Name = "UserSiteManagerDisabled";
                    ffi.DataType = FormFieldDataTypeEnum.Boolean;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.CheckBoxControl;
                    ffi.Visible = false;
                    ffi.Caption = "Site manager disabled";
                    ffi.DefaultValue = "false";
                    ffi.Description = String.Empty;
                    ffi.RegularExpression = String.Empty;

                    fi.AddFormField(ffi);

                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = CMS.DataEngine.TableManager.GetXmlSchema("CMS_User");

                    DataClassInfoProvider.SetDataClass(dci);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, false);
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent("CMS.User - Update", "Update", ex);
        }

        #endregion

        #region "Ecommerce.SKU"

        // Change form definition of system tables
        try
        {
            dci = DataClassInfoProvider.GetDataClass("ecommerce.sku");
            if (dci != null)
            {
                CMS.FormEngine.FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    FormFieldInfo ffi = fi.GetFormField("SKUName");
                    if (ffi != null)
                    {
                        ffi.Size = 440;
                        ffi.Visibility = "none";
                        ffi.IsMacro = false;
                        fi.UpdateFormField("SKUName", ffi);
                    }

                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = CMS.DataEngine.TableManager.GetXmlSchema("COM_SKU");

                    DataClassInfoProvider.SetDataClass(dci);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, false);
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent("COM.SKU - Update", "Update", ex);
        }

        #endregion

        // Clear hashtables
        CMSObjectHelper.ClearHashtables();

        // Set the path to the upgrade package
        mUpgradePackagePath = HttpContext.Current.Server.MapPath("~/CMSSiteUtils/Import/upgrade_50_55.zip");

        mWebsitePath = HttpContext.Current.Server.MapPath("~/");

        // Set data version
        ObjectHelper.SetSettingsKeyValue("CMSDataVersion", "5.5");

        CMSThread thread = new CMSThread(Update55Import);
        thread.Start();
    }


    /// <summary>
    /// Ensures asynchronous import process.
    /// </summary>
    private static void Update55Import()
    {
        // Import
        try
        {
            RequestStockHelper.Remove("CurrentDomain", true);
            CMS.CMSImportExport.SiteImportSettings set = new CMS.CMSImportExport.SiteImportSettings(CMSContext.CurrentUser);
            set.DefaultProcessObjectType = CMS.CMSImportExport.ProcessObjectEnum.All;
            set.SourceFilePath = mUpgradePackagePath;
            set.WebsitePath = mWebsitePath;
            CMS.CMSImportExport.ImportProvider.ImportObjectsData(set);

            // Regenerate time zones
            TimeZoneInfoProvider.GenerateTimeZoneRules();

            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent(EventLogProvider.EVENT_TYPE_INFORMATION, DateTime.Now, "Upgrade to 5.5", "Update");
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent("Upgrade to 5.5", "Update", ex);
        }
    }

    #endregion


    #region "Update 5.0"

    public static void Update50()
    {
        #region "Schemas and queries"

        DataClassInfo dci = DataClassInfoProvider.GetDataClass("ecommerce.customer");
        if (dci != null)
        {
            SqlGenerator.GenerateDefaultQueries(dci, true, false);
        }

        dci = DataClassInfoProvider.GetDataClass("ecommerce.order");
        if (dci != null)
        {
            SqlGenerator.GenerateDefaultQueries(dci, true, false);
        }

        dci = DataClassInfoProvider.GetDataClass("ecommerce.orderitem");
        if (dci != null)
        {
            SqlGenerator.GenerateDefaultQueries(dci, true, false);
        }

        dci = DataClassInfoProvider.GetDataClass("ecommerce.shoppingcart");
        if (dci != null)
        {
            SqlGenerator.GenerateDefaultQueries(dci, true, false);
        }

        dci = DataClassInfoProvider.GetDataClass("ecommerce.shoppingcartitem");
        if (dci != null)
        {
            SqlGenerator.GenerateDefaultQueries(dci, true, false);
        }

        dci = DataClassInfoProvider.GetDataClass("cms.user");
        if (dci != null)
        {
            SqlGenerator.GenerateDefaultQueries(dci, true, false);
        }

        dci = DataClassInfoProvider.GetDataClass("ecommerce.sku");
        if (dci != null)
        {
            dci.ClassXmlSchema = CMS.DataEngine.TableManager.GetXmlSchema("COM_SKU");

            DataClassInfoProvider.SetDataClass(dci);

            // Generate queries
            SqlGenerator.GenerateDefaultQueries(dci, true, false);
        }

        #endregion

        try
        {
            dci = DataClassInfoProvider.GetDataClass("cms.usersettings");
            if (dci != null)
            {
                CMS.FormEngine.FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    FormFieldInfo ffi = new FormFieldInfo();
                    ffi.Name = "UserUsedWebParts";
                    ffi.DataType = FormFieldDataTypeEnum.Text;
                    ffi.Size = 1000;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.TextBoxControl;
                    ffi.Visible = false;
                    ffi.Caption = "UserUsedWebParts";
                    ffi.DefaultValue = String.Empty;
                    ffi.Description = String.Empty;
                    ffi.RegularExpression = String.Empty;

                    fi.AddFormField(ffi);

                    ffi = new FormFieldInfo();
                    ffi.Name = "UserUsedWidgets";
                    ffi.DataType = FormFieldDataTypeEnum.Text;
                    ffi.Size = 1000;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.TextAreaControl;
                    ffi.Visible = false;
                    ffi.Caption = "UserUsedWidgets";
                    ffi.DefaultValue = String.Empty;
                    ffi.Description = String.Empty;
                    ffi.RegularExpression = String.Empty;

                    fi.AddFormField(ffi);

                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = CMS.DataEngine.TableManager.GetXmlSchema("CMS_UserSettings");

                    DataClassInfoProvider.SetDataClass(dci);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, false);
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent("CMS.User - Update", "Update", ex);
        }

        // Clear hashtables
        CMSObjectHelper.ClearHashtables();

        // Set data version
        ObjectHelper.SetSettingsKeyValue("CMSDataVersion", "5.0");

        // Set the path to the upgrade package
        mUpgradePackagePath = HttpContext.Current.Server.MapPath("~/CMSSiteUtils/Import/upgrade_41_50.zip");

        mWebsitePath = HttpContext.Current.Server.MapPath("~/");

        CMSThread thread = new CMSThread(Update50Import);
        thread.Start();
    }


    /// <summary>
    /// Ensures asynchronous import process.
    /// </summary>
    private static void Update50Import()
    {
        // Import
        try
        {
            RequestStockHelper.Remove("CurrentDomain", true);
            CMS.CMSImportExport.SiteImportSettings set = new CMS.CMSImportExport.SiteImportSettings(CMSContext.CurrentUser);
            set.DefaultProcessObjectType = CMS.CMSImportExport.ProcessObjectEnum.All;
            set.SourceFilePath = mUpgradePackagePath;
            set.WebsitePath = mWebsitePath;
            CMS.CMSImportExport.ImportProvider.ImportObjectsData(set);

        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent("Upgrade to 5.0", "Update", ex);
        }
    }

    #endregion


    #region "Update 4.0"

    public static void Update40()
    {

        #region "ecommerce.sku"

        // Change form definition of system tables
        try
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClass("ecommerce.sku");
            if (dci != null)
            {
                CMS.FormEngine.FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    FormFieldInfo ffi = new FormFieldInfo();
                    ffi.Name = "SKUCreated";
                    ffi.DataType = FormFieldDataTypeEnum.DateTime;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.LabelControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = CMS.DataEngine.TableManager.GetXmlSchema("COM_SKU");

                    DataClassInfoProvider.SetDataClass(dci);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, false);
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent("COM.SKU - Update", "Update", ex);
        }

        #endregion

        #region "cms.user"

        try
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClass("cms.user");
            if (dci != null)
            {
                CMS.FormEngine.FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    // Delete removed form field infos
                    fi.RemoveFormField("UserNickName");
                    fi.RemoveFormField("UserPicture");
                    fi.RemoveFormField("UserSignature");
                    fi.RemoveFormField("UserUrlReferrer");
                    fi.RemoveFormField("UserCampaign");
                    fi.RemoveFormField("UserCustomData");

                    FormFieldInfo ffi = new FormFieldInfo();
                    ffi.Name = "UserLastLogonInfo";
                    ffi.DataType = FormFieldDataTypeEnum.LongText;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.TextAreaControl;
                    ffi.Visible = false;
                    ffi.Caption = "UserLastLogonInfo";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    ffi = new FormFieldInfo();
                    ffi.Name = "UserIsHidden";
                    ffi.DataType = FormFieldDataTypeEnum.Boolean;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.CheckBoxControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "false";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    ffi = new FormFieldInfo();
                    ffi.Name = "UserVisibility";
                    ffi.DataType = FormFieldDataTypeEnum.LongText;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.TextAreaControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = CMS.DataEngine.TableManager.GetXmlSchema("CMS_User");

                    DataClassInfoProvider.SetDataClass(dci);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, false);


                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent("CMS.User - Update", "Update", ex);
        }



        #endregion

        #region "cms.blogpost"

        // Change form definition of blog document type
        try
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClass("cms.blogpost");
            if (dci != null)
            {
                FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    // Document tags
                    FormFieldInfo ffi = new FormFieldInfo();
                    ffi.Name = "DocumentTags";
                    ffi.Caption = "Tags";
                    ffi.Visible = true;
                    ffi.DataType = FormFieldDataTypeEnum.LongText;
                    ffi.FieldType = FormFieldControlTypeEnum.CustomUserControl;
                    ffi.AllowEmpty = true;
                    ffi.PrimaryKey = false;
                    ffi.System = true;
                    ffi.Description = "Tags which should be assigned to the current blog post.";
                    ffi.SpellCheck = false;
                    ffi.External = true;
                    ffi.Settings["controlname"] = "tagselector";
                    fi.AddFormField(ffi);

                    // Pinged URLs
                    ffi = new FormFieldInfo();
                    ffi.Name = "BlogPostPingedUrls";
                    ffi.Caption = "Already pinged";
                    ffi.Visible = true;
                    ffi.DataType = FormFieldDataTypeEnum.LongText;
                    ffi.FieldType = FormFieldControlTypeEnum.CustomUserControl;
                    ffi.AllowEmpty = true;
                    ffi.PrimaryKey = false;
                    ffi.System = false;
                    ffi.SpellCheck = false;
                    ffi.Settings["controlname"] = "trackbackspingedurls";
                    fi.AddFormField(ffi);

                    // Not pinged URLs
                    ffi = new FormFieldInfo();
                    ffi.Name = "BlogPostNotPingedUrls";
                    ffi.Caption = "Send trackbacks to";
                    ffi.Visible = true;
                    ffi.DataType = FormFieldDataTypeEnum.LongText;
                    ffi.FieldType = FormFieldControlTypeEnum.CustomUserControl;
                    ffi.AllowEmpty = true;
                    ffi.PrimaryKey = false;
                    ffi.System = false;
                    ffi.SpellCheck = false;
                    ffi.Settings["controlname"] = "trackbacksnotpingedurls";
                    fi.AddFormField(ffi);

                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = TableManager.GetXmlSchema("CONTENT_BlogPost");

                    DataClassInfoProvider.SetDataClass(dci);

                    //Regenerate view
                    SqlGenerator.GenerateDefaultView(dci, null);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, true);
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent("CMS.Blog - Update", "Update", ex);
        }

        #endregion

        #region "cms.blog"

        // Change form definition of blog post document type
        try
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClass("cms.blog");
            if (dci != null)
            {
                FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    // BlogRequireEmails
                    FormFieldInfo ffi = new FormFieldInfo();
                    ffi.Name = "BlogRequireEmails";
                    ffi.Caption = "Require e-mail addresses";
                    ffi.Visible = true;
                    ffi.DefaultValue = "0";
                    ffi.DataType = FormFieldDataTypeEnum.Boolean;
                    ffi.FieldType = FormFieldControlTypeEnum.CheckBoxControl;
                    ffi.AllowEmpty = true;
                    ffi.PrimaryKey = false;
                    ffi.System = false;
                    ffi.SpellCheck = false;
                    fi.AddFormField(ffi);

                    // BlogUnsubscriptionUrl
                    ffi = new FormFieldInfo();
                    ffi.Name = "BlogUnsubscriptionUrl";
                    ffi.Caption = "Unsubscription URL";
                    ffi.Visible = true;
                    ffi.DataType = FormFieldDataTypeEnum.Text;
                    ffi.FieldType = FormFieldControlTypeEnum.TextBoxControl;
                    ffi.Size = 250;
                    ffi.AllowEmpty = true;
                    ffi.PrimaryKey = false;
                    ffi.System = false;
                    ffi.SpellCheck = false;
                    fi.AddFormField(ffi);

                    // BlogEnableSubscriptions
                    ffi = new FormFieldInfo();
                    ffi.Name = "BlogEnableSubscriptions";
                    ffi.Caption = "Enable subscriptions";
                    ffi.Visible = true;
                    ffi.DefaultValue = "0";
                    ffi.DataType = FormFieldDataTypeEnum.Boolean;
                    ffi.FieldType = FormFieldControlTypeEnum.CheckBoxControl;
                    ffi.AllowEmpty = true;
                    ffi.PrimaryKey = false;
                    ffi.System = false;
                    ffi.SpellCheck = false;
                    fi.AddFormField(ffi);

                    // BlogModerators
                    ffi = new FormFieldInfo();
                    ffi.Name = "BlogModerators";
                    ffi.Caption = "Blog moderators";
                    ffi.Visible = true;
                    ffi.DataType = FormFieldDataTypeEnum.Text;
                    ffi.Size = 450;
                    ffi.FieldType = FormFieldControlTypeEnum.CustomUserControl;
                    ffi.AllowEmpty = true;
                    ffi.PrimaryKey = false;
                    ffi.System = false;
                    ffi.SpellCheck = false;
                    ffi.Settings["controlname"] = "multipleuserselector";
                    fi.AddFormField(ffi);


                    // BlogEnableTrackbacks
                    ffi = new FormFieldInfo();
                    ffi.Name = "BlogEnableTrackbacks";
                    ffi.Caption = "Enable trackbacks";
                    ffi.Visible = true;
                    ffi.DefaultValue = "1";
                    ffi.DataType = FormFieldDataTypeEnum.Boolean;
                    ffi.FieldType = FormFieldControlTypeEnum.CheckBoxControl;
                    ffi.AllowEmpty = true;
                    ffi.PrimaryKey = false;
                    ffi.System = false;
                    ffi.SpellCheck = false;
                    fi.AddFormField(ffi);


                    // BlogTeaser
                    ffi = new FormFieldInfo();
                    ffi.Name = "BlogTeaser";
                    ffi.Caption = "Blog teaser image";
                    ffi.Visible = true;
                    ffi.DefaultValue = "";
                    ffi.DataType = FormFieldDataTypeEnum.File;
                    ffi.FieldType = FormFieldControlTypeEnum.UploadControl;
                    ffi.AllowEmpty = true;
                    ffi.PrimaryKey = false;
                    ffi.System = false;
                    ffi.SpellCheck = false;
                    fi.AddFormField(ffi);


                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = TableManager.GetXmlSchema("CONTENT_Blog");

                    DataClassInfoProvider.SetDataClass(dci);

                    // Re-generate view
                    SqlGenerator.GenerateDefaultView(dci, null);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, true);
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent("CMS.BlogPost - Update", "Update", ex);
        }

        #endregion


        ObjectHelper.SetSettingsKeyValue("CMSDataVersion", "4.0");

        // Import
        try
        {
            CMS.CMSImportExport.SiteImportSettings set = new CMS.CMSImportExport.SiteImportSettings(CMSContext.CurrentUser);
            set.DefaultProcessObjectType = CMS.CMSImportExport.ProcessObjectEnum.All;
            set.SourceFilePath = HttpContext.Current.Server.MapPath("~/CMSSiteUtils/Import/upgrade_31a_40.zip");
            CMS.CMSImportExport.ImportProvider.ImportObjectsData(set);

        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent("Upgrade to 4.0", "Update", ex);
        }

        #region "Avatars"

        ds = UserSettingsInfoProvider.GetUserSettings("UserPicture IS NOT NULL", null);
        if (!DataHelper.DataSourceIsEmpty(ds))
        {

            mAvatarsPath = HttpContext.Current.Request.MapPath("~/CMSGlobalFiles/UserPictures/");
            Thread uploader = new Thread(UploadAvatars);
            uploader.Start();

        }

        #endregion
    }


    /// <summary>
    /// Upload avatars defined in dataset.
    /// </summary>
    public static void UploadAvatars()
    {
        try
        {
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string userPicture = ValidationHelper.GetString(dr["UserPicture"], string.Empty);
                    int userId = ValidationHelper.GetInteger(dr["UserSettingsUserID"], 0);

                    if ((userPicture.Contains("/")) && (userId > 0))
                    {
                        string[] values = userPicture.Split('/');
                        if ((values != null) && (values.Length == 2))
                        {
                            string realFileName = values[0];
                            string fileName = values[0];

                            string path = mAvatarsPath + realFileName;
                            if (File.Exists(path))
                            {
                                AvatarInfo ai = new AvatarInfo(path);
                                ai.AvatarIsCustom = true;
                                ai.AvatarType = "user";
                                AvatarInfoProvider.SetAvatarInfo(ai);
                                UserSettingsInfo ui = new UserSettingsInfo(dr);
                                ui.UserAvatarID = ai.AvatarID;
                                UserSettingsInfoProvider.SetUserSettingsInfo(ui);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent("Avatar upgrade", "Update", ex);
        }

    }


    #endregion


    #region "Update 3.1"

    private static string mUpdate31Error = "Upgrade to 3.1";

    private static void Update31()
    {
        // Change form definition of system tables
        try
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClass("ecommerce.order");
            if (dci != null)
            {
                CMS.FormEngine.FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    FormFieldInfo ffi = new FormFieldInfo();
                    ffi.Name = "OrderGUID";
                    ffi.DataType = FormFieldDataTypeEnum.File;
                    ffi.AllowEmpty = false;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.UploadFile;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    ffi = new FormFieldInfo();
                    ffi.Name = "OrderLastModified";
                    ffi.DataType = FormFieldDataTypeEnum.DateTime;
                    ffi.AllowEmpty = false;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.CalendarControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = CMS.DataEngine.TableManager.GetXmlSchema("COM_Order");

                    DataClassInfoProvider.SetDataClass(dci);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, false);
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent(mUpdate31Error, "Update", ex);
        }

        try
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClass("ecommerce.orderitem");
            if (dci != null)
            {
                CMS.FormEngine.FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    FormFieldInfo ffi = new FormFieldInfo();
                    ffi.Name = "OrderItemGuid";
                    ffi.DataType = FormFieldDataTypeEnum.File;
                    ffi.AllowEmpty = false;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.UploadFile;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    ffi = new FormFieldInfo();
                    ffi.Name = "OrderItemParentGuid";
                    ffi.DataType = FormFieldDataTypeEnum.File;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.UploadFile;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    ffi = new FormFieldInfo();
                    ffi.Name = "OrderItemLastModified";
                    ffi.DataType = FormFieldDataTypeEnum.DateTime;
                    ffi.AllowEmpty = false;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.CalendarControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    ffi = fi.GetFormField("OrderItemSKUName");
                    if (ffi != null)
                    {
                        ffi.Size = 450;
                        fi.UpdateFormField("OrderItemSKUName", ffi);
                    }

                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = CMS.DataEngine.TableManager.GetXmlSchema("COM_OrderItem");

                    DataClassInfoProvider.SetDataClass(dci);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, false);
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent(mUpdate31Error, "Update", ex);
        }


        try
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClass("Ecommerce.SKU");
            if (dci != null)
            {
                CMS.FormEngine.FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    FormFieldInfo ffi = new FormFieldInfo();
                    ffi.Name = "SKUOptionCategoryID";
                    ffi.DataType = FormFieldDataTypeEnum.Integer;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.LabelControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    ffi = new FormFieldInfo();
                    ffi.Name = "SKUOrder";
                    ffi.DataType = FormFieldDataTypeEnum.Integer;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.LabelControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    ffi = new FormFieldInfo();
                    ffi.Name = "SKULastModified";
                    ffi.DataType = FormFieldDataTypeEnum.DateTime;
                    ffi.AllowEmpty = false;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.CalendarControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    ffi = fi.GetFormField("SKUName");
                    if (ffi != null)
                    {
                        ffi.Size = 450;
                        fi.UpdateFormField("SKUName", ffi);
                    }

                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = CMS.DataEngine.TableManager.GetXmlSchema("COM_SKU");

                    DataClassInfoProvider.SetDataClass(dci);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, false);
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent(mUpdate31Error, "Update", ex);
        }

        try
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClass("Ecommerce.Customer");
            if (dci != null)
            {
                CMS.FormEngine.FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    FormFieldInfo ffi = new FormFieldInfo();
                    ffi.Name = "CustomerLastModified";
                    ffi.DataType = FormFieldDataTypeEnum.DateTime;
                    ffi.AllowEmpty = false;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.CalendarControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = CMS.DataEngine.TableManager.GetXmlSchema("COM_Customer");

                    DataClassInfoProvider.SetDataClass(dci);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, false);
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent(mUpdate31Error, "Update", ex);
        }


        try
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClass("newsletter.subscriber");
            if (dci != null)
            {
                CMS.FormEngine.FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    FormFieldInfo ffi = new FormFieldInfo();
                    ffi.Name = "SubscriberType";
                    ffi.DataType = FormFieldDataTypeEnum.Text;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.LabelControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";
                    ffi.Size = 100;

                    fi.AddFormField(ffi);

                    ffi = new FormFieldInfo();
                    ffi.Name = "SubscriberRelatedID";
                    ffi.DataType = FormFieldDataTypeEnum.Integer;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.LabelControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    ffi = new FormFieldInfo();
                    ffi.Name = "SubscriberLastModified";
                    ffi.DataType = FormFieldDataTypeEnum.DateTime;
                    ffi.AllowEmpty = false;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.CalendarControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    ffi = fi.GetFormField("SubscriberEmail");
                    if (ffi != null)
                    {
                        ffi.AllowEmpty = true;
                        fi.UpdateFormField("SubscriberEmail", ffi);
                    }

                    ffi = fi.GetFormField("SubscriberFullName");
                    if (ffi != null)
                    {
                        ffi.Size = 450;
                        fi.UpdateFormField("SubscriberFullName", ffi);
                    }

                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = CMS.DataEngine.TableManager.GetXmlSchema("Newsletter_Subscriber");

                    DataClassInfoProvider.SetDataClass(dci);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, false);
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent(mUpdate31Error, "Update", ex);
        }


        try
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClass("cms.user");
            if (dci != null)
            {
                CMS.FormEngine.FormInfo fi = FormHelper.GetFormInfo(dci.ClassName, true);
                if (fi != null)
                {
                    FormFieldInfo ffi = new FormFieldInfo();
                    ffi.Name = "UserMessagingNotificationEmail";
                    ffi.DataType = FormFieldDataTypeEnum.Text;
                    ffi.AllowEmpty = true;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.LabelControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";
                    ffi.Size = 100;

                    fi.AddFormField(ffi);

                    ffi = new FormFieldInfo();
                    ffi.Name = "UserGUID";
                    ffi.DataType = FormFieldDataTypeEnum.File;
                    ffi.AllowEmpty = false;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.UploadFile;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    ffi = new FormFieldInfo();
                    ffi.Name = "UserLastModified";
                    ffi.DataType = FormFieldDataTypeEnum.DateTime;
                    ffi.AllowEmpty = false;
                    ffi.PublicField = false;
                    ffi.System = true;
                    ffi.FieldType = CMS.FormEngine.FormFieldControlTypeEnum.CalendarControl;
                    ffi.Visible = false;
                    ffi.Caption = "";
                    ffi.DefaultValue = "";
                    ffi.Description = "";
                    ffi.RegularExpression = "";

                    fi.AddFormField(ffi);

                    ffi = fi.GetFormField("UserFullName");
                    if (ffi != null)
                    {
                        ffi.Size = 450;
                        fi.UpdateFormField("UserFullName", ffi);
                    }

                    dci.ClassFormDefinition = fi.GetXmlDefinition();
                    dci.ClassXmlSchema = CMS.DataEngine.TableManager.GetXmlSchema("CMS_User");

                    DataClassInfoProvider.SetDataClass(dci);

                    // Generate queries
                    SqlGenerator.GenerateDefaultQueries(dci, true, false);
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent(mUpdate31Error, "Update", ex);
        }

        // Import
        try
        {
            CMS.CMSImportExport.SiteImportSettings set = new CMS.CMSImportExport.SiteImportSettings(CMSContext.CurrentUser);
            set.DefaultProcessObjectType = CMS.CMSImportExport.ProcessObjectEnum.All;
            set.SourceFilePath = HttpContext.Current.Server.MapPath("~/CMSSiteUtils/Import/upgrade_30_31.zip");
            CMS.CMSImportExport.ImportProvider.ImportObjectsData(set);
        }
        catch (Exception ex)
        {
            EventLogProvider evp = new EventLogProvider();
            evp.LogEvent(mUpdate31Error, "Update", ex);
        }

        ObjectHelper.SetSettingsKeyValue("CMSDataVersion", "3.1");
    }

    #endregion
}
