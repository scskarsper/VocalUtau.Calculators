using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VocalUtau.Calculators
{
    [Serializable]
    public class BinaryFileStruct
    {
        Dictionary<int, List<Calculators.NoteListCalculator.NotePreRender>> _VocalTrackStructs = new Dictionary<int, List<NoteListCalculator.NotePreRender>>();

        public Dictionary<int, List<Calculators.NoteListCalculator.NotePreRender>> VocalTrackStructs
        {
            get { return _VocalTrackStructs; }
            set { _VocalTrackStructs = value; }
        }
        Dictionary<int, float> _TrackVolumes = new Dictionary<int, float>();

        public Dictionary<int, float> TrackVolumes
        {
            get { return _TrackVolumes; }
            set { _TrackVolumes = value; }
        }

        Dictionary<int, List<VocalUtau.Calculators.BarkerCalculator.BgmPreRender>> _BarkerTrackStructs = new Dictionary<int, List<VocalUtau.Calculators.BarkerCalculator.BgmPreRender>>();

        public Dictionary<int, List<VocalUtau.Calculators.BarkerCalculator.BgmPreRender>> BarkerTrackStructs
        {
            get { return _BarkerTrackStructs; }
            set { _BarkerTrackStructs = value; }
        }

        double _StartTimePosition = 0;

        public double StartTimePosition
        {
            get { return _StartTimePosition; }
            set { _StartTimePosition = value; }
        }

        float _GlobalVolume = 1.0f;

        public float GlobalVolume
        {
            get { return _GlobalVolume; }
            set { _GlobalVolume = value; }
        }
    }
}
