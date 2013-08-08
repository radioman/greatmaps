using BackMaker;
using MSR.CVE.BackMaker.ImagePipeline;
using MSR.CVE.BackMaker.MCDebug;
using MSR.CVE.BackMaker.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
    public class MainAppForm : Form, AssociationIfc, LayerControlIfc, ViewControlIfc, DocumentMutabilityControlIfc
    {
        private delegate void DKCUI(bool enabled);
        private delegate void ExitDelegate(int rc);
        private delegate void ReadyToLockChangedDelegate();
        private class Opening
        {
            public bool opening;
        }
        private class UndoAddSourceMap
        {
            private delegate void CloseViewDelegate();
            private string filename;
            private SourceMap newSourceMap;
            private Layer addedToLayer;
            private LayerControls layerControls;
            private MainAppForm mainAppForm;
            public UndoAddSourceMap(string filename, SourceMap newSourceMap, Layer addedToLayer, LayerControls layerControls, MainAppForm mainAppForm)
            {
                this.filename = filename;
                this.newSourceMap = newSourceMap;
                this.addedToLayer = addedToLayer;
                this.layerControls = layerControls;
                this.mainAppForm = mainAppForm;
            }
            public void Undo(string message)
            {
                MessageBox.Show(string.Format("Can't open {0}:\n{1}", this.filename, message), "Can't Open Source Map", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                if (this.addedToLayer != null && this.newSourceMap != null && this.layerControls != null)
                {
                    this.addedToLayer.Remove(this.newSourceMap);
                    this.layerControls.CancelSourceMap(this.addedToLayer, this.newSourceMap);
                }
                MainAppForm.UndoAddSourceMap.CloseViewDelegate method = new MainAppForm.UndoAddSourceMap.CloseViewDelegate(this.mainAppForm.CloseView);
                this.mainAppForm.Invoke(method);
            }
        }
        private const string regSourceMapFileName = "last_open_source_map";
        private const string regWindowWidth = "gui_window_width";
        private const string regWindowHeight = "gui_window_height";
        private const string regWindowX = "gui_window_x";
        private const string regWindowY = "gui_window_y";
        private const string regControlSplitterPos = "control_splitter_pos";
        private const string regMapSplitterPos = "gui_splitter_pos";
        public const string DocumentExtension = "yum";
        private const int WM_SETREDRAW = 11;
        private string programName;
        private UIPositionManager uiPosition;
        private CachePackage cachePackage;
        private CachePackage renderedTileCachePackage;
        private MapTileSourceFactory mapTileSourceFactory;
        private BackMakerRegistry backMakerRegistry = new BackMakerRegistry();
        private Mashup currentMashup;
        private RegistrationControlRecord displayedRegistration;
        private IViewManager currentView;
        private bool documentIsMutable;
        private bool alreadyExiting;
        private bool undone;
        private RenderWindow renderWindow;
        private SourceMapOverviewWindow sourceMapOverviewWindow;
        private string startDocumentPath;
        private bool renderOnLaunch;
        private MainAppForm.Opening opening = new MainAppForm.Opening();
        private int paintFrozen;
        private IContainer components;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutMSRBackMakerToolStripMenuItem;
        private ToolStripMenuItem openMashupMenuItem;
        private ToolStripMenuItem closeMashupMenuItem;
        private ToolStripMenuItem mapOptionsToolStripMenuItem2;
        private ToolStripMenuItem showCrosshairsMenuItem;
        private ToolStripMenuItem showPushPinsMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem newMashupMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem addSourceMapFromUriMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem saveMashupMenuItem;
        private ToolStripMenuItem saveMashupAsMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem viewRenderedMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem VEroadView;
        private ToolStripMenuItem VEaerialView;
        private ToolStripMenuItem VEhybridView;
        private ToolStripMenuItem AddRegLayerMenuItem;
        private ToolStripSeparator snapFeaturesToolStripSeparator;
        private SplitContainer mapSplitContainer;
        private ViewerControl smViewerControl;
        private ViewerControl veViewerControl;
        private SplitContainer controlsSplitContainer;
        private LayerControls layerControls;
        private TabControl synergyExplorer;
        private TabPage correspondencesTab;
        private registrationControls registrationControls;
        private TabPage sourceInfoTab;
        private SourceMapInfoPanel sourceMapInfoPanel;
        private SplitContainer controlSplitContainer;
        private ToolStripMenuItem viewMapCruncherTutorialToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private Button RenderLaunchButton;
        private Panel panel1;
        private ToolStripMenuItem addSourceMapMenuItem;
        private TabPage transparencyTab;
        private TransparencyPanel transparencyPanel;
        private TabPage legendTabPage;
        private LegendOptionsPanel legendOptionsPanel1;
        private ToolStripMenuItem showDMSMenuItem;
        private ToolStripMenuItem showSourceMapOverviewMenuItem;
        private ToolStripMenuItem restoreSnapViewMenuItem;
        private ToolStripMenuItem recordSnapViewMenuItem;
        private ToolStripMenuItem recordSnapZoomMenuItem;
        private ToolStripMenuItem restoreSnapZoomMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem debugToolStripMenuItem;
        private ToolStripMenuItem showTileNamesMenuItem;
        private ToolStripMenuItem showSourceCropToolStripMenuItem;
        private ToolStripMenuItem showTileBoundariesMenuItem;
        private ToolStripMenuItem showDiagnosticsUIToolStripMenuItem;
        private ToolStripSeparator debugModeToolStripSeparator;
        private ToolStripMenuItem enableDebugModeToolStripMenuItem;

        //[CompilerGenerated]
        //private static Converter<PositionAssociation, PositionAssociationView> <>9__CachedAnonymousMethodDelegate6;
        //[CompilerGenerated]
        //private static Converter<PositionAssociation, PositionAssociationView> <>9__CachedAnonymousMethodDelegate7;
        //[CompilerGenerated]
        //private static Converter<string, string> <>9__CachedAnonymousMethodDelegate9;
        //[CompilerGenerated]
        //private static Converter<PositionAssociation, PositionAssociationView> <>9__CachedAnonymousMethodDelegateb;

        private bool FreezePainting
        {
            get
            {
                return this.paintFrozen > 0;
            }
            set
            {
                if (value && base.IsHandleCreated && base.Visible && this.paintFrozen++ == 0)
                {
                    MainAppForm.SendMessage(base.Handle, 11, 0, 0);
                }
                if (!value)
                {
                    if (this.paintFrozen == 0)
                    {
                        return;
                    }
                    if (--this.paintFrozen == 0)
                    {
                        MainAppForm.SendMessage(base.Handle, 11, 1, 0);
                        base.Invalidate(true);
                    }
                }
            }
        }
        public MainAppForm(string startDocumentPath, bool renderOnLaunch)
        {
            this.startDocumentPath = startDocumentPath;
            this.renderOnLaunch = renderOnLaunch;
        }
        public void StartUpApplication()
        {
            try
            {
                D.SetDebugLevel(BuildConfig.theConfig.debugLevel);
                this.cachePackage = new CachePackage();
                this.renderedTileCachePackage = this.cachePackage.DeriveCache("renderedTile");
                this.InitializeComponent();
                this.SetOptionsPanelVisibility(OptionsPanelVisibility.Nothing);
                this.mapTileSourceFactory = new MapTileSourceFactory(this.cachePackage);
            }
            catch (ConfigurationException)
            {
                this.UndoConstruction();
                throw;
            }
            this.layerControls.SetLayerControl(this);
            this.RestoreWindowParameters();
            this.SetInterfaceNoMashupOpen();
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 10000;
            timer.Tick += new EventHandler(this.saveBackupTimer_Tick);
            timer.Start();
            this.registrationControls.ShowDMS = new MapDrawingOption(this.registrationControls, this.showDMSMenuItem, false);
            this.smViewerControl.ShowCrosshairs = new MapDrawingOption(this.smViewerControl, this.showCrosshairsMenuItem, true);
            this.smViewerControl.ShowTileBoundaries = new MapDrawingOption(this.smViewerControl, this.showTileBoundariesMenuItem, true);
            this.smViewerControl.ShowPushPins = new MapDrawingOption(this.smViewerControl, this.showPushPinsMenuItem, true);
            this.smViewerControl.ShowTileNames = new MapDrawingOption(this.smViewerControl, this.showTileNamesMenuItem, false);
            this.smViewerControl.ShowSourceCrop = new MapDrawingOption(this.smViewerControl, this.showSourceCropToolStripMenuItem, true);
            this.smViewerControl.ShowDMS = new MapDrawingOption(this.smViewerControl, this.showDMSMenuItem, false);
            this.smViewerControl.SetLLZBoxLabelStyle(LLZBox.LabelStyle.XY);
            this.SetVEMapStyle(VirtualEarthWebDownloader.RoadStyle);
            this.veViewerControl.ShowCrosshairs = new MapDrawingOption(this.veViewerControl, this.showCrosshairsMenuItem, true);
            this.veViewerControl.ShowTileBoundaries = new MapDrawingOption(this.veViewerControl, this.showTileBoundariesMenuItem, false);
            this.veViewerControl.ShowPushPins = new MapDrawingOption(this.veViewerControl, this.showPushPinsMenuItem, true);
            this.veViewerControl.ShowTileNames = new MapDrawingOption(this.veViewerControl, this.showTileNamesMenuItem, false);
            this.veViewerControl.ShowDMS = new MapDrawingOption(this.veViewerControl, this.showDMSMenuItem, false);
            this.veViewerControl.configureLLZBoxEditable();
            this.uiPosition = new UIPositionManager(this.smViewerControl, this.veViewerControl);
            this.uiPosition.GetVEPos().setPosition(this.veViewerControl.GetCoordinateSystem().GetDefaultView());
            this.recordSnapViewMenuItem.Click += new EventHandler(this.recordSnapViewMenuItem_Click);
            this.restoreSnapViewMenuItem.Click += new EventHandler(this.restoreSnapViewMenuItem_Click);
            this.recordSnapZoomMenuItem.Click += new EventHandler(this.recordSnapZoomMenuItem_Click);
            this.restoreSnapZoomMenuItem.Click += new EventHandler(this.restoreSnapZoomMenuItem_Click);
            this.registrationControls.setAssociationIfc(this);
            this.setDisplayedRegistration(null);
            this.sourceMapInfoPanel.Initialize(new SourceMapInfoPanel.PreviewSourceMapZoomDelegate(this.PreviewSourceMapZoom));
            BigDebugKnob.theKnob.AddListener(new BigDebugKnob.DebugKnobListener(this.debugKnobChanged));
            BigDebugKnob.theKnob.debugFeaturesEnabled = false;
            this.enableDebugModeToolStripMenuItem.Visible = BuildConfig.theConfig.debugModeEnabled;
            this.debugModeToolStripSeparator.Visible = BuildConfig.theConfig.debugModeEnabled;
            if (this.startDocumentPath != null)
            {
                this.LoadMashup(Path.GetFullPath(this.startDocumentPath));
            }
            else
            {
                this.NewMashup();
            }
            if (this.renderOnLaunch)
            {
                this.currentMashup.AutoSelectMaxZooms(this.mapTileSourceFactory);
                this.LaunchRenderWindow();
                this.renderWindow.StartRender(new RenderProgressPanel2.RenderCompleteDelegate(this.LaunchedRenderComplete));
                base.Shown += new EventHandler(this.MainAppForm_Shown_BringRenderWindowToFront);
            }
        }
        private void debugKnobChanged(bool enabled)
        {
            try
            {
                MainAppForm.DKCUI method = new MainAppForm.DKCUI(this.debugKnobChanged_UI);
                base.Invoke(method, new object[]
				{
					enabled
				});
            }
            catch (InvalidOperationException)
            {
                this.debugKnobChanged_UI(enabled);
            }
        }
        private void debugKnobChanged_UI(bool enabled)
        {
            this.debugToolStripMenuItem.Visible = enabled;
            this.enableDebugModeToolStripMenuItem.Checked = enabled;
        }
        private void MainAppForm_Shown_BringRenderWindowToFront(object sender, EventArgs e)
        {
            this.renderWindow.BringToFront();
        }
        private void LaunchedRenderComplete(Exception failure)
        {
            if (!this.alreadyExiting)
            {
                MainAppForm.ExitDelegate method = new MainAppForm.ExitDelegate(this.LaunchedRenderComplete_ExitApplication);
                int num = (failure == null) ? 0 : 255;
                base.Invoke(method, new object[]
				{
					num
				});
            }
        }
        private void LaunchedRenderComplete_ExitApplication(int rc)
        {
            ProgramInstance.SetApplicationResultCode(rc);
            this.TeardownApplication();
            Application.Exit();
        }
        private void saveBackupTimer_Tick(object sender, EventArgs e)
        {
            if (this.currentMashup != null)
            {
                this.currentMashup.AutoSaveBackup();
            }
        }
        private int RobustGetFromRegistry(int defaultValue, string registryEntryName)
        {
            string value = this.backMakerRegistry.GetValue(registryEntryName);
            if (value != null)
            {
                return int.Parse(value);
            }
            return defaultValue;
        }
        private void RestoreWindowParameters()
        {
            try
            {
                this.programName = this.Text;
                if (this.backMakerRegistry.GetValue("gui_window_width") != null)
                {
                    Point location = new Point(int.Parse(this.backMakerRegistry.GetValue("gui_window_x")), int.Parse(this.backMakerRegistry.GetValue("gui_window_y")));
                    if (location.X > 0 && location.Y > 0)
                    {
                        base.Location = location;
                        base.Width = this.RobustGetFromRegistry(base.Width, "gui_window_width");
                        base.Height = this.RobustGetFromRegistry(base.Height, "gui_window_height");
                        this.controlSplitContainer.SplitterDistance = this.RobustGetFromRegistry(this.controlSplitContainer.SplitterDistance, "control_splitter_pos");
                        this.mapSplitContainer.SplitterDistance = this.RobustGetFromRegistry(this.mapSplitContainer.SplitterDistance, "gui_splitter_pos");
                    }
                }
            }
            catch (Exception)
            {
            }
            this.recordSnapViewMenuItem.Visible = BuildConfig.theConfig.enableSnapFeatures;
            this.recordSnapViewMenuItem.Enabled = BuildConfig.theConfig.enableSnapFeatures;
            this.restoreSnapViewMenuItem.Visible = BuildConfig.theConfig.enableSnapFeatures;
            this.restoreSnapViewMenuItem.Enabled = BuildConfig.theConfig.enableSnapFeatures;
            this.recordSnapZoomMenuItem.Visible = BuildConfig.theConfig.enableSnapFeatures;
            this.recordSnapZoomMenuItem.Enabled = BuildConfig.theConfig.enableSnapFeatures;
            this.restoreSnapZoomMenuItem.Visible = BuildConfig.theConfig.enableSnapFeatures;
            this.restoreSnapZoomMenuItem.Enabled = BuildConfig.theConfig.enableSnapFeatures;
            this.snapFeaturesToolStripSeparator.Visible = BuildConfig.theConfig.enableSnapFeatures;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!this.CloseMashup())
            {
                e.Cancel = true;
                return;
            }
            this.TeardownApplication();
            Application.Exit();
            base.OnClosing(e);
        }
        private void TeardownApplication()
        {
            this.alreadyExiting = true;
            this.backMakerRegistry.SetValue("gui_window_width", base.Width.ToString());
            this.backMakerRegistry.SetValue("gui_window_height", base.Height.ToString());
            this.backMakerRegistry.SetValue("gui_window_x", base.Location.X.ToString());
            this.backMakerRegistry.SetValue("gui_window_y", base.Location.Y.ToString());
            this.backMakerRegistry.SetValue("control_splitter_pos", this.controlSplitContainer.SplitterDistance.ToString());
            this.backMakerRegistry.SetValue("gui_splitter_pos", this.mapSplitContainer.SplitterDistance.ToString());
            this.UndoConstruction();
        }
        public void UndoConstruction()
        {
            Monitor.Enter(this);
            try
            {
                if (!this.undone)
                {
                    this.KillRenderWindow();
                    this.cachePackage.Dispose();
                    this.renderedTileCachePackage.Dispose();
                    this.backMakerRegistry.Dispose();
                    this.undone = true;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
        public void SetVEMapStyle(string s)
        {
            if (!VirtualEarthWebDownloader.StyleIsValid(s))
            {
                return;
            }
            if (this.uiPosition != null)
            {
                this.uiPosition.GetVEPos().setStyle(s);
            }
            this.VEroadView.Checked = (s == VirtualEarthWebDownloader.RoadStyle);
            this.VEaerialView.Checked = (s == VirtualEarthWebDownloader.AerialStyle);
            this.VEhybridView.Checked = (s == VirtualEarthWebDownloader.HybridStyle);
            this.veViewerControl.SetBaseLayer(new VETileSource(this.GetCachePackage(), s));
        }
        private void roadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SetVEMapStyle(VirtualEarthWebDownloader.RoadStyle);
        }
        private void aerialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SetVEMapStyle(VirtualEarthWebDownloader.AerialStyle);
        }
        private void hybridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SetVEMapStyle(VirtualEarthWebDownloader.HybridStyle);
        }
        private void AddRegLayerMenuItem_Click(object sender, EventArgs e)
        {
            RenderedLayerDisplayInfo layerSelector = RenderedLayerSelector.GetLayerSelector(this.veViewerControl, this.renderedTileCachePackage);
            if (layerSelector != null)
            {
                foreach (ToolStripMenuItem current in layerSelector.tsmiList)
                {
                    this.mapOptionsToolStripMenuItem2.DropDownItems.Add(current);
                }
                this.uiPosition.GetVEPos().setPosition(layerSelector.defaultView);
            }
        }
        private void aboutMSRBackMakerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm(MSR.CVE.BackMaker.Resources.Version.ApplicationVersionNumber);
            aboutForm.ShowDialog();
        }
        private void viewMapCruncherTutorialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("hh.exe")
            {
                WindowStyle = ProcessWindowStyle.Normal,
                Arguments = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "MapCruncher.chm")
            });
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CloseMashup())
            {
                this.TeardownApplication();
                Application.Exit();
            }
        }
        private void viewRenderedMenuItem_Click(object sender, EventArgs e)
        {
            RenderedMashupViewer renderedMashupViewer = new RenderedMashupViewer(this.renderedTileCachePackage, this.showDMSMenuItem);
            renderedMashupViewer.Show();
        }
        public void AddNewAssociation(string newPinName)
        {
            D.Assert(!((SourceMapViewManager)this.currentView).MapsLocked());
            PositionAssociation positionAssociation = new PositionAssociation(newPinName, this.uiPosition.GetSMPos().llz, this.uiPosition.GetSMPos().llz, this.uiPosition.GetVEPos().llz, this.displayedRegistration.model.dirtyEvent);
            this.CheckForDuplicatePushpin(positionAssociation, -1);
            this.displayedRegistration.model.AddAssociation(positionAssociation);
            this.updateRegistrationDisplay();
        }
        public void UpdateAssociation(PositionAssociation assoc, string newName)
        {
            PositionAssociation newAssoc = new PositionAssociation("proposed", this.uiPosition.GetSMPos().llz, this.uiPosition.GetSMPos().llz, this.uiPosition.GetVEPos().llz, new DirtyEvent());
            this.CheckForDuplicatePushpin(newAssoc, assoc.pinId);
            assoc.UpdateAssociation(this.uiPosition.GetSMPos().llz, this.uiPosition.GetVEPos().llz);
            if (newName != null && newName != "")
            {
                assoc.associationName = newName;
            }
            this.updateRegistrationDisplay();
        }
        private void CheckForDuplicatePushpin(PositionAssociation newAssoc, int ignorePinId)
        {
            foreach (PositionAssociation current in this.displayedRegistration.model.GetAssociationList())
            {
                if (ignorePinId == -1 || ignorePinId != current.pinId)
                {
                    bool flag = current.globalPosition.pinPosition == newAssoc.globalPosition.pinPosition;
                    bool flag2 = current.imagePosition.pinPosition == newAssoc.imagePosition.pinPosition;
                    string text = "";
                    if (flag && flag2)
                    {
                        text = "reference and source";
                    }
                    else
                    {
                        if (flag)
                        {
                            text = "reference";
                        }
                        else
                        {
                            if (flag2)
                            {
                                text = "source";
                            }
                        }
                    }
                    if (text != "")
                    {
                        throw new DuplicatePushpinException(text, current.pinId, current.associationName);
                    }
                }
            }
        }
        public void RemoveAssociation(PositionAssociation assoc)
        {
            this.displayedRegistration.model.RemoveAssociation(assoc);
            this.updateRegistrationDisplay();
        }
        public void ViewAssociation(PositionAssociation pa)
        {
            this.uiPosition.GetSMPos().setPosition(pa.sourcePosition.pinPosition);
            this.uiPosition.GetVEPos().setPosition(pa.globalPosition.pinPosition);
            this.SetVEMapStyle(this.uiPosition.GetVEPos().style);
        }
        public void setDisplayedRegistration(RegistrationControlRecord display)
        {
            PositionAssociation oldSelectedPA = this.registrationControls.GetSelected();
            this.displayedRegistration = display;
            this.updateRegistrationDisplay();
            PositionAssociation selected = null;
            if (oldSelectedPA != null && this.displayedRegistration != null)
            {
                selected = this.displayedRegistration.model.GetAssociationList().Find((PositionAssociation pa) => pa.pinId == oldSelectedPA.pinId);
            }
            this.registrationControls.SetSelected(selected);
        }
        private void updateRegistrationDisplay()
        {
            if (this.displayedRegistration != null)
            {
                Converter<PositionAssociation, PositionAssociationView> converter = (PositionAssociation pa) => new PositionAssociationView(pa, PositionAssociationView.WhichPosition.global);
                this.veViewerControl.setPinList(this.displayedRegistration.model.GetAssociationList().ConvertAll<PositionAssociationView>(converter));
                Converter<PositionAssociation, PositionAssociationView> converter2 = (PositionAssociation pa) => new PositionAssociationView(pa, PositionAssociationView.WhichPosition.source);
                List<PositionAssociationView> pinList = this.displayedRegistration.model.GetAssociationList().ConvertAll<PositionAssociationView>(converter2);
                this.smViewerControl.setPinList(pinList);
            }
            else
            {
                this.veViewerControl.setPinList(new List<PositionAssociationView>());
                this.smViewerControl.setPinList(new List<PositionAssociationView>());
            }
            this.UpdateOverviewPins();
            this.registrationControls.DisplayModel(this.displayedRegistration);
        }
        private void EnableMashupInterfaceItems(bool enable)
        {
            this.saveMashupMenuItem.Enabled = enable;
            this.saveMashupAsMenuItem.Enabled = enable;
            this.closeMashupMenuItem.Enabled = enable;
            this.addSourceMapMenuItem.Enabled = enable;
            this.addSourceMapFromUriMenuItem.Enabled = enable;
            this.controlSplitContainer.Visible = enable;
        }
        private void updateWindowTitle()
        {
            if (this.currentMashup == null)
            {
                this.Text = this.programName;
                return;
            }
            this.Text = string.Format("{0} - {1}", this.currentMashup.GetDisplayName(), this.programName);
        }
        private void OpenMashup(Mashup newmash)
        {
            D.Assert(this.currentMashup == null);
            this.currentMashup = newmash;
            this.OpenView(new NothingLayerViewManager(this));
            this.EnableMashupInterfaceItems(true);
            this.updateWindowTitle();
            this.layerControls.SetMashup(this.currentMashup);
            this.currentMashup.readyToLockEvent.Add(new DirtyListener(this.ReadyToLockChangedHandler));
            this.ReadyToLockChanged();
        }
        private void ReadyToLockChangedHandler()
        {
            MainAppForm.ReadyToLockChangedDelegate method = new MainAppForm.ReadyToLockChangedDelegate(this.ReadyToLockChanged);
            base.Invoke(method);
        }
        private void ReadyToLockChanged()
        {
            this.RenderLaunchButton.Enabled = this.currentMashup.SomeSourceMapIsReadyToLock();
        }
        private bool SaveMashup()
        {
            if (this.currentMashup.GetFilename() == null)
            {
                return this.SaveMashupAs();
            }
            try
            {
                this.currentMashup.WriteXML();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Can't save mashup {0}:\n{1}", this.currentMashup.GetFilename(), ex.Message), "Error Writing Mashup", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            return true;
        }
        private void saveMashupMenuItem_Click(object sender, EventArgs e)
        {
            this.SaveMashup();
        }
        private bool SaveMashupAs()
        {
            while (true)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = string.Format("MapCruncher Mashup Files (*.{0})|*.{0}", "yum");
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.Title = "Enter new mashup filename";
                saveFileDialog.AddExtension = true;
                saveFileDialog.CheckFileExists = false;
                saveFileDialog.CheckPathExists = true;
                saveFileDialog.OverwritePrompt = false;
                if (this.currentMashup.GetFilename() != null)
                {
                    saveFileDialog.FileName = this.currentMashup.GetDisplayName();
                }
                else
                {
                    if (this.currentMashup.layerList.Count > 0 && this.currentMashup.layerList.First.Count > 0)
                    {
                        saveFileDialog.FileName = this.currentMashup.layerList.First.First.GetDisplayName() + ".yum";
                    }
                }
                if (saveFileDialog.ShowDialog() != DialogResult.OK || saveFileDialog.FileName == null)
                {
                    break;
                }
                string text = saveFileDialog.FileName;
                if (Path.GetExtension(text) != ".yum")
                {
                    text += ".yum";
                }
                if (!File.Exists(text))
                {
                    goto IL_14A;
                }
                if (MessageBox.Show(string.Format("{0} already exists.\nDo you want to replace it?", text), "Overwrite Existing File?", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                {
                    try
                    {
                        File.Delete(text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Can't overwrite {0}:\n{1}", text, ex.Message), "Error Deleting Existing File", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        goto IL_15C;
                    }
                    goto IL_14A;
                }
                IL_15C:
                if (this.currentMashup.GetFilename() != null && this.SaveMashup())
                {
                    return true;
                }
                continue;
                IL_14A:
                this.currentMashup.SetFilename(text);
                this.updateWindowTitle();
                goto IL_15C;
            }
            return false;
        }
        private void saveMashupAsMenuItem_Click(object sender, EventArgs e)
        {
            this.SaveMashupAs();
        }
        private bool CloseMashup()
        {
            if (this.currentMashup == null)
            {
                return true;
            }
            if (this.currentMashup.IsDirty())
            {
                string text;
                if (this.currentMashup.GetFilename() == null)
                {
                    text = "Save untitled mashup?";
                }
                else
                {
                    text = string.Format("Save changes to mashup {0}?", this.currentMashup.GetFilename());
                }
                DialogResult dialogResult = MessageBox.Show(text, "Save changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (dialogResult == DialogResult.Cancel)
                {
                    return false;
                }
                if (dialogResult == DialogResult.Yes && !this.SaveMashup())
                {
                    return false;
                }
            }
            this.currentMashup.Close();
            this.currentMashup = null;
            this.SetInterfaceNoMashupOpen();
            return true;
        }
        private void SetInterfaceNoMashupOpen()
        {
            this.SetOptionsPanelVisibility(OptionsPanelVisibility.Nothing);
            this.KillRenderWindow();
            this.layerControls.SetMashup(null);
            this.CloseView();
            this.EnableMashupInterfaceItems(false);
            this.updateWindowTitle();
        }
        private void LoadMashup(string fileName)
        {
            MashupFileWarningList mashupFileWarningList = null;
            Mashup mashup;
            try
            {
                mashup = Mashup.OpenMashupInteractive(fileName, out mashupFileWarningList);
                if (mashup == null)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Can't open {0}:\n{1}", fileName, ex.Message), "Error Opening Mashup", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            if (mashupFileWarningList != null)
            {
                DialogResult dialogResult = MessageBox.Show(string.Format("Warnings for {0}:\n{1}\nContinue loading file?\n", fileName, mashupFileWarningList), "Error Reading File", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand);
                if (dialogResult == DialogResult.Cancel)
                {
                    return;
                }
            }
            this.OpenMashup(mashup);
        }
        private void openMashupMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = string.Format("MapCruncher Mashup Files (*.{0})|*.{0};*.msh" + BuildConfig.theConfig.allFilesOption, "yum");
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            if (!this.CloseMashup())
            {
                return;
            }
            this.LoadMashup(openFileDialog.FileName);
        }
        private void NewMashup()
        {
            if (!this.CloseMashup())
            {
                return;
            }
            this.OpenMashup(new Mashup());
        }
        private void newMashupMenuItem_Click(object sender, EventArgs e)
        {
            this.NewMashup();
        }
        private void closeMashupMenuItem_Click(object sender, EventArgs e)
        {
            this.CloseMashup();
        }
        public void CloseView()
        {
            if (this.currentView != null)
            {
                this.currentView.Dispose();
                this.currentView = null;
                if (this.currentMashup != null)
                {
                    this.currentMashup.SetLastView(new NoView());
                }
            }
        }
        public void OpenView(IViewManager newView)
        {
            try
            {
                MainAppForm.Opening obj;
                Monitor.Enter(obj = this.opening);
                try
                {
                    if (this.opening.opening)
                    {
                        D.Sayf(0, "Warning: recursive open", new object[0]);
                        return;
                    }
                    this.opening.opening = true;
                }
                finally
                {
                    Monitor.Exit(obj);
                }
                this.CloseView();
                this.ResetOverviewWindow();
                D.Assert(this.currentView == null);
                this.currentView = newView;
                this.currentView.Activate();
                this.layerControls.SelectObject(newView.GetViewedObject());
            }
            finally
            {
                MainAppForm.Opening obj2;
                Monitor.Enter(obj2 = this.opening);
                try
                {
                    this.opening.opening = false;
                }
                finally
                {
                    Monitor.Exit(obj2);
                }
            }
        }
        [DllImport("user32")]
        private static extern bool SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        public void OpenSourceMap(SourceMap sourceMap)
        {
            this.FreezePainting = true;
            DefaultReferenceView drv;
            if (this.currentView != null)
            {
                drv = new DefaultReferenceView(this.uiPosition.GetVEPos().llz);
            }
            else
            {
                drv = new DefaultReferenceView();
            }
            SourceMapViewManager sourceMapViewManager = new SourceMapViewManager(sourceMap, this.mapTileSourceFactory, this, drv);
            this.OpenView(sourceMapViewManager);
            this.FreezePainting = false;
            this.currentMashup.SetLastView(sourceMap.lastView);
            this.SetupOverviewWindow(sourceMapViewManager);
        }
        public void OpenLayer(Layer layer)
        {
            this.OpenView(new DynamicallyCompositingLayerViewManager(layer, this.mapTileSourceFactory, this));
            this.currentMashup.SetLastView(layer.lastView);
        }
        public void OpenLegend(Legend legend)
        {
            this.OpenView(new LegendViewManager(legend, this.mapTileSourceFactory, this));
            this.currentMashup.SetLastView(legend.lastView);
        }
        private void addSourceMapMenuItem_Click(object sender, EventArgs e)
        {
            this.AddSourceMap();
        }
        private void addSourceMapFromUriMenuItem_Click(object sender, EventArgs e)
        {
            this.AddSourceMapFromUri();
        }
        public void AddSourceMap()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string arg = string.Join(";", Array.ConvertAll<string, string>(this.mapTileSourceFactory.GetKnownFileTypes(), (string ext) => "*" + ext));
            string filter = string.Format("Supported Sources ({0})|{0}" + BuildConfig.theConfig.allFilesOption, arg);
            openFileDialog.Filter = filter;
            openFileDialog.FilterIndex = 1;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            if (openFileDialog.FileName == null)
            {
                return;
            }
            MainAppForm.UndoAddSourceMap undoAddSourceMap = new MainAppForm.UndoAddSourceMap(openFileDialog.FileName, null, null, null, this);
            try
            {
                FileStream fileStream = File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                fileStream.Close();
                SourceMap sourceMap = new SourceMap(new FutureDocumentFromFilesystem(openFileDialog.FileName, 0), new SourceMap.GetFilenameContext(this.currentMashup.GetFilenameContext), this.currentMashup.dirtyEvent, this.currentMashup.readyToLockEvent);
                Layer addedToLayer = this.layerControls.AddSourceMap(sourceMap);
                undoAddSourceMap = new MainAppForm.UndoAddSourceMap(openFileDialog.FileName, sourceMap, addedToLayer, this.layerControls, this);
                new InsaneSourceMapRemover(sourceMap, this.mapTileSourceFactory, new InsaneSourceMapRemover.UndoAdddSourceMapDelegate(undoAddSourceMap.Undo));
                this.OpenSourceMap(sourceMap);
            }
            catch (Exception ex)
            {
                undoAddSourceMap.Undo(ex.Message);
            }
        }
        public void AddSourceMapFromUri()
        {
            SourceMap sourceMap = new SourceMap(new FutureDocumentFromUri(new Uri("http://www.srh.noaa.gov/ridge/lite/NCR/ATX_0.png"), 0), new SourceMap.GetFilenameContext(this.currentMashup.GetFilenameContext), this.currentMashup.dirtyEvent, this.currentMashup.readyToLockEvent);
            this.layerControls.AddSourceMap(sourceMap);
            this.OpenSourceMap(sourceMap);
        }
        public void RemoveSourceMap(SourceMap sourceMap)
        {
            this.currentMashup.layerList.RemoveSourceMap(sourceMap);
        }
        public void LaunchRenderedBrowser(Uri uri)
        {
            RenderedMashupViewer renderedMashupViewer = new RenderedMashupViewer(this.renderedTileCachePackage, this.showDMSMenuItem);
            renderedMashupViewer.AddLayersFromUri(uri);
            renderedMashupViewer.Show();
        }
        public UIPositionManager GetUIPositionManager()
        {
            return this.uiPosition;
        }
        public ViewerControlIfc GetSMViewerControl()
        {
            return this.smViewerControl;
        }
        public ViewerControl GetVEViewerControl()
        {
            return this.veViewerControl;
        }
        public SourceMapInfoPanel GetSourceMapInfoPanel()
        {
            return this.sourceMapInfoPanel;
        }
        public TransparencyPanel GetTransparencyPanel()
        {
            return this.transparencyPanel;
        }
        public LegendOptionsPanel GetLegendPanel()
        {
            return this.legendOptionsPanel1;
        }
        public void SetOptionsPanelVisibility(OptionsPanelVisibility optionsPanelVisibility)
        {
            if (optionsPanelVisibility == OptionsPanelVisibility.Nothing)
            {
                this.synergyExplorer.Visible = false;
                return;
            }
            if (optionsPanelVisibility == OptionsPanelVisibility.SourceMapOptions)
            {
                this.synergyExplorer.TabPages.Clear();
                this.synergyExplorer.TabPages.Add(this.correspondencesTab);
                this.synergyExplorer.TabPages.Add(this.sourceInfoTab);
                this.synergyExplorer.TabPages.Add(this.transparencyTab);
                this.synergyExplorer.Visible = true;
                return;
            }
            if (optionsPanelVisibility == OptionsPanelVisibility.LegendOptions)
            {
                this.synergyExplorer.TabPages.Clear();
                this.synergyExplorer.TabPages.Add(this.legendTabPage);
                this.synergyExplorer.Visible = true;
            }
        }
        public CachePackage GetCachePackage()
        {
            return this.cachePackage;
        }
        public void LockMaps()
        {
            ((SourceMapViewManager)this.currentView).LockMaps();
            this.smViewerControl.SetLLZBoxLabelStyle(LLZBox.LabelStyle.LatLon);
        }
        public void UnlockMaps()
        {
            ((SourceMapViewManager)this.currentView).UnlockMaps();
            this.smViewerControl.SetLLZBoxLabelStyle(LLZBox.LabelStyle.XY);
        }
        public void SetDocumentMutable(bool mutable)
        {
            this.documentIsMutable = mutable;
            this.synergyExplorer.Enabled = mutable;
        }
        public bool GetDocumentMutable()
        {
            return this.documentIsMutable;
        }
        private void PreviewSourceMapZoom(SourceMap sourceMap)
        {
            if (this.currentView is SourceMapViewManager && ((SourceMapViewManager)this.currentView).GetSourceMap() == sourceMap)
            {
                ((SourceMapViewManager)this.currentView).PreviewSourceMapZoom();
            }
        }
        private void LaunchRenderWindow()
        {
            if (this.renderWindow == null)
            {
                this.renderWindow = new RenderWindow();
                this.renderWindow.Disposed += new EventHandler(this.renderWindow_Disposed);
            }
            this.renderWindow.Setup(this.currentMashup.GetRenderOptions(), this.currentMashup, this.mapTileSourceFactory, new RenderProgressPanel2.LaunchRenderedBrowserDelegate(this.LaunchRenderedBrowser), new RenderState.FlushRenderedTileCachePackageDelegate(this.flushRenderedTileCachePackage));
            this.renderWindow.Visible = true;
        }
        private void flushRenderedTileCachePackage()
        {
            this.renderedTileCachePackage.Flush();
        }
        private void renderWindow_Disposed(object sender, EventArgs e)
        {
            this.renderWindow = null;
        }
        private void KillRenderWindow()
        {
            if (this.renderWindow != null)
            {
                this.renderWindow.UndoConstruction();
            }
        }
        private void RenderLaunchButton_Click(object sender, EventArgs e)
        {
            if (this.renderWindow == null)
            {
                this.LaunchRenderWindow();
            }
            this.renderWindow.BringToFront();
        }
        private void showSourceMapOverviewMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
            bool @checked = ((ToolStripMenuItem)sender).Checked;
            if (@checked && this.sourceMapOverviewWindow == null)
            {
                this.sourceMapOverviewWindow = new SourceMapOverviewWindow();
                this.sourceMapOverviewWindow.Initialize(new SourceMapOverviewWindow.ClosedDelegate(this.SourceMapOverviewWindowClosed), new MapDrawingOption(this.veViewerControl, this.showDMSMenuItem, false));
                this.sourceMapOverviewWindow.viewerControl.ShowPushPins = new MapDrawingOption(this.sourceMapOverviewWindow.viewerControl, this.showPushPinsMenuItem, true);
                this.sourceMapOverviewWindow.viewerControl.ShowSourceCrop = new MapDrawingOption(this.sourceMapOverviewWindow.viewerControl, this.showSourceCropToolStripMenuItem, true);
                this.sourceMapOverviewWindow.viewerControl.ShowDMS = new MapDrawingOption(this.sourceMapOverviewWindow.viewerControl, this.showDMSMenuItem, false);
                this.sourceMapOverviewWindow.Show();
                if (this.currentView is SourceMapViewManager)
                {
                    this.SetupOverviewWindow((SourceMapViewManager)this.currentView);
                    return;
                }
            }
            else
            {
                if (!@checked && this.sourceMapOverviewWindow != null)
                {
                    this.sourceMapOverviewWindow.Close();
                    this.sourceMapOverviewWindow = null;
                }
            }
        }
        private void UpdateOverviewPins()
        {
            if (this.sourceMapOverviewWindow != null)
            {
                List<PositionAssociationView> pinList;
                if (this.displayedRegistration != null)
                {
                    Converter<PositionAssociation, PositionAssociationView> converter = (PositionAssociation pa) => new PositionAssociationView(pa, PositionAssociationView.WhichPosition.image);
                    pinList = new RegistrationDefinition(this.displayedRegistration.model, new DirtyEvent())
                    {
                        isLocked = false
                    }.GetAssociationList().ConvertAll<PositionAssociationView>(converter);
                }
                else
                {
                    pinList = new List<PositionAssociationView>();
                }
                this.sourceMapOverviewWindow.viewerControl.setPinList(pinList);
            }
        }
        private void ResetOverviewWindow()
        {
            if (this.sourceMapOverviewWindow != null)
            {
                this.sourceMapOverviewWindow.viewerControl.ClearLayers();
                this.sourceMapOverviewWindow.viewerControl.setPinList(new List<PositionAssociationView>());
            }
        }
        private void SetupOverviewWindow(SourceMapViewManager smvm)
        {
            if (this.sourceMapOverviewWindow != null)
            {
                smvm.UpdateOverviewWindow(this.sourceMapOverviewWindow.viewerControl);
            }
            this.updateRegistrationDisplay();
        }
        public void SourceMapOverviewWindowClosed()
        {
            this.sourceMapOverviewWindow = null;
            this.showSourceMapOverviewMenuItem.Checked = false;
        }
        private void recordSnapViewMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = false;
            this.smViewerControl.RecordSnapView();
        }
        private void restoreSnapViewMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = false;
            this.smViewerControl.RestoreSnapView();
        }
        private void recordSnapZoomMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = false;
            this.smViewerControl.RecordSnapZoom();
        }
        private void restoreSnapZoomMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = false;
            this.smViewerControl.RestoreSnapZoom();
        }
        private void showDiagnosticsUIToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            DiagnosticUI.theDiagnostics.Visible = true;
        }
        private void enableDebugModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BigDebugKnob.theKnob.debugFeaturesEnabled = !BigDebugKnob.theKnob.debugFeaturesEnabled;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newMashupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMashupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMashupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMashupAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeMashupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.addSourceMapMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSourceMapFromUriMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.viewRenderedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapOptionsToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.VEroadView = new System.Windows.Forms.ToolStripMenuItem();
            this.VEaerialView = new System.Windows.Forms.ToolStripMenuItem();
            this.VEhybridView = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.showCrosshairsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showPushPinsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDMSMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.AddRegLayerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSourceMapOverviewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.snapFeaturesToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.restoreSnapViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recordSnapViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreSnapZoomMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recordSnapZoomMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugModeToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.enableDebugModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMapCruncherTutorialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutMSRBackMakerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTileNamesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSourceCropToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTileBoundariesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDiagnosticsUIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapSplitContainer = new System.Windows.Forms.SplitContainer();
            this.smViewerControl = new MSR.CVE.BackMaker.ViewerControl();
            this.veViewerControl = new MSR.CVE.BackMaker.ViewerControl();
            this.controlSplitContainer = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.RenderLaunchButton = new System.Windows.Forms.Button();
            this.controlsSplitContainer = new System.Windows.Forms.SplitContainer();
            this.layerControls = new MSR.CVE.BackMaker.LayerControls();
            this.synergyExplorer = new System.Windows.Forms.TabControl();
            this.correspondencesTab = new System.Windows.Forms.TabPage();
            this.registrationControls = new MSR.CVE.BackMaker.registrationControls();
            this.transparencyTab = new System.Windows.Forms.TabPage();
            this.transparencyPanel = new MSR.CVE.BackMaker.TransparencyPanel();
            this.sourceInfoTab = new System.Windows.Forms.TabPage();
            this.sourceMapInfoPanel = new MSR.CVE.BackMaker.SourceMapInfoPanel();
            this.legendTabPage = new System.Windows.Forms.TabPage();
            this.legendOptionsPanel1 = new MSR.CVE.BackMaker.LegendOptionsPanel();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mapSplitContainer)).BeginInit();
            this.mapSplitContainer.Panel1.SuspendLayout();
            this.mapSplitContainer.Panel2.SuspendLayout();
            this.mapSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlSplitContainer)).BeginInit();
            this.controlSplitContainer.Panel1.SuspendLayout();
            this.controlSplitContainer.Panel2.SuspendLayout();
            this.controlSplitContainer.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlsSplitContainer)).BeginInit();
            this.controlsSplitContainer.Panel1.SuspendLayout();
            this.controlsSplitContainer.Panel2.SuspendLayout();
            this.controlsSplitContainer.SuspendLayout();
            this.synergyExplorer.SuspendLayout();
            this.correspondencesTab.SuspendLayout();
            this.transparencyTab.SuspendLayout();
            this.sourceInfoTab.SuspendLayout();
            this.legendTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.mapOptionsToolStripMenuItem2,
            this.helpToolStripMenuItem,
            this.debugToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1028, 36);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newMashupMenuItem,
            this.openMashupMenuItem,
            this.saveMashupMenuItem,
            this.saveMashupAsMenuItem,
            this.closeMashupMenuItem,
            this.toolStripSeparator1,
            this.addSourceMapMenuItem,
            this.addSourceMapFromUriMenuItem,
            this.toolStripSeparator4,
            this.viewRenderedMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 32);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newMashupMenuItem
            // 
            this.newMashupMenuItem.Name = "newMashupMenuItem";
            this.newMashupMenuItem.Size = new System.Drawing.Size(306, 32);
            this.newMashupMenuItem.Text = "&New Mashup";
            this.newMashupMenuItem.Click += new System.EventHandler(this.newMashupMenuItem_Click);
            // 
            // openMashupMenuItem
            // 
            this.openMashupMenuItem.Name = "openMashupMenuItem";
            this.openMashupMenuItem.Size = new System.Drawing.Size(306, 32);
            this.openMashupMenuItem.Text = "&Open Mashup...";
            this.openMashupMenuItem.Click += new System.EventHandler(this.openMashupMenuItem_Click);
            // 
            // saveMashupMenuItem
            // 
            this.saveMashupMenuItem.Enabled = false;
            this.saveMashupMenuItem.Name = "saveMashupMenuItem";
            this.saveMashupMenuItem.Size = new System.Drawing.Size(306, 32);
            this.saveMashupMenuItem.Text = "&Save Mashup";
            this.saveMashupMenuItem.Click += new System.EventHandler(this.saveMashupMenuItem_Click);
            // 
            // saveMashupAsMenuItem
            // 
            this.saveMashupAsMenuItem.Enabled = false;
            this.saveMashupAsMenuItem.Name = "saveMashupAsMenuItem";
            this.saveMashupAsMenuItem.Size = new System.Drawing.Size(306, 32);
            this.saveMashupAsMenuItem.Text = "Save Mashup &As...";
            this.saveMashupAsMenuItem.Click += new System.EventHandler(this.saveMashupAsMenuItem_Click);
            // 
            // closeMashupMenuItem
            // 
            this.closeMashupMenuItem.Enabled = false;
            this.closeMashupMenuItem.Name = "closeMashupMenuItem";
            this.closeMashupMenuItem.Size = new System.Drawing.Size(306, 32);
            this.closeMashupMenuItem.Text = "&Close Mashup";
            this.closeMashupMenuItem.Click += new System.EventHandler(this.closeMashupMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(303, 6);
            // 
            // addSourceMapMenuItem
            // 
            this.addSourceMapMenuItem.Enabled = false;
            this.addSourceMapMenuItem.Name = "addSourceMapMenuItem";
            this.addSourceMapMenuItem.Size = new System.Drawing.Size(306, 32);
            this.addSourceMapMenuItem.Text = "Add Source &Map...";
            this.addSourceMapMenuItem.Click += new System.EventHandler(this.addSourceMapMenuItem_Click);
            // 
            // addSourceMapFromUriMenuItem
            // 
            this.addSourceMapFromUriMenuItem.Enabled = false;
            this.addSourceMapFromUriMenuItem.Name = "addSourceMapFromUriMenuItem";
            this.addSourceMapFromUriMenuItem.Size = new System.Drawing.Size(306, 32);
            this.addSourceMapFromUriMenuItem.Text = "Add Map From &Uri...";
            this.addSourceMapFromUriMenuItem.Visible = false;
            this.addSourceMapFromUriMenuItem.Click += new System.EventHandler(this.addSourceMapFromUriMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(303, 6);
            // 
            // viewRenderedMenuItem
            // 
            this.viewRenderedMenuItem.Name = "viewRenderedMenuItem";
            this.viewRenderedMenuItem.Size = new System.Drawing.Size(306, 32);
            this.viewRenderedMenuItem.Text = "Launch Mashup &Browser...";
            this.viewRenderedMenuItem.Click += new System.EventHandler(this.viewRenderedMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(303, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(306, 32);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // mapOptionsToolStripMenuItem2
            // 
            this.mapOptionsToolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.VEroadView,
            this.VEaerialView,
            this.VEhybridView,
            this.toolStripSeparator3,
            this.showCrosshairsMenuItem,
            this.showPushPinsMenuItem,
            this.showDMSMenuItem,
            this.toolStripSeparator8,
            this.AddRegLayerMenuItem,
            this.showSourceMapOverviewMenuItem,
            this.snapFeaturesToolStripSeparator,
            this.restoreSnapViewMenuItem,
            this.recordSnapViewMenuItem,
            this.restoreSnapZoomMenuItem,
            this.recordSnapZoomMenuItem,
            this.debugModeToolStripSeparator,
            this.enableDebugModeToolStripMenuItem});
            this.mapOptionsToolStripMenuItem2.Name = "mapOptionsToolStripMenuItem2";
            this.mapOptionsToolStripMenuItem2.Size = new System.Drawing.Size(65, 32);
            this.mapOptionsToolStripMenuItem2.Text = "&View";
            // 
            // VEroadView
            // 
            this.VEroadView.Name = "VEroadView";
            this.VEroadView.Size = new System.Drawing.Size(334, 32);
            this.VEroadView.Text = "&Roads";
            this.VEroadView.Click += new System.EventHandler(this.roadToolStripMenuItem_Click);
            // 
            // VEaerialView
            // 
            this.VEaerialView.Name = "VEaerialView";
            this.VEaerialView.Size = new System.Drawing.Size(334, 32);
            this.VEaerialView.Text = "&Aerial Photos";
            this.VEaerialView.Click += new System.EventHandler(this.aerialToolStripMenuItem_Click);
            // 
            // VEhybridView
            // 
            this.VEhybridView.Name = "VEhybridView";
            this.VEhybridView.Size = new System.Drawing.Size(334, 32);
            this.VEhybridView.Text = "&Hybrid";
            this.VEhybridView.Click += new System.EventHandler(this.hybridToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(331, 6);
            // 
            // showCrosshairsMenuItem
            // 
            this.showCrosshairsMenuItem.Name = "showCrosshairsMenuItem";
            this.showCrosshairsMenuItem.Size = new System.Drawing.Size(334, 32);
            this.showCrosshairsMenuItem.Text = "Show &Crosshairs";
            // 
            // showPushPinsMenuItem
            // 
            this.showPushPinsMenuItem.Name = "showPushPinsMenuItem";
            this.showPushPinsMenuItem.Size = new System.Drawing.Size(334, 32);
            this.showPushPinsMenuItem.Text = "Show &PushPins";
            // 
            // showDMSMenuItem
            // 
            this.showDMSMenuItem.Name = "showDMSMenuItem";
            this.showDMSMenuItem.Size = new System.Drawing.Size(334, 32);
            this.showDMSMenuItem.Text = "Show locations in d°m\'s\"";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(331, 6);
            // 
            // AddRegLayerMenuItem
            // 
            this.AddRegLayerMenuItem.Name = "AddRegLayerMenuItem";
            this.AddRegLayerMenuItem.Size = new System.Drawing.Size(334, 32);
            this.AddRegLayerMenuItem.Text = "Show Rendered &Layer...";
            this.AddRegLayerMenuItem.Click += new System.EventHandler(this.AddRegLayerMenuItem_Click);
            // 
            // showSourceMapOverviewMenuItem
            // 
            this.showSourceMapOverviewMenuItem.Name = "showSourceMapOverviewMenuItem";
            this.showSourceMapOverviewMenuItem.Size = new System.Drawing.Size(334, 32);
            this.showSourceMapOverviewMenuItem.Text = "Show Source Map Overview";
            this.showSourceMapOverviewMenuItem.Click += new System.EventHandler(this.showSourceMapOverviewMenuItem_Click);
            // 
            // snapFeaturesToolStripSeparator
            // 
            this.snapFeaturesToolStripSeparator.Name = "snapFeaturesToolStripSeparator";
            this.snapFeaturesToolStripSeparator.Size = new System.Drawing.Size(331, 6);
            // 
            // restoreSnapViewMenuItem
            // 
            this.restoreSnapViewMenuItem.Name = "restoreSnapViewMenuItem";
            this.restoreSnapViewMenuItem.ShortcutKeyDisplayString = "F5";
            this.restoreSnapViewMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.restoreSnapViewMenuItem.Size = new System.Drawing.Size(334, 32);
            this.restoreSnapViewMenuItem.Text = "Restore SnapView";
            // 
            // recordSnapViewMenuItem
            // 
            this.recordSnapViewMenuItem.Name = "recordSnapViewMenuItem";
            this.recordSnapViewMenuItem.ShortcutKeyDisplayString = "Shift+F5";
            this.recordSnapViewMenuItem.Size = new System.Drawing.Size(334, 32);
            this.recordSnapViewMenuItem.Text = "Record SnapView";
            // 
            // restoreSnapZoomMenuItem
            // 
            this.restoreSnapZoomMenuItem.Name = "restoreSnapZoomMenuItem";
            this.restoreSnapZoomMenuItem.ShortcutKeyDisplayString = "F6";
            this.restoreSnapZoomMenuItem.Size = new System.Drawing.Size(334, 32);
            this.restoreSnapZoomMenuItem.Text = "Restore SnapZoom";
            // 
            // recordSnapZoomMenuItem
            // 
            this.recordSnapZoomMenuItem.Name = "recordSnapZoomMenuItem";
            this.recordSnapZoomMenuItem.ShortcutKeyDisplayString = "Shift+F6";
            this.recordSnapZoomMenuItem.Size = new System.Drawing.Size(334, 32);
            this.recordSnapZoomMenuItem.Text = "Record SnapZoom";
            // 
            // debugModeToolStripSeparator
            // 
            this.debugModeToolStripSeparator.Name = "debugModeToolStripSeparator";
            this.debugModeToolStripSeparator.Size = new System.Drawing.Size(331, 6);
            // 
            // enableDebugModeToolStripMenuItem
            // 
            this.enableDebugModeToolStripMenuItem.Name = "enableDebugModeToolStripMenuItem";
            this.enableDebugModeToolStripMenuItem.Size = new System.Drawing.Size(334, 32);
            this.enableDebugModeToolStripMenuItem.Text = "Enable Debug Mode";
            this.enableDebugModeToolStripMenuItem.Click += new System.EventHandler(this.enableDebugModeToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewMapCruncherTutorialToolStripMenuItem,
            this.toolStripSeparator7,
            this.aboutMSRBackMakerToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(65, 32);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // viewMapCruncherTutorialToolStripMenuItem
            // 
            this.viewMapCruncherTutorialToolStripMenuItem.Name = "viewMapCruncherTutorialToolStripMenuItem";
            this.viewMapCruncherTutorialToolStripMenuItem.Size = new System.Drawing.Size(536, 32);
            this.viewMapCruncherTutorialToolStripMenuItem.Text = "MapCruncher for Microsoft Virtual Earth Help";
            this.viewMapCruncherTutorialToolStripMenuItem.Click += new System.EventHandler(this.viewMapCruncherTutorialToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(533, 6);
            // 
            // aboutMSRBackMakerToolStripMenuItem
            // 
            this.aboutMSRBackMakerToolStripMenuItem.Name = "aboutMSRBackMakerToolStripMenuItem";
            this.aboutMSRBackMakerToolStripMenuItem.Size = new System.Drawing.Size(536, 32);
            this.aboutMSRBackMakerToolStripMenuItem.Text = "&About MapCruncher Beta for Microsoft Virtual Earth";
            this.aboutMSRBackMakerToolStripMenuItem.Click += new System.EventHandler(this.aboutMSRBackMakerToolStripMenuItem_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showTileNamesMenuItem,
            this.showSourceCropToolStripMenuItem,
            this.showTileBoundariesMenuItem,
            this.showDiagnosticsUIToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(83, 32);
            this.debugToolStripMenuItem.Text = "&Debug";
            // 
            // showTileNamesMenuItem
            // 
            this.showTileNamesMenuItem.Name = "showTileNamesMenuItem";
            this.showTileNamesMenuItem.Size = new System.Drawing.Size(269, 32);
            this.showTileNamesMenuItem.Text = "Show Tile &Names";
            // 
            // showSourceCropToolStripMenuItem
            // 
            this.showSourceCropToolStripMenuItem.Name = "showSourceCropToolStripMenuItem";
            this.showSourceCropToolStripMenuItem.Size = new System.Drawing.Size(269, 32);
            this.showSourceCropToolStripMenuItem.Text = "Show Source Crop";
            // 
            // showTileBoundariesMenuItem
            // 
            this.showTileBoundariesMenuItem.Name = "showTileBoundariesMenuItem";
            this.showTileBoundariesMenuItem.Size = new System.Drawing.Size(269, 32);
            this.showTileBoundariesMenuItem.Text = "Show Tile &Boundaries";
            // 
            // showDiagnosticsUIToolStripMenuItem
            // 
            this.showDiagnosticsUIToolStripMenuItem.Name = "showDiagnosticsUIToolStripMenuItem";
            this.showDiagnosticsUIToolStripMenuItem.Size = new System.Drawing.Size(269, 32);
            this.showDiagnosticsUIToolStripMenuItem.Text = "Show DiagnosticsUI";
            this.showDiagnosticsUIToolStripMenuItem.Click += new System.EventHandler(this.showDiagnosticsUIToolStripMenuItem_Click_1);
            // 
            // mapSplitContainer
            // 
            this.mapSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mapSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.mapSplitContainer.Name = "mapSplitContainer";
            // 
            // mapSplitContainer.Panel1
            // 
            this.mapSplitContainer.Panel1.Controls.Add(this.smViewerControl);
            this.mapSplitContainer.Panel1MinSize = 100;
            // 
            // mapSplitContainer.Panel2
            // 
            this.mapSplitContainer.Panel2.Controls.Add(this.veViewerControl);
            this.mapSplitContainer.Panel2MinSize = 100;
            this.mapSplitContainer.Size = new System.Drawing.Size(691, 647);
            this.mapSplitContainer.SplitterDistance = 337;
            this.mapSplitContainer.TabIndex = 6;
            // 
            // smViewerControl
            // 
            this.smViewerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.smViewerControl.Location = new System.Drawing.Point(0, 0);
            this.smViewerControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.smViewerControl.Name = "smViewerControl";
            this.smViewerControl.Size = new System.Drawing.Size(337, 647);
            this.smViewerControl.TabIndex = 0;
            // 
            // veViewerControl
            // 
            this.veViewerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.veViewerControl.Location = new System.Drawing.Point(0, 0);
            this.veViewerControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.veViewerControl.Name = "veViewerControl";
            this.veViewerControl.Size = new System.Drawing.Size(350, 647);
            this.veViewerControl.TabIndex = 0;
            // 
            // controlSplitContainer
            // 
            this.controlSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlSplitContainer.Location = new System.Drawing.Point(0, 36);
            this.controlSplitContainer.Name = "controlSplitContainer";
            // 
            // controlSplitContainer.Panel1
            // 
            this.controlSplitContainer.Panel1.Controls.Add(this.panel1);
            this.controlSplitContainer.Panel1.Controls.Add(this.controlsSplitContainer);
            // 
            // controlSplitContainer.Panel2
            // 
            this.controlSplitContainer.Panel2.Controls.Add(this.mapSplitContainer);
            this.controlSplitContainer.Size = new System.Drawing.Size(1028, 650);
            this.controlSplitContainer.SplitterDistance = 330;
            this.controlSplitContainer.TabIndex = 7;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.RenderLaunchButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 608);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(330, 42);
            this.panel1.TabIndex = 9;
            // 
            // RenderLaunchButton
            // 
            this.RenderLaunchButton.Location = new System.Drawing.Point(3, 4);
            this.RenderLaunchButton.Name = "RenderLaunchButton";
            this.RenderLaunchButton.Size = new System.Drawing.Size(125, 30);
            this.RenderLaunchButton.TabIndex = 9;
            this.RenderLaunchButton.Text = "Render...";
            this.RenderLaunchButton.UseVisualStyleBackColor = true;
            this.RenderLaunchButton.Click += new System.EventHandler(this.RenderLaunchButton_Click);
            // 
            // controlsSplitContainer
            // 
            this.controlsSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.controlsSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.controlsSplitContainer.Name = "controlsSplitContainer";
            this.controlsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // controlsSplitContainer.Panel1
            // 
            this.controlsSplitContainer.Panel1.Controls.Add(this.layerControls);
            // 
            // controlsSplitContainer.Panel2
            // 
            this.controlsSplitContainer.Panel2.Controls.Add(this.synergyExplorer);
            this.controlsSplitContainer.Size = new System.Drawing.Size(327, 606);
            this.controlsSplitContainer.SplitterDistance = 130;
            this.controlsSplitContainer.TabIndex = 8;
            // 
            // layerControls
            // 
            this.layerControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerControls.Location = new System.Drawing.Point(0, 0);
            this.layerControls.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.layerControls.Name = "layerControls";
            this.layerControls.Size = new System.Drawing.Size(327, 130);
            this.layerControls.TabIndex = 10;
            // 
            // synergyExplorer
            // 
            this.synergyExplorer.Controls.Add(this.correspondencesTab);
            this.synergyExplorer.Controls.Add(this.transparencyTab);
            this.synergyExplorer.Controls.Add(this.sourceInfoTab);
            this.synergyExplorer.Controls.Add(this.legendTabPage);
            this.synergyExplorer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.synergyExplorer.Location = new System.Drawing.Point(0, 0);
            this.synergyExplorer.Multiline = true;
            this.synergyExplorer.Name = "synergyExplorer";
            this.synergyExplorer.SelectedIndex = 0;
            this.synergyExplorer.ShowToolTips = true;
            this.synergyExplorer.Size = new System.Drawing.Size(327, 472);
            this.synergyExplorer.TabIndex = 7;
            // 
            // correspondencesTab
            // 
            this.correspondencesTab.Controls.Add(this.registrationControls);
            this.correspondencesTab.Location = new System.Drawing.Point(4, 46);
            this.correspondencesTab.Name = "correspondencesTab";
            this.correspondencesTab.Padding = new System.Windows.Forms.Padding(3);
            this.correspondencesTab.Size = new System.Drawing.Size(319, 422);
            this.correspondencesTab.TabIndex = 1;
            this.correspondencesTab.Text = "Correspondences";
            this.correspondencesTab.UseVisualStyleBackColor = true;
            // 
            // registrationControls
            // 
            this.registrationControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.registrationControls.Location = new System.Drawing.Point(3, 3);
            this.registrationControls.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.registrationControls.Name = "registrationControls";
            this.registrationControls.Size = new System.Drawing.Size(313, 416);
            this.registrationControls.TabIndex = 9;
            // 
            // transparencyTab
            // 
            this.transparencyTab.Controls.Add(this.transparencyPanel);
            this.transparencyTab.Location = new System.Drawing.Point(4, 46);
            this.transparencyTab.Name = "transparencyTab";
            this.transparencyTab.Size = new System.Drawing.Size(319, 412);
            this.transparencyTab.TabIndex = 4;
            this.transparencyTab.Text = "Transparency";
            this.transparencyTab.UseVisualStyleBackColor = true;
            // 
            // transparencyPanel
            // 
            this.transparencyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.transparencyPanel.Location = new System.Drawing.Point(0, 0);
            this.transparencyPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.transparencyPanel.Name = "transparencyPanel";
            this.transparencyPanel.Size = new System.Drawing.Size(319, 412);
            this.transparencyPanel.TabIndex = 0;
            // 
            // sourceInfoTab
            // 
            this.sourceInfoTab.Controls.Add(this.sourceMapInfoPanel);
            this.sourceInfoTab.Location = new System.Drawing.Point(4, 46);
            this.sourceInfoTab.Name = "sourceInfoTab";
            this.sourceInfoTab.Padding = new System.Windows.Forms.Padding(3);
            this.sourceInfoTab.Size = new System.Drawing.Size(319, 412);
            this.sourceInfoTab.TabIndex = 3;
            this.sourceInfoTab.Text = "Source Info";
            this.sourceInfoTab.UseVisualStyleBackColor = true;
            // 
            // sourceMapInfoPanel
            // 
            this.sourceMapInfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceMapInfoPanel.Location = new System.Drawing.Point(3, 3);
            this.sourceMapInfoPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sourceMapInfoPanel.Name = "sourceMapInfoPanel";
            this.sourceMapInfoPanel.Size = new System.Drawing.Size(313, 406);
            this.sourceMapInfoPanel.TabIndex = 0;
            // 
            // legendTabPage
            // 
            this.legendTabPage.Controls.Add(this.legendOptionsPanel1);
            this.legendTabPage.Location = new System.Drawing.Point(4, 46);
            this.legendTabPage.Name = "legendTabPage";
            this.legendTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.legendTabPage.Size = new System.Drawing.Size(319, 412);
            this.legendTabPage.TabIndex = 5;
            this.legendTabPage.Text = "Legend Options";
            this.legendTabPage.UseVisualStyleBackColor = true;
            // 
            // legendOptionsPanel1
            // 
            this.legendOptionsPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.legendOptionsPanel1.Location = new System.Drawing.Point(3, 3);
            this.legendOptionsPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.legendOptionsPanel1.Name = "legendOptionsPanel1";
            this.legendOptionsPanel1.Size = new System.Drawing.Size(313, 406);
            this.legendOptionsPanel1.TabIndex = 10;
            // 
            // MainAppForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(1028, 686);
            this.Controls.Add(this.controlSplitContainer);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainAppForm";
            this.Text = "MapCruncher Beta for Microsoft Virtual Earth";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.mapSplitContainer.Panel1.ResumeLayout(false);
            this.mapSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mapSplitContainer)).EndInit();
            this.mapSplitContainer.ResumeLayout(false);
            this.controlSplitContainer.Panel1.ResumeLayout(false);
            this.controlSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.controlSplitContainer)).EndInit();
            this.controlSplitContainer.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.controlsSplitContainer.Panel1.ResumeLayout(false);
            this.controlsSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.controlsSplitContainer)).EndInit();
            this.controlsSplitContainer.ResumeLayout(false);
            this.synergyExplorer.ResumeLayout(false);
            this.correspondencesTab.ResumeLayout(false);
            this.transparencyTab.ResumeLayout(false);
            this.sourceInfoTab.ResumeLayout(false);
            this.legendTabPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
