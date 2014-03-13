using System;
using System.IO;
using VersionOne.SDK.ObjectModel;

namespace AttachmentLoadTester
{
	class Program
	{
		static void Main(string[] args)
		{
			var instance = new V1Instance("http://localhost/versionone.web/", "admin", "admin");
			var project = instance.Get.ProjectByName("System (All Projects)");
			var maxMegs = 4;
			var generator = new RandomBufferGenerator(4.Megabytes());
			var testName = DateTime.Now.ToString("d");

			try
			{
				for (int megs = 1; megs <= maxMegs; megs++)
				{
					var currentCount = 0;
					var maxCount = 1.Thousand();
					var nextPercent = 10;

					Console.WriteLine("Started creating {0} defects with {1}MB attachments at {2}", maxCount, megs, DateTime.Now);

					while (currentCount++ < maxCount)
					{
						Func<string,string> assetName = (asset) => string.Format("Load Test ({0}) {3} - {1}/{2}", testName, currentCount, maxCount, asset);
						
						var defect = instance.Create.Defect(assetName("Defect"), project);
						
						var attachmentStream = new MemoryStream(generator.GenerateBufferFromSeed(megs.Megabytes()));
						var attachment = defect.CreateAttachment(assetName(megs + "MB Attachment"), "Filename", attachmentStream);
						var link = defect.CreateLink(assetName("Link"), "http://google.com", false);

						if ((float)currentCount/maxCount*100 >= nextPercent)
						{
							Console.WriteLine("{0}% Complete", nextPercent);
							nextPercent += 10;
						}
					}

					Console.WriteLine("Finished creating {0} defects at {1}", currentCount - 1, DateTime.Now);
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			Console.WriteLine("All done");
			Console.ReadKey();
		}
	}

	public static class Extensions
	{
		public static int Thousand(this int thousands)
		{
			return thousands * 1000;
		}

		public static long Megabytes(this int megabytes)
		{
			return megabytes * 1024 * 1024;
		}
	}

	//http://stackoverflow.com/questions/2985188/how-to-fill-byte-array-with-junk
	public class RandomBufferGenerator
	{
		private readonly Random _random = new Random();
		private readonly byte[] _seedBuffer;

		public RandomBufferGenerator(long maxBufferSize)
		{
			_seedBuffer = new byte[maxBufferSize];

			_random.NextBytes(_seedBuffer);
		}

		public byte[] GenerateBufferFromSeed(long size)
		{
			return this.GenerateBufferFromSeed((int) size);
		}

		public byte[] GenerateBufferFromSeed(int size)
		{
			int randomWindow = _random.Next(0, size);

			byte[] buffer = new byte[size];

			Buffer.BlockCopy(_seedBuffer, randomWindow, buffer, 0, size - randomWindow);
			Buffer.BlockCopy(_seedBuffer, 0, buffer, size - randomWindow, randomWindow);

			return buffer;
		}
	}
}
