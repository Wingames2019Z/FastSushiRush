using System.Collections.Generic;
using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class Waypoints : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private Color pointColor = Color.yellow;
        [SerializeField] private Color lineColor = Color.blue;
        [SerializeField] private float pointSize = 0.3f;

        void OnDrawGizmos()
        {
            Gizmos.color = pointColor;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                Gizmos.DrawSphere(child.position, pointSize);
            }

            Gizmos.color = lineColor;
            for (int i = 0; i < transform.childCount - 1; i++)
            {
                Transform startPoint = transform.GetChild(i);
                Transform endPoint = transform.GetChild(i + 1);
                Gizmos.DrawLine(startPoint.position, endPoint.position);
            }
        }
#endif
        public Transform GetPoint(int index)
        {
            return transform.GetChild(index);
        }

        public List<Vector3> GetPoints()
        {
            var points = new List<Vector3>();

            foreach (Transform child in transform)
            {
                points.Add(child.position);
            }

            return points;
        }
    }
}
