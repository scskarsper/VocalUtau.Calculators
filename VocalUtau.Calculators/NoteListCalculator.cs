using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VocalUtau.DirectUI.Utils.SingerUtils;
using VocalUtau.Formats.Model.Database;
using VocalUtau.Formats.Model.Database.VocalDatabase;
using VocalUtau.Formats.Model.Utils;
using VocalUtau.Formats.Model.VocalObject;

namespace VocalUtau.Calculators
{
    public class NoteListCalculator
    {
        public class NotePreRender : IComparable, IComparer<NoteObject>
        {
            public long Tick { get; set; }
            public long Length { get; set; }
            public double Tempo { get; set; }
            public double partStartTime { get; set; }
            public double noteStartTime { get; set; }
            public double noteAbsoluteStartTime { get { return noteStartTime + partStartTime; } }
            public double noteEndTime { get { return noteStartTime + noteDuringTime; } }
            public double noteAbsoluteEndTime { get { return noteAbsoluteStartTime + noteDuringTime; } }
            public double noteDuringTime { get; set; }
            public double passTime(double curAbsoluteTime)
            {
                if (curAbsoluteTime > noteAbsoluteEndTime) return double.MaxValue;
                if (curAbsoluteTime < noteAbsoluteStartTime) return 0;
                return curAbsoluteTime - noteAbsoluteStartTime;
            }

            private SoundAtom otoat = new SoundAtom();
            public SoundAtom OtoAtom
            {
                get { if (otoat == null)otoat = new SoundAtom(); return otoat; }
                set { otoat = value; }
            }

            public int CompareTo(Object o)
            {
                if (this.Tick > ((NoteObject)o).Tick)
                    return 1;
                else if (this.Tick == ((NoteObject)o).Tick)
                    return 0;
                else
                    return -1;
            }
            public int Compare(NoteObject x, NoteObject y)
            {
                if (x.Tick < y.Tick)
                    return -1;
                else if (x.Tick == y.Tick)
                    return 0;
                else
                    return 1;
            }
        }
        
        SingerDataFinder SingerDataFinder = null;
        public NoteListCalculator(SingerDataFinder SingerDataFinder)
        {
            this.SingerDataFinder = SingerDataFinder;
        }

        List<NotePreRender> NotePreRenderList = new List<NotePreRender>();
        private double defDouble(double src, double def)
        {
            if (double.IsNaN(src) || double.IsNegativeInfinity(src) || double.IsPositiveInfinity(src))
            {
                return def;
            }
            return src;
        }
        public void FillPartsNotes(PartsObject parts,long StartTick)
        {
            if(StartTick<0)return;
            if(parts.TickLength<StartTick)return;
            double partsTempo = parts.Tempo;
            //MidiMathUtils.Tick2Time(
            int firstIndex=parts.NoteCompiler.FindTickIn(StartTick,0,parts.NoteList.Count);
            VocalIndexObject vio=SingerDataFinder.GetVocalIndexObject(parts);
            string fio = SingerDataFinder.GetSingerFolder(parts);
            if (vio == null) return;
            Object locker = new Object();
            Parallel.For(firstIndex < 0 ? 0 : firstIndex, parts.NoteList.Count, (i) => {
                if (parts.NoteList[i].Tick + parts.NoteList[i].Length > StartTick)
                {
                    NoteObject curNote = parts.NoteList[i];
                    List<NotePreRender> NPR = new List<NotePreRender>();
                    List<long> NPT = PhonemeSplitTools.SplitedTickList(curNote.Length, curNote.PhonemeAtoms);
                    long TotalTick = curNote.Tick;
                    for (int j = 0; j < curNote.PhonemeAtoms.Count; j++)
                    {
                        NotePreRender pra = new NotePreRender();
                        pra.Tempo = parts.Tempo;
                        pra.partStartTime = parts.StartTime;
                        
                        pra.Tick = TotalTick;
                        pra.Length = NPT[j];
                        TotalTick += pra.Length;
                        pra.noteStartTime = MidiMathUtils.Tick2Time(pra.Tick,pra.Tempo);
                        pra.noteDuringTime = MidiMathUtils.Tick2Time(pra.Length, pra.Tempo);

                        SoundAtom baa = new SoundAtom();
                        uint nn = curNote.PitchValue.NoteNumber;
                        string pref = "";
                        string suff = "";
                        if (vio.PrefixAtomList.PreFix.ContainsKey(nn)) pref = vio.PrefixAtomList.PreFix[nn];
                        if (vio.PrefixAtomList.SufFix.ContainsKey(nn)) suff = vio.PrefixAtomList.SufFix[nn];
                        baa.PhonemeSymbol = pref+curNote.PhonemeAtoms[j].PhonemeAtom+suff;
                        int vid=vio.SndAtomList.IndexOf(baa);
                        if (vid == -1 && pref!="" && suff!="")
                        {
                            baa.PhonemeSymbol = curNote.PhonemeAtoms[j].PhonemeAtom;
                            vid = vio.SndAtomList.IndexOf(baa);
                        }
                        if (vid > -1)
                        {
                            pra.OtoAtom = (SoundAtom)vio.SndAtomList[vid].Clone();
                            pra.OtoAtom.PreutterOverlapsArgs.PreUtterance = defDouble(curNote.PhonemeAtoms[j].PreUtterance, pra.OtoAtom.PreutterOverlapsArgs.PreUtterance);
                            pra.OtoAtom.PreutterOverlapsArgs.OverlapMs = defDouble(curNote.PhonemeAtoms[j].Overlap, pra.OtoAtom.PreutterOverlapsArgs.OverlapMs);
                            pra.OtoAtom.WavFile = PathUtils.AbsolutePath(fio,pra.OtoAtom.WavFile);
                            pra.OtoAtom.f
                            NPR.Add(pra);
                        }
                    }
                    lock (locker)
                    {
                        NotePreRenderList.AddRange(NPR.ToArray());
                    }
                }
            });
        }
    }
}
