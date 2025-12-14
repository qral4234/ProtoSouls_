using UnityEngine;
using UnityEngine.UI;

public class RageBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxRage(float maxRage)
    {
        slider.maxValue = maxRage;
        slider.value = 0; // Usually starts empty? Or keeps current.
    }

    public void SetCurrentRage(float currentRage)
    {
        slider.value = currentRage;
    }
}
