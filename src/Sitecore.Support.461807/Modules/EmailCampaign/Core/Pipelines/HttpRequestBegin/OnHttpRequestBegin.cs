namespace Sitecore.Support.Modules.EmailCampaign.Core.Pipelines.HttpRequestBegin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sitecore.EmailCampaign.Model.Web.Settings;
    using Sitecore.ExM.Framework.Diagnostics;
    using Sitecore.Modules.EmailCampaign.Core;
    using Sitecore.Modules.EmailCampaign.Core.Contacts;
    using Sitecore.Modules.EmailCampaign.Services;
    using Sitecore.Pipelines.HttpRequest;

    public class OnHttpRequestBegin : Sitecore.Modules.EmailCampaign.Core.Pipelines.HttpRequestBegin.OnHttpRequestBegin
    {
        private static readonly HashSet<string> ExmQueryStringKeys;
        private readonly ILogger _logger;

        public OnHttpRequestBegin(IContactService contactService, ILogger logger, IExmCampaignService exmCampaignService, PipelineHelper pipelineHelper)
            : base(contactService, logger, exmCampaignService, pipelineHelper)
        {
            _logger = logger;
        }

        static OnHttpRequestBegin()
        {
            ExmQueryStringKeys = new HashSet<string>
            {
                GlobalSettings.ContactIdentifierSourceQueryStringKey,
                GlobalSettings.ContactIdentifierIdentifierQueryStringKey,
                GlobalSettings.AnalyticsContactIdQueryKey,
                GlobalSettings.ConfirmSubscriptionQueryStringKey,
                GlobalSettings.EcmIdQueryStringKey,
                GlobalSettings.MessageIdQueryKey,
                GlobalSettings.RecipientQueryStringKey,
                GlobalSettings.OnlineVersionQueryStringKey,
                GlobalSettings.ExmEncryptedQueryQueryStringKey
            };
        }

        public new void Process(HttpRequestArgs args)
        {
            try
            {
                if (!GlobalSettings.Enabled)
                {
                    return;
                }

                SaveRendererUrl(args);

                if (!args.HttpContext.Request.QueryString.AllKeys.Any(ExmQueryStringKeys.Contains))
                {
                    return;
                }

                ModifyExmRequest(args);

                ApplyOutputFilter(args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                if (ex is Sitecore.XConnect.XdbCollectionUnavailableException && ExmContext.IsRenderRequest)
                    throw;
            }
        }
    }
}