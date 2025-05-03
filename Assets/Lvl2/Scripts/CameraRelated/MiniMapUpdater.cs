using UnityEngine;
using System.Collections;

namespace CameraRelated
{

    public class MinimapUpdater : MonoBehaviour
    {
        private Camera minimapCamera;

        void Start()
        {
            GameObject minimapObject = GameObject.FindWithTag("MiniMap");
            if (minimapObject != null)
            {
                minimapCamera = minimapObject.GetComponent<Camera>();
            }
            else
            {
                Debug.LogWarning("No camera found with the tag 'MiniMap'!");
            }
            
            StartCoroutine(UpdateMinimapRoutine());
        }

        IEnumerator UpdateMinimapRoutine()
        {
            while (true)
            {
                minimapCamera.enabled = true;  // Enable camera to render
                yield return new WaitForEndOfFrame(); // Wait for 1 frame to render it
                minimapCamera.enabled = false; // Disable it after rendering

                yield return new WaitForSeconds(0.1f); // Wait 0.1s before next update
            }
        }
    }

}