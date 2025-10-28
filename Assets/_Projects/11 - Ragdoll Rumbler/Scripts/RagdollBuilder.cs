using UnityEngine;

namespace Devdy.RagdollTumbler
{
    /// <summary>
    /// Editor utility to automatically build a multi-part 2D ragdoll with physics joints.
    /// Attach to an empty GameObject and click "Build Ragdoll" in Inspector.
    /// </summary>
    public class RagdollBuilder : MonoBehaviour
    {
        #region Configuration
        [Header("Ragdoll Configuration")]
        [SerializeField] private Sprite bodySprite; // Sprite for all body parts (optional)
        [SerializeField] private Color ragdollColor = Color.white; // Color for all body parts
        
        [Header("Body Part Sizes")]
        [SerializeField] private Vector2 headSize = new Vector2(0.4f, 0.4f);
        [SerializeField] private Vector2 torsoSize = new Vector2(0.5f, 1f);
        [SerializeField] private Vector2 armSize = new Vector2(0.2f, 0.6f);
        [SerializeField] private Vector2 legSize = new Vector2(0.2f, 0.8f);
        
        [Header("Physics Settings")]
        [SerializeField] private float limbMass = 0.5f;
        [SerializeField] private float torsoMass = 1f;
        
        #endregion ==================================================================

        #region Editor Build Method

        /// <summary>
        /// Builds the entire ragdoll structure with sprites, rigidbodies, colliders, and joints.
        /// Call this from a custom editor button or manually in code.
        /// </summary>
        [ContextMenu("Build Ragdoll")]
        public void BuildRagdoll()
        {
            // Clear existing children
            ClearChildren();
            
            // Create body parts
            GameObject head = CreateBodyPart("Head", headSize, new Vector3(0f, 0.7f, 0f), limbMass, true);
            GameObject torso = CreateBodyPart("Torso", torsoSize, new Vector3(0f, 0f, 0f), torsoMass, false);
            GameObject armLeft = CreateBodyPart("ArmLeft", armSize, new Vector3(-0.35f, 0.3f, 0f), limbMass, false);
            GameObject armRight = CreateBodyPart("ArmRight", armSize, new Vector3(0.35f, 0.3f, 0f), limbMass, false);
            GameObject legLeft = CreateBodyPart("LegLeft", legSize, new Vector3(-0.15f, -0.9f, 0f), limbMass, false);
            GameObject legRight = CreateBodyPart("LegRight", legSize, new Vector3(0.15f, -0.9f, 0f), limbMass, false);
            
            // Connect joints
            ConnectJoint(head, torso, new Vector2(0f, -0.2f));
            ConnectJoint(armLeft, torso, new Vector2(0f, 0.3f));
            ConnectJoint(armRight, torso, new Vector2(0f, 0.3f));
            ConnectJoint(legLeft, torso, new Vector2(0f, 0.4f));
            ConnectJoint(legRight, torso, new Vector2(0f, 0.4f));
            
            // Add trigger collider to torso for detection
            BoxCollider2D triggerCollider = torso.AddComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = torsoSize;
            
            // Add RagdollController to parent
            RagdollController controller = gameObject.AddComponent<RagdollController>();
            
            // Assign torso rigidbody to controller via reflection (since it's serialized)
            #if UNITY_EDITOR
            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(controller);
            so.FindProperty("torsoRb").objectReferenceValue = torso.GetComponent<Rigidbody2D>();
            so.ApplyModifiedProperties();
            #endif
            
            Debug.Log("Ragdoll built successfully! Create a prefab from this GameObject.");
        }

        #endregion ==================================================================

        #region Helper Methods

        /// <summary>
        /// Creates a single body part with sprite, rigidbody, and collider.
        /// </summary>
        private GameObject CreateBodyPart(string name, Vector2 size, Vector3 localPosition, float mass, bool isCircle)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(transform);
            part.transform.localPosition = localPosition;
            
            // Add Sprite Renderer
            SpriteRenderer sr = part.AddComponent<SpriteRenderer>();
            sr.sprite = bodySprite != null ? bodySprite : CreateDefaultSprite(isCircle);
            sr.color = ragdollColor;
            part.transform.localScale = new Vector3(size.x, size.y, 1f);
            
            // Add Rigidbody2D
            Rigidbody2D rb = part.AddComponent<Rigidbody2D>();
            rb.mass = mass;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            
            // Add Collider
            if (isCircle)
            {
                CircleCollider2D collider = part.AddComponent<CircleCollider2D>();
                collider.radius = 0.5f;
            }
            else
            {
                CapsuleCollider2D collider = part.AddComponent<CapsuleCollider2D>();
                collider.size = Vector2.one;
            }
            
            return part;
        }

        /// <summary>
        /// Connects two body parts with a HingeJoint2D.
        /// </summary>
        private void ConnectJoint(GameObject childPart, GameObject parentPart, Vector2 anchorPoint)
        {
            HingeJoint2D joint = childPart.AddComponent<HingeJoint2D>();
            joint.connectedBody = parentPart.GetComponent<Rigidbody2D>();
            joint.anchor = anchorPoint;
            joint.autoConfigureConnectedAnchor = true;
            
            // Optional: Add angle limits for more realistic movement
            joint.useLimits = true;
            JointAngleLimits2D limits = joint.limits;
            limits.min = -45f;
            limits.max = 45f;
            joint.limits = limits;
        }

        /// <summary>
        /// Creates a default sprite if none is assigned.
        /// </summary>
        private Sprite CreateDefaultSprite(bool isCircle)
        {
            // Try to use Unity's built-in sprites
            if (isCircle)
            {
                return Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            }
            else
            {
                return Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            }
        }

        /// <summary>
        /// Clears all child GameObjects before rebuilding.
        /// </summary>
        private void ClearChildren()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
            
            // Remove existing RagdollController if present
            RagdollController existingController = GetComponent<RagdollController>();
            if (existingController != null)
            {
                DestroyImmediate(existingController);
            }
        }

        #endregion ==================================================================
    }
}