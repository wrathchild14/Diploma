using System.Collections.Generic;
using UnityEngine;

namespace MLCar.Scripts
{
    public class TrackCheckpoints : MonoBehaviour
    {
        [SerializeField] private List<Transform> carTransformList;
        private List<CheckpointSingle> _checkpointSingleList;
        private List<int> _nextCheckpointSingleIndexList;

        private void Awake()
        {
            _checkpointSingleList = new List<CheckpointSingle>();
            foreach (Transform checkpointSingleTransform in transform)
            {
                var checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();
                checkpointSingle.SetTrackCheckpoints(this);

                _checkpointSingleList.Add(checkpointSingle);
            }

            _nextCheckpointSingleIndexList = new List<int>();
            foreach (var unused in carTransformList)
            {
                Debug.Log(unused);
                _nextCheckpointSingleIndexList.Add(0);
            }
        }

        public void CarThroughCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform)
        {
            var nextCheckpointSingleIndex =
                _nextCheckpointSingleIndexList[
                    carTransformList.IndexOf(carTransform.parent)]; // Is parent because I have my script at MainBody
            if (_checkpointSingleList.IndexOf(checkpointSingle) == nextCheckpointSingleIndex)
            {
                Debug.Log("Correct checkpoint: " + checkpointSingle.name);
                _nextCheckpointSingleIndexList[carTransformList.IndexOf(carTransform.parent)] =
                    (nextCheckpointSingleIndex + 1) % _checkpointSingleList.Count; // For multiple laps
            }
            else
            {
                Debug.Log("Wrong checkpoint");
            }
        }
    }
}