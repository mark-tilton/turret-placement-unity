using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementWidget : MonoBehaviour
{
    public GameObject _widgetPrefab;

	private Transform _widgetInstance = null;
    private Camera _camera;

	// Use this for initialization
    void Start()
    {
        _camera = transform.GetChild(1).GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(
            _camera.transform.position,
            _camera.transform.position + _camera.transform.forward * 100
        );
        RaycastHit rayResult;
        if (Physics.Raycast(
            _camera.transform.position,
            _camera.transform.forward,
            out rayResult
        ))
        {
			ShowWidget();
			_widgetInstance.position = rayResult.point;
			_widgetInstance.rotation = new Quaternion(
				rayResult.normal.x,
				rayResult.normal.y,
				rayResult.normal.z,
				0
			);
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
}
