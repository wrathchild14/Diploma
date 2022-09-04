using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Flocking.Scripts
{
    public class Flocking : MonoBehaviour
    {
        [SerializeField]
        private Vector3 baseRotation;

        [Range(0, 10)]
        [SerializeField]
        private float maxSpeed = 1f;

        [Range(.1f, .5f)]
        [SerializeField]
        private float maxForce = .03f;

        [Range(1, 10)]
        [SerializeField]
        private float neighborhoodRadius = 3f;

        [Range(0, 3)]
        [SerializeField]
        private float separationAmount = 1f;

        [Range(0, 3)]
        [SerializeField]
        private float cohesionAmount = 1f;

        [Range(0, 3)]
        [SerializeField]
        private float alignmentAmount = 1f;

        private Vector2 _acceleration;
        public Vector2 velocity; // Public, because we need access to other boids

        private Vector2 Position {
            get => gameObject.transform.position;
            set => gameObject.transform.position = value;
        }

        private void Start()
        {
            // Random angle and velocity to the boid 
            var angle = Random.Range(0, 2 * Mathf.PI);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle) + baseRotation);
            velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        private void Update()
        {
            var boidColliders = Physics2D.OverlapCircleAll(Position, neighborhoodRadius);
            var boids = boidColliders.Select(o => o.GetComponent<Flocking>()).ToList();
            boids.Remove(this);

            Flock(boids); // Takes care of the acceleration of the boid (with the rules of flocking)
            UpdateVelocity();
            UpdatePosition();
            UpdateRotation();
            WrapAround();
        }

        private void Flock(IEnumerable<Flocking> boids)
        {
            var alignment = Alignment(boids); // Velocity (average) of the flock
            var separation = Separation(boids); // Direction away of the flock
            var cohesion = Cohesion(boids); // Direction towards the average of the flock

            _acceleration = alignmentAmount * alignment + cohesionAmount * cohesion + separationAmount * separation;
        }

        private void UpdateVelocity()
        {
            velocity += _acceleration;
            velocity = LimitMagnitude(velocity, maxSpeed);
        }

        private void UpdatePosition()
        {
            Position += velocity * Time.deltaTime;
        }

        private void UpdateRotation()
        {
            var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle) + baseRotation);
        }

        private Vector2 Alignment(IEnumerable<Flocking> boids)
        {
            var velocity = Vector2.zero;
            if (!boids.Any()) return velocity;

            foreach (var boid in boids)
            {
                velocity += boid.velocity;
            }
            velocity /= boids.Count();

            var steer = Steer(velocity.normalized * maxSpeed);
            return steer;
        }

        private Vector2 Cohesion(IEnumerable<Flocking> boids)
        {
            if (!boids.Any()) return Vector2.zero;

            var sumPositions = Vector2.zero;
            foreach (var boid in boids)
            {
                sumPositions += boid.Position;
            }
            var average = sumPositions / boids.Count();
            var direction = average - Position;

            var steer = Steer(direction.normalized * maxSpeed);
            return steer;
        }

        private Vector2 Separation(IEnumerable<Flocking> boids)
        {
            var direction = Vector2.zero;
            boids = boids.Where(o => DistanceTo(o) <= neighborhoodRadius / 2);
            if (!boids.Any()) return direction;

            foreach (var boid in boids)
            {
                var difference = Position - boid.Position; // Calculate vector pointing away from neighbor
                direction += difference.normalized / difference.magnitude; // Weight by distance
            }
            direction /= boids.Count();

            var steer = Steer(direction.normalized * maxSpeed);
            return steer;
        }

        private Vector2 Steer(Vector2 desired)
        {
            var steer = desired - velocity;
            steer = LimitMagnitude(steer, maxForce);

            return steer;
        }

        private float DistanceTo(Flocking boid)
        {
            return Vector3.Distance(boid.transform.position, Position);
        }

        private static Vector2 LimitMagnitude(Vector2 baseVector, float maxMagnitude)
        {
            if (baseVector.sqrMagnitude > maxMagnitude * maxMagnitude)
            {
                baseVector = baseVector.normalized * maxMagnitude;
            }
            return baseVector;
        }

        private void WrapAround()
        {
            if (Position.x < -14) Position = new Vector2(14, Position.y);
            if (Position.y < -8) Position = new Vector2(Position.x, 8);
            if (Position.x > 14) Position = new Vector2(-14, Position.y);
            if (Position.y > 8) Position = new Vector2(Position.x, -8);
        }
    }
}