using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace St.Common
{
    public class SettingParameter
    {
        public List<VedioParameterVGA> VedioParameterVGAs { get; set; }

        public List<VedioParameterRate> VedioParameterRates { get; set; }

        public List<AudioParameterSampleRate> AudioParameterSampleRates { get; set; }

        public List<AudioParameterAAC> AudioParameterAACs { get; set; }

        public List<LiveParameterVGA> LiveParameterVGAs { get; set; }

        public List<LiveParameterRate> LiveParameterRates { get; set; }
    }

    public class VedioParameterVGA
    {
        public string VideoDisplayWidth { get; set; } //宽*高       
    }

    public class VedioParameterRate
    {
        public int VideoBitRate { get; set; }
    }

    public class AudioParameterSampleRate
    {
        public int SampleRate { get; set; }
    }

    public class AudioParameterAAC
    {
        public int AAC { get; set; }
    }

    public class LiveParameterVGA
    {
        public string LiveDisplayWidth { get; set; } //推流分辨率       
    }

    public class LiveParameterRate
    {
        public int LiveBitRate { get; set; } //  推流码率 
    }

}
