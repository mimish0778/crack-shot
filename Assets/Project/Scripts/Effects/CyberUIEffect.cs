using System.Collections.Generic;
using UnityEngine;

namespace CrackShot
{
    public class CyberUIEffect : MonoBehaviour
    {
        [Header("Shapes")]
        [SerializeField] private int shapeCount = 25;
        [SerializeField] private float shapeSize = 0.06f;
        [SerializeField] private float shapeDepth = 5f;
        [SerializeField] private float shapeFloatAmp = 0.3f;
        [SerializeField] private float shapeFloatSpeed = 0.3f;

        [Header("Comets")]
        [SerializeField] private int cometCount = 5;
        [SerializeField] private float cometOrbitMin = 4f;
        [SerializeField] private float cometOrbitMax = 15f;
        [SerializeField] private float cometSpeedMin = 60f;
        [SerializeField] private float cometSpeedMax = 200f;
        [SerializeField] private float cometHeadSize = 0.03f;
        [SerializeField] private float cometTrailTime = 0.4f;

        private Camera _cam;

        private readonly List<ShapeData> _shapes = new();
        private readonly List<CyberComet> _comets = new();

        private struct ShapeData
        {
            public CyberFx.FloatingShape shape;
            public Vector2 baseViewport;
        }

        private void Start()
        {
            _cam = Camera.main;

            BuildShapes();
            BuildComets();
        }

        private void Update()
        {
            if (_cam == null)
            {
                return;
            }
            float t = Time.time;

            UpdateShapes(t);
            UpdateComets(t);
        }

        private void BuildShapes()
        {
            for (int i = 0; i < shapeCount; i++)
            {
                bool pink = (i % 2 == 0);
                var mat = CyberFx.CreateAdditiveMaterial(CyberFx.AltColor(pink));
                var go = new GameObject($"UIShape_{i}");
                go.transform.SetParent(transform);

                var lr = CyberFx.AddLine(go, mat, worldSpace: false, loop: true);

                float s = shapeSize * Random.Range(0.5f, 1.5f);
                int type = Random.Range(0, 3);
                CyberFx.SetNeonShape(lr, type, s);

                _shapes.Add(new ShapeData
                {
                    baseViewport = new Vector2(Random.value, Random.value),
                    shape = new CyberFx.FloatingShape
                    {
                        Go = go, Mat = mat,
                        FloatPhase = CyberFx.RandomPhase(),
                        RotAxis = Random.onUnitSphere,
                        RotSpeed = Random.Range(20f, 80f),
                    },
                });
            }
        }

        private void UpdateShapes(float t)
        {
            foreach (var d in _shapes)
            {
                float vy = d.baseViewport.y + Mathf.Sin(t * shapeFloatSpeed + d.shape.FloatPhase) * shapeFloatAmp;
                Vector3 wp = _cam.ViewportToWorldPoint(new Vector3(d.baseViewport.x, vy, shapeDepth));
                d.shape.Go.transform.position = wp;
                CyberFx.AnimateFloatingShape(d.shape, t);
            }
        }

        private void BuildComets()
        {
            Vector3 center = _cam.transform.position + _cam.transform.forward * shapeDepth;

            for (int i = 0; i < cometCount; i++)
            {
                Color initCol = CyberFx.GetColor(i * 0.3f);
                var mat = CyberFx.CreateAdditiveMaterial(initCol);

                var go = new GameObject($"UIComet_{i}");
                go.transform.SetParent(transform);
                go.transform.position = center;

                var trail = go.AddComponent<TrailRenderer>();
                CyberFx.SetupTrail(trail, mat, cometTrailTime, cometHeadSize, minVertexDistance: 0.02f);
                CyberFx.ApplyTrailGradient(trail, initCol);

                float r = Random.Range(cometOrbitMin, cometOrbitMax);
                _comets.Add(CyberComet.Create(go, trail, mat, r, 0.4f, 1.6f, cometSpeedMin, cometSpeedMax, Random.Range(0f, 360f)));
            }
        }

        private void UpdateComets(float t)
        {
            Vector3 center = _cam.transform.position + _cam.transform.forward * shapeDepth;

            foreach (var c in _comets)
            {
                c.Go.transform.position = center + c.Advance(t, Time.deltaTime, 1.5f);
                c.ApplyColor(t);
            }
        }
    }
}
