(function () {
    "use strict";

    // ---------- Toasts ----------
    const TOAST_DURATION_MS = 5000;

    function dismissToast(toast) {
        toast.style.transition = "opacity .2s ease";
        toast.style.opacity = "0";
        setTimeout(() => toast.remove(), 200);
    }

    function mountToast(toast) {
        const stack = document.getElementById("toastStack");
        if (!stack) return;

        stack.appendChild(toast);
        toast.hidden = false;
        toast.querySelector("[data-toast-close]")?.addEventListener("click", () => dismissToast(toast));
        setTimeout(() => dismissToast(toast), TOAST_DURATION_MS);
    }

    /** Shows a toast from script, e.g. EduTrack.toast("Kopyalandı"). */
    function showToast(message, type = "success") {
        const toast = document.createElement("div");
        const isError = type === "error";
        toast.className = `app-toast ${isError ? "app-toast-error" : "app-toast-success"}`;
        toast.setAttribute("role", "status");
        toast.innerHTML = `<i class="bi ${isError ? "bi-exclamation-circle" : "bi-check-circle"}"></i><span></span>` +
            `<button type="button" class="btn-close" aria-label="Kapat" data-toast-close></button>`;
        toast.querySelector("span").textContent = message;
        mountToast(toast);
    }

    // ---------- Confirm dialog ----------
    let pendingAction = null;

    function openConfirm(trigger, onAccept) {
        const modalElement = document.getElementById("confirmModal");
        if (!modalElement || !window.bootstrap) {
            if (window.confirm(trigger.dataset.confirm)) onAccept();
            return;
        }

        modalElement.querySelector("#confirmModalTitle").textContent = trigger.dataset.confirmTitle || "Emin misiniz?";
        modalElement.querySelector("[data-confirm-message]").textContent = trigger.dataset.confirm;

        const accept = modalElement.querySelector("[data-confirm-accept]");
        accept.textContent = trigger.dataset.confirmOk || "Onayla";
        accept.className = `btn ${trigger.dataset.confirmVariant === "primary" ? "btn-primary" : "btn-danger"}`;

        pendingAction = onAccept;
        bootstrap.Modal.getOrCreateInstance(modalElement).show();
    }

    document.addEventListener("click", (event) => {
        const accept = event.target.closest("[data-confirm-accept]");
        if (accept && pendingAction) {
            const action = pendingAction;
            pendingAction = null;
            bootstrap.Modal.getInstance(document.getElementById("confirmModal"))?.hide();
            action();
            return;
        }

        const link = event.target.closest("a[data-confirm]");
        if (link) {
            event.preventDefault();
            openConfirm(link, () => { window.location.href = link.href; });
        }
    });

    document.addEventListener("submit", (event) => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || !form.dataset.confirm || form.dataset.confirmed === "true") return;

        event.preventDefault();
        openConfirm(form, () => {
            form.dataset.confirmed = "true";
            form.requestSubmit ? form.requestSubmit() : form.submit();
        });
    });

    // ---------- Password visibility ----------
    document.addEventListener("click", (event) => {
        const toggle = event.target.closest("[data-password-toggle]");
        if (!toggle) return;

        const input = document.getElementById(toggle.dataset.passwordToggle);
        if (!input) return;

        const reveal = input.type === "password";
        input.type = reveal ? "text" : "password";
        toggle.setAttribute("aria-label", reveal ? "Şifreyi gizle" : "Şifreyi göster");
        toggle.querySelector("i").className = reveal ? "bi bi-eye-slash" : "bi bi-eye";
    });

    // ---------- Copy course code ----------
    document.addEventListener("click", async (event) => {
        const button = event.target.closest("[data-copy-code]");
        if (!button) return;

        try {
            await navigator.clipboard.writeText(button.dataset.copyCode);
            const icon = button.querySelector("i");
            icon.className = "bi bi-check2";
            setTimeout(() => { icon.className = "bi bi-copy"; }, 1200);
            showToast("Ders kodu kopyalandı.");
        } catch {
            showToast("Kod kopyalanamadı, elle seçip kopyalayın.", "error");
        }
    });

    // ---------- Boot ----------
    document.addEventListener("DOMContentLoaded", () => {
        document.querySelectorAll("[data-toast]").forEach(mountToast);
    });

    window.EduTrack = { toast: showToast };
})();
