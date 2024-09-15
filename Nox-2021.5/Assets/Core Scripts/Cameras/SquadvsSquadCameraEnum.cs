using UnityEngine;

namespace NoxCore.Cameras
{
    public class SquadvsSquadCameraEnum : BaseCameraEnum
    {
        public static readonly SquadvsSquadCameraEnum FOLLOW_PREV_STATION = new SquadvsSquadCameraEnum(5);
        public static readonly SquadvsSquadCameraEnum FOLLOW_NEXT_STATION = new SquadvsSquadCameraEnum(6);

        protected SquadvsSquadCameraEnum(int internalValue) : base(internalValue)
        {
            this.InternalValue = internalValue;
        }
    }
}
