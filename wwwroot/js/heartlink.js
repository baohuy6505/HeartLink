/* ============================================================
   HeartLink — Core JavaScript Utility Module
   ============================================================ */
window.HeartLink = (() => {
    const AUTH_KEY = 'heartlink_auth';

    // ── Auth helpers ──────────────────────────────────────────
    function getAuth() {
        try { return JSON.parse(localStorage.getItem(AUTH_KEY) || 'null'); }
        catch { return null; }
    }

    function saveAuth(data) {
        localStorage.setItem(AUTH_KEY, JSON.stringify({
            token:     data.token,
            accountId: data.accountId,
            email:     data.email    || null,
            role:      data.role     || 'User',
            fullName:  data.fullName || null
        }));
        renderNavbar();
    }

    function clearAuth() {
        localStorage.removeItem(AUTH_KEY);
        renderNavbar();
    }

    function getToken() { return getAuth()?.token || null; }

    function requireAuth() {
        const auth = getAuth();
        if (!auth?.token) { window.location.href = '/login'; return null; }
        return auth;
    }

    function requireAdmin() {
        const auth = requireAuth();
        if (!auth) return null;
        if (auth.role !== 'Admin') { window.location.href = '/discovery'; return null; }
        return auth;
    }

    // ── API Fetch ─────────────────────────────────────────────
    async function apiFetch(url, options = {}) {
        const auth = getAuth();
        const headers = options.headers ? { ...options.headers } : {};

        if (!(options.body instanceof FormData)) {
            headers['Content-Type'] = 'application/json';
        }

        if (options.auth !== false && auth?.token) {
            headers['Authorization'] = `Bearer ${auth.token}`;
        }

        const response = await fetch(url, {
            method: options.method || 'GET',
            headers,
            body: options.body
                ? (options.body instanceof FormData ? options.body : JSON.stringify(options.body))
                : undefined
        });

        const contentType = response.headers.get('content-type') || '';
        const data = contentType.includes('application/json')
            ? await response.json()
            : await response.text();

        if (!response.ok) {
            // Handle validation problem details
            if (data?.errors) {
                const messages = Object.values(data.errors).flat().join(' ');
                throw new Error(messages || data.title || 'Lỗi validation.');
            }
            throw new Error(data?.message || data?.title || data || 'Có lỗi xảy ra khi gọi API.');
        }

        return data;
    }

    // ── Navbar ────────────────────────────────────────────────
    function renderNavbar() {
        const auth = getAuth();

        document.querySelectorAll('.guest-only').forEach(x => {
            x.style.display = auth?.token ? 'none' : '';
        });
        document.querySelectorAll('.auth-only').forEach(x => {
            x.style.display = auth?.token ? '' : 'none';
        });
        document.querySelectorAll('.admin-only').forEach(x => {
            x.style.display = auth?.role === 'Admin' ? '' : 'none';
        });

        const navUserInfo = document.getElementById('navUserInfo');
        if (navUserInfo) {
            navUserInfo.textContent = auth?.token
                ? (auth.fullName || auth.email || 'Đã đăng nhập')
                : '';
        }

        const btnLogout = document.getElementById('btnLogout');
        if (btnLogout) {
            btnLogout.onclick = async () => {
                try {
                    if (auth?.token) {
                        await apiFetch('/api/client/auth/logout', { method: 'POST' });
                    }
                } catch { /* ignore */ } finally {
                    clearAuth();
                    window.location.href = '/login';
                }
            };
        }
    }

    // ── Toast Notifications ───────────────────────────────────
    function showToast(message, type = 'info', duration = 3500) {
        let container = document.getElementById('hl-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'hl-toast-container';
            document.body.appendChild(container);
        }

        const icons = { success: '✅', danger: '❌', warning: '⚠️', info: 'ℹ️' };
        const toast = document.createElement('div');
        toast.className = `hl-toast ${type}`;
        toast.innerHTML = `
            <span class="toast-icon">${icons[type] || icons.info}</span>
            <span class="toast-msg">${escapeHtml(message)}</span>
            <span class="toast-close" onclick="this.closest('.hl-toast').remove()">✕</span>
        `;
        container.appendChild(toast);

        setTimeout(() => {
            toast.classList.add('removing');
            setTimeout(() => toast.remove(), 320);
        }, duration);
    }

    // ── Legacy helpers (kept for backward compat) ─────────────
    function setHtml(id, html) {
        const el = document.getElementById(id);
        if (el) el.innerHTML = html;
    }

    function showMessage(id, message, type = 'info') {
        const el = document.getElementById(id);
        if (!el) return;
        el.className = `alert alert-${type}`;
        el.textContent = message;
        el.style.display = 'block';
        el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    function clearMessage(id) {
        const el = document.getElementById(id);
        if (!el) return;
        el.textContent = '';
        el.style.display = 'none';
        el.className = 'alert';
    }

    // ── Formatting ────────────────────────────────────────────
    function formatDateTime(value) {
        if (!value) return '';
        return new Date(value).toLocaleString('vi-VN');
    }

    function formatDate(value) {
        if (!value) return '';
        return new Date(value).toLocaleDateString('vi-VN');
    }

    function formatTimeShort(value) {
        if (!value) return '';
        const d = new Date(value);
        const now = new Date();
        const diffH = (now - d) / 3600000;
        if (diffH < 24) return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
        return d.toLocaleDateString('vi-VN');
    }

    function calculateAge(birthDate) {
        if (!birthDate) return 0;
        const today = new Date(), dob = new Date(birthDate);
        let age = today.getFullYear() - dob.getFullYear();
        if (today.getMonth() < dob.getMonth() ||
            (today.getMonth() === dob.getMonth() && today.getDate() < dob.getDate())) age--;
        return age;
    }

    // ── Escape HTML ───────────────────────────────────────────
    function escapeHtml(str) {
        const d = document.createElement('div');
        d.textContent = str;
        return d.innerHTML;
    }

    // ── Avatar fallback ───────────────────────────────────────
    function avatarUrl(url, size = 'md') {
        if (url && url.trim()) return url.trim();
        return `https://ui-avatars.com/api/?name=User&background=ede9fe&color=7c3aed&size=${size === 'sm' ? 64 : 128}&bold=true&font-size=0.4`;
    }

    function namedAvatar(name, size = 'md') {
        const encoded = encodeURIComponent(name || 'HL');
        return `https://ui-avatars.com/api/?name=${encoded}&background=ede9fe&color=7c3aed&size=${size === 'sm' ? 64 : 128}&bold=true&font-size=0.4`;
    }

    // ── GPS Location ──────────────────────────────────────────
    function getCurrentPosition() {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation) {
                reject(new Error('Trình duyệt không hỗ trợ định vị.'));
            } else {
                navigator.geolocation.getCurrentPosition(resolve, (e) => {
                    reject(new Error('Không thể lấy vị trí. Vui lòng cho phép quyền định vị.'));
                }, { timeout: 10000 });
            }
        });
    }

    async function updateLocation() {
        try {
            const pos = await getCurrentPosition();
            const { latitude, longitude } = pos.coords;
            await apiFetch('/api/client/profile/location', {
                method: 'PUT',
                body: { latitude, longitude }
            });
            showToast('Đã cập nhật vị trí thành công! 📍', 'success');
            return { latitude, longitude };
        } catch (err) {
            showToast(err.message, 'danger');
            throw err;
        }
    }

    // ── Loading HTML ──────────────────────────────────────────
    function loadingHtml() {
        return `<div class="hl-loading"><div class="hl-spinner"></div><div>Đang tải...</div></div>`;
    }

    function emptyHtml(icon, title, subtitle = '') {
        return `
        <div class="hl-empty">
            <span class="hl-empty-icon">${icon}</span>
            <div class="hl-empty-title">${title}</div>
            ${subtitle ? `<div class="hl-meta mt-2">${subtitle}</div>` : ''}
        </div>`;
    }

    // ── Init ──────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', renderNavbar);

    return {
        getAuth, saveAuth, clearAuth, getToken,
        requireAuth, requireAdmin,
        apiFetch,
        renderNavbar,
        showToast,
        setHtml, showMessage, clearMessage,
        formatDate, formatDateTime, formatTimeShort,
        calculateAge,
        escapeHtml, avatarUrl, namedAvatar,
        updateLocation, getCurrentPosition,
        loadingHtml, emptyHtml
    };
})();