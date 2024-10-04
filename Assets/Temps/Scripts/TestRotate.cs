using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRotate : MonoBehaviour
{
    public int count = 5;
    public GameObject testObj;
    public Transform PointA;
    public Transform PointB;
    public LineRenderer Line;
    public ParticleSystem particle;

    private List<Transform> _points = new();

    private void Start()
    {
        TestSpawnAlign();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            particle.Stop();
            particle.Play();
        }

        AlignContinuous();
    }

    private void TestSpawnAlign()
    {
        Vector3 offset = PointB.position - PointA.position;
        Vector3 segment = offset / (count + 1);

        _points.Add(PointA);
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = PointA.position + segment * (i + 1);
            var x = Instantiate(testObj, pos, Quaternion.identity);
            _points.Add(x.transform);
        }

        _points.Add(PointB);
    }

    private void AlignContinuous()
    {
        Vector3 offset = PointB.position - PointA.position;
        Vector3 segment = offset / (count + 1);
        Line.positionCount = _points.Count;
        Line.SetPosition(0, _points[0].position);
        Line.SetPosition(_points.Count - 1, _points[_points.Count - 1].position);

        for (int i = 1; i <= count; i++)
        {
            Vector3 pos = PointA.position + segment * i;
            Vector3 sinVector = Vector3.up * Mathf.Sin(90 * Mathf.Deg2Rad * Time.time + i * 30 * Mathf.Deg2Rad);

            float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            sinVector = Quaternion.Euler(0, 0, angle) * sinVector;

            _points[i].position = sinVector + pos;
            Line.SetPosition(i, _points[i].position);
        }

    }
}
