// Site-wide behavior. Loaded as an ES module by _Layout.cshtml.
//
// Markup hooks:
//   data-auto-dismiss-ms="5000"   on an .alert   -> closes it after the delay
//   data-confirm-message="..."    on a link/button -> asks before continuing
//   data-download-with-loading    on a link       -> downloads with a spinner
//   #siteSidebar, #sidebarPinToggle               -> see _Sidebar.cshtml
//
// Other scripts can import the exported helpers, or use window.App.

const SIDEBAR_PINNED_KEY = 'app.sidebar.pinned.v1';
const SIDEBAR_SECTIONS_KEY = 'app.sidebar.section-state.v1';

/**
 * Closes a Bootstrap alert after the specified delay.
 * @param {HTMLElement | null} alertElement - The alert element to close.
 * @param {number} delayMilliseconds - Time to wait before closing the alert.
 */
export function dismissAlertAfterDelay(alertElement, delayMilliseconds = 5_000) {
    if (!alertElement || !Number.isFinite(delayMilliseconds) || delayMilliseconds < 0) {
        return;
    }

    window.setTimeout(() => {
        if (alertElement.isConnected) {
            bootstrap.Alert.getOrCreateInstance(alertElement).close();
        }
    }, delayMilliseconds);
}

/**
 * Shows a page-blocking loading spinner (jQuery blockUI).
 * @param {string} spinnerMessage - Text shown next to the spinner.
 */
export function beginSpinnerLoading(spinnerMessage = 'Loading') {
    if (typeof $.blockUI !== 'function') {
        return;
    }

    const message = document.createElement('div');
    message.className = 'd-flex align-items-center';

    const text = document.createElement('strong');
    text.setAttribute('role', 'status');
    text.textContent = `${spinnerMessage}...`;

    const spinner = document.createElement('div');
    spinner.className = 'spinner-border ms-3';
    spinner.setAttribute('aria-hidden', 'true');

    message.append(text, spinner);

    $.blockUI({
        message: message.outerHTML,
        css: {
            border: 'none',
            padding: '15px',
            backgroundColor: '#000',
            borderRadius: '10px',
            opacity: 0.7,
            color: '#fff',
        },
        baseZ: 2000,
    });
}

/**
 * Removes the loading spinner.
 * @param {string} selector - Blocked element; omit for the whole page.
 */
export function finishSpinnerLoading(selector) {
    if (selector && selector !== 'body') {
        $(selector).unblock();
    } else if (typeof $.unblockUI === 'function') {
        $.unblockUI();
    }
}

/**
 * Downloads a file while showing the loading spinner. Falls back to normal
 * navigation when the response is not a file (for example, a login redirect).
 * @param {string} url - The download endpoint URL.
 */
export async function downloadFileWithLoading(url) {
    beginSpinnerLoading('Preparing download, please wait');

    try {
        const response = await fetch(url, { credentials: 'same-origin' });
        const contentDisposition = response.headers.get('content-disposition') ?? '';

        if (!response.ok || !/attachment/i.test(contentDisposition)) {
            window.location.assign(url);
            return;
        }

        const file = await response.blob();
        const fileNameMatch = contentDisposition.match(/filename\*?=(?:UTF-8'')?"?([^";]+)"?/i);
        const fileName = fileNameMatch ? decodeURIComponent(fileNameMatch[1]) : 'download';
        const objectUrl = URL.createObjectURL(file);
        const downloadLink = document.createElement('a');

        downloadLink.href = objectUrl;
        downloadLink.download = fileName;
        document.body.appendChild(downloadLink);
        downloadLink.click();
        downloadLink.remove();
        URL.revokeObjectURL(objectUrl);
    } catch (error) {
        console.error('Download failed.', error);
        window.location.assign(url);
    } finally {
        finishSpinnerLoading();
    }
}

/**
 * Pins the sidebar into the desktop layout and remembers the choice.
 * Smaller viewports keep the standard Bootstrap offcanvas behavior.
 */
function initializeSidebarPinning() {
    const sidebar = document.getElementById('siteSidebar');
    const pinToggle = document.getElementById('sidebarPinToggle');
    const pinIcon = pinToggle?.querySelector('[data-sidebar-pin-icon]');
    const pinLabel = pinToggle?.querySelector('[data-sidebar-pin-label]');
    const desktopViewport = window.matchMedia('(min-width: 992px)');

    if (!sidebar || !pinToggle) {
        return;
    }

    let isPinned = false;

    try {
        isPinned = localStorage.getItem(SIDEBAR_PINNED_KEY) === 'true';
    } catch {
        // The pin control still works when browser storage is unavailable.
    }

    const applyPinnedState = () => {
        const isActive = isPinned && desktopViewport.matches;
        const label = isActive ? 'Unpin side menu' : 'Pin side menu';

        document.documentElement.classList.toggle('sidebar-pinned', isActive);
        pinToggle.setAttribute('aria-pressed', String(isActive));
        pinToggle.setAttribute('aria-label', label);
        pinIcon?.classList.toggle('bi-pin-angle', !isActive);
        pinIcon?.classList.toggle('bi-pin-angle-fill', isActive);

        if (pinLabel) {
            pinLabel.textContent = label;
        }
    };

    pinToggle.addEventListener('click', () => {
        isPinned = !isPinned;

        try {
            localStorage.setItem(SIDEBAR_PINNED_KEY, String(isPinned));
        } catch {
            // The preference cannot persist, but the current page still updates.
        }

        if (isPinned && sidebar.classList.contains('show')) {
            const offcanvas = bootstrap.Offcanvas.getInstance(sidebar);

            if (offcanvas) {
                sidebar.addEventListener('hidden.bs.offcanvas', applyPinnedState, { once: true });
                offcanvas.hide();
                return;
            }
        }

        applyPinnedState();
    });

    desktopViewport.addEventListener('change', applyPinnedState);
    applyPinnedState();
}

