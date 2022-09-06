using UnityEngine;

namespace MLCar
{
    public class CarController : MonoBehaviour
    {
        private float _moveInput;
        private float _turnInput;
        [SerializeField]
        private float forwardSpeed;
        [SerializeField]
        private float revSpeed;
        [SerializeField]
        private float turnSpeed;    
        
        [SerializeField]
        private Rigidbody rigidBody;
        
        private void Update()
        {
            // SetInput(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"));
        }

        public void SetInput(float moveInput, float turnInput)
        {
            _moveInput = moveInput;
            _turnInput = turnInput;
            _moveInput *= _moveInput > 0 ? forwardSpeed : revSpeed;
            transform.position = rigidBody.transform.position;
            var newRotation = _turnInput * turnSpeed * Time.deltaTime * _moveInput;
            transform.Rotate(0, newRotation, 0 , Space.World);
        }

        private void FixedUpdate()
        {
            rigidBody.AddForce(transform.forward * _moveInput, ForceMode.Acceleration);
        }
    }
}
