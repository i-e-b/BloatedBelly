using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace BloatedBelly
{
	public class Program
	{
		const string CarriedResourcePrefix = "Belly_";

		public static void Main(string[] args)
		{
			NeedArgs(args, 1);

			switch (args[0].ToLowerInvariant())
			{
				case "empty":
					NeedArgs(args, 2);
					WriteBlankExecutable(args[1]);
					break;

				case "add":
					NeedArgs(args, 3);
					WriteExecutableWithBinaries(args[1],
						FilesByPattern(args.Skip(2).ToArray()));
					break;

				case "deploy":
					if (!IsBundleLoaded())
					{
						NoBundleMessage();
					}
					NeedArgs(args, 2);
					DropHere();
					RunShellCommand(args[1]);
					break;

				default:
					ShowInfo();
					break;
			}
		}

		static void RunShellCommand(string command)
		{
		}

		static void DropHere()
		{
			var dir = Path.Combine(Directory.GetCurrentDirectory(), "\\Belly");
			if (Directory.Exists(dir)) Directory.Delete(dir);
			Directory.CreateDirectory(dir);

			var assm = Assembly.GetExecutingAssembly();
			var resourceNames = assm.GetManifestResourceNames().Where(name => name.StartsWith(CarriedResourcePrefix));
			foreach (var name in resourceNames)
			{
				var newName = name.Replace(CarriedResourcePrefix, "");

				using (Stream input = assm.GetManifestResourceStream(name))
				using (Stream output = File.Create(newName))
				{
					CopyStream(input, output);
				}
			}
		}

		static void NoBundleMessage()
		{
			Console.WriteLine("No bundle loaded -- can't deploy");
			Environment.Exit(1);
		}

		static IEnumerable<string> FilesByPattern(IEnumerable<string> patterns)
		{
			var dir = Directory.GetCurrentDirectory();
			foreach (var pattern in patterns)
			{
				var files = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);
				foreach (var file in files)
				{
					yield return file;
				}
			}
		}

		public static void CopyStream(Stream input, Stream output)
		{
			var buffer = new byte[8192];

			int bytesRead;
			while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				output.Write(buffer, 0, bytesRead);
			}
		}

		static void ShowInfo()
		{
			Console.WriteLine(@"BloatedBelly -- a mad deployment tool");
			Console.WriteLine(IsBundleLoaded() ? "A bundle is loaded and ready" : "no bundle is loaded");
			Console.WriteLine(@"
arguments:
    empty ""new.exe"" -- write an empty carrier to a new exe

    deploy ""..."" -- extract the carrier to working directory and call the quoted command

    add ""new.exe"" ... -- add all file patterns to a new exe
");
		}

		static void WriteBlankExecutable(string newFileName)
		{
			var assm = Assembly.GetExecutingAssembly();
			var def = AssemblyDefinition.ReadAssembly(assm.Location);
			def.MainModule.Resources.Clear();
			def.Write(Path.GetFullPath(newFileName));
		}


		static void WriteExecutableWithBinaries(string newFileName, IEnumerable<string> paths)
		{
			var assm = Assembly.GetExecutingAssembly();
			var def = AssemblyDefinition.ReadAssembly(assm.Location);
			def.MainModule.Resources.Clear();

			foreach (var path in paths)
			{
				var temp = new EmbeddedResource(CarriedResourcePrefix + path,
					ManifestResourceAttributes.Public,
					File.ReadAllBytes(path));
				def.MainModule.Resources.Add(temp);
			}
			def.Write(newFileName);
		}

		static bool IsBundleLoaded()
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceNames().Any(name => name.StartsWith(CarriedResourcePrefix));
		}

		static void NeedArgs(string[] args, int i)
		{
			if (args.Length >= i) return;
			ShowInfo();
			Environment.Exit(1);
		}
	}
}
