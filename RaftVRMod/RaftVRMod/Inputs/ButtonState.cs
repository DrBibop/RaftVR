using System;

namespace RaftVR.Inputs
{
    public class ButtonState
    {
        public bool state 
        { 
            get => _state;
            protected set
            {
                _lastState = _state;
                _state = value;
            }
        }
        public bool justPressed => state && !_lastState;
        public bool justReleased => !state && _lastState;

        private bool _state = false;
        private bool _lastState = false;

        private Func<bool> stateRetriever;

        public ButtonState(Func<bool> stateRetriever)
        {
            this.stateRetriever = stateRetriever;
        }

        public void Update()
        {
            state = stateRetriever.Invoke();
        }
    }
}
