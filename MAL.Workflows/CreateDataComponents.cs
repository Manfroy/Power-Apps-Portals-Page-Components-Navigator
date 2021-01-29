using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace MAL.PCN.Workflows
{
	public class CreateDataComponents : CodeActivity
	{
		[RequiredArgument]
		[Input("Environment URL")]
		public InArgument<string> EnvironmentURL { get; set; }

		[Output("PCN Javascript Wrapper Web Template")]
		[ReferenceTarget("adx_webtemplate")]
		public OutArgument<EntityReference> PCNJavascriptWrapper { get; set; }

		[Output("PCN Javascript Web Template")]
		[ReferenceTarget("adx_webtemplate")]
		public OutArgument<EntityReference> PCNJavascript { get; set; }

		[Output("Get Data Components Web Template")]
		[ReferenceTarget("adx_webtemplate")]
		public OutArgument<EntityReference> GetDataComponents { get; set; }

		protected override void Execute(CodeActivityContext executionContext)
		{
			IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
			IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
			IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
			ITracingService tracingService = executionContext.GetExtension<ITracingService>();

			CreateWebTemplates(executionContext, context, service, tracingService);
			CreatePageTemplate(context, service, tracingService);
			CreateWebPage(context, service, tracingService);
			CreateEntityPermissions(context, service, tracingService);
			CreateSiteSettings(executionContext, context, service, tracingService);
			CreateSiteMarker(context, service, tracingService);
			DeleteContentPages(service, tracingService);
			CreateCustomContentPage(context, service, tracingService);
		}

		/// <summary>
		/// Creates content page for the main page being used as API endpoint to retrieve page components data
		/// </summary>
		/// <param name="service"></param>
		/// <param name="tracingService"></param>
		private static void CreateCustomContentPage(IWorkflowContext context, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace($"Start of CreateCustomContentPage");

			var fetchData = new
			{
				adx_partialurl = "/",
				adx_websiteid = context.PrimaryEntityId.ToString(),
				adx_isroot = "1"
			};
			var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
							  <entity name='adx_webpage'>
								<attribute name='adx_publishingstateid' />
								<attribute name='adx_isroot' />
								<filter type='and'>
								  <condition attribute='adx_partialurl' operator='eq' value='{fetchData.adx_partialurl}' />
								  <condition attribute='adx_websiteid' operator='eq' value='{fetchData.adx_websiteid}' />
								  <condition attribute='adx_isroot' operator='eq' value='{fetchData.adx_isroot}' />
								</filter>
							  </entity>
							</fetch>";

			var rootWebPage = service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.FirstOrDefault();

			var websiteRecord = service.Retrieve("adx_website", context.PrimaryEntityId, new ColumnSet(new String[] { "adx_defaultlanguage" }));
			var websiteDefaultLanguageId = ((EntityReference)websiteRecord.Attributes.First().Value).Id;

			Entity webPageToCreate = new Entity("adx_webpage", new Guid("2fee3b3d-8f3b-4994-8615-2a7ae81e8466"));
			webPageToCreate["adx_name"] = "MAL.PCN.GetPageComponentsData";
			webPageToCreate["adx_partialurl"] = "get-page-components-data";
			webPageToCreate["adx_pagetemplateid"] = new EntityReference("adx_pagetemplate", new Guid("0c016369-c746-41f9-a11e-d1eab853977d"));
			webPageToCreate["adx_websiteid"] = new EntityReference("adx_website", context.PrimaryEntityId);
			webPageToCreate["adx_isroot"] = false;
			webPageToCreate["adx_parentpageid"] = new EntityReference("adx_webpage", rootWebPage.Id);
			webPageToCreate["adx_rootwebpageid"] = new EntityReference("adx_webpage", new Guid("3382b525-003f-4d3c-a7cf-5cabb1799853"));
			webPageToCreate["adx_webpagelanguageid"] = new EntityReference("adx_websitelanguage", websiteDefaultLanguageId);
			EntityReference rootWebPagePublishingState = rootWebPage.GetAttributeValue<EntityReference>("adx_publishingstateid");
			webPageToCreate["adx_publishingstateid"] = new EntityReference("adx_publishingstate", rootWebPagePublishingState.Id);
			webPageToCreate["adx_excludefromsearch"] = true;
			webPageToCreate["adx_hiddenfromsitemap"] = true;

			service.Create(webPageToCreate);
		}

		/// <summary>
		/// Deletes the content pages that are created for the main page being used as API endpoint to retrieve page components data
		/// </summary>
		/// <param name="service"></param>
		/// <param name="tracingService"></param>
		private static void DeleteContentPages(IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace($"Start of DeleteContentPages");
			// Get automatically created content page and delete it
			var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
								  <entity name='adx_webpage'>
									<filter type='and'>
									  <condition attribute='adx_name' operator='eq' value='MAL.PCN.GetPageComponentsData'/>
									  <condition attribute='adx_isroot' operator='eq' value='0'/>
									</filter>
								  </entity>
								</fetch>";

			var contentWebPages = service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities;
			foreach (var wp in contentWebPages)
			{
				service.Delete("adx_webpage", wp.Id);
			}
		}

		/// <summary>
		/// Create site marker for page used as API endpoint to return page components data
		/// </summary>
		/// <param name="context"></param>
		/// <param name="service"></param>
		/// <param name="tracingService"></param>
		private static void CreateSiteMarker(IWorkflowContext context, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace($"Start of CreateSiteMarker");
			Entity siteMarkerToCreate = new Entity("adx_sitemarker", new Guid("d2245270-ba22-4f5b-a9c3-37025185d60e"));
			siteMarkerToCreate["adx_name"] = "MAL.PCN.GetPageComponentsData";
			siteMarkerToCreate["adx_pageid"] = new EntityReference("adx_webpage", new Guid("3382b525-003f-4d3c-a7cf-5cabb1799853"));
			siteMarkerToCreate["adx_websiteid"] = new EntityReference("adx_website", context.PrimaryEntityId);
			service.Create(siteMarkerToCreate);
		}
		
		/// <summary>
		/// Creates site settings for Page Components Navigator
		/// </summary>
		/// <param name="executionContext"></param>
		/// <param name="context"></param>
		/// <param name="service"></param>
		/// <param name="tracingService"></param>
		private void CreateSiteSettings(CodeActivityContext executionContext, IWorkflowContext context, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace($"Start of CreateSiteSettings");
			string[][] siteSettingsToCreate =
			{
				new string[]{ "MAL.PCN.EnvironmentURL", EnvironmentURL.Get(executionContext), "d4ed9c23-6020-490a-a82c-65255b4987bc" },
				new string[]{ "MAL.PCN.EnablePCN","true","19f848a3-2b6b-442d-9204-ccd00c0ad172" },				
				new string[]{ "Webapi/mal_pcnattributemetadata/enabled", "true","ed29bb88-0a44-eb11-a812-000d3af43f23" },
				new string[]{ "Webapi/mal_pcnattributemetadata/fields", "mal_attributelogicalname,mal_EntityForm,mal_WebFormStep", "3ee93a99-0a44-eb11-a812-000d3af43f23" }
			};

			Entity siteSettingToCreate;
			foreach (var siteSetting in siteSettingsToCreate)
			{
				siteSettingToCreate = new Entity("adx_sitesetting", new Guid(siteSetting[2]));
				siteSettingToCreate["adx_name"] = siteSetting[0];
				siteSettingToCreate["adx_websiteid"] = new EntityReference("adx_website", context.PrimaryEntityId);
				siteSettingToCreate["adx_value"] = siteSetting[1];
				service.Create(siteSettingToCreate);
			}
		}

		/// <summary>
		/// Creates Entity Permissions needed for data retrieving from the portal
		/// </summary>
		/// <param name="context"></param>
		/// <param name="service"></param>
		/// <param name="tracingService"></param>
		private static void CreateEntityPermissions(IWorkflowContext context, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace($"Start of CreateEntityPermissions");
			string[][] entityPermissions =
			{
				new string[]{ "adx_entityform", "MAL.PCN.EntityForm.Read|Append", "86fa1d4b-01c4-4de9-8ae2-b80a831b9a33" },
				new string[]{ "adx_entityformmetadata", "MAL.PCN.EntityFormMetadata.Read", "6b728a1c-5040-40ac-863a-6786f17941ea" },
				new string[]{ "adx_webform", "MAL.PCN.WebForm.Read", "23e501b8-d657-4623-9f0d-daf1a613a79a" },
				new string[]{ "adx_webformstep", "MAL.PCN.WebFormStep.Read|Append", "98f25f84-5dc1-4b00-8536-a2ffc38abd0d" },
				new string[]{ "adx_webformmetadata", "MAL.PCN.WebFormMetadata.Read", "ac058973-ec71-4f38-9203-cd6fe94f2f15" },
				new string[]{ "adx_webtemplate", "MAL.PCN.WebTemplate.Read", "6481fe1a-784a-4aaf-a42f-35e354e729c3" },
				new string[]{ "adx_sitemarker", "MAL.PCN.SiteMarker.Read", "6ac03794-1351-4478-b4f6-b6304bd661ec" },
				new string[]{ "adx_webpage", "MAL.PCN.WebPage.Read", "98abfff7-6a67-483d-b490-63fed0cc4d34" },
				new string[]{ "contact", "MAL.PCN.Contact.Read", "c2bac820-f188-4957-88c0-5ea13e188329" },
				new string[]{ "adx_entitylist", "MAL.PCN.EntityList.Read", "c5d7ad70-1f72-4512-a7e6-d7b676e6c3ec" },
				new string[]{ "adx_webrole", "MAL.PCN.WebRole.Read", "d6a0a238-e40b-eb11-a813-000d3af444d9" },
				new string[]{ "mal_pcnattributemetadata", "MAL.PCN.PcnAttributeMetadata.Read|AppendTo|Write|Create|Delete", "7188062a-4e60-eb11-a812-000d3a0c609f" }
			};

			Entity entityPermissionToCreate;
			foreach (var entityPermission in entityPermissions)
			{
				entityPermissionToCreate = new Entity("adx_entitypermission", new Guid(entityPermission[2]));
				entityPermissionToCreate["adx_entityname"] = entityPermission[1];
				entityPermissionToCreate["adx_websiteid"] = new EntityReference("adx_website", context.PrimaryEntityId);
				entityPermissionToCreate["adx_scope"] = new OptionSetValue(756150000); // Set Scope to Global -> 756150000
				entityPermissionToCreate["adx_read"] = true;
				if (entityPermission[1] == "MAL.PCN.WebFormStep.Read|Append" || entityPermission[1] == "MAL.PCN.EntityForm.Read|Append")
				{
					entityPermissionToCreate["adx_append"] = true;
				}
				if (entityPermission[1] == "MAL.PCN.PcnAttributeMetadata.Read|AppendTo|Write|Create|Delete")
				{
					entityPermissionToCreate["adx_create"] = true;
					entityPermissionToCreate["adx_appendto"] = true;
					entityPermissionToCreate["adx_write"] = true;
					entityPermissionToCreate["adx_delete"] = true;
				}
				entityPermissionToCreate["adx_entitylogicalname"] = entityPermission[0];
				service.Create(entityPermissionToCreate);
			}
		}

		/// <summary>
		/// Creates page template that points to web template being used and API endpoint to return page components data
		/// </summary>
		/// <param name="context"></param>
		/// <param name="service"></param>
		/// <param name="tracingService"></param>
		private static void CreatePageTemplate(IWorkflowContext context, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace($"Start of CreatePageTemplate");
			Entity pageTemplateToCreate = new Entity("adx_pagetemplate", new Guid("0c016369-c746-41f9-a11e-d1eab853977d"));
			pageTemplateToCreate["adx_name"] = "MAL.PCN.GetPageComponentsData";
			pageTemplateToCreate["adx_usewebsiteheaderandfooter"] = false;
			pageTemplateToCreate["adx_type"] = new OptionSetValue(756150001);  // Set type to Web Template -> 75650001
			pageTemplateToCreate["adx_webtemplateid"] = new EntityReference("adx_webtemplate", new Guid("2b7eca89-9c1a-4a8d-a915-28588ce358c7"));
			pageTemplateToCreate["adx_websiteid"] = new EntityReference("adx_website", context.PrimaryEntityId);
			service.Create(pageTemplateToCreate);
		}

		/// <summary>
		/// Creates the web template to be used as API endpoint to return page components data
		/// </summary>
		/// <param name="executionContext"></param>
		/// <param name="context"></param>
		/// <param name="service"></param>
		/// <param name="tracingService"></param>
		private void CreateWebTemplates(CodeActivityContext executionContext, IWorkflowContext context, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace($"Start of CreateWebTemplates");
			string[][] webTemplatesToCreate =
			{
				new string[]{ "MAL.PCN.PCNJavascriptWrapper", "89a09a0f-bda5-4439-8fec-12c117599a9c" },
				new string[]{ "MAL.PCN.PCNJavascript", "c94e6d12-67ad-4dd4-a994-c11efe2ef01b" },
				new string[]{ "MAL.PCN.GetPageComponentsData", "2b7eca89-9c1a-4a8d-a915-28588ce358c7" }
			};

			Entity webTemplateToCreate;
			foreach (var webTemplate in webTemplatesToCreate)
			{
				webTemplateToCreate = new Entity("adx_webtemplate", new Guid(webTemplate[1]));
				webTemplateToCreate["adx_name"] = webTemplate[0];
				webTemplateToCreate["mal_enabledforpcn"] = new OptionSetValue(809840001);  //No
				webTemplateToCreate["adx_websiteid"] = new EntityReference("adx_website", context.PrimaryEntityId);
				service.Create(webTemplateToCreate);
				
				// Return references to web template to be able to update source on workflow directly
				switch (webTemplate[0])
				{
					case "MAL.PCN.PCNJavascriptWrapper": PCNJavascriptWrapper.Set(executionContext, new EntityReference("adx_webtemplate", webTemplateToCreate.Id)); break;
					case "MAL.PCN.PCNJavascript": PCNJavascript.Set(executionContext, new EntityReference("adx_webtemplate", webTemplateToCreate.Id)); break;
					case "MAL.PCN.GetPageComponentsData": GetDataComponents.Set(executionContext, new EntityReference("adx_webtemplate", webTemplateToCreate.Id)); break;
				}

			}
		}

		/// <summary>
		/// Creates the Web Page that will point to the page template that points to the web template to be used as API endpoint to return page components data
		/// </summary>
		/// <param name="context"></param>
		/// <param name="service"></param>
		/// <param name="tracingService"></param>
		private void CreateWebPage(IWorkflowContext context, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace($"Start of CreateWebPage");
			var fetchData = new
			{
				adx_partialurl = "/",
				adx_websiteid = context.PrimaryEntityId.ToString(),
				adx_isroot = "1"
			};
			var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
							  <entity name='adx_webpage'>
								<attribute name='adx_publishingstateid' />
								<attribute name='adx_isroot' />
								<filter type='and'>
								  <condition attribute='adx_partialurl' operator='eq' value='{fetchData.adx_partialurl}' />
								  <condition attribute='adx_websiteid' operator='eq' value='{fetchData.adx_websiteid}' />
								  <condition attribute='adx_isroot' operator='eq' value='{fetchData.adx_isroot}' />
								</filter>
							  </entity>
							</fetch>";

			var rootWebPage = service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.FirstOrDefault();

			Entity webPageToCreate = new Entity("adx_webpage", new Guid("3382b525-003f-4d3c-a7cf-5cabb1799853"));
			webPageToCreate["adx_name"] = "MAL.PCN.GetPageComponentsData";
			webPageToCreate["adx_partialurl"] = "get-page-components-data";
			webPageToCreate["adx_pagetemplateid"] = new EntityReference("adx_pagetemplate", new Guid("0c016369-c746-41f9-a11e-d1eab853977d"));
			webPageToCreate["adx_websiteid"] = new EntityReference("adx_website", context.PrimaryEntityId);
			webPageToCreate["adx_isroot"] = true;
			webPageToCreate["adx_parentpageid"] = new EntityReference("adx_webpage", rootWebPage.Id);
			EntityReference rootWebPagePublishingState = rootWebPage.GetAttributeValue<EntityReference>("adx_publishingstateid");
			webPageToCreate["adx_publishingstateid"] = new EntityReference("adx_publishingstate", rootWebPagePublishingState.Id);
			webPageToCreate["adx_excludefromsearch"] = true;
			webPageToCreate["adx_hiddenfromsitemap"] = true;

			service.Create(webPageToCreate);
		}
	}
}
