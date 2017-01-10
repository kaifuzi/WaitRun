﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace AhDung.WinForm
{
    /// <summary>
    /// 带等待窗体的BackgroundWorker。报告进度用一组UI操作方法
    /// </summary>
    public class BackgroundWorkerUI : BackgroundWorker
    {
        readonly IWaitForm _waitForm;

        //用于在等待窗体ShowDialog后伺机关闭它
        //之所以不直接在OnRunWorkerCompleted中使用关闭，是因为该事件跑在Invoke里，
        //这样关闭的窗体会影响窗口链，如会导致在该事件中后续弹出的模式窗体不“模式”，
        //又或者其它程序的窗口会跳到前排来，应该是Invoke机制到底跟直接调用有所不同
        readonly System.Windows.Forms.Timer _timer;

        #region 一组操作等候窗体UI的属性/方法

        /// <summary>
        /// 获取或设置进度描述
        /// </summary>
        public string WorkMessage
        {
            set
            {
                if (_waitForm.InvokeRequired)
                {
                    _waitForm.BeginInvoke(new Action(() => _waitForm.WorkMessage = value));
                    return;
                }
                _waitForm.WorkMessage = value;
            }
        }

        /// <summary>
        /// 获取或设置进度条可见性
        /// </summary>
        public bool BarVisible
        {
            set
            {
                if (_waitForm.InvokeRequired)
                {
                    _waitForm.BeginInvoke(new Action(() => _waitForm.BarVisible = value));
                    return;
                }
                _waitForm.BarVisible = value;
            }
        }

        /// <summary>
        /// 获取或设置进度条动画样式
        /// </summary>
        public ProgressBarStyle BarStyle
        {
            set
            {
                if (_waitForm.InvokeRequired)
                {
                    _waitForm.BeginInvoke(new Action(() => _waitForm.BarStyle = value));
                    return;
                }
                _waitForm.BarStyle = value;
            }
        }

        /// <summary>
        /// 获取或设置进度值
        /// </summary>
        public int BarValue
        {
            get
            {
                if (_waitForm.InvokeRequired)
                {
                    return Convert.ToInt32(_waitForm.Invoke(new Func<int>(() => _waitForm.BarValue)));
                }
                return _waitForm.BarValue;
            }
            set
            {
                if (_waitForm.InvokeRequired)
                {
                    _waitForm.BeginInvoke(new Action(() => _waitForm.BarValue = value));
                    return;
                }
                _waitForm.BarValue = value;
            }
        }

        /// <summary>
        /// 获取或设置进度条步进值
        /// </summary>
        public int BarStep
        {
            set
            {
                if (_waitForm.InvokeRequired)
                {
                    _waitForm.BeginInvoke(new Action(() => _waitForm.BarStep = value));
                    return;
                }
                _waitForm.BarStep = value;
            }
        }

        /// <summary>
        /// 使进度条步进
        /// </summary>
        public void BarPerformStep()
        {
            if (_waitForm.InvokeRequired)
            {
                _waitForm.BeginInvoke(new Action(() => _waitForm.BarPerformStep()));
                return;
            }
            _waitForm.BarPerformStep();
        }

        /// <summary>
        /// 获取或设置进度条上限值
        /// </summary>
        public int BarMaximum
        {
            set
            {
                if (_waitForm.InvokeRequired)
                {
                    _waitForm.BeginInvoke(new Action(() => _waitForm.BarMaximum = value));
                    return;
                }
                _waitForm.BarMaximum = value;
            }
        }

        /// <summary>
        /// 获取或设置进度条下限值
        /// </summary>
        public int BarMinimum
        {
            set
            {
                if (_waitForm.InvokeRequired)
                {
                    _waitForm.BeginInvoke(new Action(() => _waitForm.BarMinimum = value));
                    return;
                }
                _waitForm.BarMinimum = value;
            }
        }

        /// <summary>
        /// 获取或设置取消任务的控件的可见性
        /// </summary>
        public bool CancelControlVisible
        {
            set
            {
                if (_waitForm.InvokeRequired)
                {
                    _waitForm.BeginInvoke(new Action(() => _waitForm.CancelControlVisible = value));
                    return;
                }
                _waitForm.CancelControlVisible = value;
            }
        }

        #endregion

        /// <summary>
        /// 初始化组件
        /// </summary>
        public BackgroundWorkerUI()
            : this(new WaitForm())
        { }

        /// <summary>
        /// 初始化组件并指定等待窗体
        /// </summary>
        /// <param name="fmWait">等待窗体</param>
        public BackgroundWorkerUI(IWaitForm fmWait)
        {
            if (fmWait == null)
            {
                throw new ArgumentNullException();
            }
            _waitForm = fmWait;

            //轮询异步任务完成情况，完成后隐藏等待窗体
            _timer = new System.Windows.Forms.Timer { Interval = 500 };
            _timer.Tick += (S, E) =>
            {
                if (!IsBusy)
                {
                    _timer.Stop();
                    _waitForm.Hide();
                }
            };
        }

        /// <summary>
        /// 开始执行后台操作
        /// </summary>
        /// <param name="argument">要在DoWork事件处理程序中使用的参数</param>
        /// <remarks>通过可选参数可以同时覆盖基类无参RunWorkerAsync</remarks>
        public new void RunWorkerAsync(object argument = null)
        {
            _waitForm.CancelControlVisible = this.WorkerSupportsCancellation;
            _waitForm.CancelPending = false;//应考虑该方法是可能重复进入的

            base.RunWorkerAsync(argument);
            _timer.Start();

            Thread.Sleep(50); //所以先给异步50ms，如果它在此时间内完成，下面就不会再弹窗
            if (IsBusy)
            {
                _waitForm.ShowDialog();
            }
        }

        /// <summary>
        /// 指示是否已请求取消任务
        /// </summary>
        public new bool CancellationPending
        {
            get
            {
                return base.CancellationPending
                || (_waitForm != null && _waitForm.CancelPending);//公共方法需判空
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _waitForm.Dispose();
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
