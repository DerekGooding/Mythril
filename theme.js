// theme.js - Robust theme switching for Blazor
console.log("JS: theme.js loading...");

window.setTheme = function (theme) {
    console.log("JS: window.setTheme called with: " + theme);
    var link = document.getElementById('theme');
    if (link) {
        link.setAttribute('href', 'css/' + theme + '.css');
        console.log("JS: href updated to css/" + theme + ".css");
    } else {
        console.error("JS: Theme link element (#theme) NOT FOUND in DOM!");
    }
    localStorage.setItem('theme', theme);
};

// Console log interceptor for error reporting
(function() {
    var originalLog = console.log;
    var originalWarn = console.warn;
    var originalError = console.error;
    var logs = [];
    var maxLogs = 100;

    function addLog(type, args) {
        var message = Array.from(args).map(arg => {
            try {
                if (arg === null) return "null";
                if (arg === undefined) return "undefined";
                return typeof arg === 'object' ? JSON.stringify(arg) : String(arg);
            } catch(e) { return "[Unserializable]"; }
        }).join(' ');
        logs.push(`[${new Date().toISOString()}] [${type}] ${message}`);
        if (logs.length > maxLogs) logs.shift();
    }

    console.log = function() { addLog('LOG', arguments); originalLog.apply(console, arguments); };
    console.warn = function() { addLog('WARN', arguments); originalWarn.apply(console, arguments); };
    console.error = function() { addLog('ERROR', arguments); originalError.apply(console, arguments); };

    window.getRecentLogs = function() { return logs.join('
'); };
})();

// Confetti utility
window.triggerConfettiAt = function(element) {
    if (typeof confetti === 'function') {
        var rect = element.getBoundingClientRect();
        confetti({
            particleCount: 40,
            spread: 50,
            origin: {
                x: (rect.left + rect.right) / 2 / window.innerWidth,
                y: (rect.top + rect.bottom) / 2 / window.innerHeight
            }
        });
    } else {
        console.warn("JS: Confetti library not loaded.");
    }
};

// Initial load check
document.addEventListener('DOMContentLoaded', function() {
    console.log("JS: DOMContentLoaded, performing initial theme sync.");
    var savedTheme = localStorage.getItem('theme') || 'light-theme';
    window.setTheme(savedTheme);
});

console.log("JS: theme.js initialized.");
