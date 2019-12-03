﻿using System.Web.UI;
using SiteServer.CMS.Context;

namespace SiteServer.BackgroundPages.Controls
{
	public class Script : LiteralControl
	{
        public virtual string Src 
		{
            get
            {
                var src = ViewState["Src"] as string;
                return !string.IsNullOrEmpty(src) ? src : string.Empty;
            }
            set
            {
                ViewState["Src"] = value;
            }
        }

		protected override void Render(HtmlTextWriter writer)
		{
		    if (!string.IsNullOrEmpty(Src))
		    {
                writer.Write($@"<script src=""{(Src.StartsWith("~") ? PageUtils.ParseNavigationUrl(Src) : PageUtils.GetAdminUrl(Src))}"" type=""text/javascript""></script>");
            }
		}

	}
}
