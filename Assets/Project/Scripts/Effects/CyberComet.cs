using UnityEngine;

namespace CrackShot
{
    public class CyberComet
    {
        public GameObject Go;
        public TrailRenderer Trail;
        public Material Mat;
        public float OrbitRadiusB;
        public float RadiusBase;
        public float RadiusPhase;
        public float ColorPhase;
        public float Speed;
        public float Angle;
        public Quaternion Tilt;
        public Vector3 DriftAxis;
        public float DriftSpeed;

        public static CyberComet Create(GameObject go, TrailRenderer trail, Material mat,
            float orbitRadius, float orbitRadiusBFactorMin, float orbitRadiusBFactorMax,
            float speedMin, float speedMax, float startAngle)
        {
            return new CyberComet
            {
                Go = go,
                Trail = trail,
                Mat = mat,
                RadiusBase = orbitRadius,
                OrbitRadiusB = orbitRadius * Random.Range(orbitRadiusBFactorMin, orbitRadiusBFactorMax),
                RadiusPhase = CyberFx.RandomPhase(),
                ColorPhase = Random.Range(0f, 10f),
                Speed = Random.Range(speedMin, speedMax) * (Random.value > 0.5f ? 1f : -1f),
                Angle = startAngle,
                Tilt = Quaternion.Euler(Random.Range(-80f, 80f), Random.Range(0f, 360f), Random.Range(-40f, 40f)),
                DriftAxis = Random.onUnitSphere,
                DriftSpeed = Random.Range(3f, 12f),
            };
        }

        public Vector3 Advance(float time, float deltaTime, float yAmplitude)
        {
            Angle += Speed * deltaTime;
            Tilt = Quaternion.AngleAxis(DriftSpeed * deltaTime, DriftAxis) * Tilt;

            float drift = 1f + 0.3f * Mathf.Sin(time * 0.07f + RadiusPhase);
            float r = RadiusBase * drift;
            float b = OrbitRadiusB * drift;
            float rad = Angle * Mathf.Deg2Rad;
            return Tilt * new Vector3(Mathf.Cos(rad) * r, Mathf.Sin(rad) * yAmplitude, Mathf.Sin(rad) * b);
        }

        public void ApplyColor(float time)
        {
            Color col = CyberFx.GetColor(time * 0.5f + ColorPhase).WithAlpha(1f);
            CyberFx.ApplyColor(Mat, col);
            CyberFx.ApplyTrailGradient(Trail, col);
        }
    }
}
