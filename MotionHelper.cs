using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OPL_WpfApp.Utils
{
    /// <summary>
    /// 全站动效基础设施：统一缓动曲线、页面过场编排、状态光环，
    /// 并支持在窗口不可见 / 用户关闭动画时自动挂起，避免后台消耗 GPU/CPU。
    /// </summary>
    public static class MotionHelper
    {
        // 统一时长标准（毫秒）
        public const int FastMs = 150;
        public const int NormalMs = 260;
        public const int SlowMs = 420;
        public const int StaggerMs = 55; // 卡片错峰入场间隔

        private static readonly List<Storyboard> _looping = new List<Storyboard>();
        private static bool _suspended;

        /// <summary>动画总开关（设置项 + 系统"减少动画"偏好）</summary>
        public static bool AnimationsEnabled { get; set; } = true;

        /// <summary>实际是否播放动画：兼顾系统无障碍设置</summary>
        public static bool Effective => AnimationsEnabled && SystemParameters.ClientAreaAnimation;

        /// <summary>标准缓出曲线</summary>
        public static IEasingFunction EaseOut => new QuinticEase { EasingMode = EasingMode.EaseOut };

        /// <summary>回弹缓动（用于品牌入场等）</summary>
        public static IEasingFunction Spring => new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.35 };

        /// <summary>
        /// 对元素执行淡入 + 上移动画（一次性）。
        /// </summary>
        public static void FadeSlideIn(FrameworkElement element, double delaySeconds = 0,
                                       double slideFrom = 14, int durationMs = NormalMs)
        {
            if (element == null) return;
            if (!Effective)
            {
                element.Opacity = 1;
                return;
            }

            var offset = element.RenderTransform as TranslateTransform;
            if (offset == null)
            {
                offset = new TranslateTransform();
                element.RenderTransform = offset;
            }

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(durationMs))
            { EasingFunction = EaseOut };
            var slide = new DoubleAnimation(slideFrom, 0, TimeSpan.FromMilliseconds(durationMs))
            { EasingFunction = EaseOut };
            if (delaySeconds > 0)
            {
                fade.BeginTime = TimeSpan.FromSeconds(delaySeconds);
                slide.BeginTime = TimeSpan.FromSeconds(delaySeconds);
            }
            // 注意：先设置目标属性再 Freeze，避免在已冻结对象上修改属性导致崩溃
            fade.Freeze();
            slide.Freeze();

            element.BeginAnimation(UIElement.OpacityProperty, fade);
            offset.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        /// <summary>
        /// 页面切换过场：整页一次性淡入上移。
        /// 注意：不对页内卡片做错峰级联动画——父子层透明度嵌套叠加会导致
        /// “闪一下后反复入场”的视觉干扰，一次切换只允许一条时间线。
        /// </summary>
        public static void PlayPageTransition(FrameworkElement pageRoot)
        {
            if (pageRoot == null) return;
            if (!Effective)
            {
                pageRoot.Opacity = 1;
                return;
            }
            FadeSlideIn(pageRoot, 0, 10, NormalMs);
        }

        /// <summary>
        /// 环绕状态点呼吸光环：为目标 Ellipse 添加脉冲扩散圈，
        /// 光环颜色自动跟随状态点填充色。返回的 Storyboard 会被登记，
        /// 窗口不可见时通过 <see cref="SuspendLoops"/> 统一挂起。
        /// </summary>
        public static void AttachHalo(Panel container, Ellipse target)
        {
            if (container == null || target == null) return;

            for (int ring = 0; ring < 2; ring++)
            {
                var halo = new Ellipse
                {
                    Width = target.Width,
                    Height = target.Height,
                    StrokeThickness = 1.4,
                    Opacity = 0,
                    IsHitTestVisible = false,
                    HorizontalAlignment = target.HorizontalAlignment,
                    VerticalAlignment = target.VerticalAlignment,
                    Margin = target.Margin,
                };
                halo.SetBinding(Shape.StrokeProperty, new System.Windows.Data.Binding("Fill")
                {
                    Source = target,
                    Mode = System.Windows.Data.BindingMode.OneWay,
                });
                var scale = new ScaleTransform(1, 1);
                halo.RenderTransform = scale;
                halo.RenderTransformOrigin = new Point(0.5, 0.5);
                container.Children.Insert(0, halo);

                if (!Effective) continue;

                var sb = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
                var opacity = new DoubleAnimationUsingKeyFrames
                {
                    BeginTime = TimeSpan.FromMilliseconds(ring * 900),
                };
                opacity.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                opacity.KeyFrames.Add(new LinearDoubleKeyFrame(0.5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150))));
                opacity.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1800))));

                // 顺序要求：先设置 Target/属性路径与延时，最后才能 Freeze（对已冻结对象赋值会抛异常）
                var pulseX = new DoubleAnimation(1, 2.6, TimeSpan.FromMilliseconds(1800))
                {
                    EasingFunction = EaseOut,
                    BeginTime = TimeSpan.FromMilliseconds(ring * 900),
                };
                Storyboard.SetTarget(pulseX, halo);
                Storyboard.SetTargetProperty(pulseX, new PropertyPath("RenderTransform.ScaleX"));
                pulseX.Freeze();

                var pulseY = new DoubleAnimation(1, 2.6, TimeSpan.FromMilliseconds(1800))
                {
                    EasingFunction = EaseOut,
                    BeginTime = TimeSpan.FromMilliseconds(ring * 900),
                };
                Storyboard.SetTarget(pulseY, halo);
                Storyboard.SetTargetProperty(pulseY, new PropertyPath("RenderTransform.ScaleY"));
                pulseY.Freeze();

                Storyboard.SetTarget(opacity, halo);
                Storyboard.SetTargetProperty(opacity, new PropertyPath(UIElement.OpacityProperty));
                opacity.Freeze();

                sb.Children.Add(pulseX);
                sb.Children.Add(pulseY);
                sb.Children.Add(opacity);
                sb.Freeze();
                sb.Begin();
                _looping.Add(sb);
            }
        }

        /// <summary>挂起所有循环动画（窗口最小化/隐藏时调用，停止消耗渲染资源）</summary>
        public static void SuspendLoops()
        {
            if (_suspended) return;
            _suspended = true;
            foreach (var sb in _looping)
            {
                try { sb.Pause(); } catch { }
            }
        }

        /// <summary>恢复循环动画（窗口重新可见时调用）</summary>
        public static void ResumeLoops()
        {
            if (!_suspended) return;
            _suspended = false;
            foreach (var sb in _looping)
            {
                try { sb.Resume(); } catch { }
            }
        }

        /// <summary>设置变更后重建光环等循环动画（简单处理：下次启动生效则无需调用）</summary>
        public static bool LoopsSuspended => _suspended;
    }
}
