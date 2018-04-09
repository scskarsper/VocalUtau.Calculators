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
        public List<NoteListCalculator.NotePreRender> CalcTracker(TrackerObject tracker,double BaseTempo)
        {
            List<NoteListCalculator.NotePreRender> ResultList = new List<NoteListCalculator.NotePreRender>();
            Dictionary<int, NoteListCalculator.NotePreRender[]> PreCalcer = new Dictionary<int, NoteListCalculator.NotePreRender[]>();
            Object oLocker = new Object();
            Parallel.For(0,tracker.PartList.Count,(i)=>
            {
                NoteListCalculator nlc = new NoteListCalculator(SingerDataFinder);
                nlc.FillPartsNotes(tracker.PartList[0], 0);
                lock (oLocker)
                {
                    PreCalcer.Add(i, nlc.NotePreRenderList.ToArray());
                }
            });
            double TimeState = 0;
            for (int i = 0; i < tracker.PartList.Count; i++)
            {
                PartsObject p = tracker.PartList[i];
                double TimeDert = p.getStartTime() - TimeState;
                if (TimeDert > 0)
                {
                    long TickRStart=MidiMathUtils.Time2Tick(TimeState, BaseTempo);
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
                        npr.Note = "{R}";
                        npr.TimeLen = MidiMathUtils.Tick2Time(npr.Length, BaseTempo) * 1000;
                        ResultList.Add(npr);
                    }
                }
                ResultList.AddRange(PreCalcer[i]);
                TimeState = p.getStartTime()+p.getDuringTime();
            }
            Debug_CreateBat(ResultList,tracker, BaseTempo);
            return ResultList;
        }

        void Debug_CreateBat(List<NoteListCalculator.NotePreRender> NList,TrackerObject tracker, double BaseTempo)
        {
            //100
            using (FileStream fs = new FileStream(@"D:\\test-t"+tracker.getIndex().ToString()+".bat", FileMode.Create))
            {
                using (StreamWriter sw=new StreamWriter(fs))
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
                            sw.WriteLine(@"resampler.exe " + resStr.Replace(@"D:\VocalUtau\VocalUtau\bin\Debug\voicedb\YongQi_CVVChinese_Version2\",""));
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
