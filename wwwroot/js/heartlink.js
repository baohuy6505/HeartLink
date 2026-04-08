window.HeartLink = (() => {
    const AUTH_KEY = "heartlink_auth";

    function getAuth() {
        try {
            return JSON.parse(localStorage.getItem(AUTH_KEY) || "null");
        } catch {
            return null;
        }
    }

    function saveAuth(data) {
        localStorage.setItem(AUTH_KEY, JSON.stringify({
            token: data.token,
            accountId: data.accountId,
            email: data.email || null,
            role: data.role || "User",
            fullName: data.fullName || null
        }));
        renderNavbar();
    }

    function clearAuth() {
        localStorage.removeItem(AUTH_KEY);
        renderNavbar();
    }

    function getToken() {
        return getAuth()?.token || null;
    }

    function requireAuth() {
        const auth = getAuth();
        if (!auth?.token) {
            window.location.href = "/login";
            return null;
        }
        return auth;
    }

    function requireAdmin() {
        const auth = requireAuth();
        if (!auth) return null;

        if (auth.role !== "Admin") {
            window.location.href = "/discovery";
            return null;
        }

        return auth;
    }

    async function apiFetch(url, options = {}) {
        const auth = getAuth();
        const headers = options.headers ? { ...options.headers } : {};

        if (!(options.body instanceof FormData)) {
            headers["Content-Type"] = "application/json";
        }

        if (options.auth !== false && auth?.token) {
            headers["Authorization"] = `Bearer ${auth.token}`;
        }

        const response = await fetch(url, {
            method: options.method || "GET",
            headers,
            body: options.body
                ? (options.body instanceof FormData ? options.body : JSON.stringify(options.body))
                : undefined
        });

        const contentType = response.headers.get("content-type") || "";
        const data = contentType.includes("application/json")
            ? await response.json()
            : await response.text();

        if (!response.ok) {
            const message =
                data?.message ||
                data?.title ||
                data ||
                "Có lỗi xảy ra khi gọi API.";
            throw new Error(message);
        }

        return data;
    }

    function renderNavbar() {
        const auth = getAuth();

        document.querySelectorAll(".guest-only").forEach(x => {
            x.style.display = auth?.token ? "none" : "";
        });

        document.querySelectorAll(".auth-only").forEach(x => {
            x.style.display = auth?.token ? "" : "none";
        });

        document.querySelectorAll(".admin-only").forEach(x => {
            x.style.display = auth?.role === "Admin" ? "" : "none";
        });

        const navUserInfo = document.getElementById("navUserInfo");
        if (navUserInfo) {
            navUserInfo.textContent = auth?.token
                ? `${auth.fullName || auth.email || "Đã đăng nhập"} (${auth.role || "User"})`
                : "";
        }

        const btnLogout = document.getElementById("btnLogout");
        if (btnLogout) {
            btnLogout.onclick = async () => {
                try {
                    if (auth?.token) {
                        await apiFetch("/api/client/auth/logout", { method: "POST" });
                    }
                } catch {
                    // bỏ qua lỗi logout phía server
                } finally {
                    clearAuth();
                    window.location.href = "/login";
                }
            };
        }
    }

    function setHtml(id, html) {
        const el = document.getElementById(id);
        if (el) el.innerHTML = html;
    }

    function showMessage(id, message, type = "info") {
        const el = document.getElementById(id);
        if (!el) return;

        el.className = `alert alert-${type}`;
        el.textContent = message;
        el.style.display = "block";
    }

    function clearMessage(id) {
        const el = document.getElementById(id);
        if (!el) return;

        el.textContent = "";
        el.style.display = "none";
        el.className = "alert";
    }

    function formatDateTime(value) {
        if (!value) return "";
        return new Date(value).toLocaleString("vi-VN");
    }

    function formatDate(value) {
        if (!value) return "";
        return new Date(value).toLocaleDateString("vi-VN");
    }

    function calculateAge(birthDate) {
        if (!birthDate) return 0;
        const today = new Date();
        const dob = new Date(birthDate);
        let age = today.getFullYear() - dob.getFullYear();
        const m = today.getMonth() - dob.getMonth();
        if (m < 0 || (m === 0 && today.getDate() < dob.getDate())) age--;
        return age;
    }

    document.addEventListener("DOMContentLoaded", renderNavbar);

    return {
        getAuth,
        saveAuth,
        clearAuth,
        getToken,
        requireAuth,
        requireAdmin,
        apiFetch,
        renderNavbar,
        setHtml,
        showMessage,
        clearMessage,
        formatDate,
        formatDateTime,
        calculateAge
    };
})();