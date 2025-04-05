using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject buildingPreviewPrefab;
    public LayerMask groundLayer;
    public LayerMask unitLayer;
    public LayerMask buildingLayer;
    private GameObject buildingPreview;
    private GameObject selectedBuilding;
    private bool isPlacing = false;

    private void Update()
    {
        if (isPlacing)
        {
            UpdateBuildingPreviewPosition();
            if (Input.GetMouseButtonDown(0)) // Подтверждение постройки
            {
                PlaceBuilding();
            }
        }
    }

    public void StartBuilding(GameObject building)
    {
        selectedBuilding = building;
        isPlacing = true;
        buildingPreview = Instantiate(buildingPreviewPrefab);
        buildingPreview.SetActive(true);
    }

    private void UpdateBuildingPreviewPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector3 position = hit.point;
            position.y = 0.9f;
            buildingPreview.transform.position = position;

            bool canBuild = !Physics.CheckBox(position, new Vector3(2f, 0.5f, 2f), Quaternion.identity, unitLayer | buildingLayer);

            // Обновляем все рендереры в `buildingPreview`
            Renderer[] renderers = buildingPreview.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                foreach (Material mat in rend.materials)
                {
                    mat.color = canBuild ? Color.green : Color.red;
                }
            }
        }
    }

    private void PlaceBuilding()
    {
        if (buildingPreview.GetComponent<Renderer>().material.color == Color.green)
        {
            Instantiate(selectedBuilding, buildingPreview.transform.position, Quaternion.identity);
            Destroy(buildingPreview);  // Убираем превью
            isPlacing = false;
        }
    }
}
