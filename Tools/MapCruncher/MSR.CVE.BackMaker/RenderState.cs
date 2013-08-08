using MSR.CVE.BackMaker.ImagePipeline;
using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;
namespace MSR.CVE.BackMaker
{
	public class RenderState : ITileWorkFeedback
	{
		public delegate void FlushRenderedTileCachePackageDelegate();
		private enum States
		{
			ReadyToRender,
			Rendering,
			Paused,
			Aborted,
			DoneRendering
		}
		private class RenderAborted : Exception
		{
		}
		private class RenderDisposed : Exception
		{
		}
		private class SetupFailed : Exception
		{
			private string prettyMessage;
			public SetupFailed(bool pretty, string m) : base(m)
			{
				if (pretty)
				{
					this.prettyMessage = m;
				}
			}
			public override string ToString()
			{
				if (this.prettyMessage != null)
				{
					return this.prettyMessage;
				}
				return base.ToString();
			}
		}
		private class LayerApplierMaker
		{
			private CachePackage cachePackage;
			private Dictionary<IRenderableSource, OneLayerBoundApplier> dict = new Dictionary<IRenderableSource, OneLayerBoundApplier>();
			public LayerApplierMaker(CachePackage cachePackage)
			{
				this.cachePackage = cachePackage;
			}
			public OneLayerBoundApplier MakeApplier(IRenderableSource source, Layer layer)
			{
				if (!this.dict.ContainsKey(source))
				{
					this.dict[source] = new OneLayerBoundApplier(source, layer, this.cachePackage);
				}
				return this.dict[source];
			}
		}
		private class ProposedTileSet : Dictionary<TileAddress, CompositeTileUnit>
		{
			public CompositeTileUnit MakeLayeredTileWork(TileAddress tileAddress, Layer layer, RenderOutputMethod renderOutput, string outputFilename, OutputTileType outputTileType)
			{
				if (!base.ContainsKey(tileAddress))
				{
					base[tileAddress] = new CompositeTileUnit(layer, tileAddress, renderOutput, outputFilename, outputTileType);
				}
				return base[tileAddress];
			}
		}
		private class SourceMapRenderInfo
		{
			public SourceMap sourceMap;
			public IRenderableSource warpedMapTileSource;
			public MapRectangle renderBoundsBoundingBox;
		}
		private const int sortPseudoLayer = 1;
		private const int TileCountReportInterval = 10000;
		private const int UnreasonablyManyTiles = 1000000;
		private const int ErrorMessageLimit = 100;
		private const int RangeQuerySizeLimit = 100;
		private const string thumbnailPathPrefix = "thumbnails";
		private Mashup _mashupDocument_UseScratchCopy;
		private Mashup mashupScratchCopy;
		private RenderUIIfc renderUI;
		private RenderState.FlushRenderedTileCachePackageDelegate flushRenderedTileCachePackage;
		private MapTileSourceFactory mapTileSourceFactory;
		private bool disposeFlag;
		private RenderState.States _state;
		private bool pauseRenderingFlag;
		private EventWaitHandle startRenderEvent = new CountedEventWaitHandle(false, EventResetMode.AutoReset, "RenderState.startRenderEvent");
		private MercatorCoordinateSystem mercatorCoordinateSystem = new MercatorCoordinateSystem();
		private RenderOutputMethod renderOutput;
		private List<TileRectangle> boundsList;
		private RangeQueryData rangeQueryData;
		private Queue<RenderWorkUnit> renderQueue = new Queue<RenderWorkUnit>();
		private Dictionary<string, bool> credits = new Dictionary<string, bool>();
		private bool complainedAboutInsaneTileCount;
		private int estimateProgressLayerCount;
		private int estimateProgressSourceMapCount;
		private int estimateProgressSourceMapsThisLayer = 1;
		private Uri renderedXMLDescriptor;
		private Uri sampleHTMLUri;
		private int initialQueueSize;
		private string statusString;
		private List<string> postedMessages = new List<string>();
		private RenderComplaintBox complaintBox;
		private ImageRef lastRenderedImageRef;
		private string[] lastRenderedImageLabel = new string[0];
		private StreamWriter logWriter;
		public static string SourceDataDirName = "SourceData";
		private RenderState.States state
		{
			get
			{
				return this._state;
			}
			set
			{
				this._state = value;
				this.renderUI.uiChanged();
			}
		}
		public RenderState(Mashup mashupDocument, RenderUIIfc renderUI, RenderState.FlushRenderedTileCachePackageDelegate flushRenderedTileCachePackage, MapTileSourceFactory mapTileSourceFactory)
		{
			this._mashupDocument_UseScratchCopy = mashupDocument;
			this.renderUI = renderUI;
			this.flushRenderedTileCachePackage = flushRenderedTileCachePackage;
			this.mapTileSourceFactory = mapTileSourceFactory;
			DebugThreadInterrupter.theInstance.AddThread("RenderState", new ThreadStart(this.ThreadTask), ThreadPriority.BelowNormal);
			ResourceCounter resourceCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("RenderState", -1);
			resourceCounter.crement(1);
			this.complaintBox = new RenderComplaintBox(new RenderComplaintBox.AnnounceDelegate(this.PostMessage));
		}
		private void OpenLog()
		{
			if (this.logWriter == null)
			{
				this.logWriter = new StreamWriter(new FileStream(FileUtilities.MakeTempFilename(".", "RenderLog"), FileMode.Create));
				this.logWriter.AutoFlush = true;
			}
		}
		internal void Dispose()
		{
			this.disposeFlag = true;
			this.startRenderEvent.Set();
			ResourceCounter resourceCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("RenderState", -1);
			resourceCounter.crement(-1);
			if (this.logWriter != null)
			{
				this.logWriter.Close();
				this.logWriter.Dispose();
				this.logWriter = null;
			}
		}
		internal void RenderClick()
		{
			switch (this.state)
			{
			case RenderState.States.ReadyToRender:
			case RenderState.States.Paused:
				this.startRenderEvent.Set();
				this.renderUI.uiChanged();
				return;
			case RenderState.States.Rendering:
				this.pauseRenderingFlag = true;
				this.renderUI.uiChanged();
				return;
			default:
				return;
			}
		}
		internal void UI_UpdateRenderControlButtonLabel(Button renderControlButton)
		{
			switch (this.state)
			{
			case RenderState.States.ReadyToRender:
				renderControlButton.Text = "Start";
				renderControlButton.Enabled = true;
				return;
			case RenderState.States.Rendering:
				renderControlButton.Text = "Pause";
				renderControlButton.Enabled = true;
				return;
			case RenderState.States.Paused:
				renderControlButton.Text = "Resume";
				renderControlButton.Enabled = true;
				return;
			case RenderState.States.Aborted:
				renderControlButton.Text = "Render Aborted.";
				renderControlButton.Enabled = false;
				return;
			case RenderState.States.DoneRendering:
				renderControlButton.Text = "Render Complete.";
				renderControlButton.Enabled = false;
				return;
			default:
				D.Assert(false, "Invalid state.");
				return;
			}
		}
		public void StartRender()
		{
			this.OpenLog();
			if (this.state != RenderState.States.ReadyToRender)
			{
				throw new Exception("I kind of imagined that you'd only call this to implement renderOnLaunch.");
			}
			this.startRenderEvent.Set();
			this.renderUI.uiChanged();
		}
		internal string UI_GetStatusString()
		{
			return this.statusString;
		}
		internal List<string> UI_GetPostedMessages()
		{
			return this.postedMessages;
		}
		private void PostStatus(string statusString)
		{
			if (this.logWriter != null)
			{
				this.logWriter.Write("STATUS: " + statusString + "\n");
			}
			this.statusString = statusString;
			this.renderUI.uiChanged();
		}
		public void PostMessage(string message)
		{
			if (this.logWriter != null)
			{
				this.logWriter.Write(message + "\n");
			}
			this.postedMessages.Add(message);
			this.renderUI.uiChanged();
		}
		public void PostComplaint(NonredundantRenderComplaint complaint)
		{
			this.complaintBox.Complain(complaint);
		}
		public void PostImageResult(ImageRef image, Layer layer, string sourceMapName, TileAddress address)
		{
            //using (MemoryStream m = new MemoryStream())
            //{
            //    image.image.Save(m, ImageFormat.Png);

            //    var ret = m.ToArray();

            //    m.Close();
            //}

			ImageRef imageRef = (ImageRef)image.Duplicate("RenderState.PostImageResult");
			Monitor.Enter(this);
			ImageRef imageRef2;
			try
			{
				imageRef2 = this.lastRenderedImageRef;
				this.lastRenderedImageRef = imageRef;
			}
			finally
			{
				Monitor.Exit(this);
			}
			if (imageRef2 != null)
			{
				imageRef2.Dispose();
			}
			this.lastRenderedImageLabel = new string[]
			{
				layer.displayName,
				sourceMapName,
				address.ToString()
			};
			this.renderUI.uiChanged();
		}
		private void ThreadTask()
		{
			try
			{
				this.RenderAll();
			}
			catch (Exception arg)
			{
				this.PostMessage(string.Format("Didn't expect to see an exception here: {0}", arg));
			}
			Exception failure = null;
			if (this.state != RenderState.States.DoneRendering)
			{
				failure = new Exception("Rendering incomplete.");
			}
			this.renderUI.notifyRenderComplete(failure);
		}
		private void CheckSignal()
		{
			if (this.disposeFlag)
			{
				throw new RenderState.RenderDisposed();
			}
			if (this.pauseRenderingFlag)
			{
				this.state = RenderState.States.Paused;
				this.pauseRenderingFlag = false;
				this.renderUI.uiChanged();
				this.startRenderEvent.WaitOne();
				this.state = RenderState.States.Rendering;
				this.renderUI.uiChanged();
			}
		}
		private void RenderAll()
		{
			try
			{
				if (BuildConfig.theConfig.logInteractiveRenders)
				{
					this.OpenLog();
				}
				this.startRenderEvent.WaitOne();
				this.CheckSignal();
				this.state = RenderState.States.Rendering;
				this._mashupDocument_UseScratchCopy.AutoSelectMaxZooms(this.mapTileSourceFactory);
				this.CheckSignal();
				this.mashupScratchCopy = this.DuplicateMashupDocumentForRender();
				this.CheckSignal();
				this.flushRenderedTileCachePackage();
				this.SetupRenderOutput();
				this.EstimateOuterLoop();
				this.CheckSignal();
				this.PurgeDirectory(this.renderOutput, "legends");
				this.CheckSignal();
				this.PostStatus("Creating XML mashup descriptor");
				CrunchedFile crunchedFile = this.CreateCrunchedFileDescriptor();
				this.PostStatus("Creating HTML sample file");
				this.sampleHTMLUri = SampleHTMLWriter.Write(this.mashupScratchCopy, new SampleHTMLWriter.PostMessageDelegate(this.PostMessage), this.renderOutput);
				this.CopySourceData();
				this.CheckSignal();
				this.PostStatus("Checking for reusable tiles from prior render");
				this.ArrangeLayerDirectories(this.renderOutput);
				this.CheckSignal();
				this.PostStatus("Rendering Legends");
				this.RenderLegends();
				this.CheckSignal();
				this.PurgeDirectory(this.renderOutput, "thumbnails");
				this.PostStatus("Rendering Thumbnails");
				this.RenderThumbnails(crunchedFile);
				this.CheckSignal();
				this.WriteCrunchedFileDescriptor(crunchedFile);
				this.PostStatus("Rendering");
				while (this.renderQueue.Count > 0)
				{
					this.CheckSignal();
					RenderWorkUnit renderWorkUnit = this.renderQueue.Dequeue();
					bool flag = renderWorkUnit.DoWork(this);
					if (flag)
					{
						this.renderUI.uiChanged();
					}
					if (this.logWriter != null)
					{
						this.logWriter.Write("Completed: " + renderWorkUnit + "\n");
					}
				}
				this.CommitManifest();
				this.PostStatus("Render complete.");
				this.state = RenderState.States.DoneRendering;
				this.renderUI.uiChanged();
			}
			catch (RenderState.RenderDisposed)
			{
			}
			catch (RenderState.RenderAborted)
			{
				this.state = RenderState.States.Aborted;
			}
			catch (Exception ex)
			{
				this.PostMessage(string.Format("Something broke: {0}", ex.Message));
				this.state = RenderState.States.Aborted;
			}
		}
		private Mashup DuplicateMashupDocumentForRender()
		{
			Mashup mashup = this._mashupDocument_UseScratchCopy.Duplicate();
			List<Layer> list = new List<Layer>();
			foreach (Layer current in mashup.layerList)
			{
				if (!current.SomeSourceMapIsReadyToLock())
				{
					list.Add(current);
				}
			}
			foreach (Layer current2 in list)
			{
				mashup.layerList.Remove(current2);
			}
			return mashup;
		}
		private void EstimateOuterLoop()
		{
			this.state = RenderState.States.Rendering;
			D.Say(0, "EstimateOuterLoop starts");
			this.estimateProgressLayerCount = 0;
			this.renderQueue.Clear();
			List<RenderWorkUnit> list = new List<RenderWorkUnit>();
			this.boundsList = new List<TileRectangle>();
			this.rangeQueryData = new RangeQueryData();
			bool flag = true;
			if (this.mashupScratchCopy != null)
			{
				foreach (Layer current in this.mashupScratchCopy.layerList)
				{
					List<RenderWorkUnit> list2;
					try
					{
						list2 = this.EstimateOneLayer(current, this.boundsList);
					}
					catch (RenderState.RenderAborted)
					{
						flag = false;
						break;
					}
					catch (RenderState.RenderDisposed)
					{
						throw;
					}
					catch (Exception ex)
					{
						this.PostMessage(string.Format("Skipping layer {0}: {1}", current.displayName, ex.ToString()));
						D.Say(0, string.Format("ConstructRenderListThread failed: {0}", ex));
						continue;
					}
					list2.Sort();
					List<RangeDescriptor> list3 = new List<RangeDescriptor>();
					this.rangeQueryData[current] = list3;
					foreach (RenderWorkUnit current2 in list2)
					{
						if (list3.Count >= 100)
						{
							break;
						}
						if (current2 is CompositeTileUnit)
						{
							this.rangeQueryData[current].Add(new RangeDescriptor(((CompositeTileUnit)current2).GetTileAddress()));
						}
					}
					list.AddRange(list2);
					this.estimateProgressLayerCount++;
					this.estimateProgressSourceMapCount = 0;
					this.estimateProgressSourceMapsThisLayer = 1;
					this.renderUI.uiChanged();
				}
			}
			if (list.Count == 0)
			{
				this.PostMessage("Nothing to render.");
				flag = false;
			}
			if (flag)
			{
				this.PostStatus("Sorting");
				list.Sort();
				this.renderQueue = new Queue<RenderWorkUnit>(list);
				this.initialQueueSize = this.renderQueue.Count;
				string message = string.Format("Estimated output size: {0} tiles, about {1:f}MB", this.renderQueue.Count, (double)this.renderQueue.Count * 0.085);
				this.estimateProgressLayerCount++;
				this.PostStatus(message);
				this.PostMessage(message);
				D.Say(0, "EstimateOuterLoop ends");
				return;
			}
			this.state = RenderState.States.Aborted;
			this.PostStatus("Estimation canceled.");
			throw new RenderState.RenderAborted();
		}
		private void SetupRenderOutput()
		{
			try
			{
				RenderToOptions renderToOptions = this.mashupScratchCopy.GetRenderOptions().renderToOptions;
				RenderOutputMethod baseMethod;
				if (renderToOptions is RenderToFileOptions)
				{
					RenderToFileOptions renderToFileOptions = (RenderToFileOptions)renderToOptions;
					if (renderToFileOptions.outputFolder == "")
					{
						throw new RenderState.SetupFailed(true, "Please select an output folder.");
					}
					FileOutputMethod fileOutputMethod = new FileOutputMethod(renderToFileOptions.outputFolder);
					this.PostStatus(string.Format("Creating {0}", renderToFileOptions.outputFolder));
					try
					{
						fileOutputMethod.CreateDirectory();
					}
					catch (Exception ex)
					{
						throw new RenderState.SetupFailed(true, ex.Message);
					}
					baseMethod = fileOutputMethod;
				}
				else
				{
					if (!(renderToOptions is RenderToS3Options))
					{
						throw new Exception("Unimplemented renderToOptions type");
					}
					RenderToS3Options renderToS3Options = (RenderToS3Options)renderToOptions;
					S3Credentials s3Credentials;
					try
					{
						s3Credentials = new S3Credentials(renderToS3Options.s3credentialsFilename, false);
					}
					catch (Exception arg)
					{
						throw new RenderState.SetupFailed(false, string.Format("Can't load credentials file {0}: {1}", renderToS3Options.s3credentialsFilename, arg));
					}
					S3Adaptor s3adaptor = new S3Adaptor(s3Credentials.accessKeyId, s3Credentials.secretAccessKey);
					S3OutputMethod s3OutputMethod = new S3OutputMethod(s3adaptor, renderToS3Options.s3bucket, renderToS3Options.s3pathPrefix);
					baseMethod = s3OutputMethod;
				}
				if (BuildConfig.theConfig.usingManifests)
				{
					this.renderOutput = new ManifestOutputMethod(baseMethod);
				}
				else
				{
					this.renderOutput = baseMethod;
				}
			}
			catch (RenderState.SetupFailed setupFailed)
			{
				this.PostMessage(setupFailed.ToString());
				this.PostStatus(setupFailed.ToString());
				MessageBox.Show(setupFailed.ToString(), "Render Setup Failed");
				this.state = RenderState.States.Aborted;
				throw new RenderState.RenderAborted();
			}
		}
		private void CommitManifest()
		{
			if (this.renderOutput is ManifestOutputMethod)
			{
				((ManifestOutputMethod)this.renderOutput).CommitChanges();
			}
		}
		private void DebugEmitRenderPlan(Queue<RenderWorkUnit> renderQueue)
		{
			FileStream fileStream = new FileStream("RenderPlan.txt", FileMode.Create, FileAccess.Write);
			StreamWriter streamWriter = new StreamWriter(fileStream);
			foreach (RenderWorkUnit current in renderQueue)
			{
				streamWriter.WriteLine(current.ToString());
			}
			streamWriter.Close();
			fileStream.Close();
		}
		private List<RenderWorkUnit> EstimateOneLayer(Layer layer, List<TileRectangle> boundsList)
		{
			if (layer.Count == 0)
			{
				return new List<RenderWorkUnit>();
			}
			this.EstimateLayer_SetupUI(layer);
			int spillCountBefore = this.EstimateLayer_PrepareToSelectRenderingStrategy();
			List<RenderState.SourceMapRenderInfo> sourceMapRenderInfosBackToFront = this.EstimateLayer_MakeSourceMapList(layer);
			RenderState.ProposedTileSet proposedTileSet = this.EstimateLayer_MakeProposedTileSet(layer, boundsList, sourceMapRenderInfosBackToFront);
			bool useStagedRendering = this.EstimateLayer_SelectRenderingStrategy(layer, spillCountBefore);
			List<CompositeTileUnit> list = new List<CompositeTileUnit>(proposedTileSet.Values);
			list.Sort();
			return this.EstimateLayer_CreateRenderList(layer, sourceMapRenderInfosBackToFront, useStagedRendering, list);
		}
		private void EstimateLayer_SetupUI(Layer layer)
		{
			this.estimateProgressSourceMapCount = 0;
			this.estimateProgressSourceMapsThisLayer = layer.Count * 2 + 1;
			if (this.estimateProgressSourceMapsThisLayer == 0)
			{
				this.estimateProgressSourceMapsThisLayer = 1;
			}
			this.renderUI.uiChanged();
		}
		private int EstimateLayer_PrepareToSelectRenderingStrategy()
		{
			this.mapTileSourceFactory.PurgeOpenSourceDocumentCache();
			return this.mapTileSourceFactory.GetOpenSourceDocumentCacheSpillCount();
		}
		private List<RenderState.SourceMapRenderInfo> EstimateLayer_MakeSourceMapList(Layer layer)
		{
			List<RenderState.SourceMapRenderInfo> list = new List<RenderState.SourceMapRenderInfo>();
			foreach (SourceMap current in layer.GetBackToFront())
			{
				this.CheckSignal();
				this.PostStatus(string.Format("(opening {0})", current.displayName));
				RenderState.SourceMapRenderInfo sourceMapRenderInfo = new RenderState.SourceMapRenderInfo();
				sourceMapRenderInfo.sourceMap = current;
				if (!current.ReadyToLock())
				{
					this.PostMessage(string.Format("Skipping SourceMap {0} because it's not ready to lock.", current.GetDisplayName()));
				}
				else
				{
					try
					{
						sourceMapRenderInfo.warpedMapTileSource = this.mapTileSourceFactory.CreateRenderableWarpedSource(current);
					}
					catch (Exception ex)
					{
						this.PostMessage(string.Format("Skipping SourceMap {0} because locking is failing: {1}.", current.GetDisplayName(), ex.ToString()));
						continue;
					}
					list.Add(sourceMapRenderInfo);
					this.estimateProgressSourceMapCount++;
					this.renderUI.uiChanged();
				}
			}
			return list;
		}
		private RenderState.ProposedTileSet EstimateLayer_MakeProposedTileSet(Layer layer, List<TileRectangle> boundsList, List<RenderState.SourceMapRenderInfo> sourceMapRenderInfosBackToFront)
		{
			int num = 0;
			MapRectangle rect = layer.renderClip.rect;
			RenderState.ProposedTileSet proposedTileSet = new RenderState.ProposedTileSet();
			foreach (RenderState.SourceMapRenderInfo current in sourceMapRenderInfosBackToFront)
			{
				this.AddCredit(current.warpedMapTileSource.GetRendererCredit());
				current.warpedMapTileSource.GetOpenDocumentFuture(FutureFeatures.Cached).Realize("EstimateLayer_MakeProposedTileSet");
				BoundsPresent boundsPresent = (BoundsPresent)current.warpedMapTileSource.GetUserBounds(null, FutureFeatures.Cached).Realize("RenderState.EstimateOneLayer");
				current.renderBoundsBoundingBox = boundsPresent.GetRenderRegion().GetBoundingBox();
				if (rect != null)
				{
					current.renderBoundsBoundingBox = current.renderBoundsBoundingBox.ClipTo(rect);
				}
				current.renderBoundsBoundingBox = current.renderBoundsBoundingBox.ClipTo(CoordinateSystemUtilities.GetRangeAsMapRectangle(MercatorCoordinateSystem.theInstance));
				RenderBounds renderBounds = this.mercatorCoordinateSystem.MakeRenderBounds(current.renderBoundsBoundingBox);
				string fileSuffix = "." + this.mashupScratchCopy.GetRenderOptions().outputTileType.extn;
				RenderedTileNamingScheme renderedTileNamingScheme = new VENamingScheme(layer.GetFilesystemName(), fileSuffix);
				boundsPresent.Dispose();
				this.PostStatus(string.Format("(counting {0})", current.sourceMap.displayName));
				int num2 = Math.Max(current.sourceMap.sourceMapRenderOptions.minZoom, renderBounds.MinZoom);
				int num3 = Math.Min(current.sourceMap.sourceMapRenderOptions.maxZoom, renderBounds.MaxZoom);
				boundsList.Add(renderBounds.tileRectangle[num3]);
				string text = renderedTileNamingScheme.GetFileSuffix();
				D.Assert(text.StartsWith("."));
				text = text.Substring(1);
				for (int i = num2; i <= num3; i++)
				{
					TileRectangle tileRectangle = renderBounds.tileRectangle[i];
					for (int j = tileRectangle.TopLeft.TileY; j <= tileRectangle.BottomRight.TileY; j += tileRectangle.StrideY)
					{
						for (int k = tileRectangle.TopLeft.TileX; k <= tileRectangle.BottomRight.TileX; k += tileRectangle.StrideX)
						{
							TileAddress tileAddress = new TileAddress(k, j, i);
							proposedTileSet.MakeLayeredTileWork(tileAddress, layer, this.renderOutput.MakeChildMethod(renderedTileNamingScheme.GetFilePrefix()), renderedTileNamingScheme.GetTileFilename(tileAddress), this.mashupScratchCopy.GetRenderOptions().outputTileType);
							this.CheckSignal();
							if (!this.complainedAboutInsaneTileCount && proposedTileSet.Count > 1000000)
							{
								DialogResult dialogResult = MessageBox.Show(string.Format("Estimate exceeds {0} tiles; consider canceling the estimation and selecting lower max zoom levels.", 1000000), "That's a lot of tiles.", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
								if (dialogResult == DialogResult.Cancel)
								{
									throw new RenderState.RenderAborted();
								}
							}
							num++;
							if (num % 10000 == 0)
							{
								this.PostStatus(string.Format("(counting {0} ... {1} tiles)", current.sourceMap.displayName, proposedTileSet.Count));
							}
						}
					}
				}
				this.estimateProgressSourceMapCount++;
			}
			return proposedTileSet;
		}
		private void AddCredit(string rendererCredit)
		{
			if (rendererCredit != null && !this.credits.ContainsKey(rendererCredit))
			{
				this.credits.Add(rendererCredit, true);
				this.PostMessage(rendererCredit);
			}
		}
		private bool EstimateLayer_SelectRenderingStrategy(Layer layer, int spillCountBefore)
		{
			bool result = false;
			int openSourceDocumentCacheSpillCount = this.mapTileSourceFactory.GetOpenSourceDocumentCacheSpillCount();
			if (openSourceDocumentCacheSpillCount == spillCountBefore)
			{
				this.mapTileSourceFactory.CreateUnwarpedSource(layer.First).GetOpenDocumentFuture(FutureFeatures.Cached).Realize("RenderState.EstimateOneLayer spill test");
				openSourceDocumentCacheSpillCount = this.mapTileSourceFactory.GetOpenSourceDocumentCacheSpillCount();
			}
			if (openSourceDocumentCacheSpillCount != spillCountBefore)
			{
				this.PostMessage(string.Format("Layer {0} spills memory; using staged rendering plan.", layer.GetDisplayName()));
				result = true;
			}
			return result;
		}
		private List<RenderWorkUnit> EstimateLayer_CreateRenderList(Layer layer, List<RenderState.SourceMapRenderInfo> sourceMapRenderInfosBackToFront, bool useStagedRendering, List<CompositeTileUnit> compositeTileUnits)
		{
			VETileSource vETileSource = null;
			if (layer.simulateTransparencyWithVEBackingLayer != null && layer.simulateTransparencyWithVEBackingLayer != "")
			{
				vETileSource = new VETileSource(this.mapTileSourceFactory.GetCachePackage(), layer.simulateTransparencyWithVEBackingLayer);
			}
			this.PostStatus(string.Format("Organizing {0}", layer.displayName));
			RenderState.LayerApplierMaker layerApplierMaker = new RenderState.LayerApplierMaker(this.mapTileSourceFactory.GetCachePackage());
			List<RenderWorkUnit> list = new List<RenderWorkUnit>();
			int num = 0;
			foreach (CompositeTileUnit current in compositeTileUnits)
			{
				current.stage = num / 100;
				if (vETileSource != null)
				{
					current.AddSupplier(layerApplierMaker.MakeApplier(vETileSource, layer));
				}
				MapRectangle mapRectangle = CoordinateSystemUtilities.TileAddressToMapRectangle(this.mercatorCoordinateSystem, current.GetTileAddress());
				foreach (RenderState.SourceMapRenderInfo current2 in sourceMapRenderInfosBackToFront)
				{
					if (mapRectangle.intersects(current2.renderBoundsBoundingBox))
					{
						current.AddSupplier(layerApplierMaker.MakeApplier(current2.warpedMapTileSource, layer));
					}
				}
				list.Add(current);
				if (useStagedRendering)
				{
					foreach (SingleSourceUnit current3 in current.GetSingleSourceUnits())
					{
						list.Add(current3);
					}
				}
				num++;
				this.CheckSignal();
			}
			return list;
		}
		private void ArrangeLayerDirectories(RenderOutputMethod baseOutputMethod)
		{
			try
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				foreach (Layer current in this.mashupScratchCopy.layerList)
				{
					string filesystemName = current.GetFilesystemName();
					RenderOutputMethod outputMethod = baseOutputMethod.MakeChildMethod(filesystemName);
					EncodableHash encodableHash = new EncodableHash();
					current.AccumulateRobustHash_PerTile(this.mapTileSourceFactory.GetCachePackage(), encodableHash);
					try
					{
						LayerMetadataFile layerMetadataFile = LayerMetadataFile.Read(outputMethod);
						if (layerMetadataFile.encodableHash == encodableHash)
						{
							dictionary[filesystemName] = true;
						}
						else
						{
							dictionary2[filesystemName] = true;
						}
					}
					catch (Exception)
					{
					}
				}
				if (!(this.mashupScratchCopy.GetRenderOptions().renderToOptions is RenderToFileOptions))
				{
					D.Sayf(0, "TODO: Generalize to S3 by recording layer manifest", new object[0]);
				}
				else
				{
					string outputFolder = ((RenderToFileOptions)this.mashupScratchCopy.GetRenderOptions().renderToOptions).outputFolder;
					string[] directories = Directory.GetDirectories(outputFolder, "Layer_*");
					for (int i = 0; i < directories.Length; i++)
					{
						string path = directories[i];
						string fileName = Path.GetFileName(path);
						if (!dictionary.ContainsKey(fileName))
						{
							dictionary2[fileName] = true;
						}
					}
				}
				foreach (string current2 in dictionary2.Keys)
				{
					this.CheckSignal();
					D.Assert(!dictionary.ContainsKey(current2));
					try
					{
						this.PostMessage(string.Format("Deleting stale directory {0}", current2));
						RenderOutputMethod renderOutputMethod = baseOutputMethod.MakeChildMethod(current2);
						renderOutputMethod.EmptyDirectory();
					}
					catch (Exception arg)
					{
						this.PostMessage(string.Format("Failed to remove stale directory {0}; ignoring. Exception was {1}", current2, arg));
					}
				}
				foreach (Layer current3 in this.mashupScratchCopy.layerList)
				{
					string filesystemName2 = current3.GetFilesystemName();
					RenderOutputMethod renderOutputMethod2 = baseOutputMethod.MakeChildMethod(filesystemName2);
					EncodableHash encodableHash2 = new EncodableHash();
					current3.AccumulateRobustHash_PerTile(this.mapTileSourceFactory.GetCachePackage(), encodableHash2);
					new LayerMetadataFile(renderOutputMethod2, encodableHash2).Write();
				}
			}
			catch (Exception ex)
			{
				this.PostMessage(string.Format("Cannot prepare output directory: {0}. Please correct the problem or select a different render output directory.", ex.Message));
				throw new RenderState.RenderAborted();
			}
		}
		private void PurgeDirectory(RenderOutputMethod baseOutputMethod, string dirName)
		{
            RenderOutputMethod method = baseOutputMethod.MakeChildMethod(dirName);
            this.PostStatus(string.Format("Deleting old {0} directory", dirName));
            Label_0019:
            try
            {
                method.EmptyDirectory();
            }
            catch (Exception exception)
            {
                this.PostMessage(string.Format("Cannot delete {0} directory: {1}. Please correct the problem or select a different render output directory.", dirName, exception.Message));
                this.pauseRenderingFlag = true;
                this.CheckSignal();
                goto Label_0019;
            }
		}
		private void RenderLegends()
		{
			RenderOutputMethod renderOutputMethod = this.renderOutput.MakeChildMethod("legends");
			foreach (Layer current in this.mashupScratchCopy.layerList)
			{
				foreach (SourceMap current2 in current)
				{
					this.PostStatus("Opening " + current2.GetDisplayName() + " for legends.");
					UnwarpedMapTileSource displayableSource = this.mapTileSourceFactory.CreateUnwarpedSource(current2);
					foreach (Legend current3 in current2.legendList)
					{
						this.CheckSignal();
						string text = current2.GetLegendFilename(current3);
						text = RenderState.ForceValidFilename(text);
						this.PostStatus("Rendering legend " + text);
						ImageRef imageRef;
						try
						{
							imageRef = current3.RenderLegend(displayableSource);
						}
						catch (Legend.RenderFailedException arg)
						{
							this.PostMessage(string.Format("Skipping {0}: {1}", text, arg));
							continue;
						}
						try
						{
							RenderOutputUtil.SaveImage(imageRef, renderOutputMethod, text, ImageTypeMapper.ByExtension(Path.GetExtension(text)).imageFormat);
						}
						catch (Exception arg2)
						{
							this.PostMessage(string.Format("Failed to save {0}: {1}", text, arg2));
						}
					}
				}
			}
		}
		private void RenderThumbnails(CrunchedFile crunchedFile)
		{
			RenderOutputMethod thumbnailOutput = this.renderOutput.MakeChildMethod("thumbnails");
			foreach (int current in new List<int>
			{
				200,
				500
			})
			{
				foreach (Layer current2 in this.mashupScratchCopy.layerList)
				{
					CrunchedLayer crunchedLayer = crunchedFile[current2];
					if (current2.SomeSourceMapIsReadyToLock())
					{
						this.RenderThumbnail(thumbnailOutput, crunchedLayer, RenderState.ForceValidFilename(string.Format("{0}_{1}.png", current2.displayName, current)), new CompositeTileSource(current2, this.mapTileSourceFactory), current);
					}
					foreach (SourceMap current3 in current2)
					{
						if (current3.ReadyToLock())
						{
							SourceMapRecord thumbnailCollection = crunchedLayer[current3];
							this.RenderThumbnail(thumbnailOutput, thumbnailCollection, RenderState.ForceValidFilename(string.Format("{0}_{1}_{2}.png", current2.displayName, current3.displayName, current)), this.mapTileSourceFactory.CreateDisplayableWarpedSource(current3), current);
						}
					}
				}
			}
		}
		private void RenderThumbnail(RenderOutputMethod thumbnailOutput, ThumbnailCollection thumbnailCollection, string thumbnailFilename, IDisplayableSource displayableSource, int maxImageDimension)
		{
			this.PostStatus("Rendering thumbnail " + thumbnailFilename);
			LatentRegionHolder latentRegionHolder = new LatentRegionHolder(new DirtyEvent(), new DirtyEvent());
			Present present = displayableSource.GetUserBounds(latentRegionHolder, FutureFeatures.Cached).Realize("RenderState.RenderThumbnails");
			if (!(present is BoundsPresent))
			{
				this.PostMessage(string.Format("Failure writing thumbnail {0}; skipping.", thumbnailFilename, present));
				return;
			}
			MapRectangle boundingBox = ((BoundsPresent)present).GetRenderRegion().GetBoundingBox();
			new MercatorCoordinateSystem();
			LatLon nW = MercatorCoordinateSystem.LatLonToMercator(boundingBox.GetSW());
			LatLon sE = MercatorCoordinateSystem.LatLonToMercator(boundingBox.GetNE());
			MapRectangle mapRectangle = new MapRectangle(nW, sE);
			Size size = mapRectangle.SizeWithAspectRatio(maxImageDimension);
			IFuturePrototype imagePrototype = displayableSource.GetImagePrototype(new ImageParameterFromRawBounds(size), FutureFeatures.Cached);
			IFuture future = imagePrototype.Curry(new ParamDict(new object[]
			{
				TermName.ImageBounds,
				new MapRectangleParameter(boundingBox)
			}));
			Present present2 = future.Realize("RenderState.RenderThumbnails");
			if (!(present2 is ImageRef))
			{
				this.PostMessage(string.Format("Failure writing thumbnail {0}; skipping: {1}", thumbnailFilename, present2));
				return;
			}
			try
			{
				RenderOutputUtil.SaveImage((ImageRef)present2, thumbnailOutput, thumbnailFilename, ImageTypeMapper.ByExtension(Path.GetExtension(thumbnailFilename)).imageFormat);
				thumbnailCollection.Add(new ThumbnailRecord("thumbnails/" + thumbnailFilename, size));
			}
			catch (Exception arg)
			{
				this.PostMessage(string.Format("Failed to save {0}: {1}", thumbnailFilename, arg));
			}
		}
		private CrunchedFile CreateCrunchedFileDescriptor()
		{
			CrunchedFile crunchedFile = null;
			CrunchedFile result;
			try
			{
				string sourceMashupFilename = this.mashupScratchCopy.GetRenderOptions().publishSourceData ? string.Format("{0}/{1}", RenderState.SourceDataDirName, this.mashupScratchCopy.GetPublishedFilename()) : null;
				crunchedFile = new CrunchedFile(this.mashupScratchCopy, this.rangeQueryData, this.renderOutput, sourceMashupFilename, this.boundsList, this.mapTileSourceFactory);
				result = crunchedFile;
			}
			catch (Exception arg)
			{
				this.PostMessage(string.Format("Couldn't generate XML output file {0}: {1}", this.CrunchedFileLocation(crunchedFile), arg));
				result = null;
			}
			return result;
		}
		private void WriteCrunchedFileDescriptor(CrunchedFile crunchedFile)
		{
			try
			{
				crunchedFile.WriteXML();
				crunchedFile.WriteSourceMapLegendFrames();
				this.renderedXMLDescriptor = this.renderOutput.GetUri(crunchedFile.GetRelativeFilename());
				this.renderUI.uiChanged();
			}
			catch (Exception arg)
			{
				this.PostMessage(string.Format("Couldn't write XML output file {0}: {1}", this.CrunchedFileLocation(crunchedFile), arg));
			}
		}
		private string CrunchedFileLocation(CrunchedFile crunchedFile)
		{
			string result;
			try
			{
				result = crunchedFile.GetRelativeFilename();
			}
			catch (Exception)
			{
				result = "in " + this.mashupScratchCopy.GetRenderOptions().renderToOptions.ToString();
			}
			return result;
		}
		private void CopySourceData()
		{
			if (this.mashupScratchCopy.GetRenderOptions().publishSourceData)
			{
				this.PostStatus("Copying Source Data");
				RenderOutputMethod renderOutputMethod = this.renderOutput.MakeChildMethod(RenderState.SourceDataDirName);
				Stream stream = renderOutputMethod.CreateFile(this.mashupScratchCopy.GetPublishedFilename(), "text/xml");
				this.mashupScratchCopy.WriteXML(stream);
				stream.Close();
				foreach (Layer current in this.mashupScratchCopy.layerList)
				{
					foreach (SourceMap current2 in current)
					{
						SourceDocument sourceDocument = current2.documentFuture.RealizeSynchronously(this.mapTileSourceFactory.GetCachePackage());
						string filesystemAbsolutePath = sourceDocument.localDocument.GetFilesystemAbsolutePath();
						this.PostStatus(string.Format("Copying {0}", current2.GetDisplayName()));
						RenderOutputUtil.CopyFile(filesystemAbsolutePath, renderOutputMethod, Path.GetFileName(filesystemAbsolutePath), ImageTypeMapper.ByExtension(Path.GetExtension(filesystemAbsolutePath)).mimeType);
					}
				}
			}
		}
		private void CopyFile(string sourceFile, string targetDirectory, string mimeType)
		{
			this.CopyFileWithRename(sourceFile, targetDirectory, Path.GetFileName(sourceFile), mimeType);
		}
		private void CopyFileWithRename(string sourceFile, string targetDirectory, string targetFilename, string mimeType)
		{
			try
			{
				string relativeDestPath = Path.Combine(targetDirectory, targetFilename);
				RenderOutputUtil.CopyFile(sourceFile, this.renderOutput, relativeDestPath, mimeType);
			}
			catch (Exception ex)
			{
				this.PostMessage(string.Format("Failed to copy {0}: {1}", sourceFile, ex.ToString()));
			}
		}
		internal void UI_UpdateProgress(ProgressBar renderProgressBar)
		{
			if (this.mashupScratchCopy != null)
			{
				int num = this.estimateProgressSourceMapCount * 100 / this.estimateProgressSourceMapsThisLayer;
				int num2 = (this.mashupScratchCopy.layerList.Count + 1) * 100;
				int num3 = this.estimateProgressLayerCount * 100 + num;
				int num4 = this.initialQueueSize;
				int num5 = this.initialQueueSize - this.renderQueue.Count;
				if (num5 < 0)
				{
					num5 = 0;
				}
				if (num5 > this.initialQueueSize)
				{
					num5 = this.initialQueueSize;
				}
				double num6 = (double)num3 * 1.0 / (double)num2 * 0.1;
				double num7 = 0.0;
				if (num4 > 0)
				{
					num7 = (double)num5 * 1.0 / (double)num4 * 0.9;
				}
				double num8 = num6 + num7;
				num8 = Math.Max(Math.Min(num8, 1.0), 0.0);
				renderProgressBar.Minimum = 0;
				renderProgressBar.Maximum = 1000;
				renderProgressBar.Value = (int)(1000.0 * num8);
				renderProgressBar.Enabled = (this.state == RenderState.States.Rendering);
				return;
			}
			renderProgressBar.Minimum = 0;
			renderProgressBar.Maximum = 0;
			renderProgressBar.Value = 0;
			renderProgressBar.Enabled = false;
		}
		internal void UI_UpdateLinks(LinkLabel previewRenderedResultsLinkLabel, LinkLabel viewInBrowserLinkLabel)
		{
			previewRenderedResultsLinkLabel.Visible = (this.renderedXMLDescriptor != null);
			viewInBrowserLinkLabel.Visible = (this.sampleHTMLUri != null);
		}
		internal ImageRef UI_GetLastRenderedImageRef()
		{
			Monitor.Enter(this);
			ImageRef result;
			try
			{
				ImageRef imageRef = this.lastRenderedImageRef;
				if (imageRef == null)
				{
					result = null;
				}
				else
				{
					result = (ImageRef)imageRef.Duplicate("RenderState.UI_GetLastRenderedImageRef");
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
			return result;
		}
		internal string[] UI_GetTileDisplayLabel()
		{
			return this.lastRenderedImageLabel;
		}
		public Uri GetRenderedXMLDescriptor()
		{
			return this.renderedXMLDescriptor;
		}
		public Uri GetSampleHTMLUri()
		{
			return this.sampleHTMLUri;
		}
		public static string ForceValidFilename(string inStr)
		{
			string text = "";
			string text2 = new string(Path.GetInvalidFileNameChars()) + "%\\\"'& :<>";
			for (int i = 0; i < inStr.Length; i++)
			{
				char c = inStr[i];
				if (text2.IndexOf(c) < 0)
				{
					text += c;
				}
			}
			return text;
		}
	}
}
