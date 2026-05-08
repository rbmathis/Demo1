/*!
 * Component Showcase functionality
 * Copy-to-clipboard and iframe theme sync
 */
(() => {
    'use strict';

    // Copy-to-clipboard handler for copy-markup buttons
    document.addEventListener('click', (e) => {
        const btn = e.target.closest('.copy-btn');
        if (!btn) return;

        const text = btn.getAttribute('data-clipboard-text');
        if (!text) return;

        const originalText = btn.textContent;
        navigator.clipboard.writeText(text).then(() => {
            btn.textContent = 'Copied!';
            btn.classList.add('btn-success');
            btn.classList.remove('btn-outline-secondary');
            setTimeout(() => {
                btn.textContent = originalText;
                btn.classList.remove('btn-success');
                btn.classList.add('btn-outline-secondary');
            }, 2000);
        }).catch(() => {
            btn.textContent = 'Failed';
            setTimeout(() => {
                btn.textContent = originalText;
            }, 2000);
        });
    });

    // Sync theme to iframes
    const syncIframeThemes = () => {
        const theme = document.documentElement.getAttribute('data-bs-theme') || 'light';
        document.querySelectorAll('.component-preview-frame').forEach(iframe => {
            try {
                if (iframe.contentDocument) {
                    iframe.contentDocument.documentElement.setAttribute('data-bs-theme', theme);
                }
            } catch (e) {
                // Cross-origin iframe, skip
            }
        });
    };

    // Observe theme changes on document element
    const observer = new MutationObserver(syncIframeThemes);
    observer.observe(document.documentElement, {
        attributes: true,
        attributeFilter: ['data-bs-theme']
    });

    // Sync on iframe load
    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('.component-preview-frame').forEach(iframe => {
            iframe.addEventListener('load', syncIframeThemes);
        });
    });
})();
