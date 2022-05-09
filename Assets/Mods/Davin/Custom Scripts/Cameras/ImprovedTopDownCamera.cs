using UnityEngine;

using NoxCore.Cameras;

namespace Davin.Cameras
{
    public class ImprovedTopDownCamera : TopDown_Camera
    {
        public float targetZ = -500;
        private float scrollAdd = 0;
        private Vector3 newPosition;
        /// <summary>
        ///   Provide singleton support for this class.
        ///   The script must still be attached to a game object, but this will allow it to be called
        ///   from anywhere without specifically identifying that game object.
        /// </summary>
        private static ImprovedTopDownCamera instance;
        public static ImprovedTopDownCamera ChildInstance { get { return instance; } set { instance = value; } }
              
        
        void LateUpdate()
        {
            if (Cam == null) return;

            if (followTarget)
            {
                Vector3 targetPosition = new Vector3(followTarget.position.x + targetOffset.x, followTarget.position.y + targetOffset.y, newPosition.z);
                newPosition = Vector3.Lerp(targetPosition, transform.position, 0.95f);
                scrollAdd += Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
                newPosition.z = targetZ * 0.05f + newPosition.z * 0.95f + scrollAdd;
            }
            else
            {
                newPosition = new Vector3(transform.position.x, transform.position.y, newPosition.z);

                float modMoveSpeed = (newPosition.z - minZ) / (maxZ - minZ) * freeMoveSpeed * Time.deltaTime;

                newPosition.x += Input.GetAxis("Horizontal") * modMoveSpeed;
                newPosition.y += Input.GetAxis("Vertical") * modMoveSpeed;
                scrollAdd += Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
                newPosition.z = targetZ * 0.01f + newPosition.z * 0.99f + scrollAdd;
            }

            if (newPosition.z > minZ) newPosition.z = minZ;
            else if (newPosition.z < maxZ) newPosition.z = maxZ;

            Cam.transform.position = new Vector3(newPosition.x, newPosition.y, newPosition.z);

            if (followTarget == true)
            {
                if (Vector3.Distance(Cam.transform.position, newPosition) < minLockDistance)
                {
                    lockedOn = true;
                    lockedFactor = 10;
                }
            }
        }
    }
}