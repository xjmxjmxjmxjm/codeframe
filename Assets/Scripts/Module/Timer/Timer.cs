using System;
using System.Collections.Generic;
using System.Linq;

namespace Module.Timer
{
    /// <summary>
    /// 计时器接口
    /// </summary>
    public interface ITimer
    {
        /// <summary>
        /// 计时器唯一标识符
        /// </summary>
        string ID { get; }
        
        float CurrentTime { get; }
        float Percent { get; }
        float Duration { get; }
        bool Isloop { get; }
        bool IsComplete { get; }
        bool IsTiming { get; }

        void ResetData(string id, float duration, bool isloop);
        void Update();
        void Continue();
        void Pause();
        void Stop(bool isComplete);

        ITimer AddUpdateListener(Action onUpdate);
        ITimer AddCompleteListener(Action onComplete);
    }

    public interface ITimeManager
    {
        void Update();
        
        /// <summary>
        /// 创建计时器 若当前指定名称计时器正在计时 返回null 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="duration"></param>
        /// <param name="isloop"></param>
        /// <returns></returns>
        ITimer CreateTimer(string id, float duration, bool isloop);

        /// <summary>
        /// 重置指定id的计时器数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="duration"></param>
        /// <param name="isloop"></param>
        /// <returns></returns>
        ITimer ResetTimerData(string id, float duration, bool isloop);


        /// <summary>
        /// 指定id的timer为空 创建timer 不为null 重新启动timer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="duration"></param>
        /// <param name="isloop"></param>
        /// <returns></returns>
        ITimer CreateOrRestartTimer(string id, float duration, bool isloop);
        
        /// <summary>
        /// 通过标识获取timer
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ITimer GetTimer(string id);
        
        void ContinueAll();
        void PauseAll();
        void StopAll();


        void StopTimer(ITimer timer, bool isComplete);
    }


    /// <summary>
    /// 计时器管理器
    /// </summary>
    public class TimerManager : ITimeManager
    {
        /// <summary>
        /// 计时器
        /// </summary>
        private class Timer : ITimer
        {
            public string ID { get; private set; }

            /// <summary>
            /// 当前的时间
            /// </summary>
            public float CurrentTime
            {
                get { return _runTimeTotal; }
            }

            /// <summary>
            /// 运行百分比
            /// </summary>
            public float Percent
            {
                get { return _runTimeTotal / _duration; }
            }

            /// <summary>
            /// 单次循环持续时间
            /// </summary>
            public float Duration
            {
                get { return _duration; }
            }

            /// <summary>
            /// 是否完成
            /// </summary>
            public bool IsComplete { get; private set; }
            
            /// <summary>
            /// 是否循环执行
            /// </summary>
            public bool Isloop { get; private set; }



            private Action _onUpdate = null;
            private Action _onComplete = null;

            /// <summary>
            /// 是否正在计时
            /// </summary>
            public bool IsTiming { get; private set; }


            /// <summary>
            /// 计时开始时间
            /// </summary>
            private DateTime _startTime;

            /// <summary>
            /// 总运行时间
            /// </summary>
            private float _runTimeTotal = 0;

            /// <summary>
            /// 持续时间
            /// </summary>
            private float _duration = 0;

            /// <summary>
            /// 刷新间隔帧数
            /// </summary>
            private int _offsetFrame = 20;

            private int _frameTimes = 0;


            public Timer(string id, float duration, bool isloop)
            {
                InitData(id, duration, isloop);
            }


            private void InitData(string id, float duration, bool isloop)
            {
                ID = id;
                _duration = duration;
                Isloop = isloop;
                ResetData();
            }

            /// <summary>
            /// 重置数据
            /// </summary>
            /// <param name="duration"></param>
            /// <param name="isloop"></param>
            public void ResetData(string id, float duration, bool isloop)
            {
                InitData(id, duration, isloop);
            }

            private void ResetData()
            {
                IsTiming = true;
                _startTime = DateTime.Now;
                _runTimeTotal = 0;
                IsComplete = false;
                _onUpdate = null;
                _onComplete = null;
            }

            public void Update()
            {
                _frameTimes++;
                if (_frameTimes < _offsetFrame) return;
                _frameTimes = 0;
                
                if (!IsTiming || IsComplete) return;

                IsComplete = JudgeIsComplete();

                if (Isloop)
                {
                    Loop();
                }
                else
                {
                    NotLoop();
                }

                _onUpdate?.Invoke();
            }

            private void Loop()
            {
                if (IsComplete)
                {
                    IsComplete = false;
                    _onComplete?.Invoke();
                    ResetData();
                }
            }

            private void NotLoop()
            {
                if (IsComplete)
                {
                    _onComplete?.Invoke();
                    _onComplete = null;
                }
            }


            public void Continue()
            {
                if (IsTiming) return;
                IsTiming = true;
                _startTime = DateTime.Now;
            }

            public void Pause()
            {
                if (!IsTiming) return;
                IsTiming = false;
                _runTimeTotal += GetCurrentTimingTime();
            }

