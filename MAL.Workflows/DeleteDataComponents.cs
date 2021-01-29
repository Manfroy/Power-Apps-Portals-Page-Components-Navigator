using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Query;

namespace MAL.PCN.Workflows
{
	public class DeleteDataComponents : CodeActivity
	{
		protected override void Execute(CodeActivityContext executionContext)
		{
			IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
			IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
			IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
			
			string[][] recordsToDelete =
			{
				new string[] { "adx_sitemarker", "d2245270-ba22-4f5b-a9c3-37025185d60e" },
				new string[] { "adx_sitesetting", "d4ed9c23-6020-490a-a82c-65255b4987bc" },
				new string[] { "adx_sitesetting", "19f848a3-2b6b-442d-9204-ccd00c0ad172" },
				new string[] { "adx_sitesetting", "ed29bb88-0a44-eb11-a812-000d3af43f23" },
				new string[] { "adx_sitesetting", "3ee93a99-0a44-eb11-a812-000d3af43f23" },
				new string[] { "adx_entitypermission", "86fa1d4b-01c4-4de9-8ae2-b80a831b9a33" },
				new string[] { "adx_entitypermission", "6b728a1c-5040-40ac-863a-6786f17941ea" },
				new string[] { "adx_entitypermission", "23e501b8-d657-4623-9f0d-daf1a613a79a" },
				new string[] { "adx_entitypermission", "98f25f84-5dc1-4b00-8536-a2ffc38abd0d" },
				new string[] { "adx_entitypermission", "ac058973-ec71-4f38-9203-cd6fe94f2f15" },
				new string[] { "adx_entitypermission", "6481fe1a-784a-4aaf-a42f-35e354e729c3" },
				new string[] { "adx_entitypermission", "6ac03794-1351-4478-b4f6-b6304bd661ec" },
				new string[] { "adx_entitypermission", "98abfff7-6a67-483d-b490-63fed0cc4d34" },
				new string[] { "adx_entitypermission", "c2bac820-f188-4957-88c0-5ea13e188329" },
				new string[] { "adx_entitypermission", "c5d7ad70-1f72-4512-a7e6-d7b676e6c3ec" },
				new string[] { "adx_entitypermission", "d6a0a238-e40b-eb11-a813-000d3af444d9" },
				new string[] { "adx_entitypermission", "7188062a-4e60-eb11-a812-000d3a0c609f" },
				new string[] { "adx_pagetemplate", "0c016369-c746-41f9-a11e-d1eab853977d" },
				new string[] { "adx_webtemplate", "89a09a0f-bda5-4439-8fec-12c117599a9c" },
				new string[] { "adx_webtemplate", "c94e6d12-67ad-4dd4-a994-c11efe2ef01b" },
				new string[] { "adx_webtemplate", "2b7eca89-9c1a-4a8d-a915-28588ce358c7" },
				new string[] { "adx_webpage", "3382b525-003f-4d3c-a7cf-5cabb1799853" }
			};

			foreach(var record in recordsToDelete)
			{ 
				Entity recordToDelete = new Entity();
				try
				{
					recordToDelete = service.Retrieve(record[0], new Guid(record[1]), new ColumnSet(null));
				}
				catch (FaultException<OrganizationServiceFault>){}
				finally
				{
					if (!String.IsNullOrEmpty(recordToDelete.LogicalName))
					{
						service.Delete(record[0], new Guid(record[1]));
					}
				}
			}
		}
	}
}
