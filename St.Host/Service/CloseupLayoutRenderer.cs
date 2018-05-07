using MeetingSdk.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace St.Host.Service
{
    public class CloseupLayoutRenderer : ILayoutRenderer
    {
        public bool Render(IList<IVideoBox> videoBoxs, Size canvasSize, string specialVideoBoxName)
        {
            if (string.IsNullOrEmpty(specialVideoBoxName))
            {
                throw new ArgumentNullException(nameof(specialVideoBoxName));
            }

            videoBoxs = videoBoxs.OrderBy(vb => vb.AccountResource.AccountModel.AccountId).ToList();

            foreach (var visible in videoBoxs)
            {
                visible.PosX = 0;
                visible.PosY = 0;
                visible.Width = 0;
                visible.PosY = 0;
            }

            var specialVideoBox = videoBoxs.FirstOrDefault(v => v.Name == specialVideoBoxName);

            if (specialVideoBox == null)
            {
                return false;
                //throw new InvalidOperationException("找不到特写画面！");
            }

            specialVideoBox.PosX = 0;
            specialVideoBox.PosY = 0;
            specialVideoBox.Width = canvasSize.Width;
            specialVideoBox.Height = canvasSize.Height;

            return true;
        }
    }
}
