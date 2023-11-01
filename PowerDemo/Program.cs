using PowerDemo.Lib;

namespace PowerDemo;

public static class Program
{
	public static void Main(string[] args)
	{
		var df = new DemoFile(File.OpenRead(
			                      @"I:\Steam\steamapps\common\Team Fortress 2\tf\autorecorded\autorecording_2023-11-01_02-07-08.dem"));
		df.Parse();

		foreach (DemoFile.DemoMessage message in df.Messages) {
			Console.WriteLine($"{message.Type}");
		}
	}
}