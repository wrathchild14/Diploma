﻿using UnityEngine;

namespace FSM.Scripts.States
{
    public class Jumping : BaseState
    {
        private readonly MovementSM _sm;

        private bool _grounded;
        private const int _groundLayer = 1 << 6;

        public Jumping(StateMachine stateMachine) : base("Jumping", stateMachine)
        {
            _sm = (MovementSM)this.StateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            _sm.spriteRenderer.color = Color.green;

            var vel = _sm.rigidbody.velocity;
            vel.y += _sm.jumpForce;
            _sm.rigidbody.velocity = vel;
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            if (_grounded)
                StateMachine.ChangeState(_sm.IdleState);
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();
            _grounded = _sm.rigidbody.velocity.y < Mathf.Epsilon && _sm.rigidbody.IsTouchingLayers(_groundLayer);
        }

    }
}
