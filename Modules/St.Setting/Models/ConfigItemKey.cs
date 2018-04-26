namespace St.Setting
{
    public enum ConfigItemKey
    {
        MainCamera,
        MainColorspace,
        MainCameraResolution,
        MainCameraCodeRate,
        SecondaryCamera,
        SecondaryColorspace,
        SecondaryCameraResolution,
        SecondaryCameraCodeRate,
        MainMicrophone,
        SecondaryMicrophone,
        Speaker,
        AudioSampleRate,
        AudioCodeRate,
        LiveResolution,
        LiveCodeRate,
        Unknown
    }

    public class ConfigItemTag
    {

        public ConfigItemTag()
        {
            MainCamera = ConfigItemKey.MainCamera;
            MainColorspace = ConfigItemKey.MainColorspace;
            MainCameraResolution = ConfigItemKey.MainCameraResolution;
            MainCameraCodeRate = ConfigItemKey.MainCameraCodeRate;
            SecondaryCamera = ConfigItemKey.SecondaryCamera;
            SecondaryColorspace = ConfigItemKey.SecondaryColorspace;
            SecondaryCameraResolution = ConfigItemKey.SecondaryCameraResolution;
            SecondaryCameraCodeRate = ConfigItemKey.SecondaryCameraCodeRate;
            MainMicrophone = ConfigItemKey.MainMicrophone;
            SecondaryMicrophone = ConfigItemKey.SecondaryMicrophone;
            Speaker = ConfigItemKey.Speaker;
            AudioSampleRate = ConfigItemKey.AudioSampleRate;
            AudioCodeRate = ConfigItemKey.AudioCodeRate;
            LiveResolution = ConfigItemKey.LiveResolution;
            LiveCodeRate = ConfigItemKey.LiveCodeRate;
        }

        public ConfigItemKey MainCamera { get; set; }
        public ConfigItemKey MainColorspace { get; set; }
        public ConfigItemKey MainCameraResolution { get; set; }
        public ConfigItemKey MainCameraCodeRate { get; set; }
        public ConfigItemKey SecondaryCamera { get; set; }
        public ConfigItemKey SecondaryColorspace { get; set; }
        public ConfigItemKey SecondaryCameraResolution { get; set; }
        public ConfigItemKey SecondaryCameraCodeRate { get; set; }
        public ConfigItemKey MainMicrophone { get; set; }
        public ConfigItemKey SecondaryMicrophone { get; set; }
        public ConfigItemKey Speaker { get; set; }
        public ConfigItemKey AudioSampleRate { get; set; }
        public ConfigItemKey AudioCodeRate { get; set; }
        public ConfigItemKey LiveResolution { get; set; }
        public ConfigItemKey LiveCodeRate { get; set; }
    }
}