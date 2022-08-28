using UnityEngine;

namespace FSM.Scripts.States
{
    public class Idle : Grounded
    {
        private float _horizontalInput;

        public Idle (MovementSM stateMachine) : base("Idle", stateMachine) {}

        public override void Enter()
        {
            base.Enter();
            sm.spriteRenderer.color = Color.white;
            _horizontalInput = 0f;
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            _horizontalInput = Input.GetAxis("Horizontal");
            if (Mathf.Abs(_horizontalInput) > Mathf.Epsilon)
                StateMachine.ChangeState(sm.MovingState);
        }

    }
}
