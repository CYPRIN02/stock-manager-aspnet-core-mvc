window.onscroll = function () {
    const btn = document.getElementById("backToTopBtn");

    if (!btn) return;

    btn.style.display = document.documentElement.scrollTop > 250 ? "block" : "none";
};

function scrollToTop() {
    window.scrollTo({
        top: 0,
        behavior: "smooth"
    });
}