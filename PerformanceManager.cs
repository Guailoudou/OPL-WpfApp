using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using userdata;

namespace OPL_WpfApp.Utils
{
    /// <summary>
    /// 后台资源管家：
    /// - 窗口最小化 / 隐藏到托盘时挂起动画、暂缓界面刷新，并回收工作集内存（目标 &lt; 10MB）
    /// - 窗口恢复时唤醒渲染、补刷日志
    /// - 提供按“运行模式 + 可见性”自适应的轮询间隔
    /// - 可选联机期间阻止系统休眠
    /// 仅影响界面与刷新节奏，不干预任何联机核心逻辑。
    /// </summary>
    public static class PerformanceManager
    {
        #region Win32

        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool K32EmptyWorkingSet(IntPtr hProcess);

        [DllImport("kernel32.dll")]
        private static extern uint SetThreadExecutionState(uint esFlags);

        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_SYSTEM_REQUIRED = 0x00000001;
        private const uint ES_DISPLAY_REQUIRED = 0x00000002;

        #endregion

        private static Window _window;
        private static DispatcherTimer _trimTimer;
        private static bool _isBackground;

        /// <summary>界面当前是否可见可交互（轮询节流依据）</summary>
        public static bool UiVisible { get; private set; } = true;

        /// <summary>启动接管：挂接窗口生命周期事件</summary>
        public static void Attach(Window window)
        {
            _window = window;
            _window.StateChanged += (s, e) =>
            {
                if (_window.WindowState == WindowState.Minimized)
                    EnterBackground(trim: settings.Safe.TrimOnMinimize);
                else
                    LeaveBackground();
            };
            _window.IsVisibleChanged += (s, e) =>
            {
                if (_window.IsVisible) LeaveBackground();
                else EnterBackground(trim: settings.Safe.AggressiveTrim);
            };
            _window.Deactivated += (s, e) =>
            {
                // 失焦但仍旧可见：只降不升，仅挂起循环动画以省电
                if (UiVisible && _window.WindowState == WindowState.Normal)
                    MotionHelper.SuspendLoops();
            };
            _window.Activated += (s, e) =>
            {
                if (UiVisible) MotionHelper.ResumeLoops();
            };

            ApplyKeepAwake(settings.Safe.KeepAwake);
        }

        /// <summary>进入后台：挂起动画，可选延时回收内存</summary>
        public static void EnterBackground(bool trim)
        {
            if (_isBackground) return;
            _isBackground = true;
            UiVisible = false;
            // 日志后台缓冲策略受设置控制（关闭时依旧实时刷新界面）
            Logger.UiVisible = !settings.Safe.AutoHideLogOnBackground;
            MotionHelper.SuspendLoops();

            if (trim)
            {
                // 等 WPF 完成隐藏渲染后再回收，避免边渲染边释放
                GetTrimTimer().Start();
            }
        }

        /// <summary>回到前台：恢复动画与日志刷新，补刷缓冲日志</summary>
        public static void LeaveBackground()
        {
            if (!_isBackground) return;
            _isBackground = false;
            UiVisible = true;
            if (_trimTimer != null) _trimTimer.Stop();
            Logger.UiVisible = true;
            Logger.FlushPending();
            if (_window != null && _window.WindowState == WindowState.Normal && _window.IsActive)
                MotionHelper.ResumeLoops();
        }

        private static DispatcherTimer GetTrimTimer()
        {
            if (_trimTimer == null)
            {
                _trimTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
                _trimTimer.Tick += (s, e) =>
                {
                    _trimTimer.Stop();
                    if (_isBackground) TrimMemory();
                };
            }
            return _trimTimer;
        }

        /// <summary>
        /// 深度回收：完整 GC 两遍 + 清空工作集，
        /// 让托盘驻留时的物理内存占用降到极低水平（&lt;10MB）。
        /// </summary>
        public static void TrimMemory()
        {
            try
            {
                Logger.FlushPending();
                GC.Collect(2, GCCollectionMode.Forced, true);
                GC.WaitForPendingFinalizers();
                GC.Collect(2, GCCollectionMode.Forced, true);
                using (var p = Process.GetCurrentProcess())
                {
                    K32EmptyWorkingSet(p.Handle);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("[性能] 内存回收失败：" + ex.Message, "警告");
            }
        }

        /// <summary>
        /// 按运行模式与可见性自适应轮询间隔秒数（下限 1s，仅影响界面刷新节奏）。
        /// </summary>
        public static int ScaleSeconds(int baseSeconds)
        {
            if (baseSeconds < 1) baseSeconds = 1;
            double factor = settings.Safe.PollFactor;
            if (!UiVisible) factor = Math.Max(factor, 1) * 4; // 后台时轮询大幅放缓
            int seconds = (int)Math.Round(baseSeconds * factor);
            return seconds < 1 ? 1 : seconds;
        }

        /// <summary>联机/组网运行期间是否保持系统唤醒</summary>
        public static void ApplyKeepAwake(bool enable)
        {
            try
            {
                SetThreadExecutionState(enable
                    ? ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED
                    : ES_CONTINUOUS);
            }
            catch { }
        }
    }
}
