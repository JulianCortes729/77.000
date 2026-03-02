using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField]private GridSystem gridSystem; // Referencia al sistema de cuadrícula
    [SerializeField]private RectTransform rectTransform; // RectTransform del área interactiva
    [SerializeField]private RectTransform rectTransformCortafuegos; // RectTransform del área de cortafuegos (puede ser el mismo que rectTransform si se superponen)
    [SerializeField]private ResourceManager resourceManager; // Referencia al sistema de recursos para gestionar el presupuesto

    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 5f;
    private float currentZoom = 1f; // Empezamos en escala 1 (230.000 píxeles reales)
    [SerializeField] private Camera miCamara;

    [SerializeField] private float panSpeed = 1f; // Velocidad de desplazamiento al arrastrar

    [SerializeField] private float limitX = 100f;
    [SerializeField] private float limitY = 100f;



    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            if (GetLocalPointFromMouseClick(out Vector2 localPoint))// Obtiene el punto local del clic derecho
            {
                DrawFirewall(localPoint); // Dibuja cortafuegos en la cuadrícula según el punto local
            }
            
        }

        
        float scroll = Mouse.current.scroll.ReadValue().y; // Lee el scroll del ratón
        
        if (scroll != 0)
        {
            currentZoom += scroll * zoomSpeed; // Aplica la velocidad de zoom

            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom); // Limita el zoom entre los valores mínimo y máximo

            rectTransform.localScale = Vector3.one * currentZoom; // Aplica el zoom a la escala del RectTransform
        }

        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue(); // Lee el movimiento del ratón

            Vector3 newPosition = rectTransform.anchoredPosition3D + new Vector3(mouseDelta.x, mouseDelta.y, 0) * panSpeed; // Calcula la nueva posición

            newPosition.x = Mathf.Clamp(newPosition.x, -limitX, limitX); // Limita la posición en X
            newPosition.y = Mathf.Clamp(newPosition.y, -limitY, limitY); // Limita la posición en Y

            rectTransform.anchoredPosition3D = newPosition; // Aplica la nueva posición al RectTransform
        }

    }

    bool GetLocalPointFromMouseClick(out Vector2 localPoint)
    {
        Vector2 posClick = Mouse.current.position.ReadValue(); // Posición del ratón en pantalla
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransformCortafuegos, posClick, miCamara, out localPoint); // Pantalla -> local UI
    }

    // Aquí traduciremos la coordenada y cambiaremos el estado
    private void DrawFirewall(Vector2 localPoint)
    {

       

        localPoint.x += rectTransformCortafuegos.rect.width / 2; // Ajusta origen X (centro del rect)
        localPoint.y += rectTransformCortafuegos.rect.height / 2; // Ajusta origen Y (centro del rect)

        float porcentX = localPoint.x / rectTransformCortafuegos.rect.width; // Normaliza X a [0,1]
        float gridX = gridSystem.Width * porcentX; // Mapea a coordenada X de la cuadrícula

        float porcentY = localPoint.y / rectTransformCortafuegos.rect.height; // Normaliza Y a [0,1]
        float gridY = gridSystem.Height * porcentY; // Mapea a coordenada Y de la cuadrícula

        if(gridX < 0 || gridX >= gridSystem.Width || gridY < 0 || gridY >= gridSystem.Height)
        {
            return; // Fuera de los límites de la cuadrícula
        }
        int x = Mathf.FloorToInt(gridX); // Coordenada entera X
        int y = Mathf.FloorToInt(gridY); // Coordenada entera Y

        if (gridSystem.GetHectareState(x, y) == StateHectare.Intact && resourceManager.TrySpendBudget())// Solo cambia si el estado es Intact
        {
            gridSystem.ChangeHectareState(StateHectare.Firewall, x, y); // Aplica estado Firewall en la posición calculada
        }
    }
}
