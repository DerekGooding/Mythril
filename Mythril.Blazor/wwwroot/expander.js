window.expander = {
    expand: (el) => {
        el.style.display = "block";
        const targetHeight = el.scrollHeight + "px";
        el.style.maxHeight = targetHeight;
        
        // Transition to 'none' to allow dynamic content updates
        const onTransitionEnd = () => {
            if (el.style.maxHeight !== "0px") {
                el.style.maxHeight = "none";
            }
            el.removeEventListener("transitionend", onTransitionEnd);
        };
        el.addEventListener("transitionend", onTransitionEnd);
    },
    collapse: (el) => {
        // First set to actual height to allow transition to 0
        el.style.maxHeight = el.scrollHeight + "px";
        // Force reflow
        el.offsetHeight; 
        el.style.maxHeight = "0px";
    }
};
