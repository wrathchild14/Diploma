using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace MLCar.Scripts
{
    public class CarAgent : Agent
    {
        [SerializeField] private TrackCheckpoints trackCheckpoints;
        [SerializeField] private Transform spawnPosition;
        private CarController _carController;

        protected override void Awake()
        {
            _carController = GetComponent<CarController>();
        }

        void Start()
        {
            trackCheckpoints.OnCarCorrectCheckpoint += TrackCheckpoints_OnCarCorrectCheckpoint;
            trackCheckpoints.OnCarWrongCheckpoint += TrackCheckpoints_OnCarWrongCheckpoint;
        }

        private void TrackCheckpoints_OnCarCorrectCheckpoint(object sender, TrackCheckpoints.CarCheckpointEventArgs e)
        {
            if (e.CarTransform == transform)
            {
                AddReward(1f);
            }
        }

        private void TrackCheckpoints_OnCarWrongCheckpoint(object sender, TrackCheckpoints.CarCheckpointEventArgs e)
        {
            if (e.CarTransform == transform)
            {
                AddReward(-1f);
            }
        }

        public override void OnEpisodeBegin()
        {
            transform.position =
                spawnPosition.position + new Vector3(Random.Range(-5f, +5f), 0.5f, Random.Range(-5f, +5f));
            transform.forward = spawnPosition.forward;
            trackCheckpoints.ResetCheckpoint(transform);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            var checkpointForward = trackCheckpoints.GetNextCheckpoint(transform).transform.forward;
            var directionDot = Vector3.Dot(transform.forward, checkpointForward);
            sensor.AddObservation(directionDot);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var forwardAmount = 0f;
            var turnAmount = 0f;

            forwardAmount = actions.DiscreteActions[0] switch
            {
                0 => 0f,
                1 => +1f,
                2 => -1f,
                _ => forwardAmount
            };

            turnAmount = actions.DiscreteActions[1] switch
            {
                0 => 0f,
                1 => -1f,
                2 => +1f,
                _ => turnAmount
            };

            _carController.SetInput(forwardAmount, turnAmount);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var forwardAction = 0;
            if (Input.GetKey(KeyCode.W)) forwardAction = 1;
            if (Input.GetKey(KeyCode.S)) forwardAction = 2;

            var turnAction = 0;
            if (Input.GetKey(KeyCode.A)) turnAction = 1;
            if (Input.GetKey(KeyCode.D)) turnAction = 2;

            var discreteActions = actionsOut.DiscreteActions;
            discreteActions[0] = forwardAction;
            discreteActions[1] = turnAction;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent<Wall>(out _))
            {
                AddReward(-0.5f);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.TryGetComponent<Wall>(out _))
            {
                AddReward(-0.1f);
            }
        }
    }
}