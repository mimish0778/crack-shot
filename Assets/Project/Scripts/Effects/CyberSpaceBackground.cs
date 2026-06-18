using System.Collections.Generic;
using UnityEngine;

namespace CrackShot
{
    public class CyberSpaceBackground : Singleton<CyberSpaceBackground>
    {
        [Header("Neon Rings")]
        [SerializeField] private int ringCount = 4;
        [SerializeField] private float ringBaseRadius = 120f;
        [SerializeField] private float ringRadiusStep = 100f;
        [SerializeField] private float ringWidth = 0.8f;
        [SerializeField] private float ringTiltSpeed = 0.6f;

        [Header("Floating Shapes")]
        [SerializeField] private int shapeCount = 200;
        [SerializeField] private float shapeLineWidth = 0.08f;
        [SerializeField] private float shapeFloatSpeed = 2f;
        [SerializeField] private float shapeInnerRadius = 40f;
        [SerializeField] private float shapeOuterRadius = 120f;

        [Header("Equalizer Bars")]
        [SerializeField] private int eqBarCount = 400;
        [SerializeField] private float eqRadius = 100f;
        [SerializeField] private float eqMaxHeight = 20f;
        [SerializeField] private float eqBarWidth = 0.5f;
        [SerializeField] [Range(0.1f, 1f)] private float eqSigmoidExponent = 0.5f;

        [Header("Comets")]
        [SerializeField] private int cometCount = 15;
        [SerializeField] private float cometOrbitMin = 50f;
        [SerializeField] private float cometOrbitMax = 200f;
        [SerializeField] private float cometSpeedMin = 80f;
        [SerializeField] private float cometSpeedMax = 260f;
        [SerializeField] private float cometTrailTime = 0.4f;
        [SerializeField] private float cometHeadSize = 0.5f;

        private struct RingData { public LineRenderer lr; public Material mat; public float phaseOffset; }
        private struct ShapeData { public CyberFx.FloatingShape shape; public Vector3 basePos; }
        private struct EqBar { public GameObject go; public Material mat; public float phase; }
        private Material _lineMat;

        private static readonly Color MutedGray = new Color(0.4f, 0.4f, 0.4f);

        private readonly List<RingData> _rings = new();
        private readonly List<ShapeData> _shapes = new();
        private readonly List<EqBar> _eqBars = new();
        private readonly List<CyberComet> _comets = new();

        protected override void OnAwake()
        {
            BuildMaterials();
            Build();
        }

        private void Update()
        {
            float t = Time.time;
            UpdateRings(t);
            UpdateShapes(t);
            UpdateEqualizer(t);
            UpdateComets(t);
        }

        public void SetCenter(Vector3 worldCenter, float zOffset)
        {
            var pos = transform.position;
            pos.x = worldCenter.x;
            pos.z = worldCenter.z + zOffset;
            transform.position = pos;
        }

        private void BuildMaterials()
        {
            _lineMat = new Material(CyberFx.Unlit);
            _lineMat.color = CyberFx.Cyan;
            _lineMat.SetFloat("_Surface", 1f);
            _lineMat.SetFloat("_Blend", 0f);
        }

        private Material CloneMat(Color c)
        {
            var m = new Material(_lineMat);
            m.color = c;
            return m;
        }

        private void Build()
        {
            BuildRings();
            BuildShapes();
            BuildEqualizer();
            BuildComets();
        }

        private void BuildRings()
        {
            int seg = 64;
            for (int i = 0; i < ringCount; i++)
            {
                float r = ringBaseRadius + ringRadiusStep * i;
                bool pink = (i % 2 == 1);
                var lr = BuildLine($"Ring_{i}", ringWidth + i * 0.02f, CyberFx.AltColor(pink));
                lr.loop = true;
                CyberFx.SetCirclePositions(lr, seg, r, xzPlane: true);
                _rings.Add(new RingData { lr = lr, mat = lr.material, phaseOffset = i * 1.3f });
            }
        }

        private void UpdateRings(float t)
        {
            for (int i = 0; i < _rings.Count; i++)
            {
                var d = _rings[i];
                float tiltX = Mathf.Sin(t * ringTiltSpeed + d.phaseOffset) * 20f;
                float tiltZ = Mathf.Cos(t * ringTiltSpeed * 0.7f + d.phaseOffset) * 15f;
                d.lr.transform.localRotation = Quaternion.Euler(tiltX, t * ringTiltSpeed * 30f, tiltZ);

                Color col = Color.Lerp(CyberFx.Current, MutedGray, 0.2f)
                    .WithAlpha(0.75f + 0.25f * Mathf.Sin(t * 2f + d.phaseOffset));
                CyberFx.ApplyColor(d.mat, col);

                float w = ringWidth + ringWidth * 0.5f * Mathf.Abs(Mathf.Sin(t * 1.5f + d.phaseOffset));
                d.lr.startWidth = w;
                d.lr.endWidth = w;
            }
        }

