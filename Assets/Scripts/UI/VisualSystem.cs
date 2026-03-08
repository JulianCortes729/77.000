using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Controla la visualización de la cuadrícula en un RawImage
public class VisualSystem : MonoBehaviour
{

    // Referencia al sistema de datos de la cuadrícula
    private GridSystem gridSystem;

    Color[] basePixel; // Array para almacenar los colores base de la textura

    [SerializeField] private RawImage rawImage; // RawImage donde se pintará la textura

    [SerializeField] private Color[] colors; // Paleta de colores por StateHectare

    private Texture2D texture; // Textura que representa la rejilla de hectáreas

    WaitForEndOfFrame waitForEndOfFrame; // Reutilizado en la coroutine para sincronizar al final del frame
    WaitForSeconds waitForSeconds; // Reutilizado para la coroutine de parpadeo de brasas

    private bool needsApplyTexture; // Indica si hay cambios pendientes por aplicar a la textura

    [SerializeField] Color[] colorsGreen;

    [SerializeField] private Color emberColor;
    [SerializeField][Range(0f,1f)] private float emberFlashProbability = 0.0001f; // Probabilidad de que una brasa parpadee en cada ciclo
    private float speedFlash = 1f; // Velocidad del parpadeo de las brasas

    private float[] randomOffsets; // Array para almacenar offsets aleatorios para cada píxel, utilizado en el parpadeo de las brasas

    private HashSet<int> blinkingEmbers = new HashSet<int>(); // Almacena los índices de las brasas que están parpadeando actualmente

    private void Awake()
    {
        // Inicializa la textura con las dimensiones por defecto (debe coincidir con la cuadrícula)
        texture = new Texture2D(275, 280);

        texture.filterMode = FilterMode.Point; // Modo punto para aspecto pixelado

        rawImage.texture = texture; // Asigna la textura al RawImage

        basePixel = new Color[texture.width * texture.height]; // Crea un array para llenar la textura

        gridSystem = GetComponent<GridSystem>(); // Obtiene la referencia al GridSystem adjunto


        //Array.Fill(basePixel, colors[0]); // Llena el array con el color base (por ejemplo, para "Vacío")

        for (int i = 0; i < texture.width; i++)
        {
            for (int j = 0; j < texture.height; j++)
            {
                int indice = gridSystem.ExtractionIndice(i, j);
                float noise = Mathf.PerlinNoise(i * 0.15f, j * 0.15f);

                noise *= 2;
                noise = Mathf.Round(noise);
                noise *= 0.5f;

                Color colorGreen = Color.Lerp(colorsGreen[0], colorsGreen[1], noise); // Genera un color basado en ruido
                basePixel[indice] = colorGreen;
            }
        }

        randomOffsets = new float[texture.width * texture.height]; // Inicializa el array de offsets aleatorios

        for (int i = 0; i < randomOffsets.Length; i++)
        {
            randomOffsets[i] = UnityEngine.Random.value; // Asigna un offset aleatorio para cada píxel
        }

        texture.SetPixels(basePixel); // Aplica el color base a toda la textura
        texture.Apply(); // Aplica los cambios a la GPU

        waitForEndOfFrame = new WaitForEndOfFrame(); // Crea el objeto para la espera en la coroutine
        waitForSeconds = new WaitForSeconds(0.05f); // Crea el objeto para la espera en la coroutine de parpadeo

        StartCoroutine(ApplyTextureRoutine()); // Inicia la coroutine que aplica la textura al final del frame
        StartCoroutine(FlashEmbersRoutine());
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
        if (newState == StateHectare.Burned && UnityEngine.Random.value < emberFlashProbability)
        {
            blinkingEmbers.Add(indice); // Agrega el índice a la lista de brasas parpadeantes
        }

        Color colorAsignado = colors[(int)newState]; // Selecciona el color según el nuevo estado

       

        //texture.SetPixel(x, y, colorAsignado); // Pinta el píxel en la textura
        basePixel[indice] = colorAsignado; // Actualiza el array base con el nuevo color
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

    IEnumerator FlashEmbersRoutine()
    {
        while (true)
        {

            foreach (int indice in blinkingEmbers)
            {
               

                float valueInterpolateFade = Mathf.PingPong(( Time.time*speedFlash) + randomOffsets[indice], 1.0f); 

                Color targetColor = Color.Lerp(colors[(int)StateHectare.Burned], emberColor, valueInterpolateFade); // Interpola entre el color quemado y el color de la brasa

                basePixel[indice] = targetColor; // Actualiza el color en el array base
            }
            needsApplyTexture = true; // Marca que la textura necesita aplicarse
            texture.SetPixels(basePixel); // Actualiza la textura con los nuevos colores de las brasas
            yield return waitForSeconds; // Cambia el color cada 0.05 segundos

        }
    }

}
