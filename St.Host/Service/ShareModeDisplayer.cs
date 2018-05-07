using MeetingSdk.NetAgent.Models;
using MeetingSdk.Wpf;
using St.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace St.Host.Service
{
    public class ShareModeDisplayer : IModeDisplayer
    {
        public bool Display(IList<IVideoBox> videoBoxs, Size canvasSize)
        {
            if (videoBoxs.Count() <= 0)
            {
                return false;
                //throw new InvalidOperationException("当前没有画面！");
            }

            int hostId = int.Parse(GlobalData.TryGet(CacheKey.HostId).ToString());
            //var speakerView = videoBoxs.FirstOrDefault(v => v.AccountResource != null && v.AccountResource.AccountModel.AccountId == hostId && v.AccountResource.MediaType == MediaType.Camera);
            var dataCardView = videoBoxs.FirstOrDefault(v => v.AccountResource != null && v.AccountResource.AccountModel.AccountId == hostId && v.AccountResource.MediaType == MediaType.VideoDoc);

            if (dataCardView == null)
            {
                return false;
                //throw new InvalidOperationException("当前没有文档画面！");
            }


            double w = canvasSize.Width;
            double h = canvasSize.Height;

            double bigW = 0.8 * w;
            double bigH = 0.45 * w;

            double smallW = 0.2 * w;
            double smallH = 0.1125 * w;

            double upDownExtra = (h - bigH) / 2;

            videoBoxs = videoBoxs.OrderBy(vb => vb.AccountResource.AccountModel.AccountId).ToList();

            foreach (var visible in videoBoxs)
            {
                visible.PosX = 0;
                visible.PosY = 0;
                visible.Width = 0;
                visible.Height = 0;
            }


            videoBoxs = videoBoxs.Where(v => v.Name != dataCardView.Name).ToList();

            int nonDataCardCount = videoBoxs.Count;

            if (nonDataCardCount >0)
            {
                dataCardView.PosX = 0;
                dataCardView.PosY = upDownExtra;
                dataCardView.Width = bigW;
                dataCardView.Height = bigH;

                for (int i = 0; i < nonDataCardCount; i++)
                {
                    videoBoxs[i].PosX = bigW;
                    videoBoxs[i].PosY = upDownExtra + i * smallH;
                    videoBoxs[i].Width = smallW;
                    videoBoxs[i].Height = smallH;
                }

                return true;
            }

            dataCardView.PosX = 0;
            dataCardView.PosY = 0;
            dataCardView.Width = w;
            dataCardView.Height = h;

            return true;
        }
    }
}
