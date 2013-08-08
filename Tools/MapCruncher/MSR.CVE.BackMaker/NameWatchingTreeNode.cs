using System;
using System.Threading;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	internal class NameWatchingTreeNode : TreeNode
	{
		public NameWatchingTreeNode(HasDisplayNameIfc tag)
		{
			this.Init(tag);
		}
		public NameWatchingTreeNode(HasDisplayNameIfc tag, TreeNode[] children) : base(null, children)
		{
			this.Init(tag);
		}
		private void Init(HasDisplayNameIfc tag)
		{
			base.Tag = tag;
			this.UpdateName();
			if (tag is SourceMap)
			{
				((SourceMap)tag).readyToLockChangedEvent.Add(new DirtyListener(this.UpdateNameListener));
			}
		}
		public void Dispose()
		{
			if (base.Tag is SourceMap)
			{
				((SourceMap)base.Tag).readyToLockChangedEvent.Remove(new DirtyListener(this.UpdateNameListener));
			}
		}
		private void UpdateNameListener()
		{
			Monitor.Enter(this);
			try
			{
				if (base.TreeView != null)
				{
					base.TreeView.Invoke(new DirtyListener(this.UpdateName));
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		private void UpdateName()
		{
			string text = ((HasDisplayNameIfc)base.Tag).GetDisplayName();
			if (base.Tag is SourceMap && !((SourceMap)base.Tag).ReadyToLock())
			{
				text += " (!)";
			}
			base.Text = text;
		}
	}
}
