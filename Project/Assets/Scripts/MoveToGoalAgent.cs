using StarterAssets;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UIElements;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float moveSpeed;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-12.6f, -7f), 0f, Random.Range(-6f, 0.8f));
        targetTransform.localPosition = new Vector3(Random.Range(-12.5f, -7f), 0f, Random.Range(-6f, 0.5f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var moveX = actions.ContinuousActions[0];
        var moveZ = actions.ContinuousActions[1];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
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
