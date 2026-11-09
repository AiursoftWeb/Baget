(function () {
    'use strict';

    var baget = {};
    window.baget = baget;
    var operations = new WeakMap();

    function resetFeedback(root) {
        operations.delete(root);
        root.querySelector('.copy-button button').disabled = false;
        root.querySelector('.copy-status').textContent = '';
    }

    // Legacy fallback source: http://stackoverflow.com/questions/400212/how-do-i-copy-to-the-clipboard-in-javascript
    function copyWithLegacyApi(text) {
        if (window.clipboardData) {
            if (!window.clipboardData.setData('Text', text)) throw new Error('Copy failed');
            return;
        }
        var textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.position = 'fixed';
        textArea.style.opacity = '0';
        textArea.style.top = '0';
        textArea.style.left = '0';
        document.body.appendChild(textArea);
        try {
            textArea.select();
            if (!document.execCommand('copy')) throw new Error('Copy failed');
        } finally {
            document.body.removeChild(textArea);
        }
    }

    baget.copyTextToClipboard = async function (text, button) {
        if (button.disabled) return;
        var root = button.closest('.tabbed-info');
        var status = root.querySelector('.copy-status');
        var operation = {};
        operations.set(root, operation);
        button.disabled = true;
        status.textContent = 'Copying...';
        try {
            if (navigator.clipboard && navigator.clipboard.writeText) {
                await navigator.clipboard.writeText(text);
            } else {
                copyWithLegacyApi(text);
            }
            if (operations.get(root) !== operation) return;
            status.textContent = 'Copied';
            button.disabled = false;
            button.focus();
        } catch {
            if (operations.get(root) !== operation) return;
            status.textContent = 'Copy failed. Select the command to copy it.';
            var command = root.querySelector('.command-text');
            command.focus();
            var range = document.createRange();
            range.selectNodeContents(command);
            var selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(range);
        } finally {
            if (operations.get(root) === operation) button.disabled = false;
        }
    };

    document.addEventListener('click', function (event) {
        var tab = event.target.closest('.tabbed-info .nav a');
        if (tab) resetFeedback(tab.closest('.tabbed-info'));
    });
})();

import('../node_modules/@aiursoft/utctime.js/dist/esm/utctime.js').then(({ default: UtcTime }) => {
    new UtcTime({ disableAgo: true, disableAutoUpdate: true });
});
