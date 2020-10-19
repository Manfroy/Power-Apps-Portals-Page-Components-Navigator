using System;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System.Collections.Generic;

namespace MAL.PCN.Workflows
{
	public class GetEnvironmentURL : CodeActivity
	{
		[RequiredArgument]
		[Input("Dynamic URL")]
		public InArgument<string> DynamicURL { get; set; }

		[Output("Environment URL")]
		public OutArgument<string> EnvironmentURL { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			try
			{
				Uri uri = new Uri(DynamicURL.Get(context));
				EnvironmentURL.Set(context, uri.GetLeftPart(UriPartial.Authority));
			}
			catch (Exception ex)
			{
				throw new Exception($"Url '{DynamicURL.Get(context)}' is incorrectly formated for a Dynamics CRM Dynamics Url", ex);
			}
		}
	}
}
