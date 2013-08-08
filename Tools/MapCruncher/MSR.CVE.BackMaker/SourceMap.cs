using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class SourceMap : IDisposable, HasDisplayNameIfc, ReadyToLockIfc, PositionMemoryIfc, IRobustlyHashable, ExpandedMemoryIfc, LastViewIfc
	{
		public delegate string GetFilenameContext();
		private const string SourceMapTag = "SourceMap";
		private const string SourceMapFilenameAttribute = "SourceMapFilename";
		private const string SourceMapDisplayNameAttr = "DisplayName";
		private const string SourceMapOpenedAttr = "Opened";
		private const string SourceMapPageNumberAttr = "PageNumber";
		private const string ExpandedAttr = "Expanded";
		private const string LastSourcePositionTag_compat = "LastSourceMapPosition";
		private const string LastVEPositionTag_compat = "LastVEPosition";
		private const string LastVEStyleTag = "Style";
		private const string SnapViewTag = "SnapView";
		private const string SnapContextAttr = "Context";
		private const string SourceContextValue = "Source";
		private const string ReferenceContextValue = "Reference";
		private const string SnapZoomTag = "SnapZoom";
		private const string ZoomAttr = "Zoom";
		private GeneralDocumentFuture _documentFuture;
		private string _displayName;
		private SourceMap.GetFilenameContext filenameContextDelegate;
		private SourceMapInfo _sourceMapInfo;
		private SourceMapRenderOptions _sourceMapRenderOptions;
		private TransparencyOptions _transparencyOptions;
		private bool _expanded;
		public RegistrationDefinition registration;
		public LatentRegionHolder latentRegionHolder;
		public LegendList legendList;
		private SourceMapRegistrationView _lastView;
		public DirtyEvent dirtyEvent;
		public DirtyEvent readyToLockChangedEvent;
		public LatLonZoom sourceSnap = LatLonZoom.World();
		public LatLonZoom referenceSnap = LatLonZoom.World();
		public int sourceSnapZoom = 1;
		public int referenceSnapZoom = 1;
		public RenderRegion renderRegion
		{
			get
			{
				return this.latentRegionHolder.renderRegion;
			}
			set
			{
				this.latentRegionHolder.renderRegion = value;
			}
		}
		public bool expanded
		{
			get
			{
				return this._expanded;
			}
			set
			{
				this._expanded = value;
			}
		}
		public GeneralDocumentFuture documentFuture
		{
			get
			{
				return this._documentFuture;
			}
		}
		public string displayName
		{
			get
			{
				return this._displayName;
			}
			set
			{
				this._displayName = value;
				this.dirtyEvent.SetDirty();
			}
		}
		public SourceMapInfo sourceMapInfo
		{
			get
			{
				return this._sourceMapInfo;
			}
		}
		public SourceMapRenderOptions sourceMapRenderOptions
		{
			get
			{
				return this._sourceMapRenderOptions;
			}
		}
		public TransparencyOptions transparencyOptions
		{
			get
			{
				return this._transparencyOptions;
			}
		}
		public ICurrentView lastView
		{
			get
			{
				return this._lastView;
			}
		}
		public SourceMap(IDocumentFuture documentDescriptor, SourceMap.GetFilenameContext filenameContextDelegate, DirtyEvent parentDirty, DirtyEvent parentReadyToLockEvent)
		{
			this.dirtyEvent = new DirtyEvent(parentDirty);
			this.readyToLockChangedEvent = new DirtyEvent(parentReadyToLockEvent);
			this._documentFuture = new GeneralDocumentFuture(documentDescriptor);
			this._displayName = documentDescriptor.GetDefaultDisplayName();
			this.filenameContextDelegate = filenameContextDelegate;
			this._sourceMapInfo = new SourceMapInfo(this.dirtyEvent);
			this._sourceMapRenderOptions = new SourceMapRenderOptions(this.dirtyEvent);
			this._transparencyOptions = new TransparencyOptions(this.dirtyEvent);
			this.registration = new RegistrationDefinition(this.dirtyEvent);
			this.registration.dirtyEvent.Add(this.readyToLockChangedEvent);
			this.latentRegionHolder = new LatentRegionHolder(this.dirtyEvent, this.readyToLockChangedEvent);
			this.legendList = new LegendList(this, this.dirtyEvent, this.readyToLockChangedEvent);
			this.renderRegion = null;
		}
		public string GetDisplayName()
		{
			return this.displayName;
		}
		public void SetDisplayName(string value)
		{
			this.displayName = value;
		}
		public RenderRegion GetUserRegion()
		{
			return this.renderRegion;
		}
		public void Dispose()
		{
		}
		public static string GetXMLTag()
		{
			return "SourceMap";
		}
		public void WriteXML(MashupWriteContext wc)
		{
			XmlTextWriter writer = wc.writer;
			writer.WriteStartElement("SourceMap");
			writer.WriteAttributeString("DisplayName", this.displayName);
			writer.WriteAttributeString("Expanded", this._expanded.ToString(CultureInfo.InvariantCulture));
			wc.WriteIdentityAttr(this);
			this._documentFuture.WriteXML(wc, this.filenameContextDelegate());
			this._sourceMapInfo.WriteXML(writer);
			this._sourceMapRenderOptions.WriteXML(writer);
			this._transparencyOptions.WriteXML(writer);
			if (this._lastView != null)
			{
				this._lastView.WriteXML(writer);
			}
			writer.WriteStartElement("SnapView");
			writer.WriteAttributeString("Context", "Source");
			this.sourceSnap.WriteXML(writer);
			writer.WriteEndElement();
			writer.WriteStartElement("SnapView");
			writer.WriteAttributeString("Context", "Reference");
			this.referenceSnap.WriteXML(writer);
			writer.WriteEndElement();
			writer.WriteStartElement("SnapZoom");
			writer.WriteAttributeString("Context", "Source");
			writer.WriteAttributeString("Zoom", this.sourceSnapZoom.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement();
			writer.WriteStartElement("SnapZoom");
			writer.WriteAttributeString("Context", "Reference");
			writer.WriteAttributeString("Zoom", this.referenceSnapZoom.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement();
			this.registration.WriteXML(writer);
			if (this.renderRegion != null)
			{
				this.renderRegion.WriteXML(writer);
			}
			this.legendList.WriteXML(wc);
			writer.WriteEndElement();
		}
		public SourceMap(MashupParseContext context, SourceMap.GetFilenameContext filenameContextDelegate, DirtyEvent parentDirty, DirtyEvent parentReadyToLockEvent)
		{
			this.dirtyEvent = new DirtyEvent(parentDirty);
			this.readyToLockChangedEvent = new DirtyEvent(parentReadyToLockEvent);
			this.filenameContextDelegate = filenameContextDelegate;
			this.latentRegionHolder = new LatentRegionHolder(this.dirtyEvent, this.readyToLockChangedEvent);
			XMLTagReader xMLTagReader = context.NewTagReader("SourceMap");
			context.ExpectIdentity(this);
			string attribute = context.reader.GetAttribute("SourceMapFilename");
			if (attribute != null)
			{
				string path = Path.Combine(filenameContextDelegate(), attribute);
				int pageNumber = 0;
				context.GetAttributeInt("PageNumber", ref pageNumber);
				this._documentFuture = new GeneralDocumentFuture(new FutureDocumentFromFilesystem(path, pageNumber));
			}
			context.GetAttributeBoolean("Expanded", ref this._expanded);
			string attribute2 = context.reader.GetAttribute("DisplayName");
			MapPosition mapPosition = null;
			MapPosition mapPosition2 = null;
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs(RegistrationDefinition.GetXMLTag()))
				{
					context.AssertUnique(this.registration);
					this.registration = new RegistrationDefinition(context, this.dirtyEvent);
				}
				else
				{
					if (xMLTagReader.TagIs(GeneralDocumentFuture.GetXMLTag()))
					{
						context.AssertUnique(this._documentFuture);
						this._documentFuture = new GeneralDocumentFuture(context, filenameContextDelegate());
					}
					else
					{
						if (xMLTagReader.TagIs(LocalDocumentDescriptor.GetXMLTag()))
						{
							context.AssertUnique(this._documentFuture);
							LocalDocumentDescriptor localDocumentDescriptor = new LocalDocumentDescriptor(context, filenameContextDelegate());
							this._documentFuture = new GeneralDocumentFuture(new FutureDocumentFromFilesystem(localDocumentDescriptor.GetFilesystemAbsolutePath(), localDocumentDescriptor.GetPageNumber()));
						}
						else
						{
							if (xMLTagReader.TagIs("LastSourceMapPosition"))
							{
								XMLTagReader xMLTagReader2 = context.NewTagReader("LastSourceMapPosition");
								while (xMLTagReader2.FindNextStartTag())
								{
									if (xMLTagReader2.TagIs(MapPosition.GetXMLTag(context.version)))
									{
										mapPosition = new MapPosition(context, null, ContinuousCoordinateSystem.theInstance);
									}
								}
							}
							else
							{
								if (xMLTagReader.TagIs("LastVEPosition"))
								{
									XMLTagReader xMLTagReader3 = context.NewTagReader("LastVEPosition");
									while (xMLTagReader3.FindNextStartTag())
									{
										if (xMLTagReader3.TagIs(MapPosition.GetXMLTag(context.version)))
										{
											mapPosition2 = new MapPosition(context, null, MercatorCoordinateSystem.theInstance);
										}
									}
								}
								else
								{
									if (xMLTagReader.TagIs(RenderRegion.GetXMLTag()))
									{
										context.AssertUnique(this.renderRegion);
										this.renderRegion = new RenderRegion(context, this.dirtyEvent, ContinuousCoordinateSystem.theInstance);
									}
									else
									{
										if (xMLTagReader.TagIs(SourceMapInfo.GetXMLTag()))
										{
											context.AssertUnique(this._sourceMapInfo);
											this._sourceMapInfo = new SourceMapInfo(context, this.dirtyEvent);
										}
										else
										{
											if (xMLTagReader.TagIs(SourceMapRenderOptions.GetXMLTag()))
											{
												context.AssertUnique(this._sourceMapRenderOptions);
												this._sourceMapRenderOptions = new SourceMapRenderOptions(context, this.dirtyEvent);
											}
											else
											{
												if (xMLTagReader.TagIs(TransparencyOptions.GetXMLTag()))
												{
													this._transparencyOptions = new TransparencyOptions(context, this.dirtyEvent);
												}
												else
												{
													if (xMLTagReader.TagIs(SourceMapRegistrationView.GetXMLTag()))
													{
														context.AssertUnique(this._lastView);
														this._lastView = new SourceMapRegistrationView(this, context);
													}
													else
													{
														if (xMLTagReader.TagIs(LegendList.GetXMLTag()))
														{
															context.AssertUnique(this.legendList);
															this.legendList = new LegendList(this, context, this.dirtyEvent);
														}
														else
														{
															if (xMLTagReader.TagIs("SnapView"))
															{
																XMLTagReader xMLTagReader4 = context.NewTagReader("SnapView");
																string requiredAttribute = context.GetRequiredAttribute("Context");
																LatLonZoom latLonZoom = default(LatLonZoom);
																bool flag = false;
																bool flag2 = true;
																CoordinateSystemIfc coordSys = null;
																if (requiredAttribute == "Source")
																{
																	coordSys = ContinuousCoordinateSystem.theInstance;
																}
																else
																{
																	if (!(requiredAttribute == "Reference"))
																	{
																		throw new InvalidMashupFile(context, string.Format("Invalid {0} value {1}", "Context", requiredAttribute));
																	}
																	coordSys = MercatorCoordinateSystem.theInstance;
																}
																while (xMLTagReader4.FindNextStartTag())
																{
																	if (xMLTagReader4.TagIs(LatLonZoom.GetXMLTag()))
																	{
																		if (flag)
																		{
																			context.ThrowUnique();
																		}
																		try
																		{
																			latLonZoom = new LatLonZoom(context, coordSys);
																		}
																		catch (InvalidLLZ)
																		{
																			flag2 = false;
																		}
																		flag = true;
																	}
																}
																if (flag2)
																{
																	if (!flag)
																	{
																		context.AssertPresent(null, LatLonZoom.GetXMLTag());
																	}
																	if (requiredAttribute == "Source")
																	{
																		this.sourceSnap = latLonZoom;
																	}
																	else
																	{
																		if (requiredAttribute == "Reference")
																		{
																			this.referenceSnap = latLonZoom;
																		}
																		else
																		{
																			D.Assert(false, "handled above.");
																		}
																	}
																}
															}
															else
															{
																if (xMLTagReader.TagIs("SnapZoom"))
																{
																	context.NewTagReader("SnapZoom");
																	string requiredAttribute2 = context.GetRequiredAttribute("Context");
																	bool flag3 = false;
																	CoordinateSystemIfc theInstance;
																	if (requiredAttribute2 == "Source")
																	{
																		theInstance = ContinuousCoordinateSystem.theInstance;
																	}
																	else
																	{
																		if (!(requiredAttribute2 == "Reference"))
																		{
																			throw new InvalidMashupFile(context, string.Format("Invalid {0} value {1}", "Context", requiredAttribute2));
																		}
																		theInstance = MercatorCoordinateSystem.theInstance;
																	}
																	int num = 0;
																	try
																	{
																		theInstance.GetZoomRange().Parse(context, "Zoom");
																		flag3 = true;
																	}
																	catch (InvalidMashupFile)
																	{
																	}
																	if (flag3)
																	{
																		if (requiredAttribute2 == "Source")
																		{
																			this.sourceSnapZoom = num;
																		}
																		else
																		{
																			if (requiredAttribute2 == "Reference")
																			{
																				this.referenceSnapZoom = num;
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
				if (context.version == InlineSourceMapInfoSchema.schema)
				{
					if (xMLTagReader.TagIs("MapFileURL"))
					{
						this._sourceMapInfo.mapFileURL = XMLUtils.ReadStringXml(context, "MapFileURL");
					}
					else
					{
						if (xMLTagReader.TagIs("MapHomePage"))
						{
							this._sourceMapInfo.mapHomePage = XMLUtils.ReadStringXml(context, "MapHomePage");
						}
						else
						{
							if (xMLTagReader.TagIs("MapDescription"))
							{
								this._sourceMapInfo.mapDescription = XMLUtils.ReadStringXml(context, "MapDescription");
							}
						}
					}
				}
			}
			if (attribute2 != null)
			{
				this._displayName = attribute2;
			}
			else
			{
				this._displayName = this._documentFuture.documentFuture.GetDefaultDisplayName();
			}
			if (this._lastView == null && mapPosition != null && mapPosition2 != null)
			{
				this._lastView = new SourceMapRegistrationView(this, mapPosition.llz, mapPosition2);
			}
			if (this._documentFuture == null)
			{
				throw new Exception("Source Map element missing document descriptor tag");
			}
			if (this.registration == null)
			{
				this.registration = new RegistrationDefinition(this.dirtyEvent);
			}
			this.registration.dirtyEvent.Add(this.readyToLockChangedEvent);
			if (this.legendList == null)
			{
				this.legendList = new LegendList(this, this.dirtyEvent, this.readyToLockChangedEvent);
			}
			if (this._sourceMapInfo == null)
			{
				this._sourceMapInfo = new SourceMapInfo(this.dirtyEvent);
			}
			if (this._sourceMapRenderOptions == null)
			{
				this._sourceMapRenderOptions = new SourceMapRenderOptions(this.dirtyEvent);
			}
			if (this._transparencyOptions == null)
			{
				this._transparencyOptions = new TransparencyOptions(this.dirtyEvent);
			}
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			this._documentFuture.AccumulateRobustHash(hash);
			this.AccumulateRobustHash_Common(hash);
			this._sourceMapRenderOptions.AccumulateRobustHash(hash);
		}
		public void AccumulateRobustHash_PerTile(CachePackage cachePackage, IRobustHash hash)
		{
			hash.Accumulate("SourceMap:");
			SourceDocument sourceDocument = this._documentFuture.RealizeSynchronously(cachePackage);
			sourceDocument.localDocument.AccumulateRobustHash(hash);
			this.AccumulateRobustHash_Common(hash);
		}
		private void AccumulateRobustHash_Common(IRobustHash hash)
		{
			this.registration.AccumulateRobustHash(hash);
			if (this.renderRegion != null)
			{
				this.renderRegion.AccumulateRobustHash(hash);
			}
			else
			{
				hash.Accumulate("null-render-region");
			}
			this._transparencyOptions.AccumulateRobustHash(hash);
		}
		public bool ReadyToLock()
		{
			return this.registration != null && this.registration.ReadyToLock() && this.renderRegion != null;
		}
		public void AutoSelectMaxZoom(MapTileSourceFactory mapTileSourceFactory)
		{
			if (this.sourceMapRenderOptions.maxZoom == -1)
			{
				MapRectangle userBoundingBox = this.GetUserBoundingBox(mapTileSourceFactory);
				if (userBoundingBox == null)
				{
					return;
				}
				Size size = new Size(600, 600);
				LatLonZoom bestViewContaining = new MercatorCoordinateSystem().GetBestViewContaining(userBoundingBox, size);
				IntParameter intParameter = (IntParameter)mapTileSourceFactory.CreateUnwarpedSource(this).GetImageDetailPrototype(FutureFeatures.Cached).Curry(new ParamDict(new object[]
				{
					TermName.ImageDetail,
					new SizeParameter(size)
				})).Realize("SourceMap.AutoSelectMaxZoom");
				this.sourceMapRenderOptions.maxZoom = MercatorCoordinateSystem.theInstance.GetZoomRange().Constrain(bestViewContaining.zoom + intParameter.value + BuildConfig.theConfig.autoMaxZoomOffset);
			}
		}
		public MapRectangle GetUserBoundingBox(MapTileSourceFactory mapTileSourceFactory)
		{
			WarpedMapTileSource warpedMapTileSource = null;
			try
			{
				warpedMapTileSource = mapTileSourceFactory.CreateWarpedSource(this);
			}
			catch (InsufficientCorrespondencesException)
			{
			}
			if (warpedMapTileSource == null)
			{
				return null;
			}
			Present present = warpedMapTileSource.GetUserBounds(null, FutureFeatures.Cached).Realize("SourceMap.AutoSelectMaxZoom");
			if (!(present is BoundsPresent))
			{
				return null;
			}
			BoundsPresent boundsPresent = (BoundsPresent)present;
			MapRectangle boundingBox = boundsPresent.GetRenderRegion().GetBoundingBox();
			return boundingBox.ClipTo(CoordinateSystemUtilities.GetRangeAsMapRectangle(MercatorCoordinateSystem.theInstance));
		}
		public void NotePositionUnlocked(LatLonZoom sourceMapPosition, MapPosition referenceMapPosition)
		{
			this._lastView = new SourceMapRegistrationView(this, sourceMapPosition, referenceMapPosition);
		}
		public void NotePositionLocked(MapPosition referenceMapPosition)
		{
			this._lastView = new SourceMapRegistrationView(this, referenceMapPosition);
		}
		public string GetLegendFilename(Legend legend)
		{
			return RenderState.ForceValidFilename(string.Format("{0}_{1}.png", this.GetDisplayName(), legend.displayName));
		}
	}
}
