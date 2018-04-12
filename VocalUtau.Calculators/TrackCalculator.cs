using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VocalUtau.DirectUI.Utils.SingerUtils;
using VocalUtau.Formats.Model.Utils;
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
        public List<NoteListCalculator.NotePreRender> CalcTracker(double TimePosition,TrackerObject tracker, double BaseTempo)
        {
            List<NoteListCalculator.NotePreRender> ResultList = new List<NoteListCalculator.NotePreRender>();
            Dictionary<int, NoteListCalculator.NotePreRender[]> PreCalcer = new Dictionary<int, NoteListCalculator.NotePreRender[]>();
            Object oLocker = new Object();
            Parallel.For(0, tracker.PartList.Count, (i, ParallelLoopState) =>
            {
                if (tracker.PartList[i].getStartTime() + tracker.PartList[i].getDuringTime() < TimePosition)
                {
                    ParallelLoopState.Break();
                }
                else
                {
                    NoteListCalculator nlc = new NoteListCalculator(SingerDataFinder);
                    nlc.FillPartsNotes(tracker.PartList[i], 0);
                    lock (oLocker)
                    {
                        PreCalcer.Add(i, nlc.NotePreRenderList.ToArray());
                    }
                }
            });
            double DelayTime = TimePosition;
            double TimeState = 0;
            for (int i = 0; i < tracker.PartList.Count; i++)
            {
                PartsObject p = tracker.PartList[i];
                double TimeDert = p.getStartTime() - TimeState;
                VocalUtau.Calculators.NoteListCalculator.NotePreRender LastR = null;
                if (TimeDert > 0)
                {
                    long TickRStart = MidiMathUtils.Time2Tick(TimeState, BaseTempo);
                    long TickLength = MidiMathUtils.Time2Tick(TimeDert, BaseTempo);
                    while (TickLength > 0)
                    {
                        VocalUtau.Calculators.NoteListCalculator.NotePreRender npr = new VocalUtau.Calculators.NoteListCalculator.NotePreRender();
                        npr.OtoAtom = new VocalUtau.Formats.Model.Database.VocalDatabase.SoundAtom();
                        npr.OtoAtom.PhonemeSymbol = "{R}";
                        npr.Tick = TickRStart;
                        npr.Length = TickLength > 480 ? 480 : TickLength;
                        TickLength -= 480;
                        TickRStart += 480;
                        npr.Tempo = BaseTempo;
                        npr.partStartTime = TimeState;
                        npr.Note = "{R}";
                        npr.TimeLen = MidiMathUtils.Tick2Time(npr.Length, BaseTempo) * 1000;
                        LastR = npr;

                        VocalUtau.Calculators.NoteListCalculator.NotePreRender fn = null;
                        if (PreCalcer.ContainsKey(i))
                        {
                            try
                            {
                                fn = PreCalcer[i][0];
                            }
                            catch { ;}
                        }
                        UtauRendCommanderUtils.WavtoolArgs wa = NoteListCalculator.NPR2WavtoolArgs(npr, (TickLength>=480)?npr:fn, "{RESAMPLEROUTPUT}", "{WAVOUTPUT}");
                        npr.WavtoolArgs = wa;
                        npr.WavtoolArgList=UtauRendCommanderUtils.GetWavtoolArgs(wa);
                        ResultList.Add(npr);
                    }
                }
                //FixFirstNode
                if (PreCalcer.ContainsKey(i))
                {
                    if (LastR != null)
                    {
                        try
                        {
                            VocalUtau.Calculators.NoteListCalculator.NotePreRender Nxt = PreCalcer[i][0];
                            if (Nxt != null)
                            {
                                if (Nxt.Note != "{R}")
                                {
                                    double PRE = Nxt.RealPreUtterOverArgs.PreUtterance;
                                    double OVL = Nxt.RealPreUtterOverArgs.OverlapMs;
                                    double KickFront = PRE - OVL;
                                    double halfNote = LastR.TimeLen;
                                    if (halfNote < KickFront)
                                    {
                                        //NEED FIX
                                        double ovl = OVL / (PRE - OVL) * halfNote;
                                        double pre = PRE / (PRE - OVL) * halfNote;
                                        if (Nxt.FadeInLengthMs == OVL)
                                        {
                                            Nxt.FadeInLengthMs = ovl;
                                        }
                                        Nxt.RealPreUtterOverArgs.OverlapMs = ovl;
                                        Nxt.RealPreUtterOverArgs.PreUtterance = pre;
                                        Nxt.StartPoint = PRE - pre;
                                        Nxt.StartTimeAttend = -Nxt.RealPreUtterOverArgs.OverlapMs / 1000;
                                    }
                                }
                            }
                        }
                        catch { ;}
                    }
                    NoteListCalculator.NotePreRender[] LCL = PreCalcer[i];
                    for (int k = 0; k < LCL.Length; k++)
                    {
                        NoteListCalculator.NotePreRender pcr = LCL[k];
                        ResultList.Add(pcr);
                    }
                    TimeState = p.getStartTime() + p.getDuringTime();
                }
            }
            for (int i = 0; i < ResultList.Count; i++)
            {
                double EndTime = ResultList[i].absoluteStartTime * 1000 + ResultList[i].TimeLen;
                if (EndTime == ResultList[i].absoluteStartTime * 1000)
                {
                    long TB = 0;
                }
                if (EndTime < TimePosition*1000)
                {
                    ResultList.RemoveAt(i);
                    i--;
                }
                else if (ResultList[i].absoluteStartTime < TimePosition)
                {
                    ResultList[i].passTime = (TimePosition - ResultList[i].absoluteStartTime) * 1000;
                }
            }
            //FixFirstNodeEnd
          //  Debug_CreateBat(ResultList, tracker, BaseTempo);
            return ResultList;
        }

        void Debug_CreateBat(List<NoteListCalculator.NotePreRender> NList, TrackerObject tracker, double BaseTempo)
        {
            //100
            using (FileStream fs = new FileStream(@"D:\\test-t" + tracker.getIndex().ToString() + ".bat", FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine("mkdir \"%temp%\\utaubk\"");
                    for (int i = 0; i < NList.Count; i++)
                    {
                        //"{RESAMPLEROUTPUT}", "{WAVOUTPUT}");
                        if (NList[i].ResamplerArg != null)
                        {
                            string resStr = String.Join(" ", NList[i].ResamplerArgList);
                            resStr = resStr.Replace("{RESAMPLEROUTPUT}", @"temp$$$.wav");
                            sw.WriteLine(@"D:\VocalUtau\VocalUtau.DebugExampleFiles\UTAUKernel\resampler.exe " + resStr);
                        }
                        string wavStr = String.Join(" ", NList[i].WavtoolArgList);
                        wavStr = wavStr.Replace("{RESAMPLEROUTPUT}", @"temp$$$.wav");
                        wavStr = wavStr.Replace("{WAVOUTPUT}", @"temp.wav");
                        sw.WriteLine(@"D:\VocalUtau\VocalUtau.DebugExampleFiles\UTAUKernel\wavtool.exe " + wavStr);
                    }
                }
            }


            //101
            using (FileStream fs = new FileStream(@"D:\\test-b" + tracker.getIndex().ToString() + ".txt", FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    for (int i = 0; i < NList.Count; i++)
                    {
                        //"{RESAMPLEROUTPUT}", "{WAVOUTPUT}");
                        if (NList[i].ResamplerArg != null)
                        {
                            string resStr = String.Join(" ", NList[i].ResamplerArgList);
                            resStr = resStr.Replace("{RESAMPLEROUTPUT}", @"temp$$$.wav");
                            sw.WriteLine(@"resampler.exe " + resStr.Replace(@"D:\VocalUtau\VocalUtau\bin\Debug\voicedb\YongQi_CVVChinese_Version2\", ""));
                        }
                        string wavStr = String.Join(" ", NList[i].WavtoolArgList);
                        wavStr = wavStr.Replace("{RESAMPLEROUTPUT}", @"temp$$$.wav");
                        wavStr = wavStr.Replace("{WAVOUTPUT}", @"temp.wav");
                        sw.WriteLine(@"wavtool.exe " + wavStr.Replace(@"D:\VocalUtau\VocalUtau\bin\Debug\voicedb\YongQi_CVVChinese_Version2\", ""));
                    }
                }
            }
        }
    }
}
