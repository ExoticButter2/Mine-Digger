using System.Collections;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _fpsTextLabel;
    private bool _updateFpsEnabled = true;
    public float fpsUpdateInterval = 1f;

    private void Start()
    {
        StartCoroutine(UpdateFpsLabelOnInterval());
    }

    private void UpdateFpsCounter()
    {
        float fps = 1 / Time.deltaTime;
        int roundedFps = (int)fps;

        if (_fpsTextLabel == null)
        {
            Debug.LogWarning("FPS Text label is not found in fps counter");
            return;
        }

        _fpsTextLabel.text = $"{roundedFps.ToString()} FPS";
    }

    private IEnumerator UpdateFpsLabelOnInterval()
    {
        while (_updateFpsEnabled)
        {
            yield return new WaitForSeconds(fpsUpdateInterval);
            UpdateFpsCounter();
        }
    }
}