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
        public class NotePreRender : IComparable, IComparer<NotePreRender>
        {
            public long Tick { get; set; }
            public double StartTime { get; set; }
            public long StartTick { get; set; }
            public long Length { get; set; }
            public double StartPoint { get; set; }
            public double TimeLen { get; set; }
            public double Tempo { get; set; }
            public string Note { get; set; }
            public string PitchString { get; set; }
            public double partStartTime { get; set; }
            public double passTime { get; set; }

            public string Flags { get; set; }
            public string Resampler { get; set; }

            private SoundAtom.PreUtterOverlapArgs _realPreUtterOverArgs = new SoundAtom.PreUtterOverlapArgs();

            public SoundAtom.PreUtterOverlapArgs RealPreUtterOverArgs
            {
                get { if (_realPreUtterOverArgs == null)_realPreUtterOverArgs = new SoundAtom.PreUtterOverlapArgs(); return _realPreUtterOverArgs; }
                set { _realPreUtterOverArgs = value; }
            }

            private SoundAtom otoat = new SoundAtom();
            public SoundAtom OtoAtom
            {
                get { if (otoat == null)otoat = new SoundAtom(); return otoat; }
                set { otoat = value; }
            }
            
            double _Intensity = 100;

            public double Intensity
            {
                get { return _Intensity; }
                set { _Intensity = value; }
            }

            double _Moduration = 0;

            public double Moduration
            {
                get { return _Moduration; }
                set { _Moduration = value; }
            }
            
            double _fadeInLengthMs = 5;

            public double FadeInLengthMs
            {
                get { return _fadeInLengthMs; }
                set { _fadeInLengthMs = value; }
            }

            double _fadeOutLengthMs = 35;

            public double FadeOutLengthMs
            {
                get { return _fadeOutLengthMs; }
                set { _fadeOutLengthMs = value; }
            }

            long _volumePercentInt = 100;

            public long VolumePercentInt
            {
                get { return _volumePercentInt; }
                set { _volumePercentInt = value; }
            }

            SortedDictionary<double, long> _EnvlopePoints = new SortedDictionary<double, long>();

            public SortedDictionary<double, long> EnvlopePoints
            {
                get { return _EnvlopePoints; }
                set { _EnvlopePoints = value; }
            }

            VocalUtau.Formats.Model.Utils.UtauRendCommanderUtils.ResamplerArgs _ResamplerArg = null;

            public VocalUtau.Formats.Model.Utils.UtauRendCommanderUtils.ResamplerArgs ResamplerArg
            {
                get { return _ResamplerArg; }
                set { _ResamplerArg = value; }
            }

            string[] _ResamplerArgList = new string[0];

            public string[] ResamplerArgList
            {
                get { return _ResamplerArgList; }
                set { _ResamplerArgList = value; }
            }

            VocalUtau.Formats.Model.Utils.UtauRendCommanderUtils.WavtoolArgs _WavtoolArgs = null;

            public VocalUtau.Formats.Model.Utils.UtauRendCommanderUtils.WavtoolArgs WavtoolArgs
            {
                get { return _WavtoolArgs; }
                set { _WavtoolArgs = value; }
            }

            string[] _WavtoolArgList = new string[0];

            public string[] WavtoolArgList
            {
                get { return _WavtoolArgList; }
                set { _WavtoolArgList = value; }
            }
            
            public int CompareTo(Object o)
            {
                if (this.Tick > ((NotePreRender)o).Tick)
                    return 1;
                else if (this.Tick == ((NotePreRender)o).Tick)
                    return 0;
                else
                    return -1;
            }
            public int Compare(NotePreRender x, NotePreRender y)
            {
                if (x.Tick < y.Tick)
                    return -1;
                else if (x.Tick == y.Tick)
                    return 0;
                else
                    return 1;
            }
        }

        public static UtauRendCommanderUtils.ResamplerArgs NPR2ResamplerArgs(NotePreRender NPR, NotePreRender NextNPR, string OutputWav)
        {
            UtauRendCommanderUtils.ResamplerArgs ra = new UtauRendCommanderUtils.ResamplerArgs(NPR.OtoAtom, OutputWav, NPR.Tempo, NPR.Length, NPR.Note, NPR.Moduration, NPR.Intensity);
            ra.PitchString = NPR.PitchString;
            ra.ThisPreutterOverlapsArgs = NPR.OtoAtom.PreutterOverlapsArgs;
            ra.ThisRealPreutterOverlapsArgs = NPR.RealPreUtterOverArgs;
            ra.Flags = NPR.Flags;
            ra.NextPreutterOverlapsArgs = new SoundAtom.PreUtterOverlapArgs();
            try
            {
                ra.NextPreutterOverlapsArgs = NextNPR.OtoAtom.PreutterOverlapsArgs;
            }
            catch { ;}
            try
            {
                ra.NextRealPreutterOverlapsArgs = NextNPR.RealPreUtterOverArgs;
            }
            catch { ;}
            return ra;
        }

        public static UtauRendCommanderUtils.WavtoolArgs NPR2WavtoolArgs(NotePreRender NPR,NotePreRender NextNPR, string InputWav, string OutputWav)
        {
            UtauRendCommanderUtils.WavtoolArgs ret = new UtauRendCommanderUtils.WavtoolArgs();
            ret.EnvlopePoints = NPR.EnvlopePoints;
            ret.FadeInLengthMs = NPR.FadeInLengthMs;
            ret.FadeOutLengthMs = NPR.FadeOutLengthMs;
            ret.InputWavfile = NPR.Note=="{R}"?"{R}":InputWav;
            ret.OutputWavfile = OutputWav;
            ret.StartPointMs = NPR.StartPoint;// (long)(NPR.passTime * 1000);// (long)NPR.OtoAtom.SoundStartMs;
            ret.Tempo = NPR.Tempo;
            ret.ThisPreutterOverlapsArgs = NPR.RealPreUtterOverArgs;
            ret.NextPreutterOverlapsArgs = new SoundAtom.PreUtterOverlapArgs();
            try
            {
                ret.NextPreutterOverlapsArgs = NextNPR.RealPreUtterOverArgs;
            }
            catch { ;}
            ret.TickLength = NPR.Length;
            ret.VolumePercentInt = NPR.VolumePercentInt;
            return ret;
        }

        SingerDataFinder SingerDataFinder = null;
        public NoteListCalculator(SingerDataFinder SingerDataFinder)
        {
            this.SingerDataFinder = SingerDataFinder;
        }

        List<NotePreRender> _NotePreRenderList = new List<NotePreRender>();

        public List<NotePreRender> NotePreRenderList
        {
            get { return _NotePreRenderList; }
            set { _NotePreRenderList = value; }
        }
        private double defDouble(double src, double def)
        {
            if (double.IsNaN(src) || double.IsNegativeInfinity(src) || double.IsPositiveInfinity(src))
            {
                return def;
            }
            return src;
        }
        string[] KeyChar = new string[]{ "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        public string GetNote(uint NoteNumber)
        {
            int NN = (int)(NoteNumber % 12);
            int NC = (int)(NoteNumber / 12)-1;
            string ret = KeyChar[NN] + NC.ToString();
            return ret;
        }
        
        public void FillPartsNotes(PartsObject parts,long StartTick)
        {
            if(StartTick<0)return;
            if(parts.TickLength<StartTick)return;
            double partsTempo = parts.Tempo;
            int firstIndex=parts.NoteCompiler.FindTickIn(StartTick,0,parts.NoteList.Count);
            VocalIndexObject vio=SingerDataFinder.GetVocalIndexObject(parts);
            string fio = SingerDataFinder.GetSingerFolder(parts);
            SingerObject sobject = SingerDataFinder.GetSingerObject(parts);
            if (vio == null) return;
            Object locker = new Object();
            //完成初步演算
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
                        pra.TimeLen = MidiMathUtils.Tick2Time(pra.Length,parts.Tempo)*1000;
                        TotalTick += pra.Length;
                        pra.Note = GetNote(curNote.PitchValue.NoteNumber);
                        pra.StartPoint = defDouble(curNote.PhonemeAtoms[j].StartPoint,0);
                        string defflag="";
                        string resamp = "";
                        try
                        {
                            defflag = sobject.Flags;
                        }catch{;}
                        try
                        {
                            resamp = sobject.getRealResamplerPath();
                        }
                        catch { ;}
                        pra.Flags = curNote.PhonemeAtoms[j].getFlags(parts.getFlags(defflag));
                        pra.Resampler = parts.getResampler(resamp);

                        pra.VolumePercentInt = curNote.PhonemeAtoms[j].VolumePercentInt;

                        SoundAtom baa = new SoundAtom();
                        uint nn = curNote.PitchValue.NoteNumber;
                        string pref = "";
                        string suff = "";
                        if (vio.PrefixAtomList.PreFix.ContainsKey(nn)) pref = vio.PrefixAtomList.PreFix[nn];
                        if (vio.PrefixAtomList.SufFix.ContainsKey(nn)) suff = vio.PrefixAtomList.SufFix[nn];
                        baa.PhonemeSymbol = pref + curNote.PhonemeAtoms[j].PhonemeAtom + suff;
                        int vid = vio.SndAtomList.IndexOf(baa);
                        if (vio.SndAtomList[vid].WavFile.IndexOf("F4\\shi_shi_shi.wav")!=-1)
                        {
                            int ra = 0;
                            ra = 1;
                        }
                        if (vid == -1 && pref != "" && suff != "")
                        {
                            baa.PhonemeSymbol = curNote.PhonemeAtoms[j].PhonemeAtom;
                            vid = vio.SndAtomList.IndexOf(baa);
                        }
                        if (vid > -1)
                        {
                            pra.OtoAtom = (SoundAtom)vio.SndAtomList[vid].Clone();
                            pra.OtoAtom.PreutterOverlapsArgs.PreUtterance = defDouble(curNote.PhonemeAtoms[j].PreUtterance, pra.OtoAtom.PreutterOverlapsArgs.PreUtterance);
                            pra.OtoAtom.PreutterOverlapsArgs.OverlapMs = defDouble(curNote.PhonemeAtoms[j].Overlap, pra.OtoAtom.PreutterOverlapsArgs.OverlapMs);
                            pra.RealPreUtterOverArgs.PreUtterance = pra.OtoAtom.PreutterOverlapsArgs.PreUtterance;
                            pra.RealPreUtterOverArgs.OverlapMs = pra.OtoAtom.PreutterOverlapsArgs.OverlapMs;
                            pra.OtoAtom.WavFile = PathUtils.AbsolutePath(fio, pra.OtoAtom.WavFile);
                            pra.FadeInLengthMs = curNote.PhonemeAtoms[j].FadeInLengthMs;
                            pra.FadeOutLengthMs = curNote.PhonemeAtoms[j].FadeOutLengthMs;
                            if (j > 0)
                            {
                                pra.FadeInLengthMs = pra.OtoAtom.PreutterOverlapsArgs.OverlapMs;
                                if (pra.Tick - NPR[j - 1].Tick - NPR[j - 1].Length < 2)
                                {
                                    NPR[j - 1].FadeOutLengthMs = pra.OtoAtom.PreutterOverlapsArgs.OverlapMs;
                                }
                            }
                            pra.Intensity = defDouble(curNote.PhonemeAtoms[j].Intensity, 100);
                            pra.Moduration = defDouble(curNote.PhonemeAtoms[j].Modulation, 0);
                            pra.StartTime = MidiMathUtils.Tick2Time(pra.Tick, pra.Tempo);
                            pra.StartTime -= pra.OtoAtom.PreutterOverlapsArgs.OverlapMs / 1000;
                            pra.StartTick = MidiMathUtils.Time2Tick(pra.StartTime, pra.Tempo);
                            if (StartTick - pra.Tick > pra.Length)
                            {
                                pra.passTime = double.MaxValue;
                            }
                            else if (StartTick - pra.Tick > 0)
                            {
                                pra.passTime = MidiMathUtils.Tick2Time(StartTick - pra.Tick, pra.Tempo) * 1000;
                            }
                            else
                            {
                                pra.passTime = 0;
                            }
                            
                            if (pra.StartTick < 0) pra.StartTick = 0;
                            List<int> PointVS = new List<int>();
                            SortedDictionary<double, long> EnvVs = new SortedDictionary<double, long>();
                            long lastDYNValue = -100;
                            for (long k = pra.StartTick; k <= pra.StartTick + pra.Length + 50; k = k + 5)
                            {
                                PointVS.Add((int)(parts.PitchCompiler.getRealPitch(k) - curNote.PitchValue.NoteNumber) * 100);
                                long dyn=(long)(parts.DynCompiler.getDynValue(k)+parts.DynBaseValue);
                                if (dyn != lastDYNValue)
                                {
                                    double TimeMs = (MidiMathUtils.Tick2Time(k - pra.StartTick, parts.Tempo) * 1000);
                                    if(!EnvVs.ContainsKey(TimeMs))EnvVs.Add(TimeMs, dyn);
                                }
                            }
                            pra.EnvlopePoints = EnvVs;
                            pra.PitchString = PitchEncoderUtils.Encode(PointVS);
                            NPR.Add(pra);
                        }
                    }
                    lock (locker)
                    {
                        _NotePreRenderList.AddRange(NPR.ToArray());
                    }
                }
            });
            _NotePreRenderList.Sort();
            int TotC = _NotePreRenderList.Count;
            Parallel.For(0, TotC, (i) =>
            {
                NotePreRender Cur = _NotePreRenderList[i];
                NotePreRender Pre = null;
                if (i > 0)
                {
                    //不是第一个
                    Pre = _NotePreRenderList[i - 1];
                    long dbp = Cur.Tick - Pre.Tick - Pre.Length;
                    if (dbp > 0)
                    {
                        long totalp = dbp;
                        while (totalp > 0)
                        {
                            NotePreRender npr = new NotePreRender();
                            npr.OtoAtom = new SoundAtom();
                            npr.OtoAtom.PhonemeSymbol = "{R}";
                            npr.Tick = Pre.Tick + Pre.Length;
                            npr.Length = totalp > 480 ? 480 : totalp;
                            totalp -= 480;
                            npr.Tempo = parts.Tempo;
                            npr.Note = "{R}";
                            npr.TimeLen = MidiMathUtils.Tick2Time(npr.Length, parts.Tempo) * 1000;
                            lock (locker)
                            {
                                _NotePreRenderList.Add(npr);
                            }
                        }
                    }
                }
                else
                {
                    long dbp = Cur.Tick - StartTick;
                    if (dbp > 0)
                    {
                        long totalp = dbp;
                        while (totalp > 0)
                        {
                            NotePreRender npr = new NotePreRender();
                            npr.OtoAtom = new SoundAtom();
                            npr.OtoAtom.PhonemeSymbol = "{R}";
                            npr.Tick = dbp - totalp;
                            npr.Tempo = parts.Tempo;
                            npr.Length = totalp > 480 ? 480 : totalp;
                            totalp -= 480;
                            npr.Note="{R}";
                            npr.TimeLen = MidiMathUtils.Tick2Time(npr.Length, parts.Tempo) * 1000;
                            lock (locker)
                            {
                                _NotePreRenderList.Add(npr);
                            }
                        }
                    }
                }
            });
            _NotePreRenderList.Sort();
            
            Parallel.For(0,  _NotePreRenderList.Count, (i) =>
            {
                NotePreRender Nxt = null;
                if (i + 1 < _NotePreRenderList.Count)
                {
                    Nxt = _NotePreRenderList[i + 1];
                }

                /*
                 * 修正PreOverlap
                 */
                if (Nxt != null)
                {
                    double PRE = Nxt.RealPreUtterOverArgs.PreUtterance;
                    double OVL = Nxt.RealPreUtterOverArgs.OverlapMs;
                    double KickFront = PRE - OVL;
                    double halfNote = _NotePreRenderList[i].TimeLen / 2;
                    if (halfNote < KickFront)
                    {
                        //NEED FIX
                        double ovl = OVL / (PRE - OVL) * halfNote;
                        double pre = PRE / (PRE - OVL) * halfNote;
                        double stp = PRE / (PRE - OVL) * halfNote;
                        if (Nxt.FadeInLengthMs == OVL && _NotePreRenderList[i].FadeOutLengthMs == OVL)
                        {
                            Nxt.FadeInLengthMs = ovl;
                            _NotePreRenderList[i].FadeOutLengthMs = ovl;
                        }
                        Nxt.RealPreUtterOverArgs.OverlapMs = ovl;
                        Nxt.RealPreUtterOverArgs.PreUtterance = pre;
                        Nxt.StartPoint = PRE - pre;
                    }
                }
                /*
                 * 修正结束
                 */
                if (_NotePreRenderList[i].FadeInLengthMs < _NotePreRenderList[i].RealPreUtterOverArgs.OverlapMs)
                {
                    _NotePreRenderList[i].FadeInLengthMs = _NotePreRenderList[i].RealPreUtterOverArgs.OverlapMs;
                }
                if (Nxt != null)
                {
                    if (_NotePreRenderList[i].FadeOutLengthMs < Nxt.RealPreUtterOverArgs.OverlapMs)
                    {
                        _NotePreRenderList[i].FadeOutLengthMs = Nxt.RealPreUtterOverArgs.OverlapMs;
                    }
                }
                UtauRendCommanderUtils.ResamplerArgs ra = _NotePreRenderList[i].Note=="{R}"?null:NPR2ResamplerArgs(_NotePreRenderList[i],Nxt, "{RESAMPLEROUTPUT}");
                string[] ResList = ra == null ? new string[0] : UtauRendCommanderUtils.GetResamplerArg(ra);
                UtauRendCommanderUtils.WavtoolArgs wa = NPR2WavtoolArgs(_NotePreRenderList[i], Nxt, "{RESAMPLEROUTPUT}", "{WAVOUTPUT}");
                string[] WavList = wa == null ? new string[0] : UtauRendCommanderUtils.GetWavtoolArgs(wa);
                lock (locker)
                {
                    List<string> LSL = new List<string>(); 
                    for (int j = 0; j <= Math.Min(12, WavList.Length - 1); j++) LSL.Add(WavList[j]);
                    _NotePreRenderList[i].ResamplerArg = ra;
                    _NotePreRenderList[i].ResamplerArgList = ResList;
                    _NotePreRenderList[i].WavtoolArgs = wa;
                    _NotePreRenderList[i].WavtoolArgList = LSL.ToArray();// WavList;
                }
            });
        }
    }
}

/*L)
 * pre = PRE / (PRE - OVL) *  x / 2
 * ovl = OVL / (PRE - OVL) *  x / 2
 * STP = PRE - pre
 */
