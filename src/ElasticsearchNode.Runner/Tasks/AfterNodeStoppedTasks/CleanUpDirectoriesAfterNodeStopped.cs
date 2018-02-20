using System;
using System.IO;
using Elastic.Managed.Ephimeral.Clusters;
using Elastic.Managed.FileSystem;

namespace Elastic.Managed.Ephimeral.Tasks.AfterNodeStoppedTasks
{
	public class CleanUpDirectoriesAfterNodeStopped : AfterNodeStoppedTaskBase
	{
		public override void Run(EphimeralClusterBase cluster, INodeFileSystem fs)
		{
			if (Directory.Exists(fs.DataPath))
			{
				Console.WriteLine($"attempting to delete cluster data: {fs.DataPath}");
				Directory.Delete(fs.DataPath, true);
			}

			if (Directory.Exists(fs.LogsPath))
			{
				var files = Directory.GetFiles(fs.LogsPath, fs.ClusterName + "*.log");
				foreach (var f in files)
				{
					Console.WriteLine($"attempting to delete log file: {f}");
					File.Delete(f);
				}
			}

			if (Directory.Exists(fs.RepositoryPath))
			{
				Console.WriteLine("attempting to delete repositories");
				Directory.Delete(fs.RepositoryPath, true);
			}
		}
	}
}
