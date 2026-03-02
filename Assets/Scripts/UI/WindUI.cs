using UnityEngine;

public class WindUI : MonoBehaviour
{
    private void OnEnable()
    {
        WindSystem.OnWindChanged += UpdateWindUI;
    }

    private void OnDisable()
    {
        WindSystem.OnWindChanged -= UpdateWindUI;
    }

    private void UpdateWindUI(Vector2 newWindStrength)
    {
        float angle = Vector2.SignedAngle(Vector2.up, newWindStrength);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
