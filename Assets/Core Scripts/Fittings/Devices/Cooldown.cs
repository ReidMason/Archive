using UnityEngine;
using UnityEngine.Events;

using NoxCore.Utilities;

namespace NoxCore.Fittings.Devices
{
    [System.Serializable]
    public class DurationEvent : UnityEvent<float>
    {}

    public class Cooldown : MonoBehaviour
    {
        [SerializeField]
        protected float _maxTime;
        public float maxTime { get { return _maxTime; } set { _maxTime = value; } }

        [SerializeField]
        [ShowOnly] protected float _timer;
        public float timer { get { return _timer; } }

        public DurationEvent begin;
        public UnityEvent end;

        public void Start()
        {
            enabled = false;

            if (begin == null)
            {
                begin = new DurationEvent();
            }

            begin.AddListener(setDuration);

            if (end == null)
            {
                end = new UnityEvent();
            }
        }

        public void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= _maxTime)
            {
                end.Invoke();
                _timer = 0;
                enabled = false;
            }
        }

        void setDuration(float duration)
        {
            _maxTime = duration;
            _timer = 0;
            enabled = true;
        }
    }
}