using UnityEngine;

namespace MLCar.Scripts
{
    public class CheckpointSingle : MonoBehaviour
    {
        private TrackCheckpoints _trackCheckpoints;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<CarController>(out _))
            {
                _trackCheckpoints.CarThroughCheckpoint(this, other.transform);
            }
        }

        public void SetTrackCheckpoints(TrackCheckpoints trackCheckpoints)
        {
            _trackCheckpoints = trackCheckpoints;
        }
    }
}