using UnityEngine;

namespace Devdy.BirthdayCake
{
    /// <summary>
    /// ScriptableObject that defines different cake layer types.
    /// Create variants in Resources/Layers/ folder.
    /// </summary>
    [CreateAssetMenu(fileName = "LayerData", menuName = "BirthdayCake/Layer Data")]
    public class LayerData : ScriptableObject
    {
        public LayerType Type;
        public GameObject Prefab; // Visual prefab for this layer
        public Sprite Icon; // Optional: for UI preview
        public float Mass = 1f; // Physics mass
        public Vector2 Size = new Vector2(2f, 0.5f); // Collider size
    }

    public enum LayerType
    {
        CakeLayer,
        Strawberry,
        Cream,
        Chocolate,
        Cherry,
        Candle
    }
}
