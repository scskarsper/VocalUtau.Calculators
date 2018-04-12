using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VocalUtau.Formats.Model.VocalObject;

namespace VocalUtau.Calculators
{
    public class BarkerCalculator
    {
        [Serializable]
        public class BgmPreRender
        {
            string _FilePath = "";

            public string FilePath
            {
                get { return _FilePath; }
                set { _FilePath = value; }
            }

            double passTime = 0;

            public double PassTime
            {
                get { if (passTime < 0) return 0; return passTime; }
                set { if(value>0) passTime = value; }
            }

            public double DelayTime
            {
                get { if (passTime > 0) return 0; return -passTime; }
                set { if(value>0) passTime = -value; }
            }
        }
        public List<BgmPreRender> CalcTracker(double TimePosition, BackerObject barkerObject)
        {
            List<BgmPreRender> Ret = new List<BgmPreRender>();
            double LastEnd = 0;
            for(int i=0;i<barkerObject.WavPartList.Count;i++)
            {
                BgmPreRender bpr = new BgmPreRender();
                WavePartsObject wpo = barkerObject.WavPartList[i];
                bpr.FilePath = wpo.WavFileName;
                double endTime = wpo.getStartTime()+wpo.DuringTime;
                if (endTime > TimePosition)
                {
                    if (wpo.getStartTime() > TimePosition)
                    {
                        bpr.DelayTime = wpo.getStartTime() - TimePosition;
                        if (bpr.DelayTime >= LastEnd)
                        {
                            bpr.DelayTime -= LastEnd;
                        }
                        else
                        {
                            bpr.DelayTime = 0;
                        }
                        LastEnd = endTime;
                    }
                    else
                    {
                        bpr.PassTime = TimePosition - wpo.getStartTime();
                        LastEnd = endTime;
                    }
                    Ret.Add(bpr);
                }
            }
            return Ret;
        }

    }
}
