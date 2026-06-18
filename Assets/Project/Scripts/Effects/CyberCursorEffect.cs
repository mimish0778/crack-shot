using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrackShot
{
    public class CyberCursorEffect : Singleton<CyberCursorEffect>
    {
        [Header("Click Burst")]
        [SerializeField] private int burstCount = 12;
        [SerializeField] private float burstSpeed = 5f;
        [SerializeField] private float burstLifetime = 0.7f;
        [SerializeField] private float burstSize = 0.06f;

        [Header("Drag Trail")]
        [SerializeField] private float trailSpawnInterval = 0.06f;
        [SerializeField] private float trailSpeed = 1.5f;
        [SerializeField] private float trailLifetime = 1.0f;
        [SerializeField] private float trailSize = 0.04f;

        [Header("Common")]
        [SerializeField] private float effectDepth = 8f;

        private float _trailTimer;
        private Camera _cam;

        private void Start() => _cam = Camera.main;

        private void Update()
        {
            if (_cam == null)
            {
                _cam = Camera.main;
            }
            if (_cam == null)
            {
                return;
            }

            Vector3 mouseWorld = GetMouseWorld();

            if (GameInput.SelectPressed && !IsPointerOverButton())
            {
                StartCoroutine(Burst(mouseWorld, burstCount, burstSpeed, burstSize, burstLifetime));
            }

            bool dragging = GameInput.SelectHeld;
            if (dragging)
            {
                _trailTimer -= Time.deltaTime;
                if (_trailTimer <= 0f)
                {
                    _trailTimer = trailSpawnInterval;
                    StartCoroutine(TrailParticle(mouseWorld));
                }
            }
            else
            {
                _trailTimer = 0f;
            }
        }

        private IEnumerator Burst(Vector3 origin, int count, float speed, float size, float lifetime)
        {
            var particles = SpawnParticles(origin, count, size, speed);
            const float fadeStart = 0.3f;
            yield return CyberFx.AnimateAndDestroyParticles(particles, lifetime, velocityDamping: 2.5f,
                alphaFunc: t => t < fadeStart ? 1f : Mathf.Lerp(1f, 0f, (t - fadeStart) / (1f - fadeStart)),
                colorFunc: (p, elapsed) => CyberFx.GetColor(elapsed * 0.5f + p.Phase));
        }

        private IEnumerator TrailParticle(Vector3 origin)
        {
            var p = CreateParticle(origin, trailSize);

            Vector3 vel = Random.onUnitSphere * trailSpeed;
            vel.z = 0f;

            yield return CyberFx.Tween(trailLifetime, t =>
            {
                if (p.Go == null)
                {
                    return;
                }
                float alpha = Mathf.Lerp(0.8f, 0f, t);
                vel = Vector3.Lerp(vel, Vector3.zero, Time.deltaTime * 2f);
                p.Go.transform.position += vel * Time.deltaTime;
                p.Go.transform.Rotate(p.RotAxis, p.RotSpeed * 0.3f * Time.deltaTime, Space.World);

                CyberFx.ApplyColor(p.Mat, CyberFx.Current.WithAlpha(alpha));
            });
            if (p.Go != null)
            {
                Destroy(p.Go);
            }
        }

        private List<CyberFx.ShapeParticle> SpawnParticles(Vector3 origin, int count, float size, float speed)
        {
            var list = new List<CyberFx.ShapeParticle>();
            for (int i = 0; i < count; i++)
            {
                var p = CreateParticle(origin, size * Random.Range(0.6f, 1.4f));
                p.Velocity = Random.onUnitSphere * speed * Random.Range(0.5f, 1.5f);
                list.Add(p);
            }
            return list;
        }

        private CyberFx.ShapeParticle CreateParticle(Vector3 origin, float size)
        {
            var p = CyberFx.CreateShapeParticle("CursorParticle", origin, size, 60f, 300f);
            p.Phase = Random.Range(0f, 10f);
            return p;
        }

        private static readonly List<RaycastResult> RayResults = new();

        private static bool IsPointerOverButton()
        {
            if (EventSystem.current == null)
            {
                return false;
            }
            var data = new PointerEventData(EventSystem.current) { position = GameInput.PointerPosition };
            RayResults.Clear();
            EventSystem.current.RaycastAll(data, RayResults);
            foreach (var r in RayResults)
            {
                if (r.gameObject.GetComponentInParent<Button>() != null)
                {
                    return true;
                }
            }
            return false;
        }

        private Vector3 GetMouseWorld()
        {
            Vector3 p = GameInput.PointerPosition;
            p.z = effectDepth;
            return _cam.ScreenToWorldPoint(p);
        }
    }
}
