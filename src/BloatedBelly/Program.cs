using System;
using System.Reflection;
using Mono.Cecil;

namespace BloatedBelly
{
	public class Program
	{
		const string CarriedResourceName = "Hello";

		public static void Main(string[] args)
		{
			Console.WriteLine("Testing to see if resources can be modified");


			var assm = Assembly.GetExecutingAssembly();
			foreach (var name in assm.GetManifestResourceNames())
			{
				if (name != CarriedResourceName) continue;

				Console.WriteLine("Resource is loaded!");
				return;
				//var rdr = assm.GetManifestResourceStream(name);
			}

			Console.WriteLine("Resource not loaded... trying dirty hack");

			var newFileName = assm.Location+".new.exe";
			
			var def = AssemblyDefinition.ReadAssembly(assm.Location);
			var temp = new EmbeddedResource(CarriedResourceName, ManifestResourceAttributes.Public, 
				new byte[] {0x01,0x02,0x03,0x04});
			def.MainModule.Resources.Add(temp);
			def.Write(newFileName);

			Console.WriteLine("Go try the new file");
		}
	}
}
