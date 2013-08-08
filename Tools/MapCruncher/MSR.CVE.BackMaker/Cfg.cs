using System;
namespace MSR.CVE.BackMaker
{
	internal abstract class Cfg<T> : ParseableCfg
	{
		private string _name;
		public T value;
		public string name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}
		public Cfg(string name, T defaultValue)
		{
			this.name = name;
			this.value = defaultValue;
		}
		public abstract void ParseFrom(string str);
	}
}
