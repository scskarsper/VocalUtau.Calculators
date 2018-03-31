using System;
using System.Collections.Generic;
using System.IO;
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
            //100
            using (FileStream fs = new FileStream(@"D:\\test-t"+tracker.getIndex().ToString()+".bat", FileMode.Create))
            {
                using (StreamWriter sw=new StreamWriter(fs))
                {
                    sw.WriteLine("mkdir \"%temp%\\utaubk\"");
                    for (int i = 0; i < nlc.NotePreRenderList.Count; i++)
                    {
                        //"{RESAMPLEROUTPUT}", "{WAVOUTPUT}");
                        if (nlc.NotePreRenderList[i].ResamplerArg != null)
                        {
                            string resStr = String.Join(" ", nlc.NotePreRenderList[i].ResamplerArgList);
                            resStr = resStr.Replace("{RESAMPLEROUTPUT}", @"temp$$$.wav");
                            sw.WriteLine(@"D:\VocalUtau\VocalUtau.DebugExampleFiles\UTAUKernel\resampler.exe " + resStr);
                        }
                        string wavStr = String.Join(" ", nlc.NotePreRenderList[i].WavtoolArgList);
                        wavStr = wavStr.Replace("{RESAMPLEROUTPUT}", @"temp$$$.wav");
                        wavStr = wavStr.Replace("{WAVOUTPUT}", @"temp.wav");
                        sw.WriteLine(@"D:\VocalUtau\VocalUtau.DebugExampleFiles\UTAUKernel\wavtool.exe " + wavStr);
                    }
                }
            }
        }
    }
}
