using UnityEngine;

using NoxCore.GUIs;
using NoxCore.Managers;
using NoxCore.Utilities;

namespace NoxCore.Cameras
{
	public class TopDown_Camera : MonoBehaviour 
	{        
        public string acquireCameraTarget;
		public Transform followTarget;
		protected Transform lastKillBy;
		public Vector3 targetOffset;
        protected float minZ, maxZ;
		public float zoomSensitivity;
        public float minLockDistance;

        [ShowOnlyAttribute]
		public float freeMoveSpeed;
		private Vector3 newPos;

        public bool lockedOn;
        protected float lockedFactor = 1.0f;

		protected Camera _cam;
        public Camera Cam { get { return _cam; } set { _cam = value; } }

        protected bool camInitialised;

        /// <summary>
        ///   Provide singleton support for this class.
        ///   The script must still be attached to a game object, but this will allow it to be called
        ///   from anywhere without specifically identifying that game object.
        /// </summary>
        private static TopDown_Camera instance;
        public static TopDown_Camera Instance { get { return instance; } set { instance = value; } }

        public void reset()
        {
            instance = this;
            Cam = instance.GetComponent<Camera>();
        }

        public void init()
		{
            Cam = instance.GetComponent<Camera>();

            Cam.transparencySortMode = TransparencySortMode.Orthographic;

            minZ = -Cam.nearClipPlane;
            maxZ = -Cam.farClipPlane;

            if (followTarget == null)
            {
                if (acquireCameraTarget != null)
                {
                    GameObject initTarget = GameObject.Find(acquireCameraTarget);

                    if (initTarget != null)
                    {
                        newPos = initTarget.transform.position + targetOffset;
                    }
                    else
                    {
                        newPos = gameObject.transform.position + targetOffset;
                    }
                }
                else
                {
                    newPos = gameObject.transform.position + targetOffset;
                }
            }

            camInitialised = true;
		}

        public void setPosition(Vector3 position)
        {
            transform.position = position;
            newPos = position;
        }

		public void setLastKillBy(Transform lastKillBy)
		{
			this.lastKillBy = lastKillBy;
		}
		
		public void setFollowTarget(Transform target)
		{
			if (target != null)
			{
                // TODO - health bar functionality is in the parent NoxGUI so should probably keep all of this together there (via a UI manager script for instance)
				if (NoxGUI.Instance.healthBarMode == HealthBarMode.TRACK)
				{
					Transform hBar = followTarget.Find("HealthBar");
					
					// turn off old tracked health bar
					if (hBar != null)
					{
						GameObject healthBar = hBar.Find("Canvas").gameObject;
						healthBar.SetActive(false);
					}
					
					// switch camera follow target
					followTarget = target;
					
					hBar = followTarget.Find("HealthBar");
					
					// turn on old tracked health bar
					if (hBar != null)
					{
						GameObject healthBar = hBar.Find("Canvas").gameObject;
						healthBar.SetActive(true);
					}
				}
			}
            else
            {
                lockedOn = false;
                lockedFactor = 1;
            }

            followTarget = target;
        }
		
        void LateUpdate()
        {
            if (Cam == null)
            {
                Cam = GameManager.Instance.MainCamera.GetComponent<Camera>();

                if (Cam == null) return;
            }

            if (camInitialised == false) return;
		
			if (followTarget)
			{
                newPos = new Vector3(followTarget.position.x + targetOffset.x, followTarget.position.y + targetOffset.y, newPos.z);
                newPos.z += Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
			}
			else
			{
				newPos = new Vector3(transform.position.x, transform.position.y, newPos.z);
				
				float modMoveSpeed = (newPos.z - minZ) / (maxZ - minZ) * freeMoveSpeed * Time.deltaTime;
				
				newPos.x += Input.GetAxis("Horizontal") * modMoveSpeed;
				newPos.y += Input.GetAxis("Vertical") * modMoveSpeed;
				newPos.z += Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity; 
			}                      
            
            if (newPos.z > minZ) newPos.z = minZ;
            else if (newPos.z < maxZ) newPos.z = maxZ;
            
            Cam.transform.position = new Vector3(newPos.x, newPos.y, newPos.z);            

            if (followTarget == true)
            {
                if (Vector3.Distance(Cam.transform.position, newPos) < minLockDistance)
                {
                    lockedOn = true;
                    lockedFactor = 10;
                }
            }
		}
	}
}