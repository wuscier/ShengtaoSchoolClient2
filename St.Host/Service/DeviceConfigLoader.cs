using MeetingSdk.Wpf;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace St.Host
{
    public class DeviceConfigLoader : IDeviceConfigLoader
    {
        const string ConfigFileName = "device.json";

        public IList<DeviceConfigItem> LoadConfig()
        {
            var configs = new List<DeviceConfigItem>();
            try
            {
                var appPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var path = Path.Combine(appPath, ConfigFileName);
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var items = JsonConvert.DeserializeObject<IList<DeviceConfigItem>>(json);
                    if (items != null)
                    {
                        foreach (var deviceConfigItem in items)
                        {
                            configs.Add(deviceConfigItem);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "加载设备配置文件失败。");
            }
            return configs;
        }

        public void SaveConfig(IDeviceNameAccessor accessor)
        {
            try
            {
                var items = new Dictionary<string, DeviceConfigItem>();
                foreach (var keyValue in accessor)
                {
                    DeviceConfigItem item;
                    if (!items.TryGetValue(keyValue.Key, out item))
                    {
                        item = new DeviceConfigItem() { TypeName = keyValue.Key };
                        items.Add(keyValue.Key, item);
                    }

                    item.DeviceNames.Add(keyValue.Value);
                }

                var appPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var path = Path.Combine(appPath, ConfigFileName);
                var json = JsonConvert.SerializeObject(items.Values, Formatting.Indented);
                if (File.Exists(path))
                    File.Delete(path);

                File.WriteAllText(path, json, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Log.Error(e, "保存设备配置文件失败。");
            }
        }
    }
}
