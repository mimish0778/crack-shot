using UnityEngine;

namespace CrackShot
{
    public class DragShotEffects
    {
        private readonly LineRenderer _line;
        private readonly Material _lineMat;
        private readonly int _arrowSegments;

        private readonly GameObject _cancelZone;
        private readonly LineRenderer _cancelRing;
        private readonly Material _cancelMat;
        private readonly LineRenderer _cancelScan;
        private readonly Material _cancelScanMat;
        private readonly float _cancelAnimSpeed;
        private readonly int _cancelSegments;

        private float _scanAngle;
        private float _ringScale = 1f;

        public DragShotEffects(LineRenderer arrowLine, int arrowSegments, float arrowWidthStart, float arrowWidthEnd,
            int cancelSegments, float cancelRingWidth, float cancelAnimSpeed)
        {
            _arrowSegments = arrowSegments;
            _cancelSegments = cancelSegments;
            _cancelAnimSpeed = cancelAnimSpeed;

            _line = arrowLine;
            _line.startWidth = arrowWidthStart;
            _line.endWidth = arrowWidthEnd;
            _line.positionCount = 0;
            _line.useWorldSpace = true;
            _line.numCapVertices = 4;

            _lineMat = CyberFx.CreateTubeMaterial(CyberFx.Cyan);
            _line.material = _lineMat;

            _cancelZone = new GameObject("CancelZone");

            var ringGo = new GameObject("Ring");
            ringGo.transform.SetParent(_cancelZone.transform);
            _cancelMat = CyberFx.CreateTubeMaterial(CyberFx.Cyan);
            _cancelRing = CyberFx.AddLine(ringGo, _cancelMat, worldSpace: true, loop: true, width: cancelRingWidth);
            _cancelRing.textureMode = LineTextureMode.Tile;
            _cancelRing.positionCount = cancelSegments;

            var scanGo = new GameObject("Scan");
            scanGo.transform.SetParent(_cancelZone.transform);
            _cancelScanMat = CyberFx.CreateTubeMaterial(CyberFx.Cyan);
            _cancelScan = CyberFx.AddLine(scanGo, _cancelScanMat, worldSpace: true);
            _cancelScan.positionCount = 2;
            _cancelScan.startWidth = cancelRingWidth * 0.5f;
            _cancelScan.endWidth = 0f;
            _cancelScan.textureMode = LineTextureMode.Tile;

            _cancelZone.SetActive(false);
        }

        public void DrawArrow(Vector3 dragStart, Vector3 dragCurrent, Vector3 ballPos, float ballRadius)
        {
            Color col = CyberFx.Current;
            CyberFx.ApplyColor(_lineMat, col);

            Vector3 dir = -(dragCurrent - dragStart);
            float len = dir.magnitude * 2f;
            Vector3 dirN = dir.normalized;

            _line.positionCount = _arrowSegments;
            Vector3 origin = ballPos + dirN * ballRadius;
            for (int i = 0; i < _arrowSegments; i++)
            {
                float t = (float)i / (_arrowSegments - 1);
                _line.SetPosition(i, origin + dirN * (len * t));
            }
        }

        public void ClearArrow() => _line.positionCount = 0;

        public void HideCancelZone() => _cancelZone.SetActive(false);

        public void UpdateCancelZone(Vector3 ballPos, float cancelRadius, bool inZone)
        {
            _cancelZone.SetActive(true);

            float targetScale = inZone ? 1.2f : 1f;
            _ringScale = Mathf.Lerp(_ringScale, targetScale, Time.deltaTime * _cancelAnimSpeed);

            float r = cancelRadius * _ringScale;
            Vector3 pos = ballPos + Vector3.up * 0.02f;

            CyberFx.SetCirclePositions(_cancelRing, _cancelSegments, r, xzPlane: true, center: pos);

            _scanAngle += Time.deltaTime * (inZone ? 300f : 180f);
            float sr = _scanAngle * Mathf.Deg2Rad;
            _cancelScan.SetPosition(0, pos);
            _cancelScan.SetPosition(1, pos + new Vector3(Mathf.Cos(sr) * r, 0, Mathf.Sin(sr) * r));

            Color col = CyberFx.Current;
            CyberFx.ApplyColor(_cancelMat, col.WithAlpha(inZone ? 1f : 0.7f));
            CyberFx.ApplyColor(_cancelScanMat, col.WithAlpha(inZone ? 0.9f : 0.5f));
        }
    }
}
