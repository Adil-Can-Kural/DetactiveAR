using UnityEngine;
using UnityEngine.InputSystem; // ✅ Yeni sistem

public class SaatFinalTrigger : MonoBehaviour
{
    public Transform hedef;
    private bool tetiklendi = false;

    void Update()
    {
        if (tetiklendi) return;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(touchPosition);
            TryRaycast(ray);
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            TryRaycast(ray);
        }
    }

    private void TryRaycast(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                tetiklendi = true;
                Debug.Log("✅ Yeni sistemde parça tıklandı!");
                GetComponent<SaatParcaMover>()?.BaslatHareket(hedef);
            }
        }
    }
}
