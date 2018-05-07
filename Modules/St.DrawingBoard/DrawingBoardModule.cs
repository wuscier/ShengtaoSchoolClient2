using Prism.Modularity;
using Prism.Regions;
using St.Common;

namespace St.DrawingBoard
{
    public class DrawingBoardModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public DrawingBoardModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.DrawingRegion, typeof(DrawingBoardContentView));
        }
    }
}
