using System;
using System.Threading.Tasks;

namespace St.Common
{
    public interface IVisualizeShell
    {
        IntPtr GetShellHandle();
        void ShowShell();
        void HideShell();
        Task Logout();
        void StartingSdk();
        void FinishStartingSdk(bool succeeded, string msg);
        void SetSelectedMenu(string menuName);
        void SetTopMost(bool isTopMost);
    }
}
