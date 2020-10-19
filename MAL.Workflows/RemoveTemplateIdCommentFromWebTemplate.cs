using System;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;

namespace MAL.PCN.Workflows
{
	public class RemoveTemplateIdCommentFromWebTemplate : CodeActivity
	{
		[RequiredArgument]
		[Input("Original Source")]
		public InArgument<string> OriginalSource { get; set; }

		[Output("Filtered Source")]
		public OutArgument<string> FilteredSource { get; set; }

		protected override void Execute(CodeActivityContext executionContext)
		{
			IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
			var filteredSource = string.Empty;
			var originalSource = OriginalSource.Get(executionContext);

			if (String.IsNullOrEmpty(originalSource))
			{
				FilteredSource.Set(executionContext, filteredSource);
			}
			else
			{
				string[] lines = originalSource.Split(
					new[] { "\r\n", "\r", "\n" },
					StringSplitOptions.None
				);

				Array.Reverse(lines);

				foreach (var line in lines)
				{
					if (line.Contains($@"<!--MAL.PCN.WebTemplateId={context.PrimaryEntityId}"))
					{
						filteredSource = originalSource.Replace("\r\n" + line, "").Replace("\r" + line, "").Replace("\n" + line, "");
						break;
					}
				}

				FilteredSource.Set(executionContext, filteredSource);
			}
		}



	}
}
