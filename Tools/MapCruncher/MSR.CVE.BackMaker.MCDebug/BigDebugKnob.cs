using System;
using System.Runtime.CompilerServices;
namespace MSR.CVE.BackMaker.MCDebug
{
	public class BigDebugKnob
	{
		public delegate void DebugKnobListener(bool enabled);
		private bool _debugFeaturesEnabled;
		public static BigDebugKnob theKnob = new BigDebugKnob();

        private event BigDebugKnob.DebugKnobListener listeners;

		public bool debugFeaturesEnabled
		{
			get
			{
				return this._debugFeaturesEnabled;
			}
			set
			{
				this._debugFeaturesEnabled = value;
				this.listeners(this._debugFeaturesEnabled);
			}
		}
		public void AddListener(BigDebugKnob.DebugKnobListener listener)
		{
			this.listeners = (BigDebugKnob.DebugKnobListener)Delegate.Combine(this.listeners, listener);
		}
	}
}