        private void BuildShapes()
        {
            for (int i = 0; i < shapeCount; i++)
            {
                bool pink = (i % 2 == 0);
                Vector3 pos = RandomShapePosition();

                bool tri = (Random.value > 0.5f);
                float size = Random.Range(0.3f, 1.6f);

                var go = new GameObject($"Shape_{i}");
                go.transform.SetParent(transform);
                go.transform.localPosition = pos;

                var mat = CloneMat(CyberFx.AltColor(pink));
                var lr = CyberFx.AddLine(go, mat, worldSpace: false, loop: true, width: shapeLineWidth);

                var verts = CyberFx.GetShapeVertices(tri ? 0 : 1, size, heightRatio: 0.6f);
                lr.positionCount = verts.Length;
                for (int v = 0; v < verts.Length; v++)
                {
                    lr.SetPosition(v, verts[v]);
                }

                _shapes.Add(new ShapeData
                {
                    basePos = pos,
                    shape = new CyberFx.FloatingShape
                    {
                        Go = go, Mat = mat,
                        FloatPhase = CyberFx.RandomPhase(),
                        RotAxis = Random.onUnitSphere,
                        RotSpeed = Random.Range(10f, 60f),
                    },
                });
            }
        }

        private void BuildEqualizer()
        {
            var barMesh = BuildBarMesh(eqBarWidth, 1f);

            for (int i = 0; i < eqBarCount; i++)
            {
                float angle = 360f / eqBarCount * i;
                bool pink = (i % 2 == 0);
                float rad = angle * Mathf.Deg2Rad;
                float x = Mathf.Sin(rad) * eqRadius;
                float z = Mathf.Cos(rad) * eqRadius;

                var go = new GameObject($"EqBar_{i}");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(x, 0f, z);
                go.transform.localRotation = Quaternion.Euler(0, -angle + 90f, 0);
                go.transform.localScale = new Vector3(1f, 0.05f, 1f);

                var mf = go.AddComponent<MeshFilter>();
                var mr = go.AddComponent<MeshRenderer>();
                var mat = CloneMat(CyberFx.AltColor(pink));
                CyberFx.MakeAdditive(mat);
                mr.sharedMaterial = mat;
                CyberFx.NoShadow(mr);
                mf.sharedMesh = barMesh;

                _eqBars.Add(new EqBar { go = go, mat = mat, phase = i * 0.4f });
            }
        }

        private void UpdateShapes(float t)
        {
            for (int i = 0; i < _shapes.Count; i++)
            {
                var d = _shapes[i];
                float yOffset = Mathf.Sin(t * shapeFloatSpeed + d.shape.FloatPhase) * 1.5f;
                d.shape.Go.transform.localPosition = d.basePos + Vector3.up * yOffset;
                CyberFx.AnimateFloatingShape(d.shape, t);
            }
        }

        private void UpdateEqualizer(float t)
        {
            for (int i = 0; i < _eqBars.Count; i++)
            {
                var d = _eqBars[i];
                float h = eqMaxHeight * Mathf.Abs(
                    Mathf.Sin(t * 2.5f + d.phase) * 0.5f +
                    Mathf.Sin(t * 1.3f + d.phase * 1.7f) * 0.3f +
                    Mathf.Sin(t * 4.1f + d.phase * 0.5f) * 0.2f);
                h = Mathf.Max(h, 0.05f);

                d.go.transform.localScale = new Vector3(1f, h, 1f);

                Color vivid = CyberFx.GetSigmoidColor(t + d.phase * 2f, CyberFx.CycleSpeed, eqSigmoidExponent);
                Color col = Color.Lerp(vivid, MutedGray, 0.45f)
                    .WithAlpha(0.15f + 0.2f * (h / eqMaxHeight));
                CyberFx.ApplyColor(d.mat, col);
            }
        }

        private void BuildComets()
        {
            for (int i = 0; i < cometCount; i++)
            {
                var go = new GameObject($"Comet_{i}");
                go.transform.SetParent(transform);

                var trail = go.AddComponent<TrailRenderer>();
                var mat = new Material(_lineMat);
                CyberFx.SetupTrail(trail, mat, cometTrailTime, cometHeadSize, minVertexDistance: 0.1f);
                CyberFx.ApplyTrailGradient(trail, CyberFx.Cyan);

                float startAngle = Random.Range(0f, 360f);
                float r = Random.Range(cometOrbitMin, cometOrbitMax);

                var comet = CyberComet.Create(go, trail, mat, r, 0.5f, 1.5f, cometSpeedMin, cometSpeedMax, startAngle);

                float initRad = startAngle * Mathf.Deg2Rad;
                go.transform.localPosition = comet.Tilt * new Vector3(Mathf.Cos(initRad) * r, Mathf.Sin(initRad) * 5f, Mathf.Sin(initRad) * comet.OrbitRadiusB);
                trail.Clear();

                _comets.Add(comet);
            }
        }

        private void UpdateComets(float t)
        {
            foreach (var c in _comets)
            {
                c.Go.transform.localPosition = c.Advance(t, Time.deltaTime, 5f);
                c.ApplyColor(t);
            }
        }

        private Vector3 RandomShapePosition()
        {
            float r = Random.Range(shapeInnerRadius, shapeOuterRadius);
            return Random.onUnitSphere * r;
        }

        private LineRenderer BuildLine(string name, float width, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            var tubeMat = CyberFx.CreateTubeMaterial(color);

            var lr = CyberFx.AddLine(go, tubeMat, worldSpace: false, width: width);
            lr.textureMode = LineTextureMode.Tile;
            return lr;
        }

        private static Mesh BuildBarMesh(float width, float height)
        {
            float hw = width * 0.5f;
            return CyberFx.BuildDoubleSidedQuad(
                new Vector3(-hw, -height, -0.1f), new Vector3(hw, -height, -0.1f),
                new Vector3(hw, height, -0.1f), new Vector3(-hw, height, -0.1f));
        }
    }
}
