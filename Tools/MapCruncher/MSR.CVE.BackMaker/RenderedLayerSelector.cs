using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	internal class RenderedLayerSelector
	{
		private ToolStripMenuItem menuItem;
		private IDisplayableSource tileSource;
		private ViewerControl viewer;
		private CrunchedLayer crunchedLayer;
		public RenderedLayerSelector(CrunchedLayer crunchedLayer)
		{
			this.crunchedLayer = crunchedLayer;
		}
		public static RenderedLayerDisplayInfo GetLayerSelector(ViewerControl viewer, CachePackage cachePackage)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = string.Format("MapCruncher Rendered Layers (*{0})|*{1}{2}", CrunchedFile.CrunchedFilenameExtension, CrunchedFile.CrunchedFilenameExtension, BuildConfig.theConfig.allFilesOption);
			openFileDialog.FilterIndex = 1;
			openFileDialog.RestoreDirectory = true;
			if (openFileDialog.ShowDialog() != DialogResult.OK)
			{
				return null;
			}
			Uri uri = new Uri(openFileDialog.FileName);
			return RenderedLayerSelector.GetLayerSelector(viewer, cachePackage, uri);
		}
		public static RenderedLayerDisplayInfo GetLayerSelector(ViewerControl viewer, CachePackage cachePackage, Uri uri)
		{
			RenderedLayerDisplayInfo result;
			try
			{
				CrunchedFile renderedMashupsFromFile = RenderedLayerSelector.GetRenderedMashupsFromFile(uri);
				D.Assert(uri.IsFile);
				string localPath = uri.LocalPath;
				result = RenderedLayerSelector.BuildLayerSelector(viewer, cachePackage, Path.GetDirectoryName(localPath), renderedMashupsFromFile);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error opening crunched file ", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				result = null;
			}
			return result;
		}
		private static CrunchedFile GetRenderedMashupsFromFile(Uri uri)
		{
			CrunchedFile crunchedFile;
			try
			{
				crunchedFile = CrunchedFile.FromUri(uri);
			}
			catch (XmlException ex)
			{
				throw new Exception(string.Format("File {0} does not conform to {1} format. (Error was: {2})", uri, CrunchedFile.CrunchedFilename, ex.Message));
			}
			catch (Exception ex2)
			{
				throw new Exception(string.Format("Couldn't read {0}:\n{1}", uri, ex2.Message));
			}
			if (crunchedFile.crunchedLayers.Count == 0)
			{
				throw new Exception(string.Format("Found no rendered layers described in {0}\n", uri));
			}
			return crunchedFile;
		}
		private static RenderedLayerDisplayInfo BuildLayerSelector(ViewerControl viewer, CachePackage cachePackage, string basePath, CrunchedFile crunchedFile)
		{
			RenderedLayerDisplayInfo renderedLayerDisplayInfo = new RenderedLayerDisplayInfo();
			renderedLayerDisplayInfo.tsmiList = new List<ToolStripMenuItem>();
			foreach (CrunchedLayer current in crunchedFile.crunchedLayers)
			{
				RenderedLayerSelector renderedLayerSelector = new RenderedLayerSelector(current);
				renderedLayerSelector.viewer = viewer;
				renderedLayerSelector.menuItem = new ToolStripMenuItem(current.displayName);
				renderedLayerSelector.tileSource = new RenderedTileSource(cachePackage, new VENamingScheme(Path.Combine(basePath, current.namingScheme.GetFilePrefix()), current.namingScheme.GetFileSuffix()));
				renderedLayerSelector.menuItem.Tag = renderedLayerSelector;
				renderedLayerSelector.menuItem.Click += new EventHandler(RenderedLayerSelector.MenuItem_Click);
				renderedLayerDisplayInfo.tsmiList.Add(renderedLayerSelector.menuItem);
			}
			for (int i = 0; i < renderedLayerDisplayInfo.tsmiList.Count; i++)
			{
				((RenderedLayerSelector)renderedLayerDisplayInfo.tsmiList[renderedLayerDisplayInfo.tsmiList.Count - 1 - i].Tag).ToggleLayer();
			}
			renderedLayerDisplayInfo.defaultView = crunchedFile.crunchedLayers[0].defaultView;
			return renderedLayerDisplayInfo;
		}
		private static void MenuItem_Click(object sender, EventArgs e)
		{
			RenderedLayerSelector renderedLayerSelector = (RenderedLayerSelector)((ToolStripMenuItem)sender).Tag;
			renderedLayerSelector.ToggleLayer();
		}
		private void ToggleLayer()
		{
			this.menuItem.Checked = !this.menuItem.Checked;
			if (this.menuItem.Checked)
			{
				this.viewer.AddAlphaLayer(this.tileSource);
				return;
			}
			this.viewer.RemoveAlphaLayer(this.tileSource);
		}
	}
}
