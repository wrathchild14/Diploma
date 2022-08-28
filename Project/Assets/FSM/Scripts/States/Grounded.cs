using UnityEngine;

namespace FSM.Scripts.States
{
    public class Grounded : BaseState
    {
        protected MovementSM sm;

        protected Grounded(string name, MovementSM stateMachine) : base(name, stateMachine)
        {
            sm = (MovementSM) this.StateMachine;
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            if (Input.GetKeyDown(KeyCode.Space))
                StateMachine.ChangeState(sm.JumpingState);
        }

    }
}
