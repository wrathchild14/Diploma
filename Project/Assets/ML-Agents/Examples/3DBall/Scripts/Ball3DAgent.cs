using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.ML_Agents.Examples._3DBall.Scripts
{
    public class Ball3DAgent : Agent
    {
        [Header("Specific to Ball3D")]
        public GameObject Ball;
        [Tooltip("Whether to use vector observation. This option should be checked " +
                 "in 3DBall scene, and unchecked in Visual3DBall scene. ")]
        public bool UseVecObs;

        private Rigidbody _mBallRb;
        private EnvironmentParameters _mResetParams;

        public override void Initialize()
        {
            _mBallRb = Ball.GetComponent<Rigidbody>();
            _mResetParams = Academy.Instance.EnvironmentParameters;
            SetResetParameters();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            if (UseVecObs)
            {
                sensor.AddObservation(gameObject.transform.rotation.z);
                sensor.AddObservation(gameObject.transform.rotation.x);
                sensor.AddObservation(Ball.transform.position - gameObject.transform.position);
                sensor.AddObservation(_mBallRb.velocity);
            }
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var actionZ = 2f * Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
            var actionX = 2f * Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

            if ((gameObject.transform.rotation.z < 0.25f && actionZ > 0f) ||
                (gameObject.transform.rotation.z > -0.25f && actionZ < 0f))
            {
                gameObject.transform.Rotate(new Vector3(0, 0, 1), actionZ);
            }

            if ((gameObject.transform.rotation.x < 0.25f && actionX > 0f) ||
                (gameObject.transform.rotation.x > -0.25f && actionX < 0f))
            {
                gameObject.transform.Rotate(new Vector3(1, 0, 0), actionX);
            }
            if (Ball.transform.position.y - gameObject.transform.position.y < -2f ||
                Mathf.Abs(Ball.transform.position.x - gameObject.transform.position.x) > 3f ||
                Mathf.Abs(Ball.transform.position.z - gameObject.transform.position.z) > 3f)
            {
                SetReward(-1f);
                EndEpisode();
            }
            else
            {
                SetReward(0.1f);
            }
        }

        public override void OnEpisodeBegin()
        {
            gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
            gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
            gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
            _mBallRb.velocity = new Vector3(0f, 0f, 0f);
            Ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 4f, Random.Range(-1.5f, 1.5f))
                                      + gameObject.transform.position;
            //Reset the parameters when the Agent is reset.
            SetResetParameters();
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = -Input.GetAxis("Horizontal");
            continuousActionsOut[1] = Input.GetAxis("Vertical");
        }

        public void SetBall()
        {
            //Set the attributes of the ball by fetching the information from the academy
            _mBallRb.mass = _mResetParams.GetWithDefault("mass", 1.0f);
            var scale = _mResetParams.GetWithDefault("scale", 1.0f);
            Ball.transform.localScale = new Vector3(scale, scale, scale);
        }

        public void SetResetParameters()
        {
            SetBall();
        }
    }
}
