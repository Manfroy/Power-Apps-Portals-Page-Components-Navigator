using System;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System.Collections.Generic;
using System.Web;
using System.Threading;

namespace MAL.PCN.Workflows
{
	public class GetRecordId : CodeActivity
	{
		[RequiredArgument]
		[Input("Dynamic URL")]
		public InArgument<string> DynamicURL { get; set; }

		[Output("Record Id")]
		public OutArgument<string> RecordId { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			try
			{
				Uri uri = new Uri(DynamicURL.Get(context));
				RecordId.Set(context, HttpUtility.ParseQueryString(uri.Query).Get("id"));
			}
			catch (Exception ex)
			{
				throw new Exception($"Url '{DynamicURL.Get(context)}' is incorrectly formated for a Dynamics CRM Dynamics Url", ex);
			}
		}
	}
}
