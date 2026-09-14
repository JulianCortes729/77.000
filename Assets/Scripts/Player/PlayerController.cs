using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// Gestiona la interacci�n del jugador con el mapa: c�mara, zoom, pan y colocaci�n de cortafuegos.
/// Se comunica con Grid y ResourceManager mediante eventos para mantener baja acoplaci�n.
public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    /// Sistema de grid para consultar y modificar estados de hect�reas.
    [SerializeField] private GridSystem gridSystem;
    /// Gestor de recursos para validar y gastar presupuesto en cortafuegos.
    [SerializeField] private ResourceManager resourceManager;
    /// Rect del mapa para aplicar zoom y movimiento de c�mara.
    [SerializeField] private RectTransform mapRectTransform;

    [SerializeField] private RectTransform gridDrawArea;     // NUEVO: Para el RawImage (Clics)

    /// C�mara principal para convertir posici�n del mouse a coordenadas locales.
    [SerializeField] private Camera mainCamera;

    [Header("Settings")]
    /// Velocidad de zoom por unidad de scroll del mouse.
    [SerializeField] private float zoomSpeed = 1f;
    /// Zoom m�nimo permitido.
    [SerializeField] private float minZoom = 1f;
    /// Zoom m�ximo permitido.
    [SerializeField] private float maxZoom = 5f;
    /// Velocidad de pan (desplazamiento) del mapa con clic izquierdo.
    [SerializeField] private float panSpeed = 1f;
    /// L�mites m�ximos de desplazamiento en ejes X e Y para evitar que el mapa salga del viewport.
    [SerializeField] private Vector2 mapLimits = new Vector2(100f, 100f);

    /// Nivel de zoom actual (se va actualizando cada frame).
    private float _currentZoom = 1f;

    /// Se dispara cuando el jugador intenta colocar un cortafuegos. Par�metros: coordenadas X, Y del grid.
    public event Action<int, int> OnFirewallAttempted;

    /// Maneja entrada del jugador para movimiento de c�mara y colocaci�n de cortafuegos cada frame.
    private void Update()
    {
        HandleCameraMovement();

        if (Mouse.current.rightButton.isPressed)
        {
            TryPlaceFirewall();
        }
    }

    /// Aplica zoom y pan al mapa seg�n entrada del mouse, dentro de los l�mites configurados.
    private void HandleCameraMovement()
    {
        // Zoom
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0)
        {
            _currentZoom += Mathf.Sign(scroll) * zoomSpeed;
            _currentZoom = Mathf.Clamp(_currentZoom, minZoom, maxZoom);
            mapRectTransform.localScale = Vector3.one * Mathf.Round(_currentZoom);
        }

        // Panning
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            Vector3 newPosition = mapRectTransform.anchoredPosition3D + (Vector3)(mouseDelta * panSpeed);

            newPosition.x = Mathf.Clamp(newPosition.x, -mapLimits.x, mapLimits.x);
            newPosition.y = Mathf.Clamp(newPosition.y, -mapLimits.y, mapLimits.y);

            mapRectTransform.anchoredPosition3D = newPosition;
        }
    }

    /// Intenta colocar un cortafuegos en la coordenada del mouse si es hect�rea intacta y hay presupuesto.
    private void TryPlaceFirewall()
    {
        if (GetGridCoordinateFromMouse(out int gridX, out int gridY))
        {
            if (gridSystem.GetHectareState(gridX, gridY) == StateHectare.Intact)
            {
                if (resourceManager.TrySpendBudget())
                {
                    gridSystem.ChangeHectareState(gridX, gridY, StateHectare.Firewall);
                    OnFirewallAttempted?.Invoke(gridX, gridY);
                }
            }
        }
    }

    /// Convierte la posici�n del mouse a coordenadas del grid. Retorna false si est� fuera de los l�mites.
    private bool GetGridCoordinateFromMouse(out int x, out int y)
    {
        x = -1; y = -1;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        // ¡CAMBIO CLAVE! Usamos gridDrawArea en lugar de mapRectTransform
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(gridDrawArea, mousePos, mainCamera, out Vector2 localPoint))
        {
            // Ajustar origen al centro inferior izquierdo y normalizar
            Rect rect = gridDrawArea.rect;
            localPoint.x += rect.width * 0.5f;
            localPoint.y += rect.height * 0.5f;

            float pctX = localPoint.x / rect.width;
            float pctY = localPoint.y / rect.height;

            x = Mathf.FloorToInt(gridSystem.Width * pctX);
            y = Mathf.FloorToInt(gridSystem.Height * pctY);

            return gridSystem.IsValidCoordinate(x, y);
        }
        return false;
    }
}
