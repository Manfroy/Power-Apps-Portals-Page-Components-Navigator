using System;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System.Collections.Generic;

namespace MAL.PCN.Workflows
{
	public class FilterCode : CodeActivity
	{
		[RequiredArgument]
		[Input("Original Source")]
		public InArgument<string> OriginalSource { get; set; }

		[Output("Filtered Source")]
		public OutArgument<string> FilteredSource { get; set; }

		private static readonly Dictionary<string, string> Replacements = new Dictionary<string, string>()
																			  {
																				  {"%-", "%"},
																				  {"<script>", string.Empty},
																				  {"</script>", string.Empty},
																				  {"}", string.Empty},
																				  {"{", string.Empty},
																				  {"`", string.Empty},
																				  {"'", string.Empty},
																				  {"\"", string.Empty},
																				  {" ", string.Empty},
																				  {"\t", string.Empty}
																			  };

		protected override void Execute(CodeActivityContext context)
		{
			var filteredSource = string.Empty;
			var originalSource = OriginalSource.Get(context);

			if (String.IsNullOrEmpty(originalSource))
			{
				FilteredSource.Set(context, filteredSource);
			}
			else
			{

				originalSource = originalSource.ToLower();

				foreach (string toReplace in Replacements.Keys)
				{
					originalSource = originalSource.Replace(toReplace, Replacements[toReplace]);
				}

				string[] lines = originalSource.Split(
					new[] { "\r\n", "\r", "\n" },
					StringSplitOptions.None
				);
				
				foreach (var line in lines)
				{
					if (line.Contains("%include")
						|| line.Contains("%extends")
						|| line.Contains("%endblock")
						|| line.Contains("%webformname")
						|| line.Contains("%webformid")
						|| line.Contains("%entityformname")
						|| line.Contains("%entityformid")
						|| line.Contains("%entitylistid")
						|| line.Contains("%entitylistname")
						|| line.Contains("%entitylistkey"))
					{
						filteredSource += line;
					}
				}

				FilteredSource.Set(context, filteredSource);
			}
		}


		
	}
}
