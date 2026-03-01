window.expander = {
    expand: (el) => {
        if (!el) return;
        el.style.display = "block";
        el.style.maxHeight = "0px";
        el.style.opacity = "0";
        
        // Force reflow
        el.offsetHeight; 
        
        const targetHeight = el.scrollHeight + "px";
        el.style.maxHeight = targetHeight;
        el.style.opacity = "1";
        
        const onTransitionEnd = () => {
            if (el.style.maxHeight !== "0px") {
                el.style.maxHeight = "none";
            }
            el.removeEventListener("transitionend", onTransitionEnd);
        };
        el.addEventListener("transitionend", onTransitionEnd);
    },
    collapse: (el) => {
        if (!el) return;
        // First set to actual height to allow transition to 0
        el.style.maxHeight = el.scrollHeight + "px";
        el.style.opacity = "1";
        
        // Force reflow
        el.offsetHeight; 
        
        el.style.maxHeight = "0px";
        el.style.opacity = "0";
        
        const onTransitionEnd = () => {
            if (el.style.maxHeight === "0px") {
                el.style.display = "none";
            }
            el.removeEventListener("transitionend", onTransitionEnd);
        };
        el.addEventListener("transitionend", onTransitionEnd);
    }
};
