using DG.Tweening;
using UnityEngine;

public class DragRotate : MonoBehaviour
{
    public LayerMask draggableLayer;
    public float rotationSpeed = 0.5f;
    public float snapAngle = 90f;   // snap theo bậc 45 độ
    public float snapDuration = 0.25f; // thời gian tween snap

    private Transform target;
    private bool isDragging = false;
    private Vector3 lastMousePos;

    void Update()
    {
        if (GamePlayManager.Ins.selectedBlock != null)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, draggableLayer))
            {
                target = hit.transform;
                isDragging = true;
                lastMousePos = Input.mousePosition;

                // Kill tween cũ nếu đang chạy
                target.DOKill();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            if (target != null)
            {
                float currentY = target.eulerAngles.y;
                float snappedY = Mathf.Round(currentY / snapAngle) * snapAngle;

                // Xoay mượt về góc snap bằng DOTween
                target.DORotate(
    new Vector3(target.eulerAngles.x, snappedY, target.eulerAngles.z),
    snapDuration,
    RotateMode.Fast
)
.SetEase(Ease.OutBack); // hiệu ứng nảy

                // cho hiệu ứng bật nảy đẹp hơn

                target = null;
            }
        }

        if (isDragging && target != null)
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            target.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
            lastMousePos = Input.mousePosition;
        }
    }
}
