using UnityEngine;
using NoxCore.Placeables.Ships;

namespace NoxCore.Stats
{
    public class RaceStats
    {
        public Transform racerTransform;
        public int lap;
        public float [] lapTimes;
        public int currentNavPoint;
        public float distanceRemaining;
        public float distanceToNextNavPoint;
        public float distanceFromNextNavPoint;
        public string commanderName;
        public Sprite portrait;
        
        public RaceStats(Transform racerTransform, int maxLaps, float distanceRemaining)
        {
            this.racerTransform = racerTransform;
            lap = 0;
            lapTimes = new float[maxLaps];
            currentNavPoint = 1;
            this.distanceRemaining = distanceRemaining;
            distanceToNextNavPoint = 0;
            distanceFromNextNavPoint = 0;
            Ship ship = racerTransform.GetComponent<Ship>();
            commanderName = ship.Command.label;
            Texture2D tex = ship.Command.portrait;
            portrait = Sprite.Create(tex, new Rect(0,0, tex.width, tex.height), new Vector2(0.5f,0.5f));
        }
    }
}

