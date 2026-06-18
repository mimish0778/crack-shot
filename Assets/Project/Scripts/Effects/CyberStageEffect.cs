using System.Collections.Generic;
using UnityEngine;

namespace CrackShot
{
    public class CyberStageEffect : Singleton<CyberStageEffect>
    {
        [Header("Color Cycle")]
        [SerializeField] private float colorCycleSpeed = 0.08f;
        [SerializeField] [Range(0.1f, 1f)] private float colorCurveExponent = 0.5f;

        [Header("Geometry")]
        [SerializeField] private float lineWidth = 0.2f;
        [SerializeField] private float floorLineWidth = 0.2f;
        [SerializeField] private float creaseAngle = 25f;
        [SerializeField] private int holeRingSegments = 128;
        [SerializeField] private int holeTubeSegments = 128;
        [SerializeField] private float holeRingOffset = 0.01f;
        [SerializeField] private float holeTubeRadius = 0.08f;

        private const string FloorMeshName = "Floor";

        private readonly List<EdgeEntry> _edges = new();
        private Material _blackMat;

        private struct EdgeEntry { public Material mat; public float phase; }

        private GameObject _lastStageRoot;

        [ContextMenu("Rebuild")]
        private void Rebuild() { if (_lastStageRoot != null) { ApplyTo(_lastStageRoot); } }

        public void ApplyTo(GameObject stageRoot)
        {
            _lastStageRoot = stageRoot;
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            _edges.Clear();

            _blackMat = CyberFx.CreateUnlitMaterial(Color.black);

            foreach (var mr in stageRoot.GetComponentsInChildren<MeshRenderer>())
            {
                bool isFloor = mr.gameObject.name.Contains(FloorMeshName);

                var mats = new Material[mr.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = _blackMat;
                }
                mr.sharedMaterials = mats;

                var mf = mr.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null)
                {
                    continue;
                }

                Transform follow = mr.GetComponentInParent<ObstacleController>() != null
                    ? mr.transform : null;

                if (isFloor)
                {
                    BuildFloorTopEdges(mf.sharedMesh, mr.transform);
                }
                else
                {
                    ExtractCreaseEdges(mf.sharedMesh, mr.transform, lineWidth, follow);
                }
            }

            foreach (var hole in stageRoot.GetComponentsInChildren<HoleGoal>())
            {
                BuildHoleRing(hole);
            }
        }

        private void BuildFloorTopEdges(Mesh mesh, Transform t)
        {
            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;

            var worldVerts = new Vector3[verts.Length];
            float maxY = float.MinValue;
            for (int i = 0; i < verts.Length; i++)
            {
                worldVerts[i] = t.TransformPoint(verts[i]);
                if (worldVerts[i].y > maxY)
                {
                    maxY = worldVerts[i].y;
                }
            }

            float yThresh = maxY - 0.05f;

            var seen = new HashSet<(Vector3, Vector3)>();
            var topEdges = new List<(Vector3, Vector3)>();
            for (int i = 0; i < tris.Length; i += 3)
            {
                TryAddTopEdge(worldVerts, tris[i], tris[i+1], yThresh, seen, topEdges);
                TryAddTopEdge(worldVerts, tris[i+1], tris[i+2], yThresh, seen, topEdges);
                TryAddTopEdge(worldVerts, tris[i+2], tris[i], yThresh, seen, topEdges);
            }

            Vector3 center = Vector3.zero;
            int count = 0;
            foreach (var (a, b) in topEdges) { center += a + b; count += 2; }
            if (count == 0)
            {
                return;
            }
            center /= count;
            center.y = maxY;

            foreach (var (wa, wb) in topEdges)
            {
                CreateFloorEdgeQuad(wa, wb, center, maxY);
            }
        }

        private static void TryAddTopEdge(
            Vector3[] wv, int ia, int ib, float yThresh,
            HashSet<(Vector3, Vector3)> seen, List<(Vector3, Vector3)> result)
        {
            if (wv[ia].y < yThresh || wv[ib].y < yThresh)
            {
                return;
            }
            Vector3 a = Round(wv[ia]); Vector3 b = Round(wv[ib]);
            var key = a.GetHashCode() <= b.GetHashCode() ? (a, b) : (b, a);
            if (seen.Add(key))
            {
                result.Add((a, b));
            }
        }

