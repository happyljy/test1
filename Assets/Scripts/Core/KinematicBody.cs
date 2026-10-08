using UnityEngine;

namespace PlaneWar
{
    [RequireComponent(typeof(Collider2D))]
    public class KinematicBody : MonoBehaviour
    {
        void Awake()
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;   // ★ 关键：Kinematic 之间互相触发
            rb.gravityScale = 0f;
            rb.simulated = true;
            rb.interpolation = RigidbodyInterpolation2D.None;
        }
    }
}