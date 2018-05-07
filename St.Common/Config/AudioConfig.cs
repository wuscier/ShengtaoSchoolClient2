namespace St.Common
{
    public class AudioInfo
    {
        public string AudioSammpleDevice { get; set; }

        public string XsAudioSammpleDevice { get; set; }

        public string DocAudioSammpleDevice { get; set; }
        /// <summary>
        /// 采样率
        /// </summary>
        public int SampleRate { get; set; }
        /// <summary>
        /// 音频码率
        /// </summary>
        public int AAC { get; set; }
        /// <summary>
        /// 放音设备
        /// </summary>
        public string AudioOutPutDevice { get; set; }

    }
}