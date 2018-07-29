using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementWidget : MonoBehaviour
{
    public GameObject _widgetPrefab;
    public float _hoverAmount = 0.01f;
    public float _radius = 1;
	public float _downLength = 0.1f;
	public float _upLength = 2f;
	public int _radialTestCount = 10;
	public int _thetaTestCount = 36;

    private Transform _widgetInstance = null;
    private Camera _camera;
    private Vector3 _baseWidgetRotation;
	private Vector3 _baseWidgetScale;

    // Use this for initialization
    void Start()
    {
        _camera = transform.GetChild(1).GetComponent<Camera>();
        _baseWidgetRotation = _widgetPrefab.transform.rotation.eulerAngles;
        _baseWidgetScale = _widgetPrefab.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {		
        _widgetPrefab.transform.localScale = _baseWidgetScale * _radius;
        RaycastHit rayResult;
        if (Physics.Raycast(
            _camera.transform.position,
            _camera.transform.forward,
            out rayResult
        ))
        {
            ShowWidget();
            _widgetInstance.position = rayResult.point
                                     + rayResult.normal
                                     * _hoverAmount;
            _widgetInstance.rotation =
            _widgetInstance.rotation = Quaternion.Euler(
                Quaternion.LookRotation(rayResult.normal).eulerAngles
                + _baseWidgetRotation
            );
            var renderer = _widgetInstance.gameObject.GetComponent<Renderer>();
            if (TestPlacement())
            {
                renderer.material.color = Color.green;
            }
            else
            {
                renderer.material.color = Color.red;
            }
        }
        else
        {
            HideWidget();
        }
    }

    void ShowWidget()
    {
        if (_widgetInstance == null)
        {
            _widgetInstance = Instantiate(_widgetPrefab).transform;
        }
    }

    void HideWidget()
    {
        if (_widgetInstance != null)
        {
            Destroy(_widgetInstance.gameObject, 0);
            _widgetInstance = null;
        }
    }

    bool TestPlacement()
    {
        float rMax = _radius / 2;
        var tMax = Mathf.PI * 2;

        var rCount = _radialTestCount;
        var tCount = _thetaTestCount;

        for (int ri = 1; ri <= rCount; ri++)
            for (int ti = 0; ti < tCount; ti++)
            {
                var r = (ri / (float)rCount) * rMax;
                var t = (ti / (float)tCount) * tMax;

                var x = r * Mathf.Cos(t);
                var y = r * Mathf.Sin(t);
                var origin = _widgetInstance.rotation
                           * new Vector3(x, 0, y)
                           + _widgetInstance.position;
                var direction = _widgetInstance.rotation
                              * new Vector3(0, -1, 0);

                Debug.DrawLine(origin, origin + direction * _downLength);
                if (!Physics.Raycast(
                    origin,
                    direction,
                    _downLength
                ))
                {
                    return false;
                }
            }
        return true;
    }
}
