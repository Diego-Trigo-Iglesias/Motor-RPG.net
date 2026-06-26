(function () {
    'use strict';

    const W = 800, H = 450;
    let canvas, ctx, offscreen, offCtx;
    let scale = 1;
    let animTime = 0, lastTime = 0;
    let state = null;
    let animState = { atk: 0, hit: 0, shakeX: 0, shakeY: 0, flash: 0, breath: 0 };
    // Travel animation state (3D tunnel)
    let travelState = { active: false, fromLoc: 'village', toLoc: 'village', progress: 0, duration: 1.8 };
    let nextState = null;
    let transitioning = false;

    const COLORS = {
        black: [0, 0, 0], white: [255, 255, 255], red: [255, 60, 60],
        green: [60, 200, 60], yellow: [255, 220, 60], blue: [60, 120, 255],
        purple: [160, 80, 200], cyan: [80, 200, 230], orange: [255, 160, 40],
        grey: [160, 160, 170], darkGrey: [80, 80, 90], gold: [255, 200, 50],
        darkRed: [180, 40, 40], brown: [120, 70, 30], skin: [230, 190, 150],
        steel: [160, 170, 190], hpGreen: [60, 220, 60], hpYellow: [240, 200, 40],
        hpRed: [240, 60, 60]
    };

    function rgb(r, g, b, a) {
        if (a !== undefined) return `rgba(${r|0},${g|0},${b|0},${a})`;
        return `rgb(${r|0},${g|0},${b|0})`;
    }

    function init(canvasId, pixelScale) {
        canvas = document.getElementById(canvasId);
        if (!canvas) { console.error('Canvas not found'); return false; }
        ctx = canvas.getContext('2d');
        if (!ctx) return false;
        ctx.imageSmoothingEnabled = false;

        canvas.width = W;
        canvas.height = H;
        scale = pixelScale || 2;

        offscreen = document.createElement('canvas');
        offscreen.width = W;
        offscreen.height = H;
        offCtx = offscreen.getContext('2d');
        offCtx.imageSmoothingEnabled = false;

        canvas.style.width = (W * scale) + 'px';
        canvas.style.height = (H * scale) + 'px';

        lastTime = performance.now();
        requestAnimationFrame(loop);
        return true;
    }

    // Paletas de colores por localización (para transición de viaje)
    const LOC_PALETTES = {
        village: { p: [55, 38, 22], s: [255, 210, 80] },
        forest:  { p: [10, 22, 6],  s: [50, 150, 45] },
        cave:    { p: [6, 4, 8],    s: [60, 140, 230] },
        ruins:   { p: [45, 30, 16], s: [255, 200, 70] },
        delta:   { p: [14, 10, 24], s: [80, 200, 180] },
        dungeon: { p: [8, 2, 12],   s: [255, 100, 20] },
    };

    function lerpColor(a, b, t) {
        return [
            a[0] + (b[0] - a[0]) * t,
            a[1] + (b[1] - a[1]) * t,
            a[2] + (b[2] - a[2]) * t
        ];
    }

    function pushState(s) {
        if (!s) return;
        // Detectar viaje: cuando la pantalla cambia a MainMenu después de haber seleccionado Travel
        if (state && state.screen === 'Travel' && s.screen !== 'Travel' && s.location && state.location) {
            if (s.location !== state.location) {
                travelState.active = true;
                travelState.fromLoc = state.location;
                travelState.toLoc = s.location;
                travelState.progress = 0;
            }
        }
        if (state && state.screen === 'Combat' && s.screen === 'Combat' && s.combatRound !== state.combatRound) {
            animState.atk = 0.3;
            animState.hit = 0.5;
            animState.shakeX = s.animations.isCritical ? 10 : 4;
            animState.shakeY = s.animations.isCritical ? 6 : 2;
            animState.flash = s.animations.isCritical ? 0.8 : 0.35;
        }
        if (state && state.screen !== 'Combat' && s.screen === 'Combat') {
            animState.shakeX = 2; animState.shakeY = 1; animState.flash = 0.2;
        }
        state = JSON.parse(JSON.stringify(s));
        animState.breath = 0;
    }

    function loop(now) {
        let dt = Math.min((now - lastTime) / 1000, 0.05);
        lastTime = now;
        animTime += dt;

        if (animState.atk > 0) animState.atk = Math.max(0, animState.atk - dt);
        if (animState.hit > 0) animState.hit = Math.max(0, animState.hit - dt);
        if (animState.shakeX > 0) animState.shakeX *= 0.92;
        if (animState.shakeY > 0) animState.shakeY *= 0.92;
        if (animState.flash > 0) animState.flash = Math.max(0, animState.flash - dt * 3);
        animState.breath += dt * 2.5;

        render();
        ctx.drawImage(offscreen, 0, 0);
        requestAnimationFrame(loop);
    }

    function render() {
        if (!offCtx) return;
        offCtx.imageSmoothingEnabled = false;

        if (!state) {
            offCtx.fillStyle = '#0a0e1e';
            offCtx.fillRect(0, 0, W, H);
            return;
        }

        // ─── Travel animation overrides everything ─────────────────────────────
        if (travelState.active) {
            travelState.progress += travelState.progress < 1 ? 0.016 / travelState.duration : 0;
            drawTravelAnimation();
            if (travelState.progress >= 1) {
                travelState.active = false;
            }
            drawLog();
            drawMenu();
            return;
        }

        drawBackground();
        // Vignette
        let vgTop = offCtx.createLinearGradient(0, 0, 0, 80);
        vgTop.addColorStop(0, 'rgba(0,0,0,0.55)');
        vgTop.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = vgTop;
        offCtx.fillRect(0, 0, W, 80);
        let vgBot = offCtx.createLinearGradient(0, H - 80, 0, H);
        vgBot.addColorStop(0, 'rgba(0,0,0,0)');
        vgBot.addColorStop(1, 'rgba(0,0,0,0.55)');
        offCtx.fillStyle = vgBot;
        offCtx.fillRect(0, H - 80, W, 80);

        if (state.screen === 'Title') { drawTitle(); return; }

        let sx = animState.shakeX > 0.5 ? (Math.sin(animTime * 130) * animState.shakeX) : 0;
        let sy = animState.shakeY > 0.5 ? (Math.cos(animTime * 150) * animState.shakeY) : 0;

        offCtx.save();
        offCtx.translate(sx, sy);

        if (state.screen === 'Combat' || state.screen === 'Victory' || state.screen === 'Defeat') {
            drawCombat();
            // Combat red tint
            offCtx.fillStyle = 'rgba(200,30,30,0.18)';
            offCtx.fillRect(0, 0, W, H);
        } else {
            drawMainScene();
        }

        offCtx.restore();

        if (animState.flash > 0) {
            let c = state.animations && state.animations.isCritical ? [255, 200, 50] : [255, 255, 255];
            offCtx.fillStyle = rgb(c[0], c[1], c[2], animState.flash * 0.6);
            offCtx.fillRect(0, 0, W, H);
        }

        drawLog();
        drawMenu();
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  BACKGROUND — dispatcher
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawBackground() {
        let loc = state.location || 'village';
        switch (loc) {
            case 'village': drawBgVillage(); break;
            case 'forest':  drawBgForest();  break;
            case 'cave':    drawBgCave();    break;
            case 'ruins':   drawBgRuins();   break;
            case 'delta':   drawBgDelta();   break;
            case 'dungeon': drawBgDungeon(); break;
            default: bgGrad([10, 14, 28], [16, 20, 38]); break;
        }
        // Foreground depth layer
        drawForeground(loc);
    }

    /** Dibuja elementos de primer plano para efecto de profundidad 3D. */
    function drawForeground(loc) {
        // Base oscura inferior
        let fg = offCtx.createLinearGradient(0, H - 30, 0, H);
        fg.addColorStop(0, 'rgba(0,0,0,0.35)');
        fg.addColorStop(1, 'rgba(0,0,0,0.7)');
        offCtx.fillStyle = fg;
        offCtx.fillRect(0, H - 30, W, 30);

        switch (loc) {
            case 'village':
                for (let i = 0; i < 20; i++) {
                    let gx = (i * 53 + 17) % W;
                    let gh = 8 + (i % 4) * 3;
                    fillRect(gx, H - gh, 2, gh, 20, 40, 15, 0.55);
                    fillRect(gx + 3, H - gh + 2, 1, gh - 2, 30, 55, 20, 0.4);
                }
                break;
            case 'forest':
                for (let i = 0; i < 25; i++) {
                    let fx = (i * 37 + 11) % W;
                    let fh = 12 + (i % 5) * 4;
                    let a = 0.35 + (i % 3) * 0.08;
                    fillCircle(fx, H - fh + 4, fh / 2, rgb(8, 30, 12, a));
                    fillCircle(fx + 5, H - fh + 6, fh / 3, rgb(12, 40, 16, a - 0.1));
                }
                break;
            case 'cave':
                for (let i = 0; i < 8; i++) {
                    let rx = (i * 97 + 31) % W;
                    let rs = 12 + (i % 3) * 8;
                    fillCircle(rx, H + 5, rs, rgb(15, 12, 10, 0.6));
                    fillCircle(rx + 3, H - 2, rs - 4, rgb(25, 20, 18, 0.45));
                }
                break;
            case 'ruins':
                for (let i = 0; i < 12; i++) {
                    let rx = (i * 67 + 23) % W;
                    fillRect(rx, H - 10, 6 + (i % 3) * 4, 10, 30, 25, 20, 0.55);
                    fillRect(rx + 2, H - 8, 3 + (i % 2) * 2, 6, 45, 35, 28, 0.4);
                }
                break;
            case 'delta':
                for (let i = 0; i < 15; i++) {
                    let dx = (i * 43 + 7) % W;
                    let dh = 6 + (i % 3) * 5;
                    fillRect(dx, H - dh, 3, dh, 15, 25, 18, 0.6);
                    fillCircle(dx + 1, H - dh - 2, 3, rgb(25, 40, 30, 0.45));
                }
                break;
            case 'dungeon':
                for (let i = 0; i < 10; i++) {
                    let dx = (i * 73 + 13) % W;
                    fillRect(dx, H - 6, 8, 6, 40, 35, 30, 0.6);
                    fillRect(dx + 2, H - 8, 4, 4, 55, 45, 40, 0.45);
                }
                break;
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────────
    function bgGrad(top, bot) {
        let g = offCtx.createLinearGradient(0, 0, 0, H);
        g.addColorStop(0, rgb(top[0], top[1], top[2]));
        g.addColorStop(1, rgb(bot[0], bot[1], bot[2]));
        offCtx.fillStyle = g;
        offCtx.fillRect(0, 0, W, H);
    }

    function fillRect(x, y, w, h, r, g, b, a) {
        offCtx.fillStyle = a !== undefined ? `rgba(${r|0},${g|0},${b|0},${a})` : rgb(r, g, b);
        offCtx.fillRect(x, y, w, h);
    }

    function fillCircle(x, y, r, color) {
        offCtx.beginPath();
        offCtx.arc(x, y, Math.max(r, 0.5), 0, Math.PI * 2);
        offCtx.fill();
    }

    function drawTriangle(x1, y1, x2, y2, x3, y3, r, g, b, a) {
        offCtx.beginPath();
        offCtx.moveTo(x1, y1);
        offCtx.lineTo(x2, y2);
        offCtx.lineTo(x3, y3);
        offCtx.closePath();
        offCtx.fillStyle = a !== undefined ? `rgba(${r|0},${g|0},${b|0},${a})` : rgb(r, g, b);
        offCtx.fill();
    }

    // Silueta: casa con techo
    function drawHouse(x, y, w, h, rWall, gWall, bWall, rRoof, gRoof, bRoof, hasWindow) {
        // Techo
        drawTriangle(x + w / 2, y - h / 3, x - w / 4, y + h / 6, x + w + w / 4, y + h / 6, rRoof, gRoof, bRoof, 0.8);
        // Paredes
        fillRect(x, y, w, h, rWall, gWall, bWall, 0.8);
        // Ventana
        if (hasWindow) {
            fillRect(x + w / 2 - 3, y + h / 2 - 3, 6, 6, 255, 200, 80, 0.9);
            fillRect(x + w / 2 - 2, y + h / 2 - 5, 4, 2, 255, 220, 120, 0.5);
        }
    }

    // Silueta: árbol
    function drawTree(x, y, h, rTr, gTr, bTr, rFol, gFol, bFol, rFol2, gFol2, bFol2) {
        let aFol = 0.6, aFol2 = 0.5;
        // Tronco
        fillRect(x - 2, y, 4, h, rTr, gTr, bTr, 0.7);
        // Copa
        let cy = y - h / 3;
        fillCircle(x, cy, h / 3, rgb(rFol, gFol, bFol, aFol));
        fillCircle(x - h / 4, cy + h / 6, h / 4, rgb(rFol2, gFol2, bFol2, aFol2));
        fillCircle(x + h / 4, cy + h / 6, h / 4, rgb(rFol2, gFol2, bFol2, aFol2));
        fillCircle(x - h / 6, cy - h / 6, h / 5, rgb(rFol, gFol, bFol, aFol));
        fillCircle(x + h / 6, cy - h / 6, h / 5, rgb(rFol, gFol, bFol, aFol));
    }

    // Silueta: árbol de pantano
    function drawSwampTree(x, y, h, rTr, gTr, bTr, rFol, gFol, bFol) {
        fillRect(x - 2, y, 4, h / 2, rTr, gTr, bTr, 0.7);
        fillRect(x - 1, y + h / 2, 3, h / 4, rTr, gTr, bTr, 0.5);
        let bx = x + 8, by = y + h / 4;
        for (let i = 0; i < 5; i++)
            fillRect(bx + i * 3, by - i, 2, 3, rTr, gTr, bTr, 0.6);
        fillCircle(bx + 5, by - 8, 10, rgb(rFol, gFol, bFol, 0.5));
        fillCircle(bx + 12, by - 4, 8, rgb(rFol, gFol, bFol, 0.4));
        fillCircle(x - 3, y - h / 6, 8, rgb(rFol, gFol, bFol, 0.5));
    }

    // Estalactita
    function drawStalactite(x, y, h, r1, g1, b1, r2, g2, b2, r3, g3, b3) {
        drawTriangle(x, y, x - 6, y + h * 2 / 3, x + 6, y + h * 2 / 3, r1, g1, b1, 0.7);
        drawTriangle(x, y + h * 2 / 3, x - 3, y + h, x + 3, y + h, r3, g3, b3, 0.6);
        drawTriangle(x + 1, y, x - 3, y + h * 2 / 3, x + 5, y + h * 2 / 3, r2, g2, b2, 0.5);
    }

    // Columna
    function drawColumn(x, y, h, r, g, b, rs, gs, bs, fallen) {
        if (fallen) {
            fillRect(x - h / 2, y, h, 5, r, g, b, 0.7);
            fillRect(x - h / 2, y + 2, h - 4, 2, rs, gs, bs, 0.5);
            fillRect(x - h / 2 - 4, y - 2, 6, 8, r, g, b, 0.7);
            fillRect(x + h / 2 - 2, y - 1, 6, 6, r, g, b, 0.7);
        } else {
            fillRect(x - 3, y - h, 6, h, r, g, b, 0.7);
            fillRect(x - 2, y - h, 2, h, rs, gs, bs, 0.5);
            fillRect(x - 5, y - h, 10, 4, r, g, b, 0.7);
            fillRect(x - 5, y, 10, 3, r, g, b, 0.7);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  ALDEA — noche, estrellas, luna, casas, árboles, luciérnagas
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawBgVillage() {
        bgGrad([18, 14, 35], [45, 30, 18]);
        let g = offCtx.createLinearGradient(0, 0, 0, H / 3);
        g.addColorStop(0, 'rgba(30,25,55,0.4)');
        g.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = g;
        offCtx.fillRect(0, 0, W, H / 3);

        // Estrellas
        for (let i = 0; i < 25; i++) {
            let px = (Math.sin(animTime * 0.05 + i * 7.3) * 0.5 + 0.5) * W * 0.9 + W * 0.05;
            let py = (Math.sin(animTime * 0.03 + i * 11.7) * 0.5 + 0.5) * H * 0.3 + 5;
            let a = 0.35 + Math.sin(animTime * 1.5 + i * 2.3) * 0.3;
            fillCircle(px, py, 1 + (i % 3 === 0 ? 1 : 0), rgb(255, 240, 200, a));
        }

        // Luna creciente
        let mx = W - 120, my = 45;
        fillCircle(mx, my, 18, rgb(240, 230, 210, 0.8));
        fillCircle(mx + 5, my - 3, 14, rgb(18, 14, 35, 1));
        fillCircle(mx, my, 25, rgb(240, 230, 210, 0.1));

        // Árboles lejanos
        for (let i = 0; i < 5; i++) {
            let tx = 20 + i * W / 5 + Math.sin(i * 2.5) * 30;
            let th = 60 + (i % 3) * 30;
            drawTree(tx, H - 60 - th + 10, th, 15, 12, 20, 10, 18, 10, 8, 14, 8);
        }

        // Casas
        drawHouse(120, H - 95, 50, 40, 60, 40, 25, 130, 50, 30, true);
        drawHouse(260, H - 90, 45, 35, 55, 38, 22, 120, 45, 25, true);
        drawHouse(400, H - 100, 55, 45, 65, 45, 28, 140, 55, 35, true);
        drawHouse(560, H - 85, 40, 30, 50, 35, 20, 110, 40, 20, false);
        drawHouse(700, H - 92, 48, 38, 58, 40, 24, 125, 48, 28, true);
        drawHouse(850, H - 88, 42, 34, 52, 36, 22, 115, 42, 24, false);

        // Valla
        for (let i = 0; i < 12; i++) {
            let vx = 60 + i * 75;
            fillRect(vx, H - 42, 3, 12, 60, 40, 25);
            fillRect(vx + 5, H - 38, 50, 2, 50, 35, 20);
            fillRect(vx + 5, H - 34, 50, 2, 50, 35, 20);
        }

        // Camino
        fillRect(0, H - 18, W, 18, 80, 60, 35);
        fillRect(0, H - 16, W, 6, 60, 45, 25);

        // Luciérnagas
        for (let i = 0; i < 15; i++) {
            let px = W * 0.05 + (Math.sin(animTime * 0.6 + i * 2.1) * 0.5 + 0.5) * W * 0.9;
            let py = H * 0.3 + (Math.cos(animTime * 0.5 + i * 2.7) * 0.5 + 0.5) * H * 0.5;
            let sz = 1.5 + Math.sin(animTime * 2.5 + i * 3) * 0.8;
            let a = 0.4 + Math.sin(animTime * 2 + i * 3) * 0.3;
            fillCircle(px, py, Math.max(sz, 0.5), rgb(255, 220, 100, a));
            fillCircle(px, py, sz * 2.5, rgb(255, 220, 100, a * 0.25));
        }

        // Humo chimeneas
        for (let i = 0; i < 6; i++) {
            let hx = 145 + i * 140;
            let hy = H - 125 - Math.sin(animTime * 0.8 + i * 1.5) * 10;
            let a = 0.12 + Math.sin(animTime * 0.6 + i * 2.1) * 0.08;
            fillCircle(hx, hy, 4 + Math.sin(animTime * 1.2 + i * 1.8) * 2, rgb(180, 160, 140, a));
            fillCircle(hx + 3, hy - 8, 3 + Math.sin(animTime * 1.0 + i * 2.5) * 1.5, rgb(160, 140, 120, a * 0.7));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  BOSQUE — árboles, rayos de luna, niebla, hongos, hojas
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawBgForest() {
        bgGrad([2, 14, 6], [8, 20, 5]);
        let g1 = offCtx.createLinearGradient(0, 0, 0, H);
        g1.addColorStop(0, 'rgba(20,55,25,0.3)');
        g1.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = g1;
        offCtx.fillRect(0, 0, W, H);
        let g2 = offCtx.createLinearGradient(0, H / 3, 0, H * 2 / 3);
        g2.addColorStop(0, 'rgba(35,90,45,0.2)');
        g2.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = g2;
        offCtx.fillRect(0, H / 3, W, H / 3);

        // Árboles fondo
        let treeBg = [30, 100, 180, 280, 380, 480, 580, 680, 780, 880, 930];
        for (let tx of treeBg) {
            let th = 120 + (tx * 7 % 5) * 40;
            drawTree(tx, H - 30 - th + 15, th, 10, 20, 10, 8, 25, 12, 6, 20, 10);
        }

        // Árboles principales
        drawTree(60, H - 60, 130, 40, 25, 15, 20, 80, 25, 15, 60, 20);
        drawTree(180, H - 55, 120, 50, 30, 18, 25, 90, 30, 18, 70, 25);
        drawTree(350, H - 65, 140, 35, 20, 12, 22, 85, 28, 16, 65, 22);
        drawTree(520, H - 50, 110, 45, 28, 16, 20, 78, 25, 14, 58, 20);
        drawTree(720, H - 58, 135, 38, 22, 14, 24, 88, 30, 18, 68, 24);
        drawTree(900, H - 52, 115, 42, 26, 15, 22, 82, 28, 16, 62, 22);

        // Rayos de luna
        for (let i = 0; i < 8; i++) {
            let rx = W * 0.03 + i * W * 0.13 + Math.sin(i * 1.7) * 15;
            let a = 0.08 + Math.sin(animTime * 0.7 + i * 1.3) * 0.06;
            let rw = 3 + (i % 3) * 2;
            let g = offCtx.createLinearGradient(rx, 0, rx + rw, 0);
            g.addColorStop(0, rgb(180, 220, 190, a));
            g.addColorStop(1, rgb(180, 220, 190, 0));
            offCtx.fillStyle = g;
            offCtx.fillRect(rx, 0, rw, H);
        }

        // Niebla (3 capas)
        for (let layer = 0; layer < 3; layer++) {
            let ly = H * (2 / 3 + layer * 0.08);
            let lh = H * (0.33 - layer * 0.08);
            let g = offCtx.createLinearGradient(0, ly, 0, ly + lh);
            let a = 0.25 - layer * 0.07;
            g.addColorStop(0, rgb(40, 80, 50, a));
            g.addColorStop(1, 'rgba(0,0,0,0)');
            offCtx.fillStyle = g;
            offCtx.fillRect(0, ly, W, lh);
        }

        // Bruma partículas
        for (let i = 0; i < 24; i++) {
            let px = (Math.sin(animTime * 0.25 + i * 2.5) * 0.5 + 0.5) * W * 1.2 - W * 0.1;
            let py = H * 0.5 + (Math.cos(animTime * 0.18 + i * 3.3) * 0.5 + 0.5) * H * 0.4;
            let a = 0.1 + Math.sin(animTime * 0.5 + i * 1.7) * 0.07;
            fillCircle(px, py, 3 + Math.sin(animTime * 0.4 + i * 2.1) * 1.5, rgb(120, 200, 140, a));
        }

        // Hongos bioluminiscentes
        let mushrooms = [[95, 380], [220, 410], [460, 390], [610, 420], [800, 395], [350, 430]];
        for (let [hx, hy] of mushrooms) {
            let a = 0.25 + Math.sin(animTime * 1.8 + hx * 0.05) * 0.18;
            fillCircle(hx, hy, 3, rgb(200, 120, 60, a));
            fillCircle(hx + 3, hy - 2, 2, rgb(230, 180, 140, a * 0.8));
            fillCircle(hx, hy, 8, rgb(200, 120, 60, a * 0.2));
        }

        // Hojas cayendo
        for (let i = 0; i < 8; i++) {
            let lx = (Math.sin(animTime * 1.2 + i * 2.3) * 0.5 + 0.5) * W;
            let ly = (animTime * 25 + i * 40) % H;
            let lw = 2 + Math.sin(animTime * 3 + i * 1.5) * 1;
            fillCircle(lx, ly, lw, rgb(40, 120, 35, 0.2));
            fillCircle(lx + 2, ly + 1, lw * 0.5, rgb(50, 140, 45, 0.15));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  CUEVA — estalactitas, estalagmitas, cristales, agua, sombras
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawBgCave() {
        bgGrad([2, 2, 6], [6, 4, 3]);
        let g1 = offCtx.createLinearGradient(0, 0, 0, H);
        g1.addColorStop(0, 'rgba(8,8,20,0.35)');
        g1.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = g1;
        offCtx.fillRect(0, 0, W, H);
        let g2 = offCtx.createLinearGradient(0, H / 4, 0, H / 2);
        g2.addColorStop(0, 'rgba(15,15,35,0.25)');
        g2.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = g2;
        offCtx.fillRect(0, H / 4, W, H / 4);

        // Estalactitas
        let stalactites = [[30, 50], [80, 70], [140, 45], [210, 80], [280, 55],
            [350, 65], [420, 40], [490, 90], [560, 50], [630, 75],
            [700, 55], [770, 85], [840, 45], [910, 60]];
        for (let [sx, len] of stalactites)
            drawStalactite(sx, 0, len, 50, 45, 40, 35, 30, 28, 25, 22, 20);

        // Estalagmitas
        let stalagmites = [[50, 25], [120, 35], [190, 20], [260, 40], [340, 30],
            [410, 22], [480, 45], [550, 28], [620, 38], [690, 25],
            [760, 42], [830, 30], [890, 20]];
        for (let [gx, glen] of stalagmites)
            drawTriangle(gx, H, gx - 4, H - glen, gx + 4, H - glen, 35, 30, 28, 0.5);

        // Cristales multicolor
        let gems = [[60, 140, 0], [170, 250, 1], [300, 380, 0], [450, 200, 2],
            [540, 350, 1], [660, 120, 0], [750, 280, 2], [840, 390, 1],
            [200, 360, 2], [500, 120, 1]];
        for (let [cx, cy, hue] of gems) {
            let a = 0.45 + Math.sin(animTime * 1.8 + cx * 0.08) * 0.3;
            let sz = 2.5 + Math.sin(animTime * 2.2 + cy * 0.06) * 1.2;
            let col = hue === 1 ? [40, 200, 180] : hue === 2 ? [200, 80, 220] : [60, 140, 230];
            fillCircle(cx, cy, Math.max(sz, 0.5), rgb(col[0], col[1], col[2], a));
            fillCircle(cx, cy, sz * 2.5, rgb(col[0], col[1], col[2], a * 0.25));
            fillCircle(cx, cy, sz * 0.4, rgb(220, 240, 255, a * 0.6));
        }

        // Agua con reflejos
        let wg = offCtx.createLinearGradient(0, H - 40, 0, H);
        wg.addColorStop(0, 'rgba(5,10,30,0.7)');
        wg.addColorStop(1, 'rgba(3,6,20,0.5)');
        offCtx.fillStyle = wg;
        offCtx.fillRect(0, H - 40, W, 40);
        for (let i = 0; i < 12; i++) {
            let rx = (Math.sin(animTime * 0.3 + i * 2.5) * 0.5 + 0.5) * W;
            let ry = H - 15 + Math.sin(animTime * 1.5 + i * 1.8) * 5;
            let a = 0.1 + Math.sin(animTime * 2 + i * 3.1) * 0.08;
            fillCircle(rx, ry, 2, rgb(60, 140, 200, a));
        }

        // Sombra inferior
        let sg = offCtx.createLinearGradient(0, H / 2, 0, H);
        sg.addColorStop(0, 'rgba(0,0,0,0.2)');
        sg.addColorStop(1, 'rgba(0,0,0,0.7)');
        offCtx.fillStyle = sg;
        offCtx.fillRect(0, H / 2, W, H / 2);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  RUINAS — atardecer, columnas, polvo, luciérnagas
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawBgRuins() {
        bgGrad([28, 20, 14], [45, 30, 16]);
        let g1 = offCtx.createLinearGradient(0, 0, 0, H);
        g1.addColorStop(0, 'rgba(70,50,25,0.3)');
        g1.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = g1;
        offCtx.fillRect(0, 0, W, H);

        // Sol poniente
        offCtx.fillStyle = rgb(240, 180, 60, 0.08);
        fillCircle(W / 2, 30, 100, rgb(240, 180, 60, 0.08));
        fillCircle(W / 2, 35, 60, rgb(255, 200, 80, 0.12));
        fillCircle(W / 2, 40, 30, rgb(255, 220, 100, 0.18));
        fillCircle(W / 2, 42, 18, rgb(255, 200, 70, 0.7));
        fillCircle(W / 2, 42, 14, rgb(255, 220, 120, 0.6));

        // Nubes
        for (let i = 0; i < 6; i++) {
            let nx = (Math.sin(i * 1.3 + animTime * 0.02) * 0.5 + 0.5) * W * 0.8 + W * 0.1;
            let ny = 20 + i * 12;
            fillCircle(nx, ny, 18 + i * 2, rgb(200, 120, 60, 0.1));
            fillCircle(nx + 20, ny + 5, 14 + i, rgb(210, 140, 70, 0.08));
        }

        // Columnas en pie
        drawColumn(120, H - 30, 130, 55, 45, 35, 35, 28, 22, false);
        drawColumn(280, H - 25, 110, 50, 40, 32, 32, 26, 20, false);
        drawColumn(680, H - 28, 120, 55, 45, 35, 35, 28, 22, false);
        drawColumn(850, H - 22, 100, 50, 40, 32, 32, 26, 20, false);

        // Columnas caídas
        drawColumn(350, H - 18, 80, 45, 38, 30, 30, 25, 20, true);
        drawColumn(550, H - 15, 70, 45, 38, 30, 30, 25, 20, true);

        // Bloques
        fillRect(180, H - 16, 25, 16, 60, 50, 40, 0.6);
        fillRect(750, H - 14, 30, 14, 55, 45, 35, 0.6);
        fillRect(440, H - 12, 20, 12, 50, 40, 32, 0.6);

        // Hierba
        for (let i = 0; i < 30; i++) {
            let gx = Math.sin(i * 1.7) * W * 0.45 + W * 0.05 + i * W * 0.03;
            let gh = 5 + (i % 5) * 3;
            fillRect(gx, H - 6 - gh * 2, 2, gh, 40, 85, 35, 0.45);
            fillRect(gx + 3, H - 6 - gh * 2 + gh / 2, 1, gh / 2, 55, 105, 45, 0.35);
        }

        // Polvo dorado
        for (let i = 0; i < 20; i++) {
            let px = (Math.sin(animTime * 0.35 + i * 1.7) * 0.5 + 0.5) * W;
            let py = (Math.cos(animTime * 0.28 + i * 2.9) * 0.5 + 0.5) * H * 0.5 + H * 0.15;
            let a = 0.1 + Math.sin(animTime * 0.8 + i * 2.3) * 0.08;
            fillCircle(px, py, 1.5 + Math.sin(animTime * 0.5 + i * 1.5) * 0.5, rgb(220, 190, 130, a));
        }

        // Luciérnagas doradas
        for (let i = 0; i < 10; i++) {
            let px = W * 0.1 + (Math.sin(animTime * 0.5 + i * 2.5) * 0.5 + 0.5) * W * 0.8;
            let py = H * 0.35 + (Math.cos(animTime * 0.45 + i * 3.1) * 0.5 + 0.5) * H * 0.5;
            let a = 0.2 + Math.sin(animTime * 1.8 + i * 2.7) * 0.15;
            fillCircle(px, py, 2, rgb(255, 210, 80, a));
            fillCircle(px, py, 4, rgb(255, 210, 80, a * 0.25));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  DELTA — pantano, agua, árboles retorcidos, juncos, niebla, fuegos fatuos
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawBgDelta() {
        bgGrad([16, 10, 28], [10, 18, 14]);
        let g1 = offCtx.createLinearGradient(0, 0, 0, H / 2);
        g1.addColorStop(0, 'rgba(25,18,38,0.3)');
        g1.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = g1;
        offCtx.fillRect(0, 0, W, H / 2);

        // Luna brumosa
        let mx = W - 100, my = 50;
        fillCircle(mx, my, 22, rgb(200, 210, 190, 0.3));
        fillCircle(mx, my, 16, rgb(220, 230, 210, 0.4));
        fillCircle(mx, my, 30, rgb(200, 210, 190, 0.08));

        // Árboles retorcidos
        drawSwampTree(60, H - 40, 110, 25, 18, 12, 15, 35, 20);
        drawSwampTree(180, H - 35, 95, 22, 16, 10, 12, 30, 18);
        drawSwampTree(350, H - 45, 130, 28, 20, 14, 18, 38, 22);
        drawSwampTree(520, H - 30, 85, 22, 16, 10, 12, 30, 18);
        drawSwampTree(700, H - 42, 120, 26, 18, 12, 16, 36, 20);
        drawSwampTree(880, H - 32, 90, 24, 17, 11, 14, 32, 19);

        // Juncos
        let reeds = [[40, 30], [90, 25], [150, 35], [230, 28], [290, 32],
            [410, 28], [470, 35], [560, 25], [620, 30], [670, 22],
            [760, 32], [820, 28], [910, 25], [940, 30]];
        for (let [rx, rl] of reeds) {
            fillRect(rx, H - 10 - rl, 2, rl, 45, 65, 35, 0.6);
            fillCircle(rx + 1, H - 10 - rl, 3, rgb(80, 60, 30, 0.7));
            fillRect(rx + 3, H - 5 - (rl - 5), 2, rl - 5, 35, 55, 28, 0.5);
            fillCircle(rx + 4, H - 5 - (rl - 5), 2.5, rgb(70, 50, 25, 0.6));
        }

        // Superficie agua
        let wg = offCtx.createLinearGradient(0, H - 40, 0, H);
        wg.addColorStop(0, 'rgba(8,25,20,0.75)');
        wg.addColorStop(1, 'rgba(5,18,14,0.6)');
        offCtx.fillStyle = wg;
        offCtx.fillRect(0, H - 40, W, 40);

        // Reflejos agua
        for (let i = 0; i < 16; i++) {
            let rx = (Math.sin(animTime * 0.25 + i * 2.1) * 0.5 + 0.5) * W;
            let ry = H - 18 + Math.sin(animTime * 1.2 + i * 1.5) * 8;
            let a = 0.07 + Math.sin(animTime * 1.8 + i * 2.5) * 0.06;
            fillCircle(rx, ry, 2, rgb(100, 160, 140, a));
            fillCircle(rx + 4, ry + 2, 1.5, rgb(120, 180, 160, a * 0.5));
        }

        // Reflejos luna
        for (let i = 0; i < 5; i++) {
            let rx = mx + Math.sin(animTime * 0.5 + i * 1.3) * 15;
            let ry = H - 25 + i * 6;
            let a = 0.15 - i * 0.025;
            if (a > 0) fillCircle(rx, ry, 3 - i * 0.4, rgb(200, 220, 190, a));
        }

        // Fuegos fatuos
        for (let i = 0; i < 12; i++) {
            let px = W * 0.05 + (Math.sin(animTime * 0.4 + i * 2.8) * 0.5 + 0.5) * W * 0.9;
            let py = H * 0.25 + (Math.cos(animTime * 0.35 + i * 3.4) * 0.5 + 0.5) * H * 0.5;
            let sz = 2 + Math.sin(animTime * 2.5 + i * 3.7) * 1;
            let a = 0.25 + Math.sin(animTime * 2 + i * 2.3) * 0.18;
            let c = i % 3 === 0 ? [80, 200, 180] : i % 3 === 1 ? [180, 220, 80] : [140, 200, 220];
            fillCircle(px, py, Math.max(sz, 0.5), rgb(c[0], c[1], c[2], a));
            fillCircle(px, py, sz * 3, rgb(c[0], c[1], c[2], a * 0.25));
        }

        // Niebla baja (2 capas)
        let fg1 = offCtx.createLinearGradient(0, H * 3 / 5, 0, H);
        fg1.addColorStop(0, 'rgba(60,80,75,0.15)');
        fg1.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = fg1;
        offCtx.fillRect(0, H * 3 / 5, W, H * 2 / 5);

        let fg2 = offCtx.createLinearGradient(0, H * 4 / 5, 0, H);
        fg2.addColorStop(0, 'rgba(40,60,55,0.25)');
        fg2.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = fg2;
        offCtx.fillRect(0, H * 4 / 5, W, H / 5);

        // Bruma
        for (let i = 0; i < 20; i++) {
            let px = (Math.sin(animTime * 0.15 + i * 2.2) * 0.5 + 0.5) * W * 1.3 - W * 0.15;
            let py = H * 0.4 + (Math.cos(animTime * 0.12 + i * 3.6) * 0.5 + 0.5) * H * 0.4;
            let a = 0.05 + Math.sin(animTime * 0.4 + i * 1.9) * 0.04;
            fillCircle(px, py, 4 + Math.sin(animTime * 0.3 + i * 2.5) * 2, rgb(100, 140, 130, a));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  MAZMORRA — piedra oscura, lava, rejas, cadenas, fuego
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawBgDungeon() {
        bgGrad([4, 1, 8], [10, 2, 5]);
        let g1 = offCtx.createLinearGradient(0, 0, 0, H);
        g1.addColorStop(0, 'rgba(20,3,15,0.35)');
        g1.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = g1;
        offCtx.fillRect(0, 0, W, H);

        // Resplandor rojo
        let rg = offCtx.createLinearGradient(0, H / 2, 0, H);
        rg.addColorStop(0, 'rgba(180,30,10,0.2)');
        rg.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = rg;
        offCtx.fillRect(0, H / 2, W, H / 2);

        let rg2 = offCtx.createLinearGradient(0, H * 3 / 4, 0, H);
        rg2.addColorStop(0, 'rgba(220,40,15,0.12)');
        rg2.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = rg2;
        offCtx.fillRect(0, H * 3 / 4, W, H / 4);

        // Pilares
        for (let i = 0; i < 5; i++) {
            let px = 40 + i * W / 4 + 30;
            fillRect(px, 0, 16, H, 20, 15, 25, 0.6);
            fillRect(px + 2, 0, 3, H, 30, 22, 35, 0.4);
            fillRect(px + 11, 0, 3, H, 12, 8, 16, 0.5);
        }

        // Rejas
        for (let i = 0; i < 14; i++) {
            let bx = 80 + i * 60;
            fillRect(bx, H / 4, 3, H / 2, 60, 50, 60, 0.5);
            fillRect(bx + 1, H / 4 + 2, 1, H / 2 - 4, 90, 80, 90, 0.3);
        }
        fillRect(60, H / 3, W - 120, 4, 55, 45, 55, 0.5);
        fillRect(60, H / 2, W - 120, 4, 55, 45, 55, 0.5);

        // Llamas
        for (let i = 0; i < 7; i++) {
            let fx = W * 0.06 + i * W * 0.15 + Math.sin(animTime * 3.5 + i * 2) * 18;
            let fy = H - 12 + Math.sin(animTime * 5 + i * 2.5) * 6;

            fillCircle(fx, H - 5, 20 + Math.sin(animTime * 2 + i * 1.2) * 5, rgb(255, 60, 10, 0.12));
            fillCircle(fx, fy, 14 + Math.sin(animTime * 4 + i * 1.5) * 4, rgb(200, 60, 15, 0.35));
            fillCircle(fx, fy - 4, 9 + Math.sin(animTime * 4.5 + i * 1.8) * 3, rgb(255, 140, 30, 0.3));
            fillCircle(fx, fy - 8, 5 + Math.sin(animTime * 5 + i * 2) * 2, rgb(255, 210, 60, 0.25));
            fillCircle(fx, fy - 11, 2 + Math.sin(animTime * 5.5 + i * 2.2) * 1, rgb(255, 250, 220, 0.2));
        }

        // Ascuas
        for (let i = 0; i < 15; i++) {
            let sx = (Math.sin(animTime * 2 + i * 1.3) * 0.5 + 0.5) * W;
            let sy = H - 30 - (animTime * 35 + i * 20) % (H * 0.6);
            let a = 0.3 - ((H - sy) / H) * 0.2;
            if (a > 0) fillCircle(sx, sy, 1.5 + Math.sin(animTime * 3 + i * 2.5) * 0.5, rgb(255, 150, 30, a));
        }

        // Cadenas
        for (let i = 0; i < 4; i++) {
            let cx = 100 + i * (W - 200) / 3;
            for (let j = 0; j < 8; j++) {
                let cy = 40 + j * 18;
                let sway = Math.sin(animTime * 1.5 + i * 1.7 + j * 0.5) * 4;
                fillRect(cx + sway, cy, 2, 8, 60, 55, 60, 0.4);
                fillRect(cx + sway + 3, cy + 2, 3, 4, 45, 40, 45, 0.3);
            }
        }

        // Techo opresivo y esquinas
        let tg = offCtx.createLinearGradient(0, 0, 0, H / 3);
        tg.addColorStop(0, 'rgba(0,0,0,0.75)');
        tg.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = tg;
        offCtx.fillRect(0, 0, W, H / 3);

        let cgL = offCtx.createLinearGradient(0, 0, 60, 0);
        cgL.addColorStop(0, 'rgba(0,0,0,0.4)');
        cgL.addColorStop(1, 'rgba(0,0,0,0)');
        offCtx.fillStyle = cgL;
        offCtx.fillRect(0, 0, 60, H);

        let cgR = offCtx.createLinearGradient(W - 60, 0, W, 0);
        cgR.addColorStop(0, 'rgba(0,0,0,0)');
        cgR.addColorStop(1, 'rgba(0,0,0,0.4)');
        offCtx.fillStyle = cgR;
        offCtx.fillRect(W - 60, 0, 60, H);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  TRAVEL ANIMATION — sprite transition between locations
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawTravelAnimation() {
        let p = Math.min(travelState.progress, 1);
        let cx = W / 2, cy = H / 2;

        // Fondo oscuro
        let bgG = offCtx.createLinearGradient(0, 0, 0, H);
        bgG.addColorStop(0, '#080a16');
        bgG.addColorStop(1, '#101226');
        offCtx.fillStyle = bgG;
        offCtx.fillRect(0, 0, W, H);

        // Posiciones animadas
        let fromX, fromY, fromW, fromH, fromAlpha;
        let toX, toY, toW, toH, toAlpha;

        let sw = 160, sh = 80; // sprite display size

        if (p < 0.5) {
            // Fase 1 (0-0.5): origen visible, destino apareciendo
            let phase = p / 0.5;
            fromX = cx - sw / 2 - 100 - phase * 60;
            fromY = cy - sh / 2;
            fromW = sw * (1 - phase * 0.2);
            fromH = sh * (1 - phase * 0.2);
            fromAlpha = 1 - phase * 0.5;
            toX = cx + 40 - (1 - phase) * 100;
            toY = cy - sh / 2 + (1 - phase) * 20;
            toW = sw * (0.5 + phase * 0.5);
            toH = sh * (0.5 + phase * 0.5);
            toAlpha = phase * 0.7;
        } else {
            // Fase 2 (0.5-1): origen desaparece, destino se centra
            let phase = (p - 0.5) / 0.5;
            fromX = cx - sw / 2 - 160 - phase * 50;
            fromY = cy - sh / 2 + phase * 15;
            fromW = sw * 0.8 * (1 - phase * 0.7);
            fromH = sh * 0.8 * (1 - phase * 0.7);
            fromAlpha = 0.5 * (1 - phase);
            toX = cx - sw / 2 + phase * 50;
            toY = cy - sh / 2;
            toW = sw;
            toH = sh;
            toAlpha = 0.7 + phase * 0.3;
        }

        // Sprite origen
        if (fromAlpha > 0 && state.playerSprite) {
            offCtx.globalAlpha = fromAlpha;
            drawSprite(state.playerSprite, fromX, fromY, fromW, fromH);
            offCtx.globalAlpha = 1;
        }

        // Sprite destino (location sprite from state)
        if (toAlpha > 0 && state.locationSprite) {
            offCtx.globalAlpha = toAlpha;
            drawSprite(state.locationSprite, toX, toY, toW, toH);
            offCtx.globalAlpha = 1;
        }

        // Línea separadora animada
        let lineY = cy + 50;
        let lineW = 200 + Math.sin(p * Math.PI) * 100;
        let lineA = 0.3 + Math.sin(p * Math.PI) * 0.3;
        offCtx.fillStyle = rgb(255, 200, 50, lineA);
        offCtx.fillRect(cx - lineW / 2, lineY, lineW, 2);

        // Nombre destino
        let textAlpha = Math.min((p - 0.3) * 3, 1);
        if (textAlpha > 0 && state.locationName) {
            let ty = lineY + 16;
            let glow = 0.8 + Math.sin(animTime * 4 + p * 2) * 0.2;
            offCtx.fillStyle = rgb(255, 220, 50, textAlpha * glow);
            offCtx.font = 'bold 20px monospace';
            offCtx.textAlign = 'center';
            offCtx.fillText('→ ' + state.locationName + ' ←', cx, ty);
        }

        // Partículas brillantes
        for (let i = 0; i < 8; i++) {
            let px = cx + Math.sin(animTime * 3 + i * 1.5 + p * 2) * 80 * (1 + p);
            let py = cy + Math.cos(animTime * 2.5 + i * 2.1 + p * 1.5) * 60 * (1 + p);
            let pa = 0.3 + Math.sin(animTime * 4 + i * 2) * 0.2;
            fillCircle(px, py, 2 + i % 2, rgb(255, 220, 100, pa * 0.8));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  TITLE SCREEN
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawTitle() {
        let cx = W / 2;
        offCtx.fillStyle = rgb(255, 200, 50);
        offCtx.font = 'bold 36px monospace';
        offCtx.textAlign = 'center';
        offCtx.fillText('TEXTRPG', cx, 120);

        offCtx.fillStyle = rgb(130, 140, 165);
        offCtx.font = '16px monospace';
        offCtx.fillText('RPG de Texto Premium', cx, 155);

        if (state.playerSprite) drawSprite(state.playerSprite, cx - 60, 180, 120, 120);
        if (state.enemySprite) drawSprite(state.enemySprite, cx + 60 - 30, 200, 60, 60);

        let pulse = 0.5 + Math.sin(animTime * 3) * 0.3;
        offCtx.fillStyle = rgb(60, 75, 110, pulse);
        offCtx.font = '14px monospace';
        offCtx.fillText('Pulsa ENTER o haz clic para comenzar', cx, 390);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  MAIN SCENE
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawMainScene() {
        let cx = W / 2;
        let breath = Math.sin(animState.breath) * 3;

        if (state.playerSprite) drawSprite(state.playerSprite, 60, 90 + breath, 120, 120);

        offCtx.fillStyle = rgb(255, 200, 50);
        offCtx.font = 'bold 14px monospace';
        offCtx.textAlign = 'center';
        offCtx.fillText(state.playerName || '', 120, 230);

        if (state.hpBar) drawHpBar(40, 240, 160, 14, state.hpBar.current, state.hpBar.max, COLORS.hpGreen);

        if (state.locationSprite) drawSprite(state.locationSprite, W - 140, 20, 100, 50);

        offCtx.fillStyle = rgb(255, 200, 50);
        offCtx.font = '14px monospace';
        offCtx.textAlign = 'left';
        offCtx.fillText(state.locationName || '', W - 200, 14);
        offCtx.fillStyle = rgb(100, 100, 120);
        offCtx.font = '11px monospace';
        let desc = state.locationDesc || '';
        if (desc.length > 60) desc = desc.substring(0, 60) + '...';
        offCtx.fillText(desc, W - 200, 32);

        offCtx.fillStyle = rgb(130, 140, 165);
        offCtx.font = '12px monospace';
        offCtx.textAlign = 'right';
        offCtx.fillText('Lv.' + (state.playerLevel || 1) + ' ' + (state.playerClass || ''), W - 10, 250);
        offCtx.fillText('ATK:' + (state.playerAtk || 0) + ' DEF:' + (state.playerDef || 0), W - 10, 266);
        offCtx.fillText('Oro:' + (state.playerGold || 0), W - 10, 282);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  COMBAT
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawCombat() {
        let atkP = animState.atk > 0 ? Math.sin(animState.atk / 0.3 * Math.PI) : 0;
        let aOff = atkP * 30;
        let hOff = animState.hit > 0 ? Math.sin(animState.hit / 0.5 * Math.PI * 1.5) * 15 : 0;
        let breath = Math.sin(animState.breath) * 3;

        let px = 120, py = 80;
        let ex = 580, ey = 80;

        // Player
        if (state.playerSprite) {
            offCtx.save();
            offCtx.translate(aOff, 0);
            drawSprite(state.playerSprite, px - 60, py + breath, 120, 120);
            offCtx.restore();
        }

        // Enemy
        if (state.enemySprite) {
            let hf = animState.hit > 0 ? Math.sin(animState.hit * 30) * 0.4 + 0.6 : 1;
            offCtx.save();
            offCtx.translate(hOff, 0);
            offCtx.globalAlpha = hf;
            drawSprite(state.enemySprite, ex - 40, ey, 80, 80);
            offCtx.globalAlpha = 1;
            offCtx.restore();
        }

        // Attack effect
        if (animState.atk > 0.12 && state.effectSprite) {
            let aa = Math.min((animState.atk - 0.12) / 0.18, 1);
            offCtx.save();
            offCtx.globalAlpha = aa;
            offCtx.translate(hOff, 0);
            let rot = Math.sin(animTime * 30) * 0.4;
            offCtx.translate(ex, ey + 40);
            offCtx.rotate(rot);
            drawSprite(state.effectSprite, -40, -40, 80, 80);
            offCtx.restore();
        }

        // Names
        offCtx.fillStyle = rgb(225, 230, 240);
        offCtx.font = 'bold 12px monospace';
        offCtx.textAlign = 'center';
        offCtx.fillText(state.playerName || '', px, py + 130);
        offCtx.fillStyle = rgb(240, 70, 70);
        offCtx.fillText(state.enemyName || '', ex, ey + 90);

        // HP bars
        if (state.playerHpBar) drawHpBar(px - 60, py + 137, 120, 10, state.playerHpBar.current, state.playerHpBar.max, COLORS.hpGreen);
        if (state.enemyHpBar) drawHpBar(ex - 60, ey + 96, 120, 10, state.enemyHpBar.current, state.enemyHpBar.max, COLORS.hpRed);

        // Round counter
        offCtx.fillStyle = rgb(130, 140, 165);
        offCtx.font = '11px monospace';
        offCtx.textAlign = 'center';
        offCtx.fillText('Ronda ' + (state.combatRound || 0), W / 2, 200);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  SPRITE DRAWING
    // ═══════════════════════════════════════════════════════════════════════════════
    const _spriteCache = new Map();

    function _getSpriteKey(sprite) {
        if (!sprite || !sprite.pixels || sprite.width <= 0 || sprite.height <= 0) return '';
        let key = sprite.width + 'x' + sprite.height;
        let sampled = 0;
        for (let i = 0; i < sprite.pixels.length && sampled < 50; i++) {
            let p = sprite.pixels[i];
            if (p && p.a > 0) {
                key += '-' + (p.r|0) + ',' + (p.g|0) + ',' + (p.b|0);
                sampled++;
            }
        }
        return key;
    }

    function drawSprite(sprite, x, y, w, h) {
        if (!sprite || !sprite.pixels) return;
        let sw = sprite.width, sh = sprite.height;
        if (sw <= 0 || sh <= 0) return;

        let key = _getSpriteKey(sprite);
        if (!key) return;

        let tmp = _spriteCache.get(key);
        if (!tmp) {
            tmp = document.createElement('canvas');
            tmp.width = sw;
            tmp.height = sh;
            let tc = tmp.getContext('2d');
            for (let py = 0; py < sh; py++) {
                for (let px = 0; px < sw; px++) {
                    let p = sprite.pixels[py * sw + px];
                    if (!p || p.a === 0) continue;
                    tc.fillStyle = rgb(p.r, p.g, p.b);
                    tc.fillRect(px, py, 1, 1);
                }
            }
            if (_spriteCache.size >= 32) {
                const firstKey = _spriteCache.keys().next().value;
                _spriteCache.delete(firstKey);
            }
            _spriteCache.set(key, tmp);
        }
        offCtx.imageSmoothingEnabled = false;
        offCtx.drawImage(tmp, x, y, w, h);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  HP BAR
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawHpBar(x, y, w, h, current, max, color) {
        let pct = max > 0 ? current / max : 0;
        let c = pct > 0.5 ? COLORS.hpGreen : pct > 0.25 ? COLORS.hpYellow : COLORS.hpRed;
        offCtx.fillStyle = '#333';
        offCtx.fillRect(x, y, w, h);
        offCtx.fillStyle = rgb(c[0], c[1], c[2]);
        offCtx.fillRect(x, y, Math.floor(w * pct), h);
        offCtx.strokeStyle = '#888';
        offCtx.lineWidth = 1;
        offCtx.strokeRect(x, y, w, h);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  LOG
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawLog() {
        if (!state.messages || state.messages.length === 0) return;
        let msgs = state.messages.slice(-6);
        offCtx.fillStyle = rgb(10, 10, 20, 0.85);
        offCtx.fillRect(0, H - 110, W, 110);
        offCtx.strokeStyle = '#333';
        offCtx.lineWidth = 1;
        offCtx.strokeRect(0, H - 110, W, 110);

        offCtx.font = '11px monospace';
        offCtx.textAlign = 'left';
        for (let i = 0; i < msgs.length; i++) {
            let msg = msgs[i];
            let color = [200, 200, 200];
            if (msg.includes('CRITICO') || msg.includes('critico')) color = [255, 160, 40];
            else if (msg.includes('Victoria') || msg.includes('SUBISTE')) color = [80, 220, 80];
            else if (msg.includes('Derrota') || msg.includes('caido')) color = [240, 60, 60];
            else if (msg.includes('oro') || msg.includes('EXP')) color = [255, 210, 60];
            else if (msg.includes('esquiva')) color = [100, 200, 220];
            offCtx.fillStyle = rgb(color[0], color[1], color[2]);
            offCtx.fillText(msg, 10, H - 93 + i * 15);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  MENU
    // ═══════════════════════════════════════════════════════════════════════════════
    function drawMenu() {
        if (!state.menu || state.menu.length === 0) return;
        offCtx.font = '12px monospace';
        offCtx.textAlign = 'center';
        for (let i = 0; i < state.menu.length; i++) {
            let isFight = state.menu[i].includes('Atacar');
            let isStart = state.menu[i].includes('Iniciar');
            offCtx.fillStyle = isFight ? rgb(180, 50, 50) : isStart ? rgb(50, 160, 50) : rgb(40, 80, 140);
            let bw = 180, bh = 26;
            let bx = W / 2 - bw / 2;
            let by = 420 + i * 28;
            offCtx.fillRect(bx, by, bw, bh);
            offCtx.strokeStyle = isFight ? '#8c3a3a' : isStart ? '#4a8c4a' : '#1a5276';
            offCtx.lineWidth = 1;
            offCtx.strokeRect(bx, by, bw, bh);
            offCtx.fillStyle = rgb(225, 230, 240);
            offCtx.fillText(state.menu[i], W / 2, by + 17);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  FOCUS / PERSISTENCE
    // ═══════════════════════════════════════════════════════════════════════════════
    function focusGame() {
        const container = document.querySelector('.game-container');
        if (container) {
            container.focus();
            canvas.addEventListener('click', () => container.focus());
        }
    }

    function saveGame(json) {
        try { localStorage.setItem('TextRPG_save', json); return true; }
        catch(e) { console.warn('Save failed:', e); return false; }
    }

    function loadGame() {
        try { return localStorage.getItem('TextRPG_save') || null; }
        catch(e) { return null; }
    }

    function deleteSave() {
        try { localStorage.removeItem('TextRPG_save'); } catch(e) {}
    }

    function hasSave() {
        try { return localStorage.getItem('TextRPG_save') !== null; }
        catch(e) { return false; }
    }

    window.TextRPG = window.TextRPG || {};
    window.TextRPG.Renderer = { init, pushState, focusGame, saveGame, loadGame, deleteSave, hasSave };
})();
