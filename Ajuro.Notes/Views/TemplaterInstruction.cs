namespace Ajuro.Notes.View
{
	public class TemplaterInstruction
	{
		// Idented CSV to build the json
		public string CSV { get; set; }

		// TemplateFilePath
		public string Template { get; set; }

		// VariablesFilePath
		public string Model { get; set; }

		// Save the template here
		public string Ready { get; set; }

		// Save the template here
		public string TempCode { get; set; }

		public string TempData { get; set; }

		// Save the template here
		public string TempText { get; set; }

		// Save the template here
		public string TempJson { get; set; }

		// Save the template here
		public string Project { get; set; }
	}
}