using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrackShot
{
    public static class CyberFx
    {
        public static readonly Color Cyan = new Color(0f, 0.9f, 1f, 1f);
        public static readonly Color White = new Color(0.95f, 1f, 1f, 1f);
        public static readonly Color Pink = new Color(1f, 0.1f, 0.7f, 1f);

        public static readonly Color MutedCyan = new Color(0.2f, 0.55f, 0.6f);
        public static readonly Color MutedPink = new Color(0.6f, 0.2f, 0.45f);

        public static readonly string CyanHex = ColorUtility.ToHtmlStringRGB(Cyan);
        public static readonly string PinkHex = ColorUtility.ToHtmlStringRGB(Pink);
        public static readonly string WhiteHex = ColorUtility.ToHtmlStringRGB(White);

        public const float CycleSpeed = 0.28f;

        public static Color WithAlpha(this Color c, float a) => new Color(c.r, c.g, c.b, a);

        public static float RandomPhase() => Random.Range(0f, Mathf.PI * 2f);

        public static Color AltColor(bool pink) => pink ? Pink : Cyan;

        public static float SigmoidEase01(float t, float exponent)
        {
            float x = t * 2f - 1f;
            float shaped = Mathf.Sign(x) * Mathf.Pow(Mathf.Abs(x), exponent);
            return (shaped + 1f) * 0.5f;
        }

        public static Color LerpViaWhite(Color from, Color to, float t) =>
            t < 0.5f
                ? Color.Lerp(from, White, t * 2f)
                : Color.Lerp(White, to, (t - 0.5f) * 2f);

        public static Color GetSigmoidColor(float time, float speed = 0.08f, float exponent = 0.65f)
        {
            float raw = Mathf.PingPong(time * speed, 1f);
            float c = SigmoidEase01(raw, exponent);
            return LerpViaWhite(Cyan, Pink, c);
        }

        public static Color Current => GetSigmoidColor(Time.time);

        public static Color StageOrCurrentColor =>
            CyberStageEffect.Instance != null
                ? CyberStageEffect.Instance.CurrentColor
                : Current;

        public static Color GetColor(float t) => GetSigmoidColor(t, CycleSpeed, 1f);

        private static Texture2D _glowTex;

        public static Texture2D GlowTexture
        {
            get
            {
                if (_glowTex != null)
                {
                    return _glowTex;
                }
                const int h = 64;
                _glowTex = new Texture2D(1, h, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                };
                for (int i = 0; i < h; i++)
                {
                    float t = Mathf.Abs(i / ((h - 1) * 0.5f) - 1f);
                    float a = Mathf.Pow(Mathf.Max(0f, 1f - t), 1.4f);
                    _glowTex.SetPixel(0, i, new Color(1f, 1f, 1f, a));
                }
                _glowTex.Apply();
                return _glowTex;
            }
        }

        public static void ApplyColor(Material mat, Color color)
        {
            mat.SetColor("_BaseColor", color);
            mat.SetColor("_Color", color);
            mat.color = color;
        }

        public static void NoShadow(Renderer r)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        private static Shader _unlit;

        public static Shader Unlit =>
            _unlit != null ? _unlit
            : (_unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));

        public static void MakeAdditive(Material mat)
        {
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        public static Material CreateUnlitMaterial(Color color) => new Material(Unlit) { color = color };

        public static Material CreateAdditiveMaterial(Color color)
        {
            var mat = new Material(Unlit);
            mat.SetFloat("_Surface", 1f);
            MakeAdditive(mat);
            mat.color = color;
            return mat;
        }

        public static Material CreateTubeMaterial(Color color)
        {
            var mat = CreateAdditiveMaterial(color);
            mat.mainTexture = GlowTexture;
            mat.SetColor("_BaseColor", color);
            mat.SetColor("_Color", color);
            return mat;
        }

        public static Mesh BuildDoubleSidedQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            var mesh = new Mesh();
            mesh.vertices = new[] { v0, v1, v2, v3 };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3,
                                     0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();
            return mesh;
        }

        public static Vector3[] GetShapeVertices(int type, float size, float heightRatio = 1f)
        {
            switch (type)
            {
                case 0:
                    return new[]
                    {
                        new Vector3(0f, size, 0f),
                        new Vector3( size * 0.866f, -size * 0.5f, 0f),
                        new Vector3(-size * 0.866f, -size * 0.5f, 0f),
                    };
                case 1:
                    float h = size * heightRatio;
                    return new[]
                    {
                        new Vector3(-size, h, 0f),
                        new Vector3( size, h, 0f),
                        new Vector3( size, -h, 0f),
                        new Vector3(-size, -h, 0f),
                    };
                default:
                    return new[]
                    {
                        new Vector3(0f, size * 1.3f, 0f),
                        new Vector3( size, 0f, 0f),
                        new Vector3(0f, -size * 1.3f, 0f),
                        new Vector3(-size, 0f, 0f),
                    };
            }
        }

        public static LineRenderer AddLine(GameObject go, Material mat, bool worldSpace, bool loop = false, float width = -1f)
        {
            var lr = go.AddComponent<LineRenderer>();
            lr.material = mat;
            lr.useWorldSpace = worldSpace;
            lr.loop = loop;
            if (width >= 0f)
            {
                lr.startWidth = lr.endWidth = width;
            }
            NoShadow(lr);
            return lr;
        }

        public static void SetNeonShape(LineRenderer lr, int type, float size)
        {
            lr.startWidth = lr.endWidth = size * 0.15f;
            var verts = GetShapeVertices(type, size);
            lr.positionCount = verts.Length;
            for (int i = 0; i < verts.Length; i++)
            {
                lr.SetPosition(i, verts[i]);
            }
        }

        public static void SetCirclePositions(LineRenderer lr, int segments, float radius, bool xzPlane, Vector3 center = default)
        {
            lr.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float a = 2f * Mathf.PI * i / segments;
                float c = Mathf.Cos(a) * radius;
                float s = Mathf.Sin(a) * radius;
                lr.SetPosition(i, center + (xzPlane ? new Vector3(c, 0f, s) : new Vector3(c, s, 0f)));
            }
        }

        public static IEnumerator Tween(float duration, System.Action<float> onUpdate)
        {
            if (duration <= 0f)
            {
                onUpdate(1f);
                yield break;
            }
            float e = 0f;
            while (e < duration)
            {
                e += Time.deltaTime;
                onUpdate(Mathf.Clamp01(e / duration));
                yield return null;
            }
            onUpdate(1f);
        }

        public class FloatingShape
        {
            public GameObject Go;
            public Material Mat;
            public Vector3 RotAxis;
            public float RotSpeed;
            public float FloatPhase;
        }

        public static void AnimateFloatingShape(FloatingShape shape, float t)
        {
            shape.Go.transform.Rotate(shape.RotAxis, shape.RotSpeed * Time.deltaTime, Space.World);
            ApplyColor(shape.Mat, GetColor(t + shape.FloatPhase * 2f).WithAlpha(0.7f));
        }

        public class ShapeParticle
        {
            public GameObject Go;
            public Material Mat;
            public Vector3 Velocity;
            public Vector3 RotAxis;
            public float RotSpeed;
            public float Phase;
        }

        public static ShapeParticle CreateShapeParticle(string name, Vector3 origin, float size, float rotSpeedMin, float rotSpeedMax)
        {
            var go = new GameObject(name);
            go.transform.position = origin;

            var mat = CreateAdditiveMaterial(Cyan);

            var lr = AddLine(go, mat, worldSpace: false, loop: true);
            SetNeonShape(lr, Random.Range(0, 3), size);

            return new ShapeParticle
            {
                Go = go,
                Mat = mat,
                RotAxis = Random.onUnitSphere,
                RotSpeed = Random.Range(rotSpeedMin, rotSpeedMax),
            };
        }

        public static IEnumerator AnimateAndDestroyParticles(
            IEnumerable<ShapeParticle> particles, float lifetime, float velocityDamping,
            System.Func<float, float> alphaFunc, System.Func<ShapeParticle, float, Color> colorFunc)
        {
            yield return Tween(lifetime, t =>
            {
                float elapsed = t * lifetime;
                float alpha = alphaFunc(t);

                foreach (var p in particles)
                {
                    if (p.Go == null)
                    {
                        continue;
                    }
                    p.Velocity = Vector3.Lerp(p.Velocity, Vector3.zero, Time.deltaTime * velocityDamping);
                    p.Go.transform.position += p.Velocity * Time.deltaTime;
                    p.Go.transform.Rotate(p.RotAxis, p.RotSpeed * Time.deltaTime, Space.World);

                    ApplyColor(p.Mat, colorFunc(p, elapsed).WithAlpha(alpha));
                }
            });

            foreach (var p in particles)
            {
                if (p.Go != null)
                {
                    Object.Destroy(p.Go);
                }
            }
        }

        private static readonly Gradient TrailGradient = new Gradient();
        private static readonly GradientColorKey[] TrailColorKeys = new GradientColorKey[3];
        private static readonly GradientAlphaKey[] TrailAlphaKeys =
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(0.8f, 0.3f),
            new GradientAlphaKey(0f, 1f),
        };

        public static void SetupTrail(TrailRenderer trail, Material mat, float time, float width, float minVertexDistance)
        {
            trail.material = mat;
            trail.time = time;
            trail.startWidth = width;
            trail.endWidth = 0f;
            trail.minVertexDistance = minVertexDistance;
            NoShadow(trail);
        }

        public static void ApplyTrailGradient(TrailRenderer trail, Color color)
        {
            TrailColorKeys[0] = new GradientColorKey(Color.white, 0f);
            TrailColorKeys[1] = new GradientColorKey(color, 0.25f);
            TrailColorKeys[2] = new GradientColorKey(color, 1f);
            TrailGradient.SetKeys(TrailColorKeys, TrailAlphaKeys);
            trail.colorGradient = TrailGradient;
        }
    }
}
