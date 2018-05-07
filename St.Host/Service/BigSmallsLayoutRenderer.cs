using MeetingSdk.Wpf;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace St.Host.Service
{
    public class BigSmallsLayoutRenderer : ILayoutRenderer
    {
        public bool Render(IList<IVideoBox> videoBoxs, Size canvasSize, string specialVideoBoxName)
        {
            if (videoBoxs.Count() <= 0)
            {
                return false;
                //throw new InvalidOperationException("当前没有画面！");
            }

            if (string.IsNullOrEmpty(specialVideoBoxName))
            {
                return false;
                //throw new ArgumentNullException(nameof(specialVideoBoxName));
            }

            if (videoBoxs.Count() <= 1)
            {
                return false;
                //throw new InvalidOperationException("画面数不够，无法设置一大多小！");
            }

            double w = canvasSize.Width;
            double h = canvasSize.Height;

            double bigW = 0.8 * w;
            double bigH = 0.45 * w;

            double smallW = 0.2 * w;
            double smallH = 0.1125 * w;

            // upDownExtra may be positive if w/h is less than 16/9,
            // or could be negative if w/h is greater than 16/9
            double upDownExtra = (h - bigH) / 2;

            videoBoxs = videoBoxs.OrderBy(vb => vb.AccountResource.AccountModel.AccountId).ToList();

            foreach (var visible in videoBoxs)
            {
                visible.PosX = 0;
                visible.PosY = 0;
                visible.Height = 0;
                visible.Width = 0;
            }

            var specialVideoBox = videoBoxs.FirstOrDefault(v => v.Name == specialVideoBoxName);

            videoBoxs = videoBoxs.Where(v => v.Name != specialVideoBoxName).ToList();

            if (specialVideoBox == null)
            {
                return false;
                //throw new InvalidOperationException("找不到大画面！");
            }

            specialVideoBox.PosX = 0;
            specialVideoBox.PosY = upDownExtra;
            specialVideoBox.Width = bigW;
            specialVideoBox.Height = bigH;

            for (int i = 0; i < videoBoxs.Count(); i++)
            {
                videoBoxs[i].PosX = bigW;
                videoBoxs[i].PosY = upDownExtra + i * smallH;
                videoBoxs[i].Width = smallW;
                videoBoxs[i].Height = smallH;
            }

            return true;
        }
    }
}
