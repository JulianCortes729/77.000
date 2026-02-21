using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    private GridSystem gridSystem; // Referencia al sistema de cuadrícula
    private RectTransform rectTransform; // RectTransform del área interactiva

    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 5f;
    private float currentZoom = 1f; // Empezamos en escala 1 (230.000 píxeles reales)
    [SerializeField] private Camera miCamara;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridSystem = GetComponent<GridSystem>(); // Cachea GridSystem adjunto
        rectTransform = GetComponent<RectTransform>(); // Cachea el RectTransform
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            if(!GetLocalPointFromMouseClick(out Vector2 localPoint)) return; // Obtiene el punto local del clic derecho
            DrawFirewall(localPoint); // Dibuja cortafuegos en la cuadrícula según el punto local
        }

        
        float scroll = Mouse.current.scroll.ReadValue().y; // Lee el scroll del ratón
        
        if (scroll != 0)
        {
            currentZoom += scroll * zoomSpeed; // Aplica la velocidad de zoom

            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom); // Limita el zoom entre los valores mínimo y máximo

            rectTransform.localScale = Vector3.one * currentZoom; // Aplica el zoom a la escala del RectTransform
        }

    }

    bool GetLocalPointFromMouseClick(out Vector2 localPoint)
    {
        Vector2 posClick = Mouse.current.position.ReadValue(); // Posición del ratón en pantalla
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, posClick, miCamara, out localPoint); // Pantalla -> local UI
    }

    // Aquí traduciremos la coordenada y cambiaremos el estado
    private void DrawFirewall(Vector2 localPoint)
    {

       

        localPoint.x += rectTransform.rect.width / 2; // Ajusta origen X (centro del rect)
        localPoint.y += rectTransform.rect.height / 2; // Ajusta origen Y (centro del rect)

        float porcentX = localPoint.x / rectTransform.rect.width; // Normaliza X a [0,1]
        float gridX = gridSystem.Width * porcentX; // Mapea a coordenada X de la cuadrícula

        float porcentY = localPoint.y / rectTransform.rect.height; // Normaliza Y a [0,1]
        float gridY = gridSystem.Height * porcentY; // Mapea a coordenada Y de la cuadrícula

        if(gridX < 0 || gridX >= gridSystem.Width || gridY < 0 || gridY >= gridSystem.Height)
        {
            return; // Fuera de los límites de la cuadrícula
        }
        int x = Mathf.FloorToInt(gridX); // Coordenada entera X
        int y = Mathf.FloorToInt(gridY); // Coordenada entera Y
        gridSystem.ChangeHectareState(StateHectare.Firewall, x, y); // Aplica estado Firewall en la posición calculada

        
    }
}