        private void CreateFloorEdgeQuad(Vector3 wa, Vector3 wb, Vector3 center, float y)
        {
            Vector3 mid = (wa + wb) * 0.5f;
            Vector3 inward = center - mid;
            inward.y = 0f;
            if (inward.sqrMagnitude < 0.0001f)
            {
                return;
            }
            inward.Normalize();

            float h = y + 0.02f;
            Vector3 v0 = new Vector3(wa.x, h, wa.z);
            Vector3 v1 = new Vector3(wb.x, h, wb.z);
            Vector3 v2 = new Vector3(wb.x + inward.x * floorLineWidth, h, wb.z + inward.z * floorLineWidth);
            Vector3 v3 = new Vector3(wa.x + inward.x * floorLineWidth, h, wa.z + inward.z * floorLineWidth);

            var mesh = CyberFx.BuildDoubleSidedQuad(v0, v1, v2, v3);

            var go = new GameObject("FloorEdge");
            go.transform.SetParent(transform);

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mat = CyberFx.CreateUnlitMaterial(CyberFx.Cyan);
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            CyberFx.NoShadow(mr);

            _edges.Add(new EdgeEntry { mat = mat, phase = CyberFx.RandomPhase() });
        }

        private void ExtractCreaseEdges(Mesh mesh, Transform t, float width, Transform follow = null)
        {
            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;
            int triCount = tris.Length / 3;

            var triNormals = new Vector3[triCount];
            for (int i = 0; i < triCount; i++)
            {
                Vector3 a = t.TransformPoint(verts[tris[i*3]]);
                Vector3 b = t.TransformPoint(verts[tris[i*3+1]]);
                Vector3 c = t.TransformPoint(verts[tris[i*3+2]]);
                triNormals[i] = Vector3.Cross(b - a, c - a).normalized;
            }

            var edgeMap = new Dictionary<(Vector3, Vector3), List<int>>();

            for (int i = 0; i < triCount; i++)
            {
                RegisterEdge(edgeMap, verts[tris[i*3]], verts[tris[i*3+1]], t, i);
                RegisterEdge(edgeMap, verts[tris[i*3+1]], verts[tris[i*3+2]], t, i);
                RegisterEdge(edgeMap, verts[tris[i*3+2]], verts[tris[i*3]], t, i);
            }

            float cosThresh = Mathf.Cos(creaseAngle * Mathf.Deg2Rad);

            foreach (var kv in edgeMap)
            {
                var triList = kv.Value;
                bool show;
                if (triList.Count == 1)
                {
                    show = true;
                }
                else
                {
                    float dot = Vector3.Dot(triNormals[triList[0]], triNormals[triList[1]]);
                    show = dot < cosThresh;
                }

                if (!show)
                {
                    continue;
                }

                var (la, lb) = kv.Key;
                Vector3 wa = t.TransformPoint(la);
                Vector3 wb = t.TransformPoint(lb);
                CreateEdgeLine(wa, wb, width, follow);
            }
        }

        private static void RegisterEdge(
            Dictionary<(Vector3, Vector3), List<int>> map,
            Vector3 a, Vector3 b, Transform t, int triangleIndex)
        {
            Vector3 ra = Round(a);
            Vector3 rb = Round(b);
            var key = ra.GetHashCode() <= rb.GetHashCode() ? (ra, rb) : (rb, ra);
            if (!map.TryGetValue(key, out var list))
            {
                list = new List<int>();
                map[key] = list;
            }
            if (!list.Contains(triangleIndex))
            {
                list.Add(triangleIndex);
            }
        }

