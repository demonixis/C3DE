using System;

namespace Gwen.Anim.Size
{
    class Width : TimedAnimation
    {
        private int m_StartSize;
        private int m_Delta;
        private bool m_Hide;

        public Width(int startSize, int endSize, float length, bool hide = false, float delay = 0.0f, float ease = 1.0f)
            : base(length, delay, ease)
        {
            m_StartSize = startSize;
            m_Delta = endSize - m_StartSize;
            m_Hide = hide;
        }

        protected override void OnStart()
        {
            base.OnStart();
            //m_Control.ActualWidth = m_StartSize;
        }

        protected override void Run(float delta)
        {
            base.Run(delta);
            //m_Control.ActualWidth = (int)Math.Round(m_StartSize + (m_Delta * delta));
        }

        protected override void OnFinish()
        {
            base.OnFinish();
            //m_Control.ActualWidth = m_StartSize + m_Delta;
            m_Control.IsHidden = m_Hide;
        }
    }
}
