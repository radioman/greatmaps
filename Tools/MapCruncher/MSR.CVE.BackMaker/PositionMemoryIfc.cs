using System;
namespace MSR.CVE.BackMaker
{
	public interface PositionMemoryIfc
	{
		void NotePositionUnlocked(LatLonZoom sourceMapPosition, MapPosition referenceMapPosition);
		void NotePositionLocked(MapPosition referenceMapPosition);
	}
}