        private void CreateEdgeLine(Vector3 wa, Vector3 wb, float width, Transform follow = null)
        {
            var parent = follow != null ? follow : transform;
            bool worldSpace = follow == null;
            if (follow != null)
            {
                wa = follow.InverseTransformPoint(wa);
                wb = follow.InverseTransformPoint(wb);
            }

            var mat = CyberFx.CreateTubeMaterial(CyberFx.Cyan);

            var go = new GameObject("NeonEdge");
            go.transform.SetParent(parent, false);

            var lr = CyberFx.AddLine(go, mat, worldSpace, width: width);
            lr.positionCount = 2;
            lr.textureMode = LineTextureMode.Tile;
            lr.SetPosition(0, wa);
            lr.SetPosition(1, wb);

            _edges.Add(new EdgeEntry { mat = mat, phase = CyberFx.RandomPhase() });
        }

        private void BuildHoleRing(HoleGoal hole)
        {
            float r = 0.5f;
            var sc = hole.GetComponent<SphereCollider>();
            if (sc != null)
            {
                r = sc.radius * hole.transform.lossyScale.x;
            }
            else
            {
                var cc = hole.GetComponent<CapsuleCollider>();
                if (cc != null)
                {
                    r = cc.radius * hole.transform.lossyScale.x;
                }
            }
            r += holeRingOffset;

            Vector3 center = hole.transform.position;

            var go = new GameObject("HoleRing");
            go.transform.SetParent(transform);

            float topY = hole.GetComponentInChildren<Renderer>() is Renderer hr
                ? hr.bounds.max.y
                : center.y;

            var mat = CyberFx.CreateUnlitMaterial(CyberFx.Cyan);
            mat.SetFloat("_Cull", 0f);
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            CyberFx.NoShadow(mr);

            mf.sharedMesh = BuildTorusMesh(
                new Vector3(center.x, topY, center.z),
                r, holeTubeRadius, holeRingSegments, holeTubeSegments);

            _edges.Add(new EdgeEntry { mat = mat, phase = 0f });
        }

        public Color CurrentColor { get; private set; } = CyberFx.Cyan;

        private void Update()
        {
            CurrentColor = CyberFx.GetSigmoidColor(Time.time, colorCycleSpeed, colorCurveExponent);
            foreach (var e in _edges)
            {
                CyberFx.ApplyColor(e.mat, CurrentColor);
            }
        }

        private static Mesh BuildTorusMesh(Vector3 center, float R, float r, int majorSeg, int minorSeg)
        {
            int vertCount = majorSeg * minorSeg;
            var verts = new Vector3[vertCount];
            var norms = new Vector3[vertCount];
            var tris = new int[majorSeg * minorSeg * 6];

            for (int i = 0; i < majorSeg; i++)
            {
                float u = 2f * Mathf.PI * i / majorSeg;
                float cu = Mathf.Cos(u), su = Mathf.Sin(u);

                for (int j = 0; j < minorSeg; j++)
                {
                    float v = 2f * Mathf.PI * j / minorSeg;
                    float cv = Mathf.Cos(v), sv = Mathf.Sin(v);

                    float x = (R + r * cv) * cu;
                    float y = r * sv;
                    float z = (R + r * cv) * su;

                    int index = i * minorSeg + j;
                    verts[index] = center + new Vector3(x, y, z);
                    norms[index] = new Vector3(cv * cu, sv, cv * su);
                }
            }

            int ti = 0;
            for (int i = 0; i < majorSeg; i++)
            {
                int ni = (i + 1) % majorSeg;
                for (int j = 0; j < minorSeg; j++)
                {
                    int nj = (j + 1) % minorSeg;
                    int a = i * minorSeg + j;
                    int b = ni * minorSeg + j;
                    int c = ni * minorSeg + nj;
                    int d = i * minorSeg + nj;
                    tris[ti++] = a; tris[ti++] = b; tris[ti++] = c;
                    tris[ti++] = a; tris[ti++] = c; tris[ti++] = d;
                }
            }

            var mesh = new Mesh();
            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.triangles = tris;
            return mesh;
        }

        private static Vector3 Round(Vector3 v)
        {
            const float inv = 100f;
            return new Vector3(
                Mathf.Round(v.x * inv) / inv,
                Mathf.Round(v.y * inv) / inv,
                Mathf.Round(v.z * inv) / inv);
        }
    }
}
