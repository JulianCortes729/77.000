using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


// Controla la visualización de la cuadrícula en un RawImage
public class VisualSystem : MonoBehaviour
{

    // Referencia al sistema de datos de la cuadrícula
    private GridSystem gridSystem;

    [SerializeField] private RawImage rawImage; // RawImage donde se pintará la textura

    [SerializeField] private Color[] colors; // Paleta de colores por StateHectare

    private Texture2D texture; // Textura que representa la rejilla de hectáreas

    WaitForEndOfFrame waitForEndOfFrame; // Reutilizado en la coroutine para sincronizar al final del frame

    private bool needsApplyTexture; // Indica si hay cambios pendientes por aplicar a la textura

    private void Awake()
    {
        // Inicializa la textura con las dimensiones por defecto (debe coincidir con la cuadrícula)
        texture = new Texture2D(275, 200);

        texture.filterMode = FilterMode.Point; // Modo punto para aspecto pixelado

        rawImage.texture = texture; // Asigna la textura al RawImage

        Color[] basePixel = new Color[texture.width * texture.height]; // Crea un array para llenar la textura
        Array.Fill(basePixel, colors[0]); // Llena el array con el color base (por ejemplo, para "Vacío")
        texture.SetPixels(basePixel); // Aplica el color base a toda la textura
        texture.Apply(); // Aplica los cambios a la GPU

       

        gridSystem = GetComponent<GridSystem>(); // Obtiene la referencia al GridSystem adjunto

        waitForEndOfFrame = new WaitForEndOfFrame(); // Crea el objeto para la espera en la coroutine

        StartCoroutine(ApplyTextureRoutine()); // Inicia la coroutine que aplica la textura al final del frame
    }
    private void OnDestroy()
    {
        Destroy(texture); // Limpia la textura para liberar memoria cuando el objeto se destruya
    }
    private void OnDisable()
    {
        // Evita fugas: se da de baja del evento al desactivar el componente
        if (gridSystem != null) gridSystem.OnHectareChanged -= UpdateVisuals;
    }

    private void OnEnable()
    {
        // Se suscribe para actualizar la visual cuando cambie el estado de una hectárea
        gridSystem.OnHectareChanged += UpdateVisuals;
    }

    // Actualiza el píxel correspondiente cuando cambia el estado de una hectárea
    void UpdateVisuals(int indice, StateHectare newState)
    {
        Color colorAsignado = colors[(int)newState]; // Selecciona el color según el nuevo estado

        int x = indice % gridSystem.Width; // Calcula la coordenada X desde el índice
        int y = indice / gridSystem.Width; // Calcula la coordenada Y desde el índice

        texture.SetPixel(x, y, colorAsignado); // Pinta el píxel en la textura

        needsApplyTexture = true; // Marca que la textura necesita aplicarse
    }

    // Coroutine que aplica cambios a la textura al final de cada frame
    IEnumerator ApplyTextureRoutine()
    {
        while (true)
        {
            yield return waitForEndOfFrame;
            if (needsApplyTexture)
            {
                texture.Apply(); // Aplica los cambios pendientes a la GPU
                needsApplyTexture = false; // Reinicia la bandera
            }
        }
    }

}
