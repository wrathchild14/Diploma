using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Assets.Scripts
{
    public class MoveToGoalAgent : Agent
    {
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private float _moveSpeed;

        public override void OnEpisodeBegin()
        {
            while (true)
            {
                var playerPosition = new Vector3(Random.Range(-12.6f, -7f), 0f, Random.Range(-6f, 0.8f));
                var goalPosition = new Vector3(Random.Range(-12.5f, -7f), 0f, Random.Range(-6f, 0.5f));

                if ((playerPosition - goalPosition).sqrMagnitude > 2.5f)
                {
                    transform.localPosition = playerPosition;
                    _targetTransform.localPosition = goalPosition;
                }
                else
                {
                    continue;
                }

                break;
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(transform.localPosition);
            sensor.AddObservation(_targetTransform.localPosition);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var moveX = actions.ContinuousActions[0];
            var moveZ = actions.ContinuousActions[1];

            transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * _moveSpeed;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActions = actionsOut.ContinuousActions;
            continuousActions[0] = Input.GetAxisRaw("Horizontal");
            continuousActions[1] = Input.GetAxisRaw("Vertical");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag($"Finish")) {
                SetReward(+1f);
                EndEpisode();
            }
            if (other.CompareTag($"Respawn")) {
                SetReward(-1f);
                EndEpisode();
            }
        }
    }
}
