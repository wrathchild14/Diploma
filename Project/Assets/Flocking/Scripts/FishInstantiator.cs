using UnityEngine;

namespace Flocking.Scripts
{
    public class FishInstantiator : MonoBehaviour
    {
        public GameObject fishPrefab;

        [Range(0, 300)]
        public int number;

        private void Start()
        {
            for (var i = 0; i < number; i++)
            {
                Instantiate(fishPrefab, Vector3.zero, Quaternion.identity);
            }
        }
    }
}