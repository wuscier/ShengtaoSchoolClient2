using MeetingSdk.Wpf;
using System.Collections.Generic;
using System.Windows;
using System.Linq;

namespace St.Host.Service
{
    public class InteractionModeDisplayer : IModeDisplayer
    {
        public bool Display(IList<IVideoBox> videoBoxs, Size canvasSize)
        {
            double w = canvasSize.Width;
            double h = canvasSize.Height;
            double halfW = canvasSize.Width / 2;
            double halfH = canvasSize.Height / 2;

            int visibleCount = videoBoxs.Count;

            double actualH = (halfW * 0.5625);

            videoBoxs = videoBoxs.OrderBy(vb => vb.AccountResource.AccountModel.AccountId).ToList();

            switch (visibleCount)
            {
                case 1:
                    videoBoxs[0].PosX = 0;
                    videoBoxs[0].PosY = 0;
                    videoBoxs[0].Width = w;
                    videoBoxs[0].Height = h;
                    break;
                case 2:
                    for (int i = 0; i < visibleCount; i++)
                    {
                        videoBoxs[i].PosX = i * halfW;
                        videoBoxs[i].PosY = halfH - actualH / 2;
                        videoBoxs[i].Width = halfW;
                        videoBoxs[i].Height = actualH;
                    }
                    break;
                case 3:
                case 4:
                    double actualY = halfH - actualH;
                    for (int i = 0; i < visibleCount; i++)
                    {
                        videoBoxs[i].PosX = (i % 2) * halfW;
                        videoBoxs[i].PosY = i < 2 ? actualY : halfH;

                        videoBoxs[i].Width = halfW;
                        videoBoxs[i].Height = actualH;
                    }
                    break;
                case 5:
                case 6:
                    double oneThirdW = canvasSize.Width / 3;
                    double actualH_ = (oneThirdW * 0.5625);
                    double actualY_ = halfH - actualH_;

                    for (int i = 0; i < visibleCount; i++)
                    {
                        videoBoxs[i].PosX = (i % 3) * oneThirdW;
                        videoBoxs[i].PosY = i < 3 ? actualY_ : halfH;
                        videoBoxs[i].Width = oneThirdW;
                        videoBoxs[i].Height = actualH_;
                    }

                    break;
            }

            return true;
        }
    }
}
