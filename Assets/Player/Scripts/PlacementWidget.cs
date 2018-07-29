using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public int _maxIterations = 500;

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
		Debug.DrawLine(_camera.transform.position, 
		_camera.transform.position + _camera.transform.forward * 100);
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
        	_widgetInstance.localScale = _baseWidgetScale * _radius;

            var renderer = _widgetInstance.gameObject.GetComponent<Renderer>();
            if (AdjustPlacement())
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

    bool AdjustPlacement()
    {
        float rMax = _radius / 2;
        var tMax = Mathf.PI * 2;

        var rCount = _radialTestCount;
        var tCount = _thetaTestCount;

        var direction = _widgetInstance.rotation
                    * new Vector3(0, -1, 0);

        var offset = Vector3.zero;
        for (int i = 0; i < _maxIterations; i++)
        {
            var slices = new List<List<Vector3>>();
            for (int ti = 0; ti < tCount; ti++)
            {
                var slice = new List<Vector3>();
                slices.Add(slice);
                for (int ri = 1; ri <= rCount; ri++)
                {
                    var r = (ri / (float)rCount) * rMax;
                    var t = (ti / (float)tCount) * tMax;

                    var x = r * Mathf.Cos(t);
                    var y = r * Mathf.Sin(t);
                    var testPoint = new Vector3(x, 0, y);
                    var origin = _widgetInstance.rotation
                               * (testPoint + offset)
                               + _widgetInstance.position;

                    Debug.DrawLine(origin, origin + direction * _downLength);
                    if (!Physics.Raycast(
                        origin,
                        direction,
                        _downLength
                    ))
					{
						slice.Add(testPoint);
					}
                }
            }

            if (slices.All(x => x.Count == 0))
            {
                _widgetInstance.position = _widgetInstance.rotation
                                         * offset
                                         + _widgetInstance.position;
                return true;
            }

			var correctionVector = Vector3.zero;
			var correctionAmount = 0f;
			Vector3 maxStart = Vector3.zero;
			Vector3 maxEnd = Vector3.zero;
			Vector3 maxEdge = Vector3.zero;
			foreach (var slice in slices.Where(x => x.Count != 0))
			{
				var farthestPoint = Vector3.zero;
				var farthestMag = 0f;
				foreach (var vector in slice)
				{
					var r = vector.magnitude;
					if (r > farthestMag)
					{
						farthestMag = r;
						farthestPoint = vector;
					}
				}
				var start = _widgetInstance.rotation
						  * offset
						  + _widgetInstance.position;
				var end = _widgetInstance.rotation
						* (farthestPoint + offset)
						+ _widgetInstance.position;
				var edge = FindEdge(
					start,
					end,
					direction,
					_downLength,
					0.0f,
					true
				);
				var distance = (end - edge).magnitude;
				if (distance > correctionAmount)
				{
					correctionAmount = distance;
					correctionVector = farthestPoint.normalized;
					maxStart = start;
					maxEnd= end;
					maxEdge = edge;
				}
			}
			Debug.DrawLine(maxEnd, maxEnd + -direction * 1, Color.red);
			Debug.DrawLine(maxEdge, maxEdge + -direction * 1, Color.blue);

			correctionAmount = Mathf.Max(correctionAmount, 0.0001f);

			var oldOffset = offset;
            offset -= correctionAmount * correctionVector;

			// TESTING CODE
            var oldOffsetPos = _widgetInstance.rotation
                      * oldOffset
                      + _widgetInstance.position;
            var newOffsetPos = _widgetInstance.rotation
                      * offset
                      + _widgetInstance.position;
			Debug.DrawLine(oldOffsetPos, newOffsetPos, Color.black);

        }
		_widgetInstance.position = _widgetInstance.rotation
								 * offset
								 + _widgetInstance.position;
        return false;
    }

	Vector3 ToPolar(Vector3 point)
	{
		return new Vector3(
			point.magnitude, 
			Mathf.Atan2(point.y, point.x), 
			point.z
		);
	}

	Vector3 ToCartesian(Vector3 point)
	{
		return new Vector3(
			point.x * Mathf.Cos(point.y),
			point.x * Mathf.Sin(point.y),
			point.z
		);
	}

    Vector3 FindEdge(
        Vector3 start,
        Vector3 end,
        Vector3 direction,
        float depth,
        float tolerance,
        bool requireHit)
    {
        var lastHit = start;
        var lastMiss = end;

        var lastTestWasHit = false;
        var distance = tolerance + 1;

        Vector3 currentPoint = Vector3.zero;
        var nextPoint = start;
        while (distance > tolerance || (requireHit && !lastTestWasHit))
        {
            currentPoint = nextPoint;
        	Debug.DrawLine(currentPoint, currentPoint + direction * depth, Color.green);
            if (Physics.Raycast(
                currentPoint,
                direction,
                depth
            ))
            {
                lastHit = currentPoint;
                lastTestWasHit = true;
                nextPoint = (currentPoint + lastMiss) / 2.0f;
            }
            else
            {
                lastMiss = currentPoint;
                lastTestWasHit = false;
                nextPoint = (currentPoint + lastHit) / 2.0f;
            }
            distance = (currentPoint - nextPoint).magnitude;
			if (distance == 0)
			{
				break;
			}
        }
        return currentPoint;
    }
}
