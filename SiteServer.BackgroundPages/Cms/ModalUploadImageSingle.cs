﻿using System;
using System.Collections.Specialized;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using SiteServer.CMS.Context;
using SiteServer.Abstractions;
using SiteServer.CMS.Context.Enumerations;
using SiteServer.CMS.Core;

namespace SiteServer.BackgroundPages.Cms
{
    public class ModalUploadImageSingle : BasePageCms
    {
        public HtmlInputFile HifUpload;
        public Literal LtlScript;

        private string _currentRootPath;
        private string _textBoxClientId;
        //是否需要水印（广告物料不需要）
        private bool _isNeedWaterMark = true;

        protected override bool IsSinglePage => true;

        public static string GetOpenWindowStringToTextBox(int siteId, string textBoxClientId)
        {
            return LayerUtils.GetOpenScript("上传图片", PageUtils.GetCmsUrl(siteId, nameof(ModalUploadImageSingle), new NameValueCollection
            {
                {"TextBoxClientID", textBoxClientId}
            }), 520, 220);
        }

        public static string GetOpenWindowStringToTextBox(int siteId, string textBoxClientId, bool isNeedWaterMark)
        {
            return LayerUtils.GetOpenScript("上传图片", PageUtils.GetCmsUrl(siteId, nameof(ModalUploadImageSingle), new NameValueCollection
            {
                {"TextBoxClientID", textBoxClientId},
                {"IsNeedWaterMark", isNeedWaterMark.ToString()}
            }), 520, 220);
        }

        public static string GetOpenWindowStringToList(int siteId, string currentRootPath)
        {
            return LayerUtils.GetOpenScript("上传图片", PageUtils.GetCmsUrl(siteId, nameof(ModalUploadImageSingle), new NameValueCollection
            {
                {"CurrentRootPath", currentRootPath}
            }), 520, 220);
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            PageUtils.CheckRequestParameter("siteId");
            _currentRootPath = AuthRequest.GetQueryString("CurrentRootPath");
            if (!string.IsNullOrEmpty(_currentRootPath) && !_currentRootPath.StartsWith("@"))
            {
                _currentRootPath = "@/" + _currentRootPath;
            }
            _textBoxClientId = AuthRequest.GetQueryString("TextBoxClientID");
            _isNeedWaterMark = AuthRequest.GetQueryBool("IsNeedWaterMark", true);
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            if (HifUpload.PostedFile == null || "" == HifUpload.PostedFile.FileName) return;

            var filePath = HifUpload.PostedFile.FileName;
            try
            {
                var fileExtName = PathUtils.GetExtension(filePath).ToLower();
                var localDirectoryPath = PathUtility.GetUploadDirectoryPath(Site, fileExtName);
                if (!string.IsNullOrEmpty(_currentRootPath))
                {
                    localDirectoryPath = PathUtility.MapPath(Site, _currentRootPath);
                    DirectoryUtils.CreateDirectoryIfNotExists(localDirectoryPath);
                }
                var localFileName = PathUtility.GetUploadFileName(Site, filePath);

                var localFilePath = PathUtils.Combine(localDirectoryPath, localFileName);

                if (!PathUtility.IsImageExtenstionAllowed(Site, fileExtName))
                {
                    FailMessage("上传失败，上传图片格式不正确！");
                    return;
                }
                if (!PathUtility.IsImageSizeAllowed(Site, HifUpload.PostedFile.ContentLength))
                {
                    FailMessage("上传失败，上传图片超出规定文件大小！");
                    return;
                }

                HifUpload.PostedFile.SaveAs(localFilePath);

                var isImage = EFileSystemTypeUtils.IsImage(fileExtName);

                if (isImage && _isNeedWaterMark)
                {
                    FileUtility.AddWaterMark(Site, localFilePath);
                }

                if (string.IsNullOrEmpty(_textBoxClientId))
                {
                    LayerUtils.Close(Page);
                }
                else
                {
                    var imageUrl = PageUtility.GetSiteUrlByPhysicalPathAsync(Site, localFilePath, true).GetAwaiter().GetResult();
                    var textBoxUrl = PageUtility.GetVirtualUrl(Site, imageUrl);

                    LtlScript.Text = $@"
<script type=""text/javascript"" language=""javascript"">
if (parent.document.getElementById('{_textBoxClientId}') != null)
{{
    parent.document.getElementById('{_textBoxClientId}').value = '{textBoxUrl}';
}}
{LayerUtils.CloseScript}
</script>";
                }
            }
            catch (Exception ex)
            {
                FailMessage(ex, "图片上传失败！");
            }
        }

    }
}
