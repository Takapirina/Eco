using UnityEngine;

public class MenuFollow : MonoBehaviour
{
    public Camera cam;                 
    public RectTransform rect;        
    public Vector2 offset = new Vector2(0f, 120f);

    private Transform target;

    private void Awake()
    {
        if (!rect) rect = GetComponent<RectTransform>();
        if (!cam) cam = Camera.main;
        gameObject.SetActive(false);
    }

    public void Show(Transform target)
    {
        this.target = target;

        UpdatePos();         
        gameObject.SetActive(true);
        UpdatePos();      
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        target = null;
    }

    private void LateUpdate()
    {
        if (target != null && gameObject.activeSelf)
            UpdatePos();
    }

    private void UpdatePos()
    {
        if (!cam || !rect || !target) return;

        Vector3 screen = cam.WorldToScreenPoint(target.position);
        rect.position = (Vector2)screen + offset;
    }
}