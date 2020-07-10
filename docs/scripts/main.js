// Events
let lastScrollYPosition = 0;
const getScrollYPosition = () => window.scrollY;

const scrollDetection = (homeCallback, downCallback, upCallback) => {
    let currentYPosition = getScrollYPosition();
    
    if (currentYPosition === 0) {
        homeCallback();
    } else if (currentYPosition > lastScrollYPosition) {
        downCallback();
    } else {
        upCallback();
    }

    lastScrollYPosition = currentYPosition;
    return;
}

window.addEventListener("scroll", () => scrollDetection(navbar_opaque, navbar_collapse, navbar_show))

// Navbar
function navbar_collapse() {
    const navbar = document.getElementById("navbarBox").classList;

    navbar.remove("show");
    navbar.remove("opaque");
    navbar.add("collapse");
    
}

function navbar_opaque() {
    const navbar = document.getElementById("navbarBox").classList;

    navbar.remove("collapse");
    navbar.remove("show");
    navbar.add("opaque");
}

function navbar_show() {
    const navbar = document.getElementById("navbarBox").classList;

    navbar.remove("collapse");
    navbar.remove("opaque");
    navbar.add("show");
}

function GetParams() {
    const queryString = window.location.search;
    const UrlParams = new URLSearchParams(queryString);

    return UrlParams.get('p');
}

function RenderMarkdown(markdown) {
    if (markdown == null || markdown == undefined || !markdown.endsWith(".md")) {
        markdown = "README.md";
    }
    fetch(`./${markdown}`).then((res) => {
        if (res.ok) {
            res.blob().then(
                (blob) => {
                    blob.text().then(
                        content => document.getElementById("MarkdownBox").innerHTML = marked(content)
                    );
                }
            )
        } else {
            document.getElementById("MarkdownBox").innerHTML = marked("## Page not found :(");
        }
    });
}