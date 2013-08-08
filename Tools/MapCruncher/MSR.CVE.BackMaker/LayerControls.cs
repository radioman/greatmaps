using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class LayerControls : UserControl
	{
		private delegate void ReloadDelegate();
		private IContainer components;
		private GroupBox groupBox1;
		private TreeView _layerTreeView;
		private Label getStartedLabel1;
		private Label getStartedLabel2;
		private GroupBox gettingStartedLabel;
		private Mashup _mashup;
		private LayerControlIfc layerControl;
		private MenuItem renameMenuItem;
		private MenuItem addLayerItem;
		private MenuItem removeItem;
		private MenuItem addSourceMapItem;
		private MenuItem addLegendItem;
		private TreeNode clickedNode;
		private Dictionary<object, TreeNode> tagToTreeNodeDict = new Dictionary<object, TreeNode>();
		private static string dataTypeName = typeof(NameWatchingTreeNode).Namespace + "." + typeof(NameWatchingTreeNode).Name;
		private TreeNode currentDropHighlight;
		public TreeView layerTreeView
		{
			get
			{
				return this._layerTreeView;
			}
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
			this.groupBox1 = new GroupBox();
			this.gettingStartedLabel = new GroupBox();
			this.getStartedLabel1 = new Label();
			this.getStartedLabel2 = new Label();
			this._layerTreeView = new TreeView();
			this.groupBox1.SuspendLayout();
			this.gettingStartedLabel.SuspendLayout();
			base.SuspendLayout();
			this.groupBox1.Controls.Add(this.gettingStartedLabel);
			this.groupBox1.Controls.Add(this._layerTreeView);
			this.groupBox1.Dock = DockStyle.Fill;
			this.groupBox1.Location = new Point(0, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new Size(221, 237);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Map Layers";
			this.gettingStartedLabel.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.gettingStartedLabel.BackColor = SystemColors.ControlLightLight;
			this.gettingStartedLabel.Controls.Add(this.getStartedLabel1);
			this.gettingStartedLabel.Controls.Add(this.getStartedLabel2);
			this.gettingStartedLabel.Location = new Point(13, 23);
			this.gettingStartedLabel.Name = "gettingStartedLabel";
			this.gettingStartedLabel.Size = new Size(196, 70);
			this.gettingStartedLabel.TabIndex = 3;
			this.gettingStartedLabel.TabStop = false;
			this.getStartedLabel1.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.getStartedLabel1.AutoSize = true;
			this.getStartedLabel1.BackColor = SystemColors.ControlLightLight;
			this.getStartedLabel1.Font = new Font("Microsoft Sans Serif", 11f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.getStartedLabel1.ForeColor = Color.Red;
			this.getStartedLabel1.Location = new Point(7, 11);
			this.getStartedLabel1.Name = "getStartedLabel1";
			this.getStartedLabel1.Size = new Size(179, 22);
			this.getStartedLabel1.TabIndex = 1;
			this.getStartedLabel1.Text = "To get started, select";
			this.getStartedLabel1.TextAlign = ContentAlignment.MiddleCenter;
			this.getStartedLabel2.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.getStartedLabel2.AutoSize = true;
			this.getStartedLabel2.BackColor = SystemColors.ControlLightLight;
			this.getStartedLabel2.Font = new Font("Microsoft Sans Serif", 11f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.getStartedLabel2.ForeColor = Color.Red;
			this.getStartedLabel2.Location = new Point(5, 36);
			this.getStartedLabel2.Name = "getStartedLabel2";
			this.getStartedLabel2.Size = new Size(187, 22);
			this.getStartedLabel2.TabIndex = 2;
			this.getStartedLabel2.Text = "File | Add Source Map";
			this.getStartedLabel2.TextAlign = ContentAlignment.MiddleCenter;
			this._layerTreeView.AllowDrop = true;
			this._layerTreeView.Dock = DockStyle.Fill;
			this._layerTreeView.HideSelection = false;
			this._layerTreeView.LabelEdit = true;
			this._layerTreeView.Location = new Point(3, 16);
			this._layerTreeView.Name = "_layerTreeView";
			this._layerTreeView.Size = new Size(215, 218);
			this._layerTreeView.TabIndex = 0;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(this.groupBox1);
			base.Name = "LayerControls";
			base.Size = new Size(221, 237);
			this.groupBox1.ResumeLayout(false);
			this.gettingStartedLabel.ResumeLayout(false);
			this.gettingStartedLabel.PerformLayout();
			base.ResumeLayout(false);
		}
		public LayerControls()
		{
			this.InitializeComponent();
			this.layerTreeView.Tag = this;
			this.layerTreeView.AfterSelect += new TreeViewEventHandler(this.NodeSelectedHandler);
			this.layerTreeView.AfterExpand += new TreeViewEventHandler(this.NodeExpandedHandler);
			this.layerTreeView.AfterCollapse += new TreeViewEventHandler(this.NodeExpandedHandler);
			this.layerTreeView.AfterLabelEdit += new NodeLabelEditEventHandler(this.NodeLabelEditHandler);
			this.layerTreeView.MouseDown += new MouseEventHandler(this.layerTreeView_MouseDown);
			this.layerTreeView.ItemDrag += new ItemDragEventHandler(this.layerTreeView_ItemDrag);
			this.layerTreeView.DragEnter += new DragEventHandler(this.layerTreeView_DragEnter);
			this.layerTreeView.DragOver += new DragEventHandler(this.layerTreeView_DragOver);
			this.layerTreeView.DragDrop += new DragEventHandler(this.layerTreeView_DragDrop);
			this.layerTreeView.DragLeave += new EventHandler(this.layerTreeView_DragLeave);
			this.ContextMenu = new ContextMenu();
			this.ContextMenu.Popup += new EventHandler(this.PopupHandler);
			this.addLayerItem = new MenuItem("Add Layer", new EventHandler(this.AddLayerHandler));
			this.ContextMenu.MenuItems.Add(this.addLayerItem);
			this.addSourceMapItem = new MenuItem("Add Source Map", new EventHandler(this.AddSourceMapHandler));
			this.ContextMenu.MenuItems.Add(this.addSourceMapItem);
			this.addLegendItem = new MenuItem("Add Legend", new EventHandler(this.AddLegendHandler));
			this.ContextMenu.MenuItems.Add(this.addLegendItem);
			this.renameMenuItem = new MenuItem("Rename", new EventHandler(this.RenameHandler));
			this.ContextMenu.MenuItems.Add(this.renameMenuItem);
			this.removeItem = new MenuItem("Remove", new EventHandler(this.RemoveHandler));
			this.ContextMenu.MenuItems.Add(this.removeItem);
		}
		private void layerTreeView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			base.DoDragDrop(e.Item, DragDropEffects.Move);
		}
		private void layerTreeView_DragEnter(object sender, DragEventArgs e)
		{
			D.Sayf(0, "e.Data = {0}", new object[]
			{
				e.Data.ToString()
			});
			if (e.Data.GetDataPresent(LayerControls.dataTypeName, true))
			{
				e.Effect = DragDropEffects.Move;
				return;
			}
			e.Effect = DragDropEffects.None;
		}
		private void layerTreeView_DragOver(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(LayerControls.dataTypeName, true))
			{
				return;
			}
			TreeNode treeNode = (TreeNode)e.Data.GetData(LayerControls.dataTypeName);
			TreeView treeView = (TreeView)sender;
			Point pt = treeView.PointToClient(new Point(e.X, e.Y));
			TreeNode nodeAt = treeView.GetNodeAt(pt);
			if (nodeAt == null || (treeNode.Tag is Layer && !(nodeAt.Tag is Layer)))
			{
				((LayerControls)treeView.Tag).SetDropHighlight(null);
				e.Effect = DragDropEffects.None;
				return;
			}
			((LayerControls)treeView.Tag).SetDropHighlight(nodeAt);
			e.Effect = DragDropEffects.Move;
		}
		private void layerTreeView_DragDrop(object sender, DragEventArgs e)
		{
			TreeView treeView = (TreeView)sender;
			try
			{
				if (e.Data.GetDataPresent(LayerControls.dataTypeName, true))
				{
					TreeNode treeNode = (TreeNode)e.Data.GetData(LayerControls.dataTypeName);
					TreeNode treeNode2 = ((LayerControls)treeView.Tag).currentDropHighlight;
					if (treeNode2 != null)
					{
						D.Assert(treeView.Tag == this, "Not currently designed to support drags from one layer control to another.");
						if (treeNode2.Tag == treeNode.Tag)
						{
							D.Sayf(0, "Ignoring drop onto self", new object[0]);
						}
						else
						{
							if (treeNode.Tag is SourceMap)
							{
								SourceMap sourceMap = (SourceMap)treeNode.Tag;
								Layer layer = (Layer)treeNode.Parent.Tag;
								if (treeNode2.Tag is Layer)
								{
									Layer layer2 = (Layer)treeNode2.Tag;
									layer.Remove(sourceMap);
									layer2.AddAt(sourceMap, 0);
								}
								else
								{
									if (treeNode2.Tag is SourceMap)
									{
										SourceMap targetSourceMap = (SourceMap)treeNode2.Tag;
										Layer layer3 = (Layer)treeNode2.Parent.Tag;
										layer.Remove(sourceMap);
										int index = layer3.GetIndexOfSourceMap(targetSourceMap) + 1;
										layer3.AddAt(sourceMap, index);
									}
									else
									{
										D.Assert(false, "unknown case");
									}
								}
								object tag = this.layerTreeView.SelectedNode.Tag;
								this.Reload();
								IEnumerator enumerator = this.layerTreeView.Nodes.GetEnumerator();
								try
								{
									while (enumerator.MoveNext())
									{
										TreeNode treeNode3 = (TreeNode)enumerator.Current;
										if (treeNode3.Tag == tag)
										{
											this.layerTreeView.SelectedNode = treeNode3;
											break;
										}
									}
									goto IL_226;
								}
								finally
								{
									IDisposable disposable = enumerator as IDisposable;
									if (disposable != null)
									{
										disposable.Dispose();
									}
								}
							}
							if (treeNode.Tag is Layer)
							{
								Layer layer4 = (Layer)treeNode.Tag;
								if (!(treeNode2.Tag is Layer))
								{
									return;
								}
								Layer layer5 = (Layer)treeNode2.Tag;
								if (layer5 == layer4)
								{
									return;
								}
								this._mashup.layerList.Remove(layer4);
								this._mashup.layerList.AddAfter(layer4, layer5);
								this.Reload();
							}
							else
							{
								D.Assert(false, "didn't think tree could contain anything else");
							}
						}
						IL_226:
						treeNode2.EnsureVisible();
					}
				}
			}
			finally
			{
				((LayerControls)treeView.Tag).SetDropHighlight(null);
			}
		}
		private void layerTreeView_DragLeave(object sender, EventArgs e)
		{
			this.SetDropHighlight(null);
		}
		private void SetDropHighlight(TreeNode targetNode)
		{
			if (this.currentDropHighlight == targetNode)
			{
				return;
			}
			if (this.currentDropHighlight != null)
			{
				this.currentDropHighlight.BackColor = this.layerTreeView.BackColor;
			}
			this.currentDropHighlight = targetNode;
			if (this.currentDropHighlight != null)
			{
				this.currentDropHighlight.BackColor = Color.ForestGreen;
			}
		}
		public void SetLayerControl(LayerControlIfc layerControl)
		{
			this.layerControl = layerControl;
		}
		public void SetMashup(Mashup mashup)
		{
			this._mashup = mashup;
			this.Reload();
		}
		private void Reload()
		{
			this.layerTreeView.Nodes.Clear();
			if (this._mashup != null)
			{
				foreach (Layer current in this._mashup.layerList)
				{
					List<TreeNode> list = new List<TreeNode>();
					foreach (SourceMap current2 in current)
					{
						List<TreeNode> list2 = new List<TreeNode>();
						foreach (Legend current3 in current2.legendList)
						{
							TreeNode treeNode = new NameWatchingTreeNode(current3);
							this.tagToTreeNodeDict[current3] = treeNode;
							list2.Add(treeNode);
						}
						TreeNode treeNode2 = new NameWatchingTreeNode(current2, list2.ToArray());
						this.tagToTreeNodeDict[current2] = treeNode2;
						if (current2.expanded)
						{
							treeNode2.Expand();
						}
						else
						{
							treeNode2.Collapse();
						}
						list.Add(treeNode2);
					}
					TreeNode treeNode3 = new NameWatchingTreeNode(current, list.ToArray());
					this.tagToTreeNodeDict[current] = treeNode3;
					if (current.expanded)
					{
						treeNode3.Expand();
					}
					else
					{
						treeNode3.Collapse();
					}
					this.layerTreeView.Nodes.Add(treeNode3);
				}
			}
			if (this._mashup != null)
			{
				if (this._mashup.lastView is SourceMapRegistrationView)
				{
					this.SelectObject(((SourceMapRegistrationView)this._mashup.lastView).sourceMap);
				}
				else
				{
					if (this._mashup.lastView is LayerView)
					{
						this.SelectObject(((LayerView)this._mashup.lastView).layer);
					}
					else
					{
						if (this._mashup.lastView is LegendView)
						{
							this.SelectObject(((LegendView)this._mashup.lastView).legend);
						}
						else
						{
							if (this._mashup.lastView is NoView)
							{
								this.SelectObject(null);
							}
							else
							{
								this.SelectObject(null);
							}
						}
					}
				}
			}
			this.gettingStartedLabel.Visible = (this._mashup == null || this._mashup.layerList.Count <= 0);
			base.Invalidate();
		}
		public void SelectObject(object obj)
		{
			if (obj == null)
			{
				this.layerTreeView.SelectedNode = null;
				return;
			}
			this.layerTreeView.SelectedNode = this.tagToTreeNodeDict[obj];
		}
		public Layer AddSourceMap(SourceMap sourceMap)
		{
			TreeNode selectedNode = this.layerTreeView.SelectedNode;
			Layer layer;
			if (selectedNode != null)
			{
				if (selectedNode.Tag is Layer)
				{
					layer = (Layer)selectedNode.Tag;
				}
				else
				{
					layer = (Layer)selectedNode.Parent.Tag;
				}
			}
			else
			{
				if (this._mashup.layerList.Count == 0)
				{
					this._mashup.layerList.AddNewLayer();
				}
				layer = this._mashup.layerList.First;
			}
			layer.Add(sourceMap);
			this.Reload();
			return layer;
		}
		public void CancelSourceMap(Layer layer, SourceMap sourceMap)
		{
			LayerControls.ReloadDelegate method = new LayerControls.ReloadDelegate(this.Reload);
			base.Invoke(method);
		}
		private void NodeSelectedHandler(object sender, TreeViewEventArgs e)
		{
			try
			{
				if (e.Node.Tag is SourceMap)
				{
					SourceMap sourceMap = (SourceMap)e.Node.Tag;
					this.layerControl.OpenSourceMap(sourceMap);
				}
				else
				{
					if (e.Node.Tag is Layer)
					{
						Layer layer = (Layer)e.Node.Tag;
						this.layerControl.OpenLayer(layer);
					}
					else
					{
						if (e.Node.Tag is Legend)
						{
							Legend legend = (Legend)e.Node.Tag;
							this.layerControl.OpenLegend(legend);
						}
					}
				}
			}
			catch (UnknownImageTypeException)
			{
			}
		}
		private void NodeExpandedHandler(object sender, TreeViewEventArgs e)
		{
			((ExpandedMemoryIfc)e.Node.Tag).expanded = e.Node.IsExpanded;
		}
		private void NodeLabelEditHandler(object sender, NodeLabelEditEventArgs e)
		{
			if (e.Label == null)
			{
				e.CancelEdit = true;
				return;
			}
			D.Say(0, string.Format("From {0} to {1}", e.Node.Text, e.Label));
			((HasDisplayNameIfc)e.Node.Tag).SetDisplayName(e.Label);
			e.Node.Text = e.Label;
		}
		private void PopupHandler(object sender, EventArgs e)
		{
			this.layerTreeView.SelectedNode = this.clickedNode;
			this.addLayerItem.Enabled = true;
			this.addSourceMapItem.Enabled = true;
			this.removeItem.Enabled = false;
			this.removeItem.Text = "Remove";
			this.removeItem.Enabled = false;
			this.renameMenuItem.Enabled = false;
			this.addLegendItem.Enabled = false;
			if (this.layerTreeView.SelectedNode != null)
			{
				object tag = this.layerTreeView.SelectedNode.Tag;
				this.addLayerItem.Enabled = (tag is Layer);
				this.addSourceMapItem.Enabled = (tag is Layer);
				this.addLegendItem.Enabled = (tag is SourceMap);
				this.renameMenuItem.Enabled = true;
				if (tag is Layer)
				{
					if (this._mashup.layerList.Count < 2)
					{
						this.removeItem.Enabled = false;
						this.removeItem.Text = "Remove Layer (no other layers)";
						return;
					}
					if (((Layer)tag).Count > 0)
					{
						this.removeItem.Enabled = false;
						this.removeItem.Text = "Remove Layer (layer not empty)";
						return;
					}
					this.removeItem.Enabled = true;
					this.removeItem.Text = "Remove Layer";
					return;
				}
				else
				{
					if (tag is SourceMap)
					{
						this.removeItem.Enabled = true;
						this.removeItem.Text = "Remove Source Map";
						return;
					}
					if (tag is Legend)
					{
						this.removeItem.Enabled = true;
						this.removeItem.Text = "Remove Legend";
					}
				}
			}
		}
		private void RenameHandler(object sender, EventArgs e)
		{
			this.layerTreeView.SelectedNode.BeginEdit();
		}
		private void AddLayerHandler(object sender, EventArgs e)
		{
			this._mashup.layerList.AddNewLayer();
			this.Reload();
		}
		private void RemoveHandler(object sender, EventArgs e)
		{
			object tag = this.layerTreeView.SelectedNode.Tag;
			if (tag is Layer)
			{
				this._mashup.layerList.Remove((Layer)tag);
			}
			else
			{
				if (tag is SourceMap)
				{
					SourceMap sourceMap = (SourceMap)tag;
					DialogResult dialogResult = MessageBox.Show(string.Format("Are you sure you want to remove source map {0}?", sourceMap.displayName), "Remove source map?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
					if (dialogResult != DialogResult.OK)
					{
						return;
					}
					this.layerControl.RemoveSourceMap(sourceMap);
				}
				else
				{
					if (tag is Legend)
					{
						Legend legend = (Legend)tag;
						legend.sourceMap.legendList.RemoveLegend(legend);
					}
					else
					{
						D.Assert(false);
					}
				}
			}
			object obj = null;
			if (this.layerTreeView.SelectedNode.Parent != null)
			{
				obj = this.layerTreeView.SelectedNode.Parent.Tag;
			}
			this.Reload();
			if (obj != null)
			{
				this.layerTreeView.SelectedNode = this.tagToTreeNodeDict[obj];
				return;
			}
			if (this._mashup.layerList.Count > 0)
			{
				this.layerTreeView.SelectedNode = this.tagToTreeNodeDict[this._mashup.layerList.First];
			}
		}
		private void AddSourceMapHandler(object sender, EventArgs e)
		{
			this.layerControl.AddSourceMap();
		}
		private void AddLegendHandler(object sender, EventArgs e)
		{
			SourceMap sourceMap = (SourceMap)this.layerTreeView.SelectedNode.Tag;
			Legend key = sourceMap.legendList.AddNewLegend();
			this.Reload();
			this.layerTreeView.SelectedNode = this.tagToTreeNodeDict[key];
		}
		private void layerTreeView_MouseDown(object sender, MouseEventArgs e)
		{
			this.clickedNode = this.layerTreeView.GetNodeAt(e.X, e.Y);
		}
	}
}
