using System;
using System.IO;
using System.Linq;
using Elastic.Managed.Configuration;
using Elastic.Managed.ConsoleWriters;
using Elastic.Managed.Ephemeral.Clusters;
using Elastic.Managed.Ephemeral.Plugins;
using Elastic.Managed.FileSystem;

namespace Elastic.Managed.Ephemeral.Tasks.InstallationTasks
{
	public class InstallPlugins : InstallationTaskBase
	{
		public override void Run(EphemeralClusterBase cluster, INodeFileSystem fs)
		{
			var v = fs.Version;
			var requiredPlugins = cluster.RequiredPlugins;
			//on 2.x we so not support tests requiring plugins for 2.x since we can not reliably install them
			if (v.IsSnapshot && v.Major == 2) return;
			var plugins =
				from plugin in requiredPlugins
				let validForCurrentVersion = plugin.IsValid(v)
				let alreadyInstalled = AlreadyInstalled(plugin, fs)
				where !alreadyInstalled && validForCurrentVersion
				select plugin;

			foreach (var plugin in plugins)
			{
				var installParameter = !v.IsSnapshot ? plugin.Moniker : DownloadSnapshotIfNeeded(cluster.Writer, fs, plugin, v);
				ExecuteBinary(cluster.Writer, fs.PluginBinary, $"install elasticsearch plugin: {plugin.Moniker}", "install --batch", installParameter);
			}
		}

		private static bool AlreadyInstalled(ElasticsearchPlugin plugin, INodeFileSystem fileSystem)
		{
			var folder = plugin.Moniker;
			var pluginFolder = Path.Combine(fileSystem.ElasticsearchHome, "plugins", folder);

			// assume plugin already installed
			return Directory.Exists(pluginFolder);
		}

		private static string DownloadSnapshotIfNeeded(IConsoleLineWriter writer, INodeFileSystem fileSystem, ElasticsearchPlugin plugin, ElasticsearchVersion v)
		{
			var downloadLocation = Path.Combine(fileSystem.LocalFolder, plugin.SnapshotZip(v));
			DownloadPluginSnapshot(writer, downloadLocation, plugin, v);
			//transform downloadLocation to file uri and use that to install from
			return new Uri(downloadLocation).AbsoluteUri;
		}

		private static void DownloadPluginSnapshot(IConsoleLineWriter writer, string downloadLocation, ElasticsearchPlugin plugin, ElasticsearchVersion v)
		{
			if (File.Exists(downloadLocation)) return;
			var downloadUrl = plugin.SnapshotDownloadUrl(v);
			writer?.WriteDiagnostic($"{{{nameof(DownloadPluginSnapshot)}}} downloading [{plugin.Moniker}] from {{{downloadUrl}}}");
			try
			{
				DownloadFile(downloadUrl, downloadLocation);
				writer?.WriteDiagnostic($"{{{nameof(DownloadPluginSnapshot)}}} downloaded [{plugin.Moniker}] to {{{downloadLocation}}}");
			}
			catch (Exception)
			{
				writer?.WriteDiagnostic($"{{{nameof(DownloadPluginSnapshot)}}} download failed! [{plugin.Moniker}] from {{{downloadUrl}}}");
				throw;
			}
		}
	}
}
