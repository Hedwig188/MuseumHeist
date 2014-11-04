using UnityEngine;
using System.Collections;

public class DragLight_VLS : MonoBehaviour 
{
    Vector3 offset = Vector3.zero;
    void OnMouseDown()
    {
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition); 
        CreateAtClick_2DVLS.CurrentLight = gameObject.GetComponent<Light2DRadial>();
    }

    void OnMouseDrag()
    {
        Vector3 p = offset + Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(p.x, p.y, 0);
    }
}
