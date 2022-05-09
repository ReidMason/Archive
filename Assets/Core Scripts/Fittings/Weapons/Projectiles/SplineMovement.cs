using UnityEngine;
using UnityEngine.Events;
using PathCreation;

namespace NoxCore.Fittings.Weapons
{
    public class SplineMovement : MonoBehaviour
    {
        protected SplineProjectile splineProjectile;
        public PathCreator path;
        public EndOfPathInstruction endPathInstruction = EndOfPathInstruction.Stop;
        public bool moveOnStart;
        public bool destroyOnComplete;
        public Transform root;
        public float speed;
        public AnimationCurve speedOverLife;
        public int maxLoops;
        public bool rotationFollowsMovement;

        protected PathCreator pathCreator;
        protected float distanceTravelled;
        protected Vector3 startPos;
        protected int numLoops;
        protected bool isReversing;

        protected UnityEvent reachedEndOfPath;
        protected bool active;
        protected bool rootMotion;

        void Awake()
        {
            splineProjectile = GetComponent<SplineProjectile>();

            if (path == null)
            {
                //Debug.Log("No path found, assign in inspector");
                Destroy(gameObject);
            }

            if (moveOnStart)
            {
                initMovement();
                startMovement();
            }
        }

        public virtual void initMovement()
        {
            reachedEndOfPath = new UnityEvent();
            reachedEndOfPath.AddListener(OnReachedEndOfPath);

            pathCreator = path.GetComponent<PathCreator>();
            startPos = transform.position - pathCreator.bezierPath.GetPoint(0);

            if (root == null)
            {
                rootMotion = false;
            }
            else
            {
                rootMotion = true;
            }

            switch (endPathInstruction)
            {
                case EndOfPathInstruction.Stop:
                    speedOverLife.postWrapMode = WrapMode.Clamp;
                    break;

                case EndOfPathInstruction.Loop:
                    speedOverLife.postWrapMode = WrapMode.Loop;
                    break;

                case EndOfPathInstruction.Reverse:
                    speedOverLife.postWrapMode = WrapMode.PingPong;
                    break;
            }
        }

        public void startMovement()
        {
            active = true;
        }

        public void stopMovement()
        {
            active = false;
        }

        void Update()
        {
            if (active)
            {
                //Distance
                float linearT = distanceTravelled / pathCreator.path.length;
                float scaledSpeed = speedOverLife.Evaluate(linearT);
                distanceTravelled += (speed * scaledSpeed) * Time.deltaTime;

                //Projectile Movement
                if (rootMotion && root != null)
                {
                    transform.position = (root.position - pathCreator.bezierPath.GetPoint(0)) + (splineProjectile.LaunchRotation * pathCreator.path.GetPointAtDistance(distanceTravelled, endPathInstruction));

                    // cache root position in case character dies
                    startPos = root.position;
                }
                else
                {
                    transform.position = startPos + (splineProjectile.LaunchRotation * pathCreator.path.GetPointAtDistance(distanceTravelled, endPathInstruction));
                }

                //Rotation
                if (rotationFollowsMovement)
                {
                    Vector3 direction = pathCreator.path.GetDirectionAtDistance(distanceTravelled);
                    if (isReversing)
                    {
                        direction *= -1;
                    }

                    transform.rotation = Quaternion.FromToRotation(Vector2.right, direction);
                }

                //End Path
                if (distanceTravelled >= (pathCreator.path.length * (numLoops + 1)))
                {
                    reachedEndOfPath.Invoke();
                }
            }
        }

        public virtual void OnReachedEndOfPath()
        {
            switch (endPathInstruction)
            {
                case EndOfPathInstruction.Loop:
                case EndOfPathInstruction.Reverse:
                    if (numLoops == maxLoops - 1)
                    {
                        if (destroyOnComplete)
                        {
                            Destroy(gameObject);
                        }
                        else
                        {
                            stopMovement();
                        }
                    }
                    else
                    {
                        numLoops++;
                    }
                    if (endPathInstruction == EndOfPathInstruction.Reverse)
                    {
                        isReversing = !isReversing;
                    }

                    break;
                case EndOfPathInstruction.Stop:
                    if (destroyOnComplete)
                    {
                        Destroy(gameObject);
                    }
                    break;
            }
        }
    }
}