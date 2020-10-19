using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using System;


namespace MAL.PCN.Workflows
{
	public class RunWorkflowOnFetchResults : CodeActivity
	{
		[RequiredArgument]
		[Input("Fetch XML")]
		public InArgument<string> FetchXML { get; set; }

		[RequiredArgument]
		[Input("Workflow to Execute")]
		[ReferenceTarget("workflow")]		
		public InArgument<EntityReference> WorkflowToExecute { get; set; }

		protected override void Execute(CodeActivityContext executionContext)
		{
			IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
			IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
			IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

			string fetchXMLQuery = FetchXML.Get(executionContext);
			EntityCollection recordsToProcess = service.RetrieveMultiple(new FetchExpression(fetchXMLQuery));
			EntityReference processEntityReference = WorkflowToExecute.Get(executionContext);

			foreach (var entity in recordsToProcess.Entities)
			{
				try
				{
					var request = new ExecuteWorkflowRequest
					{
						EntityId = entity.Id,
						WorkflowId = processEntityReference.Id
					};
					service.Execute(request);
				}
				catch (Exception ex)
				{
					throw new Exception($"Error executing workflow on {entity.Id}: {ex.Message}", ex);
				}
			}
		}
	}
}
