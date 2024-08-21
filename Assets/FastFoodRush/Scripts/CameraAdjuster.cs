using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraAdjuster : MonoBehaviour
{
    public Vector2 startPos;
    public Vector2 mouseStartPos;
    float dist0 = 0f;
    float dist1 = 0f;
    float scale = 0f;
    float oldDist = 0f;//前回の2点間の距離
    float minRate = 3f;
    float maxRate = 20f;
    float cameraSize = 0f;
    void Update()
    {
        if (Input.touchCount >= 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);
            if (t2.phase == TouchPhase.Began)
            {
                dist0 = Vector2.Distance(t1.position, t2.position);
                oldDist = dist0;
            }
            else if (t1.phase == TouchPhase.Moved && t2.phase == TouchPhase.Moved)
            {
                dist1 = Vector2.Distance(t1.position, t2.position);
                if (dist0 < 0.001f || dist1 < 0.001f)
                {
                    return;
                }
                else
                {
                    cameraSize = Camera.main.orthographicSize;
                    scale = cameraSize;
                    scale += (dist1 - oldDist) / 200f;
                    if (scale > maxRate) { scale = maxRate; }
                    if (scale < minRate) { scale = minRate; }
                    oldDist = dist1;
                }
                Camera.main.orthographicSize = scale;
            }
        }
    }
}
