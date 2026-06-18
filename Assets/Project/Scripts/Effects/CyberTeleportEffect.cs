using System.Collections;
using UnityEngine;

namespace CrackShot
{
    public class CyberTeleportEffect : Singleton<CyberTeleportEffect>
    {
        [SerializeField] private int particleCount = 18;
        [SerializeField] private float burstSpeed = 6f;
        [SerializeField] private float particleSize = 0.15f;
        [SerializeField] private float lifetime = 0.6f;

        public void Burst(Vector3 worldPos) => StartCoroutine(DoBurst(worldPos));

        private IEnumerator DoBurst(Vector3 origin)
        {
            var particles = new CyberFx.ShapeParticle[particleCount];

            for (int i = 0; i < particleCount; i++)
            {
                var p = CyberFx.CreateShapeParticle("CyberParticle", origin, particleSize * Random.Range(0.5f, 1.4f), 120f, 400f);

                Vector3 vel = Random.onUnitSphere * burstSpeed;
                vel.y = Mathf.Abs(vel.y) * 0.5f;
                p.Velocity = vel;

                particles[i] = p;
            }

            yield return CyberFx.AnimateAndDestroyParticles(particles, lifetime, velocityDamping: 3f,
                alphaFunc: t => Mathf.Lerp(1f, 0f, Ease.InQuad(t)),
                colorFunc: (p, elapsed) => CyberFx.Current);
        }
    }
}
