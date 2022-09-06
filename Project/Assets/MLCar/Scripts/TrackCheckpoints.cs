using System;
using System.Collections.Generic;
using UnityEngine;

namespace MLCar.Scripts
{
    public class TrackCheckpoints : MonoBehaviour
    {
        public event EventHandler<CarCheckpointEventArgs> OnCarCorrectCheckpoint;
        public event EventHandler<CarCheckpointEventArgs> OnCarWrongCheckpoint;

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
                OnCarCorrectCheckpoint?.Invoke(this, new CarCheckpointEventArgs(carTransform));
            }
            else
            {
                Debug.Log("Wrong checkpoint");
                OnCarWrongCheckpoint?.Invoke(this, new CarCheckpointEventArgs(carTransform));
            }
        }

        public class CarCheckpointEventArgs : EventArgs
        {
            public readonly Transform CarTransform;

            public CarCheckpointEventArgs(Transform carTransform)
            {
                CarTransform = carTransform;
            }
        }

        public void ResetCheckpoint(Transform carTransform)
        {
            _nextCheckpointSingleIndexList[carTransformList.IndexOf(carTransform.parent)] = 0;
        }

        public CheckpointSingle GetNextCheckpoint(Transform carTransform)
        {
            var checkpoint =
                _checkpointSingleList[_nextCheckpointSingleIndexList[carTransformList.IndexOf(carTransform.parent)]];
            // Its heavy, but used in observation collection
            return checkpoint;
        }
    }
}