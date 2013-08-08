using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker
{
	public class RenderComplaintBox
	{
		public delegate void AnnounceDelegate(string complaint);
		private RenderComplaintBox.AnnounceDelegate announce;
		private Dictionary<NonredundantRenderComplaint, bool> complaints = new Dictionary<NonredundantRenderComplaint, bool>();
		public RenderComplaintBox(RenderComplaintBox.AnnounceDelegate announce)
		{
			this.announce = announce;
		}
		public void Complain(NonredundantRenderComplaint complaint)
		{
			if (!this.complaints.ContainsKey(complaint))
			{
				this.complaints.Add(complaint, false);
				this.announce(complaint.ToString());
			}
		}
	}
}
