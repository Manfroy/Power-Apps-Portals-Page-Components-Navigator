using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;
using System.ServiceModel;

namespace MAL.PCN.Workflows
{
    public class AssociatePCNEntityPermissionsToPortalAdminWebRole : CodeActivity
    {
        protected override void Execute(CodeActivityContext executionContext)
        {
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            EntityCollection entityPermissions = RetrieveEntityPermissions(service, context);         
            Entity portalAdminWebRole = RetrievePortalAdminWebRole(service, context);

			try
			{
                Associate(service, "adx_webrole", portalAdminWebRole.Id, "adx_entitypermission_webrole", entityPermissions);
			}
            catch (FaultException<OrganizationServiceFault> ex)
            {
                tracingService.Trace($"Error : {ex.Message}");
                throw ex;
            }
        }

        /// <summary>
        /// Retrieves entity permissions needed to access page components data from portal
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static EntityCollection RetrieveEntityPermissions(IOrganizationService service, IWorkflowContext context)
        {
            var entityPermissionsFetchData = new
            {
                adx_entityname = "%MAL.PCN.%",
                adx_websiteid = context.PrimaryEntityId.ToString(),
                webRoleValue = "null"
            };

            var entityPermissionsFetchXml = $@"
                <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                    <entity name='adx_entitypermission'>
                        <filter type='and'>
                            <condition attribute='adx_entityname' operator='like' value='{entityPermissionsFetchData.adx_entityname/*%MAL.PCN.%*/}'/>
                            <condition attribute='adx_websiteid' operator='eq' value='{entityPermissionsFetchData.adx_websiteid}'/>
                        </filter>
                        <link-entity name='adx_entitypermission_webrole' from='adx_entitypermissionid' to='adx_entitypermissionid' intersect='true' link-type='outer'>
                            <link-entity name='adx_webrole' from='adx_webroleid' to='adx_webroleid' link-type='outer' alias='ac' />
                        </link-entity>
                        <filter type='and'>
                            <condition entityname='ac' attribute='adx_webroleid'  operator='{entityPermissionsFetchData.webRoleValue/*null*/}'/>
                        </filter>
                    </entity>
                </fetch>";

            EntityCollection entityPermissions = service.RetrieveMultiple(new FetchExpression(entityPermissionsFetchXml));
            return entityPermissions;
        }

        /// <summary>
        /// Retrieves the OOB Portal Administrator web role
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static Entity RetrievePortalAdminWebRole(IOrganizationService service, IWorkflowContext context)
        {
            var portalAdminFetchData = new
            {
                adx_anonymoususersrole = "0",
                adx_websiteid = context.PrimaryEntityId.ToString(),
                adx_authenticatedusersrole = "0",
                lastname = "SYSTEM"
            };
            var portalAdminfetchXml = $@"
                <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                    <entity name='adx_webrole'>
                        <order attribute='createdon' descending='false' />
                        <filter type='and'>
                            <condition attribute='adx_anonymoususersrole' operator='eq' value='{portalAdminFetchData.adx_anonymoususersrole/*0*/}'/>
                            <condition attribute='adx_authenticatedusersrole' operator='eq' value='{portalAdminFetchData.adx_authenticatedusersrole/*0*/}'/>
                            <condition attribute='adx_websiteid' operator='eq' value='{portalAdminFetchData.adx_websiteid}'/>
                        </filter>
                    </entity>
                </fetch>";

            var portalAdminWebRole = service.RetrieveMultiple(new FetchExpression(portalAdminfetchXml)).Entities.FirstOrDefault();
            return portalAdminWebRole;
        }

        /// <summary>
        /// Associates records
        /// </summary>
        /// <param name="service"></param>
        /// <param name="primaryEntityLogicalName"></param>
        /// <param name="primaryEntityId"></param>
        /// <param name="relationshipName"></param>
        /// <param name="entityPermissions"></param>
        private static void Associate(IOrganizationService service, string primaryEntityLogicalName, Guid primaryEntityId, string relationshipName, EntityCollection entityPermissions)
		{
            if (entityPermissions.Entities.Count != 0)
            {
                var entityPermissionReferences = new EntityReferenceCollection();

                entityPermissions.Entities.ToList().ForEach(x =>
                {
                    entityPermissionReferences.Add(x.ToEntityReference());
                });

                Relationship relationship = new Relationship(relationshipName);

                service.Associate(primaryEntityLogicalName, primaryEntityId, relationship, entityPermissionReferences);
            }
        }
    }
}