            public void Stop(bool isComplete)
            {
                if (IsComplete && isComplete)
                {
                    _onComplete?.Invoke();
                }
                _onComplete = null;
                _runTimeTotal = 0;
                IsTiming = false;
            }

            public ITimer AddUpdateListener(Action onUpdate)
            {
                _onUpdate += onUpdate;
                return this;
            }

            public ITimer AddCompleteListener(Action onComplete)
            {
                _onComplete += onComplete;
                return this;
            }

            private float GetCurrentTimingTime()
            {
                var timeSpan = DateTime.Now - _startTime;
                return (float) timeSpan.TotalSeconds;
            }

            /// <summary>
            /// 判断当前是否执行完毕
            /// </summary>
            /// <returns></returns>
            private bool JudgeIsComplete()
            {
                return (_runTimeTotal + GetCurrentTimingTime()) >= _duration;
            }
        }

        private List<ITimer> _activeTimers;
        private HashSet<ITimer> _inactiveTimers;
        private HashSet<ITimer>.Enumerator _tempActiveEnum;
        private Dictionary<string, ITimer> _timersDic;
        

        public TimerManager()
        {
            _activeTimers = new List<ITimer>();
            _inactiveTimers = new HashSet<ITimer>();
            _timersDic = new Dictionary<string, ITimer>();
        }

        /// <summary>
        /// 创建新Timer
        /// 返回 null 说明这个timer正在运作
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="isloop"></param>
        /// <returns></returns>
        public ITimer CreateTimer(string id, float duration, bool isloop)
        {
            ITimer timer = null;
            if (_timersDic.ContainsKey(id))
            {
                timer = _timersDic[id];
                if (!timer.IsTiming)
                {
                    ResetTimer(timer, id, duration, isloop);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (_inactiveTimers.Count > 0)
                {
                    timer = _inactiveTimers.First();
                    //_timersDic.Remove(timer.ID);
                    
                    ResetTimer(timer, id, duration, isloop);
                }
                else
                {
                    timer = new Timer(id, duration, isloop);
                    _activeTimers.Add(timer);
                    _timersDic.Add(id, timer);
                } 
                
            }
            timer.AddCompleteListener(() => TimerComplete(timer));
            return timer;
        }

        public ITimer ResetTimerData(string id, float duration, bool isloop)
        {
            var timer = GetTimer(id);
            if (timer != null)
            {
                if (timer.IsTiming)
                {
                    ResetTimer(timer, id, duration, isloop);
                }
                return timer;
            }

            return null;
        }

        public ITimer CreateOrRestartTimer(string id, float duration, bool isloop)
        {
            ITimer timer = CreateTimer(id, duration, isloop);
            if (timer == null)
            {
                return ResetTimerData(id, duration, isloop);
            }
            else
            {
                return timer;
            }
        }

        private void ResetTimer(ITimer timer, string id, float duration, bool isloop)
        {
            if (_inactiveTimers.Contains(timer))
            {
                _inactiveTimers.Remove(timer);
                _activeTimers.Add(timer);
            }
            timer.ResetData(id, duration, isloop);
        }

        public void StopTimer(ITimer timer, bool isComplete)
        {
            timer.Stop(isComplete);
            SetInActiveTimer(timer);
        }

        public ITimer GetTimer(string id)
        {
            ITimer timer = null;
            _timersDic.TryGetValue(id, out timer);
            return timer;
        }

        private void TimerComplete(ITimer timer)
        {
            if (!timer.Isloop)
            {
                SetInActiveTimer(timer);
            }
        }

        private void SetInActiveTimer(ITimer timer)
        {
            if (_activeTimers.Contains(timer))
            {
                _activeTimers.Remove(timer);
                _inactiveTimers.Add(timer);
            }
        }

        public void Update()
        {
            int count = _activeTimers.Count;
            if (count > 0)
            {

                for (int i = 0; i < _activeTimers.Count; i++)
                {
                    ITimer timer = _activeTimers[i];
                    if (timer != null)
                    {
                        timer.Update();
                    }
                    
                }
                /*_tempActiveEnum = _activeTimers.GetEnumerator();

                for (int i = 0; i < count; i++)
                {
                    if (_tempActiveEnum.Equals(null) || !_tempActiveEnum.MoveNext())
                    {
                        continue;
                    }
                    else
                    {
                        _tempActiveEnum.Current.Update();
                    }
                }*/
            }
        }

        
        /// <summary>
        /// 继续执行所有计时器
        /// </summary>
        public void ContinueAll()
        {
            foreach (Timer timer in _activeTimers)
            {
                timer.Continue();
            }
        }
        /// <summary>
        /// 暂停所有计时器
        /// </summary>
        public void PauseAll()
        {
            foreach (Timer timer in _activeTimers)
            {
                timer.Pause();
            }
        }
        /// <summary>
        /// 结束所有计时器
        /// </summary>
        public void StopAll()
        {
            foreach (Timer timer in _activeTimers)
            {
                StopTimer(timer, false);
            }
        }
    }
}