(function () {
    'use strict';

    const W = 800, H = 450;
    let canvas, ctx, offscreen, offCtx;
    let scale = 1;
    let animTime = 0, lastTime = 0;
    let state = null;
    let animState = { atk: 0, hit: 0, shakeX: 0, shakeY: 0, flash: 0, breath: 0 };
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

    function pushState(s) {
        if (!s) return;
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

        drawBackground();
        if (state.screen === 'Title') { drawTitle(); return; }

        let sx = animState.shakeX > 0.5 ? (Math.sin(animTime * 130) * animState.shakeX) : 0;
        let sy = animState.shakeY > 0.5 ? (Math.cos(animTime * 150) * animState.shakeY) : 0;

        offCtx.save();
        offCtx.translate(sx, sy);

        if (state.screen === 'Combat' || state.screen === 'Victory' || state.screen === 'Defeat') {
            drawCombat();
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

    // ─── Background ───
    function drawBackground() {
        let loc = state.location || 'village';
        switch (loc) {
            case 'village': bgGrad([55, 38, 22], [75, 55, 30]); bgStars(12, [255, 210, 80]); break;
            case 'forest': bgGrad([4, 18, 8], [10, 22, 6]); bgMoonRays(); bgFog(); break;
            case 'cave': bgGrad([4, 4, 8], [8, 6, 4]); bgCrystals(); break;
            case 'ruins': bgGrad([32, 24, 16], [48, 35, 18]); bgSunset(); bgDust(); break;
            case 'dungeon': bgGrad([5, 2, 10], [12, 3, 6]); bgFires(); break;
            default: bgGrad([10, 14, 28], [16, 20, 38]); break;
        }
    }

    function bgGrad(top, bot) {
        let g = offCtx.createLinearGradient(0, 0, 0, H);
        g.addColorStop(0, rgb(top[0], top[1], top[2]));
        g.addColorStop(1, rgb(bot[0], bot[1], bot[2]));
        offCtx.fillStyle = g;
        offCtx.fillRect(0, 0, W, H);
    }

    function bgStars(n, color) {
        for (let i = 0; i < n; i++) {
            let px = (Math.sin(animTime * 0.7 + i * 2.1) * 0.5 + 0.5) * W * 0.85 + W * 0.05;
            let py = (Math.cos(animTime * 0.5 + i * 2.7) * 0.5 + 0.5) * H * 0.6 + H * 0.1;
            let sz = 2 + Math.sin(animTime * 2 + i * 3) * 1;
            let a = 0.5 + Math.sin(animTime * 2.5 + i * 3) * 0.3;
            offCtx.fillStyle = rgb(color[0], color[1], color[2], a);
            offCtx.beginPath(); offCtx.arc(px, py, Math.max(sz, 1), 0, Math.PI * 2); offCtx.fill();
            offCtx.fillStyle = rgb(color[0], color[1], color[2], a * 0.3);
            offCtx.beginPath(); offCtx.arc(px, py, Math.max(sz * 2, 2), 0, Math.PI * 2); offCtx.fill();
        }
    }

    function bgMoonRays() {
        for (let i = 0; i < 6; i++) {
            let rx = W * 0.05 + i * W * 0.18;
            let a = 0.12 + Math.sin(animTime * 0.8 + i * 1.3) * 0.08;
            let g = offCtx.createLinearGradient(rx, 0, rx + 5 + (i % 2) * 3, 0);
            g.addColorStop(0, rgb(200, 230, 210, a));
            g.addColorStop(1, rgb(200, 230, 210, 0));
            offCtx.fillStyle = g;
            offCtx.fillRect(rx, 0, 5 + (i % 2) * 3, H);
        }
    }

    function bgFog() {
        for (let i = 0; i < 12; i++) {
            let px = (Math.sin(animTime * 0.3 + i * 2.5) * 0.5 + 0.5) * W;
            let py = (Math.cos(animTime * 0.2 + i * 3.3) * 0.5 + 0.5) * H;
            offCtx.fillStyle = rgb(120, 200, 140, 0.2);
            offCtx.beginPath(); offCtx.arc(px, py, 2, 0, Math.PI * 2); offCtx.fill();
        }
    }

    function bgCrystals() {
        let gems = [[60, 100], [160, 220], [640, 70], [720, 190], [350, 340], [580, 280], [760, 350], [50, 380]];
        for (let g of gems) {
            let a = 0.5 + Math.sin(animTime * 2 + g[0] * 0.1) * 0.3;
            let sz = 3 + Math.sin(animTime * 2.5 + g[1] * 0.1) * 1.5;
            offCtx.fillStyle = rgb(60, 140, 230, a);
            offCtx.beginPath(); offCtx.arc(g[0], g[1], Math.max(sz, 1), 0, Math.PI * 2); offCtx.fill();
            offCtx.fillStyle = rgb(60, 140, 230, a * 0.25);
            offCtx.beginPath(); offCtx.arc(g[0], g[1], Math.max(sz * 2.5, 2), 0, Math.PI * 2); offCtx.fill();
        }
    }

    function bgSunset() {
        let g = offCtx.createRadialGradient(W / 2, 40, 10, W / 2, 40, 200);
        g.addColorStop(0, rgb(255, 180, 60, 0.3));
        g.addColorStop(1, rgb(255, 180, 60, 0));
        offCtx.fillStyle = g;
        offCtx.fillRect(0, 0, W, H);
    }

    function bgDust() {
        for (let i = 0; i < 16; i++) {
            let px = (Math.sin(animTime * 0.4 + i * 1.7) * 0.5 + 0.5) * W;
            let py = (Math.cos(animTime * 0.3 + i * 2.9) * 0.5 + 0.5) * H * 0.6 + H * 0.2;
            offCtx.fillStyle = rgb(220, 180, 120, 0.2);
            offCtx.beginPath(); offCtx.arc(px, py, 2, 0, Math.PI * 2); offCtx.fill();
        }
    }

    function bgFires() {
        for (let i = 0; i < 6; i++) {
            let fx = W * 0.08 + i * W * 0.17 + Math.sin(animTime * 4 + i * 2) * 15;
            let fy = H - 20 + Math.sin(animTime * 6 + i * 2.5) * 8;
            offCtx.fillStyle = rgb(255, 100, 20, 0.4);
            offCtx.beginPath(); offCtx.arc(fx, fy, 8 + Math.sin(animTime * 4 + i * 1.5) * 3, 0, Math.PI * 2); offCtx.fill();
            offCtx.fillStyle = rgb(255, 180, 40, 0.3);
            offCtx.beginPath(); offCtx.arc(fx, fy - 4, 4 + Math.sin(animTime * 5 + i * 1.8) * 2, 0, Math.PI * 2); offCtx.fill();
            offCtx.fillStyle = rgb(255, 220, 80, 0.25);
            offCtx.beginPath(); offCtx.arc(fx, fy - 7, 2 + Math.sin(animTime * 6 + i * 2) * 1, 0, Math.PI * 2); offCtx.fill();
        }
    }

    // ─── Title ───
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

    // ─── Main Scene ───
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

    // ─── Combat ───
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

    // ─── Sprite drawing ───
    const _spriteCache = new Map();

    function drawSprite(sprite, x, y, w, h) {
        if (!sprite || !sprite.pixels) return;
        let sw = sprite.width, sh = sprite.height;
        if (sw <= 0 || sh <= 0) return;

        let key = sprite;
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
            _spriteCache.set(key, tmp);
        }
        offCtx.imageSmoothingEnabled = false;
        offCtx.drawImage(tmp, x, y, w, h);
    }

    // ─── HP Bar ───
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

    // ─── Log ───
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

    // ─── Menu ───
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

    window.TextRPG = window.TextRPG || {};
    window.TextRPG.Renderer = { init, pushState };
})();
