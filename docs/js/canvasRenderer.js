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
