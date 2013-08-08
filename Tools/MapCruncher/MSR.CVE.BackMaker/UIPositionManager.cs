using System;
namespace MSR.CVE.BackMaker
{
	public class UIPositionManager : PositionUpdateIfc
	{
		private PositionUpdateIfc smUpdate;
		private PositionUpdateIfc veUpdate;
		private PositionMemoryIfc positionMemory;
		private bool slaved;
		private MapPosition _smPos;
		private MapPosition _vePos;
		private MapPosition slavedPos;
		public UIPositionManager(ViewerControl smViewer, ViewerControl veViewer)
		{
			this._smPos = new MapPosition(this);
			this._vePos = new MapPosition(this);
			smViewer.Initialize(new MapPositionDelegate(this.GetSMPos), "Source Map Position");
			veViewer.Initialize(new MapPositionDelegate(this.GetVEPos), "Virtual Earth Position");
			this.smUpdate = smViewer;
			this.veUpdate = veViewer;
			this.slaved = false;
		}
		public void SetPositionMemory(PositionMemoryIfc positionMemory)
		{
			this.positionMemory = positionMemory;
		}
		public MapPosition GetSMPos()
		{
			if (this.slaved)
			{
				return this.slavedPos;
			}
			return this._smPos;
		}
		public MapPosition GetVEPos()
		{
			if (this.slaved)
			{
				return this.slavedPos;
			}
			return this._vePos;
		}
		private void UpdatePositions()
		{
			this.smUpdate.PositionUpdated(this.GetSMPos().llz);
			this.veUpdate.PositionUpdated(this.GetVEPos().llz);
		}
		public void switchSlaved()
		{
			this.slaved = true;
			if (this._vePos != null)
			{
				this.slavedPos = new MapPosition(this._vePos, this);
			}
			else
			{
				this.slavedPos = new MapPosition(this);
			}
			this._smPos = null;
			this._vePos = null;
			this.UpdatePositions();
		}
		public void switchFree()
		{
			this.slaved = false;
			MapPosition prototype;
			if (this.slavedPos != null)
			{
				prototype = this.slavedPos;
			}
			else
			{
				prototype = new MapPosition(null);
			}
			this._vePos = new MapPosition(prototype, this);
			this._smPos = new MapPosition(prototype, this);
			this.slavedPos = null;
			this.UpdatePositions();
		}
		public void PositionUpdated(LatLonZoom llz)
		{
			this.PositionUpdated();
		}
		public void ForceInteractiveUpdate()
		{
			this.smUpdate.ForceInteractiveUpdate();
			this.veUpdate.ForceInteractiveUpdate();
		}
		internal void PositionUpdated()
		{
			this.UpdatePositions();
			if (this.positionMemory != null)
			{
				if (this.slaved)
				{
					this.positionMemory.NotePositionLocked(this.GetVEPos());
					return;
				}
				this.positionMemory.NotePositionUnlocked(this.GetSMPos().llz, this.GetVEPos());
			}
		}
	}
}
