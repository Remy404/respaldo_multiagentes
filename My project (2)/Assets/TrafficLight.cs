using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    private Renderer renderer;

    void Start()
    {
        renderer = GetComponent<Renderer>(); // Obtener el componente Renderer
    }

    public void SetState(bool state)
    {
        if (renderer != null)
        {
            renderer.material.color = state ? Color.red : Color.green; // Verde si es false, rojo si es true
        }
    }
}