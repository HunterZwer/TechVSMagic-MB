using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinimapClickHandler : MonoBehaviour
{
    public GameObject mainCamera;  
    public RectTransform minimapRect;  
    public float cameraHeight = 10f;
    public Image clickIndicator;

    public float xCor;
    public float yCor;

    public void OnMinimapClick(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;
        if (pointerData == null) return;

        Vector2 clickPosition = pointerData.position;
        Vector2 worldPosition = WorldPositionFromMinimap(clickPosition);

        Vector3 newCameraPosition = new Vector3(worldPosition.x, cameraHeight, worldPosition.y);
        mainCamera.transform.position = newCameraPosition;

        ShowClickIndicator(clickPosition);
    }

    private Vector2 WorldPositionFromMinimap(Vector2 minimapClickPos)
    {
        Vector2 minimapSize = minimapRect.sizeDelta;

        float worldX = (minimapClickPos.x / minimapSize.x) * xCor; 
        float worldY = (minimapClickPos.y / minimapSize.y) * yCor;

        return new Vector2(worldX, worldY);
    }

    private void ShowClickIndicator(Vector2 minimapClickPos)
    {
        if (clickIndicator == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, minimapClickPos, null, out localPoint);

        clickIndicator.rectTransform.localPosition = localPoint;

        clickIndicator.gameObject.SetActive(true);
        StartCoroutine(HideClickIndicator());
    }

    private IEnumerator HideClickIndicator()
    {
        yield return new WaitForSeconds(0.5f); 
        clickIndicator.gameObject.SetActive(false);
    }
}
