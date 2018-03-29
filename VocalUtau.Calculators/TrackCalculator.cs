using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VocalUtau.DirectUI.Utils.SingerUtils;
using VocalUtau.Formats.Model.VocalObject;

namespace VocalUtau.Calculators
{
    public class VocalTrackCalculator
    {
        SingerDataFinder SingerDataFinder = null;
        public VocalTrackCalculator(SingerDataFinder SingerDataFinder)
        {
            this.SingerDataFinder = SingerDataFinder;
        }
        public void CalcTracker(TrackerObject tracker)
        {
            NoteListCalculator nlc = new NoteListCalculator(SingerDataFinder);
            nlc.FillPartsNotes(tracker.PartList[0],0);
        }
    }
}
