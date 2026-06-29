# TextRPG — Terminal RPG

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet)](https://dotnet.microsoft.com/)
[![Raylib-cs](https://img.shields.io/badge/Raylib-5.0-green)](https://github.com/ChrisDill/Raylib-cs)
[![Version](https://img.shields.io/badge/version-1.0.0--demo-blue)](https://github.com/diego-trigo-iglesias/Motor-RPG.net/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Jugar Demo](https://img.shields.io/badge/Jugar-Demo-ff5722?logo=githubpages)](https://diego-trigo-iglesias.github.io/Motor-RPG.net/)

**TextRPG** es un juego de rol por turnos con sprites pixel art (48×48) renderizados con Raylib, fondos dinámicos con profundidad 3D por localización, animación de viaje con transición de sprites, y versión web en Blazor WebAssembly.

---

## Probar Demo

**Juega desde el navegador:** [https://diego-trigo-iglesias.github.io/Motor-RPG.net/](https://diego-trigo-iglesias.github.io/Motor-RPG.net/)

Demo en WebAssembly sin instalación.

---

## Características

| Característica | Descripción |
|---------------|-------------|
| **Sprites 48×48** | Personajes construidos con shapes programáticas |
| **Sprites de localización 32×16** | Pixel art detallado con paletas de 6-7 colores |
| **Animaciones de ataque** | El personaje se desplaza hacia el enemigo, este retrocede al recibir golpe |
| **Fondos dinámicos 3D** | Cada localización tiene 3 capas de profundidad: fondo, siluetas y foreground |
| **6 localizaciones** | Aldea, Bosque, Cueva, Ruinas, Delta, Mazmorra — cada una con atmósfera única |
| **Animación de viaje** | Transición animada con sprites al cambiar de localización |
| **Efectos visuales** | Partículas en críticos, texto flotante de daño, screen shake, flash en impactos |
| **Respiración idle** | El personaje se mueve ligeramente arriba y abajo |
| **Barras de HP animadas** | Transición suave entre valores actual y nuevo |
| **Menús numerados** | Navegación con teclas 1-6 |
| **Inventario de pociones** | Comprar y usar pociones manualmente en cualquier momento |
| **Historial coloreado** | Mensajes clasificados por tipo con colores distintos |

---

## Cómo ejecutar

```bash
# Requiere .NET 8 SDK
dotnet run --project src/TextRPG
```

### Controles

| Tecla | Acción |
|-------|--------|
| `1` `2` `3` `4` `5` `6` | Elegir opción del menú |
| `U` | Usar poción desde el inventario |
| `ENTER` / `SPACE` | Confirmar / Atacar |
| `B` / `ESC` | Volver / Huir / Salir |
| Teclado alfanumérico | Escribir nombre del personaje |

---

## Arquitectura

```
TextRPG.sln
├── TextRPG/                     -- Juego principal (Raylib)
│   ├── Program.cs               -- Entry point
│   ├── Game.cs                  -- Bucle 60fps + state machine
│   ├── Input/
│   │   └── InputManager.cs      -- Teclado con detección por frame
│   ├── Rendering/
│   │   └── GameRenderer.cs      -- Coordinador de renderizado
│   ├── Drawing/
│   │   ├── Backgrounds.cs       -- Fondos multi-capa por localización
│   │   ├── UIElements.cs        -- Paneles, HP, log, menús
│   │   ├── Effects.cs           -- Partículas, floating text, flash, shake
│   │   └── SpriteLoader.cs      -- Carga texturas desde PixelSprite
│   ├── Screens/
│   │   └── Menus.cs             -- Generación de menús textuales
│   └── Core/
│       └── Actions.cs           -- Puente dominio ↔ UI Desktop
│
├── TextRPG.Core/                -- Librería compartida
│   ├── Models/                  -- Player, Enemy, Location, GameState
│   ├── Engine/                  -- CombatEngine, ICombatEngine
│   ├── Services/                -- Shop, Travel, Exploration, Heal, Inventory
│   ├── PixelArt/                -- Sprite data + animations (48×48, 32×32, 16×16)
│   ├── SaveSystem/              -- ISaveManager, SaveManager (JSON)
│   ├── Rendering/               -- IRenderer, IconPalette
│   ├── GameActions.cs           -- Lógica pura de dominio (usado por Desktop y Web)
│   └── Enums/                   -- CharacterClass
│
├── TextRPG.Web/                 -- Demo para GitHub Pages (Blazor WASM)
├── TextRPG.Tests/               -- Tests unitarios
└── docs/                        -- GitHub Pages output
```

---

## Tests

```bash
dotnet test
```

44 tests que cubren:
- **Combate**: simulación de rondas, críticos, esquivas, victoria/derrota
- **Jugador**: creación por clase, level up, curación, experiencia
- **Saves**: guardado/carga JSON, múltiples slots, archivos corruptos

---

## GitHub Pages

La demo web (v1.0.0-demo) se despliega desde la rama `master` usando la carpeta `/docs`.

Para publicar cambios manualmente:
```bash
dotnet publish src/TextRPG.Web -c Release -o publish
# Copiar publish/wwwroot/* a docs/
```

Configuración en **Settings > Pages**: Branch `master`, folder `/docs`.

---

## Tecnologías

- **[.NET 8](https://dotnet.microsoft.com/)** — Runtime y SDK
- **[Raylib-cs](https://github.com/ChrisDill/Raylib-cs)** — Renderizado 2D con aceleración hardware
- **[Blazor WASM](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)** — Versión web para GitHub Pages
- **[xUnit](https://xunit.net/)** — Tests unitarios
- **System.Text.Json** — Serialización de partidas

---

## Licencia

MIT
