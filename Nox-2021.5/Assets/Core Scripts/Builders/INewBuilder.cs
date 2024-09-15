using UnityEngine;
using System.Collections;

namespace NoxCore.Builders
{
    public interface INewBuilder : IBuilder
    {
        void Reset();
    }
}