using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class TransparencyPanel : UserControl
	{
		private IContainer components;
		private Button addTransparencyButton;
		private DataGridView colorGrid;
		private Button removeTransparencyButton;
		private RadioButton normalTransparencyButton;
		private RadioButton invertedTransparencyButton;
		private RadioButton noTransparencyButton;
		private NumericUpDown fuzzSpinner;
		private NumericUpDown haloSpinner;
		private Label label1;
		private Label label2;
		private DataGridViewImageColumn color;
		private DataGridViewTextBoxColumn Epsilon;
		private DataGridViewTextBoxColumn HaloWidth;
		private CheckBox useDocumentTransparencyCheckbox;
		private SourceMap sourceMap;
		private TransparencyIfc transparencyIfc;
		private bool needUpdate;
		private bool disableSpinnerUpdate;
		private bool suspendDocUpdate;
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
			DataGridViewCellStyle dataGridViewCellStyle = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
			this.addTransparencyButton = new Button();
			this.colorGrid = new DataGridView();
			this.color = new DataGridViewImageColumn();
			this.Epsilon = new DataGridViewTextBoxColumn();
			this.HaloWidth = new DataGridViewTextBoxColumn();
			this.removeTransparencyButton = new Button();
			this.normalTransparencyButton = new RadioButton();
			this.invertedTransparencyButton = new RadioButton();
			this.noTransparencyButton = new RadioButton();
			this.fuzzSpinner = new NumericUpDown();
			this.haloSpinner = new NumericUpDown();
			this.label1 = new Label();
			this.label2 = new Label();
			this.useDocumentTransparencyCheckbox = new CheckBox();
			((ISupportInitialize)this.colorGrid).BeginInit();
			((ISupportInitialize)this.fuzzSpinner).BeginInit();
			((ISupportInitialize)this.haloSpinner).BeginInit();
			base.SuspendLayout();
			this.addTransparencyButton.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.addTransparencyButton.Location = new Point(3, 95);
			this.addTransparencyButton.Name = "addTransparencyButton";
			this.addTransparencyButton.Size = new Size(210, 23);
			this.addTransparencyButton.TabIndex = 0;
			this.addTransparencyButton.Text = "Add Color Under Crosshairs";
			this.addTransparencyButton.UseVisualStyleBackColor = true;
			this.addTransparencyButton.Click += new EventHandler(this.addTransparencyButton_Click);
			this.colorGrid.AllowUserToAddRows = false;
			this.colorGrid.AllowUserToDeleteRows = false;
			this.colorGrid.AllowUserToResizeRows = false;
			this.colorGrid.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			dataGridViewCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle.BackColor = SystemColors.Control;
			dataGridViewCellStyle.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			dataGridViewCellStyle.ForeColor = SystemColors.WindowText;
			dataGridViewCellStyle.SelectionBackColor = SystemColors.Highlight;
			dataGridViewCellStyle.SelectionForeColor = SystemColors.HighlightText;
			dataGridViewCellStyle.WrapMode = DataGridViewTriState.True;
			this.colorGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
			this.colorGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.colorGrid.Columns.AddRange(new DataGridViewColumn[]
			{
				this.color,
				this.Epsilon,
				this.HaloWidth
			});
			this.colorGrid.Location = new Point(3, 124);
			this.colorGrid.MultiSelect = false;
			this.colorGrid.Name = "colorGrid";
			this.colorGrid.ReadOnly = true;
			this.colorGrid.RowHeadersVisible = false;
			this.colorGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			this.colorGrid.Size = new Size(213, 158);
			this.colorGrid.TabIndex = 1;
			this.colorGrid.SelectionChanged += new EventHandler(this.pinList_SelectedIndexChanged);
			this.color.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			this.color.HeaderText = "Color";
			this.color.Name = "color";
			this.color.ReadOnly = true;
			this.color.Resizable = DataGridViewTriState.True;
			this.color.SortMode = DataGridViewColumnSortMode.Automatic;
			this.Epsilon.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
			this.Epsilon.DefaultCellStyle = dataGridViewCellStyle2;
			this.Epsilon.HeaderText = "Fuzziness (+/--)";
			this.Epsilon.Name = "Epsilon";
			this.Epsilon.ReadOnly = true;
			this.HaloWidth.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;
			this.HaloWidth.DefaultCellStyle = dataGridViewCellStyle3;
			this.HaloWidth.HeaderText = "Halo";
			this.HaloWidth.Name = "HaloWidth";
			this.HaloWidth.ReadOnly = true;
			this.removeTransparencyButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.removeTransparencyButton.Location = new Point(3, 288);
			this.removeTransparencyButton.Name = "removeTransparencyButton";
			this.removeTransparencyButton.Size = new Size(213, 23);
			this.removeTransparencyButton.TabIndex = 2;
			this.removeTransparencyButton.Text = "Remove Selected Color";
			this.removeTransparencyButton.UseVisualStyleBackColor = true;
			this.removeTransparencyButton.Click += new EventHandler(this.removeTransparencyButton_Click);
			this.normalTransparencyButton.AutoSize = true;
			this.normalTransparencyButton.Checked = true;
			this.normalTransparencyButton.Location = new Point(3, 31);
			this.normalTransparencyButton.Name = "normalTransparencyButton";
			this.normalTransparencyButton.Size = new Size(182, 17);
			this.normalTransparencyButton.TabIndex = 3;
			this.normalTransparencyButton.TabStop = true;
			this.normalTransparencyButton.Text = "Make selected colors transparent";
			this.normalTransparencyButton.UseVisualStyleBackColor = true;
			this.normalTransparencyButton.CheckedChanged += new EventHandler(this.normalTransparencyButton_CheckedChanged);
			this.invertedTransparencyButton.AutoSize = true;
			this.invertedTransparencyButton.Location = new Point(3, 55);
			this.invertedTransparencyButton.Name = "invertedTransparencyButton";
			this.invertedTransparencyButton.Size = new Size(155, 17);
			this.invertedTransparencyButton.TabIndex = 4;
			this.invertedTransparencyButton.TabStop = true;
			this.invertedTransparencyButton.Text = "Display only selected colors";
			this.invertedTransparencyButton.UseVisualStyleBackColor = true;
			this.invertedTransparencyButton.CheckedChanged += new EventHandler(this.invertedTransparencyButton_CheckedChanged);
			this.noTransparencyButton.AutoSize = true;
			this.noTransparencyButton.Location = new Point(3, 78);
			this.noTransparencyButton.Name = "noTransparencyButton";
			this.noTransparencyButton.Size = new Size(124, 17);
			this.noTransparencyButton.TabIndex = 5;
			this.noTransparencyButton.TabStop = true;
			this.noTransparencyButton.Text = "Disable transparency";
			this.noTransparencyButton.UseVisualStyleBackColor = true;
			this.noTransparencyButton.CheckedChanged += new EventHandler(this.noTransparencyButton_CheckedChanged);
			this.fuzzSpinner.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.fuzzSpinner.Location = new Point(62, 321);
			this.fuzzSpinner.Name = "fuzzSpinner";
			this.fuzzSpinner.Size = new Size(42, 20);
			this.fuzzSpinner.TabIndex = 6;
			this.fuzzSpinner.ValueChanged += new EventHandler(this.exactnessSpinner_ValueChanged);
			this.haloSpinner.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.haloSpinner.Location = new Point(165, 321);
			this.haloSpinner.Name = "haloSpinner";
			this.haloSpinner.Size = new Size(48, 20);
			this.haloSpinner.TabIndex = 7;
			this.haloSpinner.ValueChanged += new EventHandler(this.haloSpinner_ValueChanged);
			this.label1.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.label1.AutoSize = true;
			this.label1.Location = new Point(0, 323);
			this.label1.Name = "label1";
			this.label1.Size = new Size(53, 13);
			this.label1.TabIndex = 8;
			this.label1.Text = "Fuzziness";
			this.label2.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.label2.AutoSize = true;
			this.label2.Location = new Point(129, 323);
			this.label2.Name = "label2";
			this.label2.Size = new Size(29, 13);
			this.label2.TabIndex = 9;
			this.label2.Text = "Halo";
			this.useDocumentTransparencyCheckbox.AutoSize = true;
			this.useDocumentTransparencyCheckbox.Location = new Point(3, 8);
			this.useDocumentTransparencyCheckbox.Name = "useDocumentTransparencyCheckbox";
			this.useDocumentTransparencyCheckbox.Size = new Size(159, 17);
			this.useDocumentTransparencyCheckbox.TabIndex = 10;
			this.useDocumentTransparencyCheckbox.Text = "Use document transparency";
			this.useDocumentTransparencyCheckbox.UseVisualStyleBackColor = true;
			this.useDocumentTransparencyCheckbox.CheckedChanged += new EventHandler(this.useDocumentTransparencyCheckbox_CheckedChanged);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(this.useDocumentTransparencyCheckbox);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.haloSpinner);
			base.Controls.Add(this.fuzzSpinner);
			base.Controls.Add(this.noTransparencyButton);
			base.Controls.Add(this.invertedTransparencyButton);
			base.Controls.Add(this.normalTransparencyButton);
			base.Controls.Add(this.removeTransparencyButton);
			base.Controls.Add(this.colorGrid);
			base.Controls.Add(this.addTransparencyButton);
			base.Name = "TransparencyPanel";
			base.Size = new Size(216, 345);
			((ISupportInitialize)this.colorGrid).EndInit();
			((ISupportInitialize)this.fuzzSpinner).EndInit();
			((ISupportInitialize)this.haloSpinner).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
		public TransparencyPanel()
		{
			this.InitializeComponent();
			this.haloSpinner.Minimum = TransparencyOptions.HaloSizeRange.min;
			this.haloSpinner.Maximum = TransparencyOptions.HaloSizeRange.max;
			this.fuzzSpinner.Minimum = TransparencyOptions.FuzzRange.min;
			this.fuzzSpinner.Maximum = TransparencyOptions.FuzzRange.max;
			this.disableSpinnerUpdate = false;
		}
		public void Configure(SourceMap sourceMap, TransparencyIfc transparencyIfc)
		{
			this.transparencyIfc = transparencyIfc;
			if (this.sourceMap != null)
			{
				this.sourceMap.transparencyOptions.transparencyOptionsChangedEvent -= new TransparencyOptionsChangedDelegate(this.TransparencyChangedHandler);
			}
			this.sourceMap = sourceMap;
			if (this.sourceMap != null)
			{
				this.sourceMap.transparencyOptions.transparencyOptionsChangedEvent += new TransparencyOptionsChangedDelegate(this.TransparencyChangedHandler);
			}
			this.update();
		}
		private void TransparencyChangedHandler()
		{
			this.needUpdate = true;
			base.Invalidate();
			if (this.transparencyIfc != null)
			{
				this.transparencyIfc.InvalidatePipeline();
			}
		}
		public void SetSelected(TransparencyColor tc)
		{
			try
			{
				if (tc != null)
				{
					foreach (DataGridViewRow dataGridViewRow in (IEnumerable)this.colorGrid.Rows)
					{
						if (((TransparencyColor)dataGridViewRow.Tag).color == tc.color)
						{
							dataGridViewRow.Selected = true;
							this.colorGrid.CurrentCell = dataGridViewRow.Cells[0];
							this.pinList_SelectedIndexChanged(null, null);
							return;
						}
					}
					this.UnselectAll();
				}
			}
			catch (Exception ex)
			{
				D.Sayf(0, "the bad thing happened: {0}", new object[]
				{
					ex.Message
				});
			}
		}
		private void UnselectAll()
		{
			foreach (DataGridViewRow dataGridViewRow in this.colorGrid.SelectedRows)
			{
				dataGridViewRow.Selected = false;
			}
			this.pinList_SelectedIndexChanged(null, null);
		}
		public TransparencyColor GetSelected()
		{
			foreach (DataGridViewRow dataGridViewRow in (IEnumerable)this.colorGrid.Rows)
			{
				if (dataGridViewRow.Selected)
				{
					return (TransparencyColor)dataGridViewRow.Tag;
				}
			}
			return null;
		}
		private void update()
		{
			if (this.sourceMap != null)
			{
				this.useDocumentTransparencyCheckbox.Checked = this.sourceMap.transparencyOptions.useDocumentTransparency;
				TransparencyOptions.TransparencyMode mode = this.sourceMap.transparencyOptions.GetMode();
				this.suspendDocUpdate = true;
				if (mode == TransparencyOptions.TransparencyMode.Off)
				{
					this.noTransparencyButton.Checked = true;
				}
				else
				{
					if (mode == TransparencyOptions.TransparencyMode.Inverted)
					{
						this.invertedTransparencyButton.Checked = true;
					}
					else
					{
						if (mode == TransparencyOptions.TransparencyMode.Normal)
						{
							this.normalTransparencyButton.Checked = true;
						}
					}
				}
				this.suspendDocUpdate = false;
			}
			this.disableSpinnerUpdate = true;
			TransparencyColor selected = this.GetSelected();
			this.colorGrid.Rows.Clear();
			if (this.sourceMap != null)
			{
				foreach (TransparencyColor current in this.sourceMap.transparencyOptions.colorList)
				{
					DataGridViewRow dataGridViewRow = new DataGridViewRow();
					Bitmap bitmap = new Bitmap(40, 12);
					Graphics graphics = Graphics.FromImage(bitmap);
					graphics.FillRectangle(new SolidBrush(current.color.ToColor()), new Rectangle(new Point(0, 0), bitmap.Size));
					dataGridViewRow.CreateCells(this.colorGrid, new object[]
					{
						bitmap,
						current.fuzz.ToString(),
						current.halo.ToString()
					});
					dataGridViewRow.Tag = current;
					this.colorGrid.Rows.Add(dataGridViewRow);
				}
			}
			this.disableSpinnerUpdate = false;
			this.SetSelected(selected);
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			if (D.CustomPaintDisabled())
			{
				return;
			}
			if (this.needUpdate)
			{
				this.needUpdate = false;
				this.update();
			}
			base.OnPaint(e);
		}
		private void addTransparencyButton_Click(object sender, EventArgs e)
		{
			if (this.sourceMap != null && this.transparencyIfc != null)
			{
				Pixel baseLayerCenterPixel = this.transparencyIfc.GetBaseLayerCenterPixel();
				if (baseLayerCenterPixel is UndefinedPixel)
				{
					return;
				}
				foreach (TransparencyColor current in this.sourceMap.transparencyOptions.colorList)
				{
					if (current.color == baseLayerCenterPixel)
					{
						this.SetSelected(current);
						return;
					}
				}
				TransparencyColor selected = this.sourceMap.transparencyOptions.AddColor(baseLayerCenterPixel);
				this.update();
				this.SetSelected(selected);
			}
		}
		private void removeTransparencyButton_Click(object sender, EventArgs e)
		{
			if (this.sourceMap != null && this.colorGrid.SelectedRows.Count >= 1)
			{
				this.sourceMap.transparencyOptions.RemoveColor((TransparencyColor)this.colorGrid.SelectedRows[0].Tag);
			}
		}
		private void normalTransparencyButton_CheckedChanged(object sender, EventArgs e)
		{
			if (this.suspendDocUpdate)
			{
				return;
			}
			if (this.sourceMap != null)
			{
				this.sourceMap.transparencyOptions.SetNormalTransparency();
			}
		}
		private void invertedTransparencyButton_CheckedChanged(object sender, EventArgs e)
		{
			if (this.suspendDocUpdate)
			{
				return;
			}
			if (this.sourceMap != null)
			{
				this.sourceMap.transparencyOptions.SetInvertedTransparency();
			}
		}
		private void noTransparencyButton_CheckedChanged(object sender, EventArgs e)
		{
			if (this.suspendDocUpdate)
			{
				return;
			}
			if (this.sourceMap != null)
			{
				this.sourceMap.transparencyOptions.SetDisabledTransparency();
			}
		}
		private void exactnessSpinner_ValueChanged(object sender, EventArgs e)
		{
			if (this.sourceMap != null && this.colorGrid.SelectedRows.Count >= 1)
			{
				this.sourceMap.transparencyOptions.SetFuzz((TransparencyColor)this.colorGrid.SelectedRows[0].Tag, (int)this.fuzzSpinner.Value);
			}
		}
		private void haloSpinner_ValueChanged(object sender, EventArgs e)
		{
			if (this.sourceMap != null && this.colorGrid.SelectedRows.Count >= 1)
			{
				this.sourceMap.transparencyOptions.SetHalo((TransparencyColor)this.colorGrid.SelectedRows[0].Tag, (int)this.haloSpinner.Value);
			}
		}
		private void pinList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.disableSpinnerUpdate)
			{
				return;
			}
			TransparencyColor selected = this.GetSelected();
			if (selected == null)
			{
				this.haloSpinner.Enabled = false;
				this.fuzzSpinner.Enabled = false;
				this.removeTransparencyButton.Enabled = false;
				return;
			}
			if (this.haloSpinner.Value != selected.halo)
			{
				this.haloSpinner.Value = selected.halo;
			}
			if (this.fuzzSpinner.Value != selected.fuzz)
			{
				this.fuzzSpinner.Value = selected.fuzz;
			}
			this.haloSpinner.Enabled = true;
			this.fuzzSpinner.Enabled = true;
			this.removeTransparencyButton.Enabled = true;
		}
		private void useDocumentTransparencyCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			this.sourceMap.transparencyOptions.useDocumentTransparency = this.useDocumentTransparencyCheckbox.Checked;
		}
	}
}
