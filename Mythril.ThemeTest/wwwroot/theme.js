// theme.js - Standalone test
window.setTheme = function(theme) {
    console.log("TEST: setTheme(" + theme + ")");
    document.body.className = theme;
    localStorage.setItem('theme-test', theme);
};
