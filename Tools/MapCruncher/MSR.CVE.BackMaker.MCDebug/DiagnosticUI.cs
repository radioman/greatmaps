using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker.MCDebug
{
	public class DiagnosticUI : Form, ListUIIfc
	{
		private delegate void CACDelegate();
		private delegate void UQLDelegate();
		private static DiagnosticUI _theDiagnostics;
		private Dictionary<string, ResourceCounter> resourceCountersByName = new Dictionary<string, ResourceCounter>();
		private Dictionary<ResourceCounter, DataGridViewRow> resourceCounterToGridRow = new Dictionary<ResourceCounter, DataGridViewRow>();
		private EventWaitHandle queueListChangedEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "DiagnosticUI.queueListChangedEvent");
		private List<string> newResourceNames = new List<string>();
		private bool canInvoke;
		private List<object> queueList;
		private DateTime lastQueueDraw;
		private IContainer components;
		private DataGridView resourceCountersGridView;
		private DataGridViewTextBoxColumn resourceName;
		private DataGridViewTextBoxColumn Count;
		private ListBox renderQueueListBox;
		private SplitContainer splitContainer1;
		public static DiagnosticUI theDiagnostics
		{
			get
			{
				if (DiagnosticUI._theDiagnostics == null)
				{
					DiagnosticUI._theDiagnostics = new DiagnosticUI();
				}
				return DiagnosticUI._theDiagnostics;
			}
		}
		public DiagnosticUI()
		{
			this.InitializeComponent();
			base.Shown += new EventHandler(this.DiagnosticUI_Shown);
			base.Closing += new CancelEventHandler(this.DiagnosticUI_Closing);
			DebugThreadInterrupter.theInstance.AddThread("QueueListRedrawThread", new ThreadStart(this.UpdateQueueListThread), ThreadPriority.BelowNormal);
		}
		private void DiagnosticUI_Closing(object sender, CancelEventArgs e)
		{
			this.canInvoke = false;
		}
		private void DiagnosticUI_Shown(object sender, EventArgs e)
		{
			this.CreateAllCounters();
			this.canInvoke = true;
			this.queueListChangedEvent.Set();
		}
		public ResourceCounter fetchResourceCounter(string resourceName, int period)
		{
			Monitor.Enter(this);
			ResourceCounter result;
			try
			{
				if (!this.resourceCountersByName.ContainsKey(resourceName))
				{
					this.resourceCountersByName[resourceName] = new ResourceCounter(resourceName, period, new ResourceCounter.NotifyDelegate(this.ResourceCounterCallback));
					if (this.canInvoke)
					{
						DebugThreadInterrupter.theInstance.AddThread("DiagnosticUI.CreateAllCountersInvokeThread", new ThreadStart(this.CreateAllCountersInvokeThread), ThreadPriority.Normal);
					}
				}
				result = this.resourceCountersByName[resourceName];
			}
			finally
			{
				Monitor.Exit(this);
			}
			return result;
		}
		private void CreateAllCountersInvokeThread()
		{
			DiagnosticUI.CACDelegate method = new DiagnosticUI.CACDelegate(this.CreateAllCounters);
			base.Invoke(method);
		}
		private void CreateAllCounters()
		{
			foreach (string current in this.resourceCountersByName.Keys)
			{
				ResourceCounter resourceCounter = this.resourceCountersByName[current];
				if (!this.resourceCounterToGridRow.ContainsKey(resourceCounter))
				{
					int index = this.resourceCountersGridView.Rows.Add();
					DataGridViewRow dataGridViewRow = this.resourceCountersGridView.Rows[index];
					dataGridViewRow.Cells[0].Value = current;
					dataGridViewRow.Cells[1].Value = resourceCounter.Value;
					this.resourceCounterToGridRow.Add(resourceCounter, dataGridViewRow);
				}
			}
		}
		private void ResourceCounterCallback(ResourceCounter resourceCounter)
		{
			if (this.canInvoke)
			{
				try
				{
					this.resourceCounterToGridRow[resourceCounter].Cells[1].Value = resourceCounter.Value;
				}
				catch (KeyNotFoundException)
				{
				}
			}
		}
		public void listChanged(List<object> prefix)
		{
			this.queueList = prefix;
			this.queueListChangedEvent.Set();
		}
		private void updateQueueList()
		{
			List<object> list = this.queueList;
			this.renderQueueListBox.Items.Clear();
			foreach (object current in list)
			{
				this.renderQueueListBox.Items.Add(current);
			}
			this.renderQueueListBox.Refresh();
		}
		private void UpdateQueueListThread()
		{
			Thread.CurrentThread.IsBackground = true;
			while (true)
			{
				this.queueListChangedEvent.WaitOne();
				DateTime now = DateTime.Now;
				if (this.lastQueueDraw.AddMilliseconds(200.0) < now && this.canInvoke)
				{
					DiagnosticUI.UQLDelegate method = new DiagnosticUI.UQLDelegate(this.updateQueueList);
					base.BeginInvoke(method);
					this.lastQueueDraw = now;
				}
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
			this.resourceCountersGridView = new DataGridView();
			this.resourceName = new DataGridViewTextBoxColumn();
			this.Count = new DataGridViewTextBoxColumn();
			this.renderQueueListBox = new ListBox();
			this.splitContainer1 = new SplitContainer();
			((ISupportInitialize)this.resourceCountersGridView).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			base.SuspendLayout();
			this.resourceCountersGridView.AllowUserToAddRows = false;
			this.resourceCountersGridView.AllowUserToDeleteRows = false;
			this.resourceCountersGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.resourceCountersGridView.Columns.AddRange(new DataGridViewColumn[]
			{
				this.resourceName,
				this.Count
			});
			this.resourceCountersGridView.Dock = DockStyle.Fill;
			this.resourceCountersGridView.Location = new Point(0, 0);
			this.resourceCountersGridView.Name = "resourceCountersGridView";
			this.resourceCountersGridView.ReadOnly = true;
			this.resourceCountersGridView.Size = new Size(283, 455);
			this.resourceCountersGridView.TabIndex = 0;
			this.resourceName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			this.resourceName.HeaderText = "Resource";
			this.resourceName.Name = "resourceName";
			this.resourceName.ReadOnly = true;
			this.Count.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			this.Count.HeaderText = "Count";
			this.Count.Name = "Count";
			this.Count.ReadOnly = true;
			this.renderQueueListBox.Dock = DockStyle.Fill;
			this.renderQueueListBox.FormattingEnabled = true;
			this.renderQueueListBox.Location = new Point(0, 0);
			this.renderQueueListBox.Name = "renderQueueListBox";
			this.renderQueueListBox.Size = new Size(293, 446);
			this.renderQueueListBox.TabIndex = 1;
			this.splitContainer1.Dock = DockStyle.Fill;
			this.splitContainer1.Location = new Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Panel1.Controls.Add(this.resourceCountersGridView);
			this.splitContainer1.Panel2.Controls.Add(this.renderQueueListBox);
			this.splitContainer1.Size = new Size(580, 455);
			this.splitContainer1.SplitterDistance = 283;
			this.splitContainer1.TabIndex = 2;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(580, 455);
			base.Controls.Add(this.splitContainer1);
			base.Name = "DiagnosticUI";
			this.Text = "DiagnosticUI";
			((ISupportInitialize)this.resourceCountersGridView).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			base.ResumeLayout(false);
		}
	}
}
