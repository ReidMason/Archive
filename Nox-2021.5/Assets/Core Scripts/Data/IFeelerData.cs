using UnityEngine;

namespace NoxCore.Data
{
    public interface IFeelerData
    {
        float Length { get; set; }
        float Direction { get; set; }
        Color Colour { get; }
        Vector2 Dir { get; set; }
    }
}