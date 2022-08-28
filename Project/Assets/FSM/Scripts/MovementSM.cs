using FSM.Scripts.States;
using UnityEngine;

namespace FSM.Scripts
{
    public class MovementSM : StateMachine
    {
        public float speed = 4f;
        public float jumpForce = 14f;
        public new Rigidbody2D rigidbody;
        public SpriteRenderer spriteRenderer;

        [HideInInspector]
        public Idle IdleState;
        [HideInInspector]
        public Moving MovingState;
        [HideInInspector]
        public Jumping JumpingState;

        private void Awake()
        {
            IdleState = new Idle(this);
            MovingState = new Moving(this);
            JumpingState = new Jumping(this);
        }

        protected override BaseState GetInitialState()
        {
            return IdleState;
        }
    }
}
