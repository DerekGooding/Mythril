window.expander = {
    expand: (el) => {
        el.style.display = "block";
        const targetHeight = el.scrollHeight + "px";
        el.style.maxHeight = targetHeight;
    },
    collapse: (el) => {
        el.style.maxHeight = "0px";
    }
};
