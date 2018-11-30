using UnityEngine;

//ref : https://hackernoon.com/making-your-pixel-art-game-look-pixel-perfect-in-unity3d-3534963cad1d
public class SnapToGrid : MonoBehaviour
{
    public float PPU = 16f; // pixels per unit (your tile size)

    private void LateUpdate()
    {
        Vector3 position = transform.localPosition;

        position.x = (Mathf.Round(transform.parent.position.x * PPU) / PPU) - transform.parent.position.x;
        position.z = (Mathf.Round(transform.parent.position.z * PPU) / PPU) - transform.parent.position.z;

        transform.localPosition = position;
    }
}
