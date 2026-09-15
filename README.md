# 77.000 🔥

> Juego desarrollado para la **Arde La Patagonia Game Jam 2026**.

**[▶ Ver en itch.io](https://juliancortes729.itch.io/77000)**

Motor: **Unity** · Lenguaje: **C#** · Género: **Estrategia 2D**

---

## Sobre el juego

Un proyecto para crear conciencia sobre los incendios forestales en la Patagonia argentina.

El mapa representa a escala real las **77.000 hectáreas** afectadas. El fuego no sigue un guion:
se propaga hectárea por hectárea con un modelo procedural influido por la dirección y la fuerza del viento.

## Cómo funciona la propagación del fuego

El corazón del juego es un **autómata celular sobre una grilla de hectáreas**:

- Cada hectárea es una celda con estado propio y un contador de ticks en llamas.
- En cada *tick* (independiente del framerate, no del `Update`), las celdas encendidas evalúan sus **8 vecinos**.
- La probabilidad de que el fuego pase a una celda vecina se modifica según la **dirección del viento**: cada una de las ocho direcciones tiene su propio multiplicador, recalculado cuando el viento cambia.
- El conjunto de celdas ardiendo se guarda en un `HashSet<int>` en vez de recorrer toda la grilla: solo se procesa lo que está en llamas.
- Las celdas que se van a encender se acumulan en un conjunto aparte y se aplican al final del tick, para que el fuego no se propague en cascada dentro de la misma iteración.

Los sistemas se comunican por **eventos de C#** (`OnFireExtinguished`, `OnFireActiveCountChanged`, `OnWindChanged`), así que la lógica del fuego no conoce a la UI.

## Dónde mirar el código

| Archivo | Qué hace |
| :--- | :--- |
| `Assets/Scripts/Fire/FireManager.cs` | Propagación del fuego, ticks, multiplicadores de viento |
| `Assets/Scripts/Grid/GridSystem.cs` | Estructura de datos de la grilla de hectáreas |
| `Assets/Scripts/WindSystem.cs` | Dirección y cambio del viento |
| `Assets/Scripts/NarrativeManager.cs` | Textos y eventos narrativos |
| `Assets/Scripts/UI/` | Visualización de la grilla, hectáreas quemadas, recursos y viento |

**Por dónde empezar:** `FireManager.cs`.

## Créditos

- **Julián Cortés** — Programación (proyecto desarrollado individualmente en la parte de código)

Se utilizó asistencia de IA durante el desarrollo.