/**
 * Restores and remembers the expanded state of each collapsible sidebar
 * section. Sections are expanded until the user collapses them.
 */
function initializeSidebarSectionState() {
    const sidebar = document.getElementById('siteSidebar');

    if (!sidebar) {
        return;
    }

    let sectionState = {};

    try {
        sectionState = JSON.parse(localStorage.getItem(SIDEBAR_SECTIONS_KEY) ?? '{}');
    } catch {
        sectionState = {};
    }

    const saveSectionState = () => {
        try {
            localStorage.setItem(SIDEBAR_SECTIONS_KEY, JSON.stringify(sectionState));
        } catch {
            // The sidebar stays usable when browser storage is unavailable.
        }
    };

    sidebar.querySelectorAll('.collapse[id]').forEach((section) => {
        const isExpanded = sectionState[section.id] ?? true;
        const toggleButton = sidebar.querySelector(`[aria-controls="${section.id}"]`);

        section.classList.toggle('show', isExpanded);
        toggleButton?.setAttribute('aria-expanded', String(isExpanded));

        section.addEventListener('shown.bs.collapse', () => {
            sectionState[section.id] = true;
            saveSectionState();
        });

        section.addEventListener('hidden.bs.collapse', () => {
            sectionState[section.id] = false;
            saveSectionState();
        });
    });
}

/** Marks the sidebar link for the current page as active. */
function highlightActiveSidebarLink() {
    const currentPath = window.location.pathname.replace(/\/+$/, '').toLowerCase();

    document.querySelectorAll('#siteSidebar .btn-toggle-nav a[href]').forEach((link) => {
        const linkPath = new URL(link.href, window.location.origin)
            .pathname.replace(/\/+$/, '')
            .toLowerCase();

        if (linkPath === currentPath) {
            link.classList.add('active');
            link.setAttribute('aria-current', 'page');
        }
    });
}

document.addEventListener('DOMContentLoaded', () => {
    initializeSidebarPinning();
    initializeSidebarSectionState();
    highlightActiveSidebarLink();

    document.querySelectorAll('[data-auto-dismiss-ms]').forEach((alertElement) => {
        dismissAlertAfterDelay(alertElement, Number(alertElement.dataset.autoDismissMs));
    });

    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach((element) => {
        bootstrap.Tooltip.getOrCreateInstance(element);
    });

    document.addEventListener('click', (event) => {
        const confirmationTrigger = event.target.closest('[data-confirm-message]');

        if (confirmationTrigger && !window.confirm(confirmationTrigger.dataset.confirmMessage)) {
            event.preventDefault();
        }
    });

    document.addEventListener('click', async (event) => {
        const link = event.target.closest('a[data-download-with-loading]');

        if (!link || event.defaultPrevented || event.ctrlKey || event.metaKey
            || event.shiftKey || event.altKey) {
            return;
        }

        event.preventDefault();
        await downloadFileWithLoading(link.href);
    });

    // Submit buttons outside their form (form="id"): show the spinner once
    // jQuery validation accepts the form.
    $('button[type=submit][form]').on('click', function () {
        const form = $(`#${$(this).attr('form')}`);

        if (form.length && typeof form.valid === 'function' && form.valid()) {
            beginSpinnerLoading();
        }
    });

    // Nested dropdown menus (.dropdown-submenu): Bootstrap 5 has no submenus.
    $('.dropdown-menu a.dropdown-toggle').on('click', function () {
        if (!$(this).next().hasClass('show')) {
            $(this).parents('.dropdown-menu').first().find('.show').removeClass('show');
        }

        $(this).next('.dropdown-menu').toggleClass('show');

        $(this).parents('li.nav-item.dropdown.show').on('hidden.bs.dropdown', () => {
            $('.dropdown-submenu .show').removeClass('show');
        });

        return false;
    });
});

// For inline scripts that are not modules.
window.App = {
    dismissAlertAfterDelay,
    beginSpinnerLoading,
    finishSpinnerLoading,
    downloadFileWithLoading,
};
