# TextRPG — Terminal RPG Épico

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet)](https://dotnet.microsoft.com/)
[![Raylib-cs](https://img.shields.io/badge/Raylib-5.0-green)](https://github.com/ChrisDill/Raylib-cs)
[![Version](https://img.shields.io/badge/version-1.0.0--demo-blue)](https://github.com/diego-trigo-iglesias/Motor-RPG.net/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Jugar Demo](https://img.shields.io/badge/Jugar-Demo-ff5722?logo=githubpages)](https://diego-trigo-iglesias.github.io/Motor-RPG.net/)

**TextRPG** es un juego de rol de texto con interfaz de terminal PREMIUM, sprites de personajes 48×48 renderizados con Raylib, y fondos dinámicos por localización. Combina lo mejor de un RPG clásico por turnos con una presentación visual moderna y efectos espectaculares.

---

## 🕹️ Probar Demo

👉 **Juega ahora desde tu navegador:** [https://diego-trigo-iglesias.github.io/Motor-RPG.net/](https://diego-trigo-iglesias.github.io/Motor-RPG.net/)

*(Demo en WebAssembly — sin instalación, funciona en PC y móvil)*

---

## ✨ Características

| Característica | Descripción |
|---------------|-------------|
| **Sprites 48×48** | Personajes construidos con shapes programáticas (círculos, triángulos, rectángulos) — mucho más detalle que pixel art manual |
| **Animaciones de ataque** | El personaje se lanza hacia adelante, el enemigo retrocede al recibir golpe |
| **Fondos dinámicos 3D** | Cada localización tiene su propia atmósfera: luciérnagas en la aldea, rayos de luna en el bosque, cristales en la cueva, polvo dorado en ruinas, llamas infernales en la mazmorra |
| **Efectos visuales** | Partículas en críticos, floating text con daño, screen shake, flash en impactos |
| **Respiración idle** | El personaje se mueve ligeramente arriba y abajo |
| **Barras de HP animadas** | Transición suave entre valores actual y nuevo |
| **Menús numerados** | Navegación tipo consola con teclas 1-5 |
| **Historial coloreado** | Mensajes clasificados por tipo con colores distintos |

---

## 🚀 Cómo ejecutar

```bash
# Requiere .NET 8 SDK
dotnet run --project src/TextRPG
```

### Controles

| Tecla | Acción |
|-------|--------|
| `1` `2` `3` `4` `5` | Elegir opción del menú |
| `ENTER` / `SPACE` | Confirmar / Atacar |
| `B` / `ESC` | Volver / Huir / Salir |
| Teclado alfanumérico | Escribir nombre del personaje |

---

## 🏗️ Arquitectura

```
TextRPG.sln
├── TextRPG/                     ← 🎮 Juego principal (Raylib)
│   ├── Program.cs               ← Entry point (3 líneas)
│   ├── Game.cs                  ← Bucle 60fps + state machine (137 líneas)
│   ├── Input/
│   │   └── InputManager.cs      ← Teclado con detección por frame
│   ├── Rendering/
│   │   └── GameRenderer.cs      ← Coordinador de renderizado (139 líneas)
│   ├── Drawing/
│   │   ├── Backgrounds.cs       ← Fondos multi-capa por localización
│   │   ├── UIElements.cs        ← Paneles, HP, log, menús
│   │   ├── Effects.cs           ← Partículas, floating text, flash, shake
│   │   └── SpriteLoader.cs      ← Carga texturas desde PixelSprite
│   ├── Screens/
│   │   └── Menus.cs             ← Generación de menús textuales
│   └── Core/
│       └── Actions.cs           ← Lógica de combate, viaje, tienda
│
├── TextRPG.Core/                ← 📚 Librería compartida
│   ├── Models/                  ← Player, Enemy, Location, GameState
│   ├── Engine/                  ← CombatEngine, ICombatEngine
│   ├── Services/                ← Shop, Travel, Exploration (para consola/web)
│   ├── PixelArt/                ← Sprite data + animations (48×48, 32×32, 16×16)
│   ├── SaveSystem/              ← ISaveManager, SaveManager (JSON)
│   ├── Rendering/               ← IRenderer, IconPalette
│   └── Enums/                   ← CharacterClass
│
├── TextRPG.Web/                 ← 🌐 Demo para GitHub Pages (Blazor WASM)
├── TextRPG.Tests/               ← 🧪 Tests unitarios (44 tests)
└── docs/                        ← 📄 GitHub Pages output
```

---

## 🧪 Tests

```bash
dotnet test
```

44 tests que cubren:
- **Combate**: simulación de rondas, críticos, esquivas, victoria/derrota
- **Jugador**: creación por clase, level up, curación, experiencia
- **Saves**: guardado/carga JSON, múltiples slots, archivos corruptos

---

## 🌐 GitHub Pages

La demo web (v1.0.0-demo) se despliega automáticamente desde la rama `master` usando la carpeta `/docs`.

Para publicar cambios manualmente:
```bash
dotnet publish src/TextRPG.Web -c Release -o publish
# Copiar publish/wwwroot/* a docs/
```

Configuración en **Settings > Pages**: Branch `master`, folder `/docs`.

---

## 📦 Tecnologías

- **[.NET 8](https://dotnet.microsoft.com/)** — Runtime y SDK
- **[Raylib-cs](https://github.com/ChrisDill/Raylib-cs)** — Renderizado 2D con aceleración hardware
- **[Blazor WASM](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)** — Versión web para GitHub Pages
- **[xUnit](https://xunit.net/)** — Tests unitarios
- **System.Text.Json** — Serialización de partidas

---

## 📄 Licencia

MIT — Haz lo que quieras con esto, solo pásatelo bien.
