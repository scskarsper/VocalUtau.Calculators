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
        Dictionary<int, float> _VocalTrackVolumes = new Dictionary<int, float>();

        public Dictionary<int, float> VocalTrackVolumes
        {
            get { return _VocalTrackVolumes; }
            set { _VocalTrackVolumes = value; }
        }

        Dictionary<int, List<VocalUtau.Calculators.BarkerCalculator.BgmPreRender>> _BarkerTrackStructs = new Dictionary<int, List<VocalUtau.Calculators.BarkerCalculator.BgmPreRender>>();

        public Dictionary<int, List<VocalUtau.Calculators.BarkerCalculator.BgmPreRender>> BarkerTrackStructs
        {
            get { return _BarkerTrackStructs; }
            set { _BarkerTrackStructs = value; }
        }
        Dictionary<int, float> _BarkerTrackVolumes = new Dictionary<int, float>();

        public Dictionary<int, float> BarkerTrackVolumes
        {
            get { return _BarkerTrackVolumes; }
            set { _BarkerTrackVolumes = value; }
        }

        double _StartTimePosition = 0;

        public double StartTimePosition
        {
            get { return _StartTimePosition; }
            set { _StartTimePosition = value; }
        }
    }
}
