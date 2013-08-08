using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class registrationControls : UserControl, InvalidatableViewIfc
	{
		private IContainer components;
		private Button removeAllPushpinsButton;
		private Button removePushPinButton;
		private Button unlockTransformButton;
		private Button lockTransformButton;
		private Button addPushPinButton;
		private TextBox lockStatusText;
		private DataGridView pinList;
		private Button updatePushPinButton;
		private TableLayoutPanel LockButtonTable;
		private TableLayoutPanel pinNameTable;
		private TextBox pinText;
		private CheckBox forceAffineCheckBox;
		private ToolTip toolTip;
		private GroupBox getStartedBox;
		private TextBox textBox;
		private Panel panel1;
		private DataGridViewTextBoxColumn pinIDcolumn;
		private DataGridViewTextBoxColumn pinNameColumn;
		private DataGridViewTextBoxColumn LocationColumn;
		private DataGridViewTextBoxColumn Error;
		private AssociationIfc associationIfc;
		private RegistrationControlRecord registrationControl;
		private DegreesMinutesSeconds dms = new DegreesMinutesSeconds();
		private MapDrawingOption _ShowDMS;
		public MapDrawingOption ShowDMS
		{
			set
			{
				this._ShowDMS = value;
				this._ShowDMS.SetInvalidatableView(this);
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
			this.components = new Container();
			this.getStartedBox = new GroupBox();
			this.textBox = new TextBox();
			this.pinNameTable = new TableLayoutPanel();
			this.pinText = new TextBox();
			this.addPushPinButton = new Button();
			this.updatePushPinButton = new Button();
			this.LockButtonTable = new TableLayoutPanel();
			this.removePushPinButton = new Button();
			this.removeAllPushpinsButton = new Button();
			this.unlockTransformButton = new Button();
			this.lockTransformButton = new Button();
			this.pinList = new DataGridView();
			this.pinIDcolumn = new DataGridViewTextBoxColumn();
			this.pinNameColumn = new DataGridViewTextBoxColumn();
			this.LocationColumn = new DataGridViewTextBoxColumn();
			this.Error = new DataGridViewTextBoxColumn();
			this.lockStatusText = new TextBox();
			this.forceAffineCheckBox = new CheckBox();
			this.toolTip = new ToolTip(this.components);
			this.panel1 = new Panel();
			this.getStartedBox.SuspendLayout();
			this.pinNameTable.SuspendLayout();
			this.LockButtonTable.SuspendLayout();
			((ISupportInitialize)this.pinList).BeginInit();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.getStartedBox.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.getStartedBox.BackColor = SystemColors.ControlLightLight;
			this.getStartedBox.Controls.Add(this.textBox);
			this.getStartedBox.Location = new Point(9, 61);
			this.getStartedBox.Name = "getStartedBox";
			this.getStartedBox.Size = new Size(196, 95);
			this.getStartedBox.TabIndex = 8;
			this.getStartedBox.TabStop = false;
			this.textBox.BorderStyle = BorderStyle.None;
			this.textBox.Dock = DockStyle.Fill;
			this.textBox.Font = new Font("Microsoft Sans Serif", 11f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.textBox.ForeColor = Color.Red;
			this.textBox.Location = new Point(3, 16);
			this.textBox.Multiline = true;
			this.textBox.Name = "textBox";
			this.textBox.Size = new Size(190, 76);
			this.textBox.TabIndex = 0;
			this.textBox.Text = "Place corresponding points under crosshairs and click Add.";
			this.pinNameTable.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.pinNameTable.ColumnCount = 4;
			this.pinNameTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
			this.pinNameTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
			this.pinNameTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
			this.pinNameTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
			this.pinNameTable.Controls.Add(this.pinText, 0, 0);
			this.pinNameTable.Controls.Add(this.addPushPinButton, 2, 0);
			this.pinNameTable.Controls.Add(this.updatePushPinButton, 3, 0);
			this.pinNameTable.Location = new Point(2, 3);
			this.pinNameTable.Name = "pinNameTable";
			this.pinNameTable.RowCount = 1;
			this.pinNameTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.pinNameTable.Size = new Size(223, 24);
			this.pinNameTable.TabIndex = 10;
			this.pinText.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.pinNameTable.SetColumnSpan(this.pinText, 2);
			this.pinText.Location = new Point(0, 0);
			this.pinText.Margin = new Padding(0);
			this.pinText.Name = "pinText";
			this.pinText.Size = new Size(110, 20);
			this.pinText.TabIndex = 2;
			this.addPushPinButton.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.addPushPinButton.Location = new Point(113, 0);
			this.addPushPinButton.Margin = new Padding(3, 0, 0, 0);
			this.addPushPinButton.Name = "addPushPinButton";
			this.addPushPinButton.Size = new Size(52, 23);
			this.addPushPinButton.TabIndex = 0;
			this.addPushPinButton.Text = "Add";
			this.toolTip.SetToolTip(this.addPushPinButton, "To create a registration point, position the crosshairs over corresponding points on both maps.  Then click Add.");
			this.addPushPinButton.Click += new EventHandler(this.addPushPinButton_Click);
			this.updatePushPinButton.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.updatePushPinButton.Location = new Point(168, 0);
			this.updatePushPinButton.Margin = new Padding(3, 0, 0, 0);
			this.updatePushPinButton.Name = "updatePushPinButton";
			this.updatePushPinButton.Size = new Size(55, 23);
			this.updatePushPinButton.TabIndex = 8;
			this.updatePushPinButton.Text = "Update";
			this.toolTip.SetToolTip(this.updatePushPinButton, "To move an existing point, highlight it on the list below.  Then reposition the crosshairs and click update.");
			this.updatePushPinButton.Click += new EventHandler(this.updatePushPinButton_Click);
			this.LockButtonTable.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
			this.LockButtonTable.ColumnCount = 2;
			this.LockButtonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			this.LockButtonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			this.LockButtonTable.Controls.Add(this.removePushPinButton, 0, 0);
			this.LockButtonTable.Controls.Add(this.removeAllPushpinsButton, 1, 0);
			this.LockButtonTable.Controls.Add(this.unlockTransformButton, 0, 1);
			this.LockButtonTable.Controls.Add(this.lockTransformButton, 1, 1);
			this.LockButtonTable.Location = new Point(69, 293);
			this.LockButtonTable.Name = "LockButtonTable";
			this.LockButtonTable.RowCount = 2;
			this.LockButtonTable.RowStyles.Add(new RowStyle());
			this.LockButtonTable.RowStyles.Add(new RowStyle());
			this.LockButtonTable.Size = new Size(156, 59);
			this.LockButtonTable.TabIndex = 9;
			this.removePushPinButton.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.removePushPinButton.Enabled = false;
			this.removePushPinButton.Location = new Point(3, 3);
			this.removePushPinButton.Name = "removePushPinButton";
			this.removePushPinButton.Size = new Size(72, 23);
			this.removePushPinButton.TabIndex = 1;
			this.removePushPinButton.Text = "Remove";
			this.toolTip.SetToolTip(this.removePushPinButton, "Removes the highlighted correspondence point.");
			this.removePushPinButton.Click += new EventHandler(this.removePushPinButton_Click);
			this.removeAllPushpinsButton.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.removeAllPushpinsButton.Enabled = false;
			this.removeAllPushpinsButton.Location = new Point(81, 3);
			this.removeAllPushpinsButton.Name = "removeAllPushpinsButton";
			this.removeAllPushpinsButton.Size = new Size(72, 23);
			this.removeAllPushpinsButton.TabIndex = 4;
			this.removeAllPushpinsButton.Text = "Remove All";
			this.toolTip.SetToolTip(this.removeAllPushpinsButton, "Removes all correspondence points.");
			this.removeAllPushpinsButton.Click += new EventHandler(this.removeAllPushpinsButton_Click);
			this.unlockTransformButton.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.unlockTransformButton.Location = new Point(3, 32);
			this.unlockTransformButton.Name = "unlockTransformButton";
			this.unlockTransformButton.Size = new Size(72, 24);
			this.unlockTransformButton.TabIndex = 0;
			this.unlockTransformButton.Text = "Unlock";
			this.toolTip.SetToolTip(this.unlockTransformButton, "Unlocks the source map from Virtual Earth, allowing additional points to be added.");
			this.unlockTransformButton.Click += new EventHandler(this.unlockTransformButton_Click);
			this.lockTransformButton.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.lockTransformButton.Location = new Point(81, 32);
			this.lockTransformButton.Name = "lockTransformButton";
			this.lockTransformButton.Size = new Size(72, 24);
			this.lockTransformButton.TabIndex = 0;
			this.lockTransformButton.Text = "Lock";
			this.toolTip.SetToolTip(this.lockTransformButton, "Warps the source map to fit Virtual Earth using the existing correspondence points.");
			this.lockTransformButton.Click += new EventHandler(this.lockTransformButton_Click);
			this.pinList.AllowUserToAddRows = false;
			this.pinList.AllowUserToDeleteRows = false;
			this.pinList.AllowUserToOrderColumns = true;
			this.pinList.AllowUserToResizeRows = false;
			this.pinList.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.pinList.BackgroundColor = SystemColors.ButtonHighlight;
			this.pinList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.pinList.Columns.AddRange(new DataGridViewColumn[]
			{
				this.pinIDcolumn,
				this.pinNameColumn,
				this.LocationColumn,
				this.Error
			});
			this.pinList.GridColor = SystemColors.ActiveCaptionText;
			this.pinList.Location = new Point(2, 32);
			this.pinList.Margin = new Padding(2);
			this.pinList.MultiSelect = false;
			this.pinList.Name = "pinList";
			this.pinList.RowHeadersVisible = false;
			this.pinList.RowTemplate.Height = 24;
			this.pinList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			this.pinList.Size = new Size(224, 202);
			this.pinList.TabIndex = 7;
			this.pinList.DoubleClick += new EventHandler(this.pinList_ItemActivate);
			this.pinList.SelectionChanged += new EventHandler(this.pinList_SelectedIndexChanged);
			this.pinIDcolumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			this.pinIDcolumn.HeaderText = "ID";
			this.pinIDcolumn.MinimumWidth = 15;
			this.pinIDcolumn.Name = "pinIDcolumn";
			this.pinIDcolumn.ReadOnly = true;
			this.pinNameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			this.pinNameColumn.HeaderText = "Name";
			this.pinNameColumn.Name = "pinNameColumn";
			this.pinNameColumn.ReadOnly = true;
			this.LocationColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			this.LocationColumn.HeaderText = "Location";
			this.LocationColumn.Name = "LocationColumn";
			this.Error.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			this.Error.HeaderText = "Error";
			this.Error.Name = "Error";
			this.lockStatusText.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.lockStatusText.BackColor = SystemColors.ControlLightLight;
			this.lockStatusText.BorderStyle = BorderStyle.None;
			this.lockStatusText.Location = new Point(3, 239);
			this.lockStatusText.Multiline = true;
			this.lockStatusText.Name = "lockStatusText";
			this.lockStatusText.ReadOnly = true;
			this.lockStatusText.Size = new Size(223, 48);
			this.lockStatusText.TabIndex = 6;
			this.lockStatusText.TabStop = false;
			this.forceAffineCheckBox.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.forceAffineCheckBox.AutoSize = true;
			this.forceAffineCheckBox.Location = new Point(9, 361);
			this.forceAffineCheckBox.Name = "forceAffineCheckBox";
			this.forceAffineCheckBox.Size = new Size(83, 17);
			this.forceAffineCheckBox.TabIndex = 11;
			this.forceAffineCheckBox.Text = "Force Affine";
			this.toolTip.SetToolTip(this.forceAffineCheckBox, "Selecting \"Affine\" forces MapCruncher to preserve straight lines in your map.  This reduces position accuracy.");
			this.forceAffineCheckBox.UseMnemonic = false;
			this.forceAffineCheckBox.UseVisualStyleBackColor = true;
			this.forceAffineCheckBox.CheckedChanged += new EventHandler(this.checkBox1_CheckedChanged);
			this.panel1.Controls.Add(this.getStartedBox);
			this.panel1.Controls.Add(this.lockStatusText);
			this.panel1.Controls.Add(this.pinNameTable);
			this.panel1.Controls.Add(this.forceAffineCheckBox);
			this.panel1.Controls.Add(this.LockButtonTable);
			this.panel1.Controls.Add(this.pinList);
			this.panel1.Dock = DockStyle.Fill;
			this.panel1.Location = new Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new Size(228, 388);
			this.panel1.TabIndex = 12;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(this.panel1);
			base.Name = "registrationControls";
			base.Size = new Size(228, 388);
			this.getStartedBox.ResumeLayout(false);
			this.getStartedBox.PerformLayout();
			this.pinNameTable.ResumeLayout(false);
			this.pinNameTable.PerformLayout();
			this.LockButtonTable.ResumeLayout(false);
			((ISupportInitialize)this.pinList).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			base.ResumeLayout(false);
		}
		public registrationControls()
		{
			this.InitializeComponent();
			this.unlockTransformButton.Enabled = false;
			this.lockTransformButton.Enabled = false;
			this.forceAffineCheckBox.Visible = false;
		}
		private void WarpStyleClick(object sender, EventArgs e)
		{
			if (this.registrationControl != null)
			{
				this.registrationControl.model.warpStyle = (TransformationStyle)((RadioButton)sender).Tag;
				this.updateWarpStyle();
			}
		}
		public void setAssociationIfc(AssociationIfc ai)
		{
			this.associationIfc = ai;
		}
		public void SetSelected(PositionAssociation pa)
		{
			foreach (DataGridViewRow dataGridViewRow in (IEnumerable)this.pinList.Rows)
			{
				if (dataGridViewRow.Tag == pa)
				{
					dataGridViewRow.Selected = true;
					this.pinList.CurrentCell = dataGridViewRow.Cells[0];
					return;
				}
			}
			this.UnselectAll();
		}
		private void UnselectAll()
		{
			foreach (DataGridViewRow dataGridViewRow in this.pinList.SelectedRows)
			{
				dataGridViewRow.Selected = false;
			}
		}
		public PositionAssociation GetSelected()
		{
			foreach (DataGridViewRow dataGridViewRow in (IEnumerable)this.pinList.Rows)
			{
				if (dataGridViewRow.Selected)
				{
					return (PositionAssociation)dataGridViewRow.Tag;
				}
			}
			return null;
		}
		public void DisplayModel(RegistrationControlRecord registrationControl)
		{
			this.registrationControl = registrationControl;
			this.forceAffineCheckBox.Visible = BuildConfig.theConfig.forceAffineControlVisible;
			this.pinList.Rows.Clear();
			if (registrationControl != null)
			{
				foreach (PositionAssociation current in registrationControl.model.GetAssociationList())
				{
					DataGridViewRow dataGridViewRow = new DataGridViewRow();
					string text = string.Format("{0}, {1}", this.dms.FormatLatLon(current.globalPosition.pinPosition.lat), this.dms.FormatLatLon(current.globalPosition.pinPosition.lon));
					dataGridViewRow.CreateCells(this.pinList, new object[]
					{
						current.pinId,
						current.associationName,
						text,
						current.qualityMessage
					});
					dataGridViewRow.Tag = current;
					this.pinList.Rows.Add(dataGridViewRow);
				}
				if (registrationControl.model.isLocked)
				{
					this.pinText.Enabled = (this.addPushPinButton.Enabled = (this.updatePushPinButton.Enabled = false));
					this.removePushPinButton.Enabled = false;
					this.removeAllPushpinsButton.Enabled = false;
					this.getStartedBox.Visible = false;
				}
				else
				{
					bool flag = registrationControl.model.GetAssociationList().Count > 0;
					this.pinText.Enabled = (this.addPushPinButton.Enabled = (this.updatePushPinButton.Enabled = true));
					this.removeAllPushpinsButton.Enabled = flag;
					this.getStartedBox.Visible = !flag;
				}
			}
			else
			{
				this.removePushPinButton.Enabled = false;
				this.removeAllPushpinsButton.Enabled = false;
				this.pinText.Enabled = (this.addPushPinButton.Enabled = (this.updatePushPinButton.Enabled = false));
			}
			this.updateWarpStyle();
		}
		private void updateWarpStyle()
		{
			if (this.registrationControl == null)
			{
				this.lockTransformButton.Enabled = false;
				this.unlockTransformButton.Enabled = false;
				this.forceAffineCheckBox.Enabled = false;
				this.lockStatusText.Text = "";
				return;
			}
			this.forceAffineCheckBox.Checked = (this.registrationControl.model.warpStyle != TransformationStyleFactory.getDefaultTransformationStyle());
			if (this.registrationControl.model.isLocked)
			{
				this.lockTransformButton.Enabled = false;
				this.lockStatusText.Text = "Explore the map. Select Render tab when done, or Unlock to improve.";
				this.unlockTransformButton.Enabled = true;
				this.forceAffineCheckBox.Enabled = false;
				return;
			}
			bool enabled = this.registrationControl.readyToLock.ReadyToLock();
			this.lockTransformButton.Enabled = enabled;
			this.unlockTransformButton.Enabled = false;
			this.lockStatusText.Lines = this.registrationControl.model.GetLockStatusText();
			this.forceAffineCheckBox.Enabled = true;
		}
		private void addPushPinButton_Click(object sender, EventArgs e)
		{
			try
			{
				this.associationIfc.AddNewAssociation(this.pinText.Text);
			}
			catch (DuplicatePushpinException ex)
			{
				this.DisplayDuplicatePushpinMessage("Cannot add pushpin.", ex);
			}
			this.pinText.Text = "";
			this.UnselectAll();
		}
		private void updatePushPinButton_Click(object sender, EventArgs e)
		{
			if (this.pinList.SelectedRows.Count == 0)
			{
				return;
			}
			int firstDisplayedScrollingRowIndex = this.pinList.FirstDisplayedScrollingRowIndex;
			PositionAssociation positionAssociation = (PositionAssociation)this.pinList.SelectedRows[0].Tag;
			try
			{
				this.associationIfc.UpdateAssociation(positionAssociation, this.pinText.Text);
			}
			catch (DuplicatePushpinException ex)
			{
				this.DisplayDuplicatePushpinMessage("Cannot update pushpin.", ex);
			}
			this.pinText.Text = "";
			this.SetSelected(positionAssociation);
			this.pinList.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;
		}
		private void DisplayDuplicatePushpinMessage(string action, DuplicatePushpinException ex)
		{
			MessageBox.Show(string.Format("{0} {1}", action, ex.ToString()), "Duplicate Pushpin");
		}
		private void removePushPinButton_Click(object sender, EventArgs e)
		{
			int firstDisplayedScrollingRowIndex = this.pinList.FirstDisplayedScrollingRowIndex;
			DataGridViewRow dataGridViewRow = this.pinList.SelectedRows[0];
			int index = dataGridViewRow.Index;
			this.associationIfc.RemoveAssociation((PositionAssociation)dataGridViewRow.Tag);
			if (this.pinList.Rows.Count > 0)
			{
				this.pinList.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;
			}
			if (this.pinList.Rows.Count > index)
			{
				this.pinList.Rows[index].Selected = true;
				return;
			}
			if (this.pinList.Rows.Count > 0)
			{
				this.pinList.Rows[this.pinList.Rows.Count - 1].Selected = true;
			}
		}
		private void removeAllPushpinsButton_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Remove all pushpins?", "Are you sure?", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
			{
				return;
			}
			while (this.pinList.Rows.Count > 0)
			{
				this.associationIfc.RemoveAssociation((PositionAssociation)this.pinList.Rows[0].Tag);
			}
		}
		private void pinList_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.removePushPinButton.Enabled = (this.pinList.SelectedRows.Count != 0 && this.registrationControl != null && !this.registrationControl.model.isLocked);
			this.updatePushPinButton.Enabled = this.removePushPinButton.Enabled;
		}
		private void pinList_ItemActivate(object sender, EventArgs e)
		{
			if (this.pinList.SelectedRows.Count > 0)
			{
				this.associationIfc.ViewAssociation((PositionAssociation)this.pinList.SelectedRows[0].Tag);
			}
		}
		private void lockTransformButton_Click(object sender, EventArgs e)
		{
			this.associationIfc.LockMaps();
		}
		private void unlockTransformButton_Click(object sender, EventArgs e)
		{
			this.associationIfc.UnlockMaps();
		}
		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			if (this.registrationControl != null)
			{
				this.registrationControl.model.warpStyle = (((CheckBox)sender).Checked ? TransformationStyleFactory.getTransformationStyle(1) : TransformationStyleFactory.getDefaultTransformationStyle());
				this.updateWarpStyle();
			}
		}
		public void InvalidateView()
		{
			this.dms.outputMode = (this._ShowDMS.Enabled ? DegreesMinutesSeconds.OutputMode.DMS : DegreesMinutesSeconds.OutputMode.DecimalDegrees);
			this.DisplayModel(this.registrationControl);
		}
	}
}
