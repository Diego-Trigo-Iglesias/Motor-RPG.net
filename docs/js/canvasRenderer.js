// TextRPG Canvas Renderer
// Dibuja sprites pixel art en un HTML Canvas con escalado nearest-neighbor.
//

const TextRPG = window.TextRPG || {};

(function () {
    'use strict';

    let canvas = null;
    let ctx = null;
    let offscreen = null;
    let offCtx = null;
    let scale = 2;       // Escala de píxeles
    let canvasW = 0;
    let canvasH = 0;

    // Paleta de colores predefinidos (para texto y UI)
    const COLORS = {
        black:     [0, 0, 0],
        white:     [255, 255, 255],
        red:       [255, 60, 60],
        green:     [60, 200, 60],
        yellow:    [255, 220, 60],
        blue:      [60, 120, 255],
        purple:    [160, 80, 200],
        cyan:      [80, 200, 230],
        orange:    [255, 160, 40],
        grey:      [160, 160, 170],
        darkGrey:  [80, 80, 90],
        gold:      [255, 200, 50],
        darkRed:   [180, 40, 40],
        brown:     [120, 70, 30],
        skin:      [230, 190, 150],
        steel:     [160, 170, 190],
        hpGreen:   [60, 220, 60],
        hpYellow:  [240, 200, 40],
        hpRed:     [240, 60, 60],
        expYellow: [255, 220, 80],
    };

    // Inicialización
    function init(canvasId, pixelScale) {
        canvas = document.getElementById(canvasId);
        if (!canvas) { console.error('Canvas not found:', canvasId); return false; }

        ctx = canvas.getContext('2d');
        if (!ctx) { console.error('No 2d context'); return false; }

        // Desactivar suavizado para pixel art nítido
        ctx.imageSmoothingEnabled = false;

        // Crear offscreen para dibujar a resolución nativa
        canvasW = canvas.width;
        canvasH = canvas.height;
        scale = pixelScale || 2;

        const nativeW = Math.floor(canvasW / scale);
        const nativeH = Math.floor(canvasH / scale);

        offscreen = document.createElement('canvas');
        offscreen.width = nativeW;
        offscreen.height = nativeH;
        offCtx = offscreen.getContext('2d');
        offCtx.imageSmoothingEnabled = false;

        return true;
    }

    // Limpiar
    function clear(r, g, b) {
        if (!offCtx) return;
        offCtx.fillStyle = rgb(r || 0, g || 0, b || 0);
        offCtx.fillRect(0, 0, offscreen.width, offscreen.height);
    }

    // Dibujar píxel
    function drawPixel(x, y, r, g, b) {
        if (!offCtx) return;
        if (x < 0 || x >= offscreen.width || y < 0 || y >= offscreen.height) return;
        offCtx.fillStyle = rgb(r, g, b);
        offCtx.fillRect(x, y, 1, 1);
    }

    // Dibujar sprite (array de colores)
    // spriteData: { width, height, pixels: [{r,g,b} | null] }
    function drawSprite(spriteData, offsetX, offsetY) {
        if (!offCtx || !spriteData) return;
        const ox = offsetX || 0;
        const oy = offsetY || 0;

        for (let y = 0; y < spriteData.height; y++) {
            for (let x = 0; x < spriteData.width; x++) {
                const idx = y * spriteData.width + x;
                const pixel = spriteData.pixels[idx];
                if (pixel && pixel.a !== 0) {
                    offCtx.fillStyle = `rgb(${pixel.r},${pixel.g},${pixel.b})`;
                    offCtx.fillRect(ox + x, oy + y, 1, 1);
                }
            }
        }
    }

    // Dibujar sprite en escala (para Canvas de Spectre.Console)
    // Cada píxel del sprite se dibuja como un bloque scale×scale
    function drawSpriteScaled(spriteData, offsetX, offsetY, pixelScale) {
        if (!offCtx || !spriteData) return;
        const ox = offsetX || 0;
        const oy = offsetY || 0;
        const s = pixelScale || 2;
        const w = spriteData.width;
        const h = spriteData.height;

        for (let y = 0; y < h; y++) {
            for (let x = 0; x < w; x++) {
                const idx = y * w + x;
                const pixel = spriteData.pixels[idx];
                if (pixel && pixel.a !== 0) {
                    offCtx.fillStyle = `rgb(${pixel.r},${pixel.g},${pixel.b})`;
                    offCtx.fillRect(ox + x * s, oy + y * s, s, s);
                }
            }
        }
    }

    // Dibujar rectángulo relleno
    function drawRect(x, y, w, h, r, g, b) {
        if (!offCtx) return;
        offCtx.fillStyle = rgb(r, g, b);
        offCtx.fillRect(x, y, w, h);
    }

    // Dibujar texto
    function drawText(text, x, y, r, g, b, size) {
        if (!offCtx) return;
        offCtx.font = `${size || 10}px monospace`;
        offCtx.fillStyle = rgb(r, g, b);
        offCtx.fillText(text, x, y);
    }

    // Dibujar barra de HP
    function drawHpBar(x, y, w, h, current, max) {
        if (!offCtx) return;
        const pct = max > 0 ? current / max : 0;
        let color;
        if (pct > 0.5) color = COLORS.hpGreen;
        else if (pct > 0.25) color = COLORS.hpYellow;
        else color = COLORS.hpRed;

        // Fondo
        offCtx.fillStyle = '#333';
        offCtx.fillRect(x, y, w, h);
        // Barra
        offCtx.fillStyle = rgb(color[0], color[1], color[2]);
        offCtx.fillRect(x, y, Math.floor(w * pct), h);
        // Borde
        offCtx.strokeStyle = '#888';
        offCtx.lineWidth = 1;
        offCtx.strokeRect(x, y, w, h);
    }

    // Renderizar al canvas visible
    function render() {
        if (!ctx || !canvas || !offscreen) return;
        ctx.imageSmoothingEnabled = false;
        ctx.drawImage(offscreen, 0, 0, canvasW, canvasH);
    }

    // Limpiar y renderizar en un paso
    function clearAndRender(r, g, b) {
        clear(r, g, b);
        render();
    }

    // Helpers
    function rgb(r, g, b) {
        return `rgb(${r|0},${g|0},${b|0})`;
    }

    // API pública
    TextRPG.CanvasRenderer = {
        init,
        clear,
        render,
        clearAndRender,
        drawPixel,
        drawSprite,
        drawSpriteScaled,
        drawRect,
        drawText,
        drawHpBar,
        COLORS,
    };

    window.TextRPG = TextRPG;
})();
