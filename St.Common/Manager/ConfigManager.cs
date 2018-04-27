using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using JsonConfig;
using Newtonsoft.Json;
using Serilog;

namespace St.Common
{
    public static class ConfigManager
    {
        public static string ConfigFileName = Path.Combine(Environment.CurrentDirectory, GlobalResources.ConfigPath);
        public static string SettingFileName = Path.Combine(Environment.CurrentDirectory, GlobalResources.SettingPath);
        public static string DevConfigFileName = Path.Combine(Environment.CurrentDirectory, GlobalResources.DevConfigPath);

        public static BaseResult ReadConfig()
        {
            if (!File.Exists(ConfigFileName))
            {
                Log.Logger.Debug($"【read config】：config file does not exist, use default config");

                string defaultConfigString = Config.Default.ToString();
                using (StreamWriter sw = new StreamWriter(ConfigFileName, false, Encoding.UTF8))
                {
                    sw.Write(defaultConfigString);
                }
            }

            if (!File.Exists(SettingFileName))
            {
                Type type = MethodBase.GetCurrentMethod().DeclaringType;
                string nspace = type.Namespace;
                string resourceName = nspace + ".Parameter.xml";

                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(resourceName);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(stream);

                xmlDoc.Save(SettingFileName);
            }

            ConfigObject configObject = Config.ApplyJsonFromPath(ConfigFileName);

            if (GlobalData.Instance.RunMode == RunMode.Development && File.Exists(DevConfigFileName))
            {
                configObject = Config.ApplyJsonFromPath(DevConfigFileName, configObject);
            }

            Config.SetUserConfig(Config.User.GetType() == typeof(NullExceptionPreventer)
                ? configObject
                : Config.ApplyJson(Config.User.ToString(), configObject));


            AggregatedConfig aggregatedConfig;
            try
            {
                aggregatedConfig = JsonConvert.DeserializeObject<AggregatedConfig>(Config.User.ToString());
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"【read config exception】：{ex}");
                return new BaseResult()
                {
                    Status = "-1",
                    Message = Messages.ErrorReadConfigFailed
                };
            }

            if (aggregatedConfig == null)
            {
                Log.Logger.Error($"【read config】：empty config");
                return new BaseResult()
                {
                    Status = "-1",
                    Message = Messages.ErrorReadEmptyConfig
                };
            }

            if (aggregatedConfig.MainVideoInfo == null)
            {
                aggregatedConfig.MainVideoInfo = new VideoInfo();
            }

            if (aggregatedConfig.DocVideoInfo == null)
            {
                aggregatedConfig.DocVideoInfo = new VideoInfo();
            }

            if (aggregatedConfig.AudioInfo == null)
            {
                aggregatedConfig.AudioInfo = new AudioInfo();
            }


            GlobalData.Instance.AggregatedConfig = aggregatedConfig;
            return new BaseResult()
            {
                Status = "0",
            };
        }

        public static BaseResult WriteConfig()
        {
            try
            {
                if (GlobalData.Instance.AggregatedConfig == null)
                {
                    return new BaseResult()
                    {
                        Status = "-1",
                        Message = Messages.ErrorWriteEmptyConfig
                    };
                }
                string configJson = JsonConvert.SerializeObject(GlobalData.Instance.AggregatedConfig);

                File.WriteAllText(ConfigFileName, configJson, Encoding.UTF8);

                return new BaseResult() {Status = "0"};
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"【write config exception】：{ex}");
                return new BaseResult()
                {
                    Status = "-1",
                    Message = Messages.ErrorWriteConfigFailed
                };
            }
        }
    }
}
