using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BloatedBelly.Properties;

namespace BloatedBelly
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Testing to see if resources can be modified");

			var temp = new global::System.Resources.ResourceManager("BloatedBelly.Properties.Resources", typeof(Resources).Assembly);

		}
	}
}
