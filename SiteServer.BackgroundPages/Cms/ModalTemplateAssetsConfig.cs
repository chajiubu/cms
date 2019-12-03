﻿using System;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using SiteServer.CMS.Context;
using SiteServer.CMS.Repositories;

namespace SiteServer.BackgroundPages.Cms
{
    public class ModalTemplateAssetsConfig : BasePageCms
    {
        public TextBox TbDirectoryPath;

        private string _type;
        private string _assetsDir;

        public static string GetOpenWindowString(int siteId, string type)
        {
            var name = string.Empty;
            if (type == PageTemplateAssets.TypeInclude)
            {
                name = PageTemplateAssets.NameInclude;
            }
            else if (type == PageTemplateAssets.TypeJs)
            {
                name = PageTemplateAssets.NameJs;
            }
            else if (type == PageTemplateAssets.TypeCss)
            {
                name = PageTemplateAssets.NameCss;
            }

            return LayerUtils.GetOpenScript($"{name}文件夹设置",
                PageUtils.GetCmsUrl(siteId, nameof(ModalTemplateAssetsConfig), new NameValueCollection
                {
                    {"type", type}
                }), 500, 400);
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            PageUtils.CheckRequestParameter("siteId", "type");
            _type = AuthRequest.GetQueryString("type");

            _type = AuthRequest.GetQueryString("type");
            if (_type == PageTemplateAssets.TypeInclude)
            {
                _assetsDir = Site.TemplatesAssetsIncludeDir.Trim('/');
            }
            else if (_type == PageTemplateAssets.TypeJs)
            {
                _assetsDir = Site.TemplatesAssetsJsDir.Trim('/');
            }
            else if (_type == PageTemplateAssets.TypeCss)
            {
                _assetsDir = Site.TemplatesAssetsCssDir.Trim('/');
            }

            if (string.IsNullOrEmpty(_assetsDir)) return;

            if (IsPostBack) return;

            TbDirectoryPath.Text = _assetsDir;
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            var isSuccess = false;

            try
            {
                var assetsDir = TbDirectoryPath.Text.Trim('/');
                if (_type == PageTemplateAssets.TypeInclude)
                {
                    Site.TemplatesAssetsIncludeDir = assetsDir;
                }
                else if (_type == PageTemplateAssets.TypeJs)
                {
                    Site.TemplatesAssetsJsDir = assetsDir;
                }
                else if (_type == PageTemplateAssets.TypeCss)
                {
                    Site.TemplatesAssetsCssDir = assetsDir;
                }

                DataProvider.SiteRepository.UpdateAsync(Site).GetAwaiter().GetResult();

                AuthRequest.AddSiteLogAsync(SiteId, "模板文件夹设置").GetAwaiter().GetResult();

                isSuccess = true;
            }
            catch (Exception ex)
            {
                FailMessage(ex, ex.Message);
            }

            if (isSuccess)
            {
                LayerUtils.CloseAndRedirect(Page, PageTemplateAssets.GetRedirectUrl(SiteId, _type));
            }
        }
    }
}
