using System;
using UnityEngine;

namespace Davin.GUIs
{
    [Flags]
    public enum BoundsOffset
    {
        None = 0,
        PosX = 1,
        NegX = 2,
        PosY = 4,
        NegY = 8
    }

    public class StructureScrollingText : ScrollingText
    {
        [Tooltip("Offset from structure bounds")]
        [SerializeField]
        protected BoundsOffset structureBoundsOffset;
        public BoundsOffset StructureBoundsOffset { get { return structureBoundsOffset; } set { structureBoundsOffset = value; } }

        [Tooltip("Addtional Offset (useful for enhanced label placement) ")]
        [SerializeField]
        protected Vector2 offset;
        public Vector2 Offset { get { return offset; } set { offset = value; } }
    }
}