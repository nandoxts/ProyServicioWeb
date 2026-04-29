/* ============================================================
   APP.JS — Módulo central del frontend (NexusShop)
   ------------------------------------------------------------
   Expone window.App con utilidades compartidas:
     App.toast(msg, opts?)              -> toast emergente
     App.confirm(opts) -> Promise<bool> -> modal de confirmación
     App.fetchJson(url, opts?)          -> wrapper de fetch
     App.money(n, currency?)            -> formatea como S/ 123.45
     App.formatDate(d, opts?)           -> formatea fecha local
     App.copy(text)                     -> copia al portapapeles
     App.qs / App.qsa                   -> selectores cortos
     App.debounce(fn, wait?)            -> debounce
   ============================================================ */
(function (global) {
    'use strict';

    /* ── Selectores rápidos ────────────────────────────────── */
    const qs  = (sel, root = document) => root.querySelector(sel);
    const qsa = (sel, root = document) => Array.from(root.querySelectorAll(sel));

    /* ── Debounce ──────────────────────────────────────────── */
    function debounce(fn, wait = 250) {
        let t;
        return function (...args) {
            clearTimeout(t);
            t = setTimeout(() => fn.apply(this, args), wait);
        };
    }

    /* ── Toast host ────────────────────────────────────────── */
    function ensureToastHost() {
        let host = qs('.app-toast-host');
        if (!host) {
            host = document.createElement('div');
            host.className = 'app-toast-host';
            document.body.appendChild(host);
        }
        return host;
    }

    const TOAST_ICONS = {
        success: '✓',
        danger:  '✕',
        warning: '!',
        info:    'i'
    };

    function toast(message, opts = {}) {
        const {
            type = 'info',
            title = null,
            duration = 3500
        } = (typeof opts === 'string') ? { type: opts } : opts;

        const host = ensureToastHost();
        const el = document.createElement('div');
        el.className = `app-toast app-toast--${type}`;
        el.setAttribute('role', 'status');

        const icon  = document.createElement('span');
        icon.className = 'app-toast__icon';
        icon.textContent = TOAST_ICONS[type] || TOAST_ICONS.info;

        const body = document.createElement('div');
        body.className = 'app-toast__body';
        if (title) {
            const t = document.createElement('div');
            t.className = 'app-toast__title';
            t.textContent = title;
            body.appendChild(t);
        }
        const msg = document.createElement('div');
        msg.className = title ? 'app-toast__text' : 'app-toast__title';
        msg.textContent = message;
        body.appendChild(msg);

        const close = document.createElement('button');
        close.className = 'app-toast__close';
        close.type = 'button';
        close.setAttribute('aria-label', 'Cerrar');
        close.innerHTML = '&times;';
        close.addEventListener('click', dismiss);

        el.appendChild(icon);
        el.appendChild(body);
        el.appendChild(close);
        host.appendChild(el);

        let timer = duration > 0 ? setTimeout(dismiss, duration) : null;

        function dismiss() {
            if (timer) clearTimeout(timer);
            el.classList.add('is-leaving');
            setTimeout(() => el.remove(), 200);
        }

        return { dismiss };
    }

    /* ── Confirmación (Promise<boolean>) ───────────────────── */
    function confirm(opts = {}) {
        const {
            title = 'Confirmar',
            message = '¿Estás seguro?',
            okText = 'Aceptar',
            cancelText = 'Cancelar',
            danger = false
        } = (typeof opts === 'string') ? { message: opts } : opts;

        return new Promise(resolve => {
            const backdrop = document.createElement('div');
            backdrop.className = 'app-confirm-backdrop';

            const box = document.createElement('div');
            box.className = 'app-confirm';
            box.setAttribute('role', 'dialog');
            box.setAttribute('aria-modal', 'true');

            box.innerHTML = `
                <h3 class="app-confirm__title"></h3>
                <p class="app-confirm__msg"></p>
                <div class="app-confirm__actions">
                    <button type="button" class="app-btn app-btn--ghost" data-act="cancel"></button>
                    <button type="button" class="app-btn ${danger ? 'app-btn--danger' : 'app-btn--primary'}" data-act="ok"></button>
                </div>
            `;
            box.querySelector('.app-confirm__title').textContent = title;
            box.querySelector('.app-confirm__msg').textContent = message;
            box.querySelector('[data-act="cancel"]').textContent = cancelText;
            box.querySelector('[data-act="ok"]').textContent = okText;

            function close(value) {
                document.removeEventListener('keydown', onKey);
                backdrop.remove();
                resolve(value);
            }
            function onKey(e) {
                if (e.key === 'Escape') close(false);
                if (e.key === 'Enter')  close(true);
            }

            backdrop.addEventListener('click', e => {
                if (e.target === backdrop) close(false);
            });
            box.querySelector('[data-act="cancel"]').addEventListener('click', () => close(false));
            box.querySelector('[data-act="ok"]').addEventListener('click', () => close(true));
            document.addEventListener('keydown', onKey);

            backdrop.appendChild(box);
            document.body.appendChild(backdrop);
            box.querySelector('[data-act="ok"]').focus();
        });
    }

    /* ── fetchJson: wrapper con manejo de errores ──────────── */
    async function fetchJson(url, opts = {}) {
        const init = Object.assign({
            headers: { 'Accept': 'application/json' },
            credentials: 'same-origin'
        }, opts);

        if (init.body && typeof init.body === 'object' && !(init.body instanceof FormData)) {
            init.headers['Content-Type'] = 'application/json';
            init.body = JSON.stringify(init.body);
        }

        const res = await fetch(url, init);
        const ct = res.headers.get('content-type') || '';
        const data = ct.includes('application/json') ? await res.json().catch(() => null) : null;

        if (!res.ok) {
            const err = new Error((data && (data.message || data.error)) || `HTTP ${res.status}`);
            err.status = res.status;
            err.data = data;
            throw err;
        }
        return data;
    }

    /* ── Formato de moneda ─────────────────────────────────── */
    function money(value, currency = 'PEN') {
        const n = Number(value) || 0;
        try {
            return new Intl.NumberFormat('es-PE', {
                style: 'currency',
                currency
            }).format(n);
        } catch {
            return `S/ ${n.toFixed(2)}`;
        }
    }

    /* ── Formato de fecha ──────────────────────────────────── */
    function formatDate(d, opts = { dateStyle: 'medium' }) {
        const date = (d instanceof Date) ? d : new Date(d);
        if (isNaN(date)) return '';
        try {
            return new Intl.DateTimeFormat('es-PE', opts).format(date);
        } catch {
            return date.toLocaleDateString();
        }
    }

    /* ── Copiar al portapapeles ────────────────────────────── */
    async function copy(text) {
        try {
            await navigator.clipboard.writeText(text);
            toast('Copiado al portapapeles', { type: 'success', duration: 1800 });
            return true;
        } catch {
            toast('No se pudo copiar', { type: 'danger' });
            return false;
        }
    }

    /* ── Auto-init: data-attributes declarativos ──────────── */
    function autoInit() {
        // Confirmación en links/forms con data-confirm="mensaje"
        document.addEventListener('click', async function (e) {
            const trigger = e.target.closest('[data-confirm]');
            if (!trigger) return;

            // Si ya está confirmado (segunda vuelta), dejar pasar
            if (trigger.dataset.confirmed === '1') return;

            e.preventDefault();
            e.stopPropagation();
            const ok = await confirm({
                message: trigger.dataset.confirm,
                danger: trigger.dataset.confirmDanger === '1' || trigger.classList.contains('app-btn--danger'),
                okText: trigger.dataset.confirmOk || 'Confirmar',
                cancelText: trigger.dataset.confirmCancel || 'Cancelar'
            });
            if (!ok) return;

            trigger.dataset.confirmed = '1';
            // Re-disparar el click natural
            if (trigger.tagName === 'A') {
                window.location.href = trigger.href;
            } else if (trigger.tagName === 'BUTTON' && trigger.form) {
                trigger.form.submit();
            } else {
                trigger.click();
            }
        }, true);

        // Toasts del servidor: <div data-app-toast="success">Mensaje</div>
        qsa('[data-app-toast]').forEach(el => {
            toast(el.textContent.trim(), { type: el.dataset.appToast || 'info' });
            el.remove();
        });

        // Botones de copiar: <button data-copy="texto">
        document.addEventListener('click', e => {
            const btn = e.target.closest('[data-copy]');
            if (!btn) return;
            e.preventDefault();
            copy(btn.dataset.copy || btn.textContent);
        });
    }

    /* ── Exponer API global ────────────────────────────────── */
    const App = {
        toast,
        confirm,
        fetchJson,
        money,
        formatDate,
        copy,
        debounce,
        qs, qsa,
        version: '1.0.0'
    };

    global.App = App;

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', autoInit);
    } else {
        autoInit();
    }

})(window);
