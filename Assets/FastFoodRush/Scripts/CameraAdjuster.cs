using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraAdjuster : MonoBehaviour
{
    [SerializeField] float _minRate = 3f;
    [SerializeField] float _maxRate = 20f;
    [SerializeField] float _speed = 1f;
    bool _isZooming = false;
    int _direction = 1;
    void Update()
    {
        if (_isZooming && Camera.main.orthographicSize >= _minRate  && Camera.main.orthographicSize <= _maxRate)
        {
            Camera.main.orthographicSize += Time.deltaTime * _direction * _speed;
        }

        if(Camera.main.orthographicSize < _minRate)
        {
            Camera.main.orthographicSize = _minRate;
        }
        else if (Camera.main.orthographicSize > _maxRate)
        {
            Camera.main.orthographicSize = _maxRate;
        }
    }
    public void OnButtonDown(int direction)
    {
        _direction = direction;
        _isZooming = true;
    }
    public void OnButtonUp()
    {
        _direction = 0;
        _isZooming = false;
    }
}
