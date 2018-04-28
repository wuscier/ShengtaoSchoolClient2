using Caliburn.Micro;
using MeetingSdk.Wpf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;

namespace St.Common
{
    public sealed class GlobalData
    {
        private GlobalData()
        {
            RunMode = RunMode.Product;
            ActiveModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public static GlobalData Instance { get; } = new GlobalData();

        public HashSet<string> ActiveModules { get; set; }
        public string SerialNo { get; set; }
        public Version Version { get; set; }
        public Device Device { get; set; }
        public Window UpdatingDialog { get; set; }
        public ViewArea ViewArea { get; set; }
        public RunMode RunMode { get; set; }
        public IntPtr CurWindowHwnd { get; set; }


        private static IMeetingWindowManager _windowManager;

        private static readonly ConcurrentDictionary<CacheKey, object> _cache = new ConcurrentDictionary<CacheKey, object>();

        public static void AddOrUpdate(CacheKey key, object value)
        {
            if (key == CacheKey.HostId)
            {
                _windowManager = IoC.Get<IMeetingWindowManager>();

                _windowManager.HostId = int.Parse(value.ToString());
            }

            _cache.AddOrUpdate(key, value, (cacheKey, oldValue) =>
            {
                return value;
            });
        }

        public static bool TryRemove(CacheKey key)
        {
            object obj;
            return _cache.TryRemove(key, out obj);
        }

        public static object TryGet(CacheKey key)
        {
            object obj;
            _cache.TryGetValue(key, out obj);
            return obj;
        }


        public AggregatedConfig AggregatedConfig { get; set; }
    }
}
