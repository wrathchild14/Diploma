using UnityEngine;

namespace FSM.Scripts
{
    public class StateMachine : MonoBehaviour
    {
        private BaseState _currentState;

        void Start()
        {
            _currentState = GetInitialState();
            if (_currentState != null)
                _currentState.Enter();
        }

        void Update()
        {
            if (_currentState != null)
                _currentState.UpdateLogic();
        }

        void LateUpdate()
        {
            if (_currentState != null)
                _currentState.UpdatePhysics();
        }

        protected virtual BaseState GetInitialState()
        {
            return null;
        }

        public void ChangeState(BaseState newState)
        {
            _currentState.Exit();

            _currentState = newState;
            newState.Enter();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10f, 10f, 200f, 100f));
            var content = _currentState != null ? _currentState.Name : "(no current state)";
            GUILayout.Label($"<color='black'><size=40>{content}</size></color>");
            GUILayout.EndArea();
        }
    }
}
