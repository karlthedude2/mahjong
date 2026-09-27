// Browser helpers used by BrowserInterop.cs.

// Sounds are decoded once with the Web Audio API and then played from memory, which starts
// them almost instantly (an <audio> element has to reload the file each time it's copied).
let audioContext = null;
const sounds = new Map();

function context() {
    audioContext ??= new (window.AudioContext || window.webkitAudioContext)();
    return audioContext;
}

function load(url) {
    let sound = sounds.get(url);
    if (!sound) {
        sound = fetch(url)
            .then(response => response.arrayBuffer())
            .then(data => context().decodeAudioData(data))
            .catch(() => null);
        sounds.set(url, sound);
    }
    return sound;
}

export function preloadSound(url) {
    load(url);
}

export async function playSound(url) {
    // Muted with the speaker button in the header.
    if (getSetting("sound") === "off") {
        return;
    }

    const audio = context();
    if (audio.state === "suspended") {
        // Browsers start audio suspended until the page has been clicked.
        audio.resume();
    }

    const buffer = await load(url);
    if (buffer) {
        const source = audio.createBufferSource();
        source.buffer = buffer;
        source.connect(audio.destination);
        source.start();
    }
}

export function getSetting(key) {
    try {
        return localStorage.getItem("mahjong." + key);
    } catch {
        return null;
    }
}

export function setSetting(key, value) {
    try {
        localStorage.setItem("mahjong." + key, value);
    } catch {
        // Private windows or blocked storage: settings just won't persist.
    }
}

export function pushAd(slot) {
    if (!slot || slot.dataset.adPushed) {
        return;
    }
    slot.dataset.adPushed = "true";
    try {
        (window.adsbygoogle = window.adsbygoogle || []).push({});
    } catch {
        // Ad blockers or AdSense not loaded yet.
    }
}

// Keeps an element (the score bar) as wide as the board is drawn. The board is an SVG scaled to
// fit its area without stretching, so its width depends on the area's shape and the layout's; this
// follows window resizes and layout changes (a new viewBox).
export function matchBoardWidth(area, target) {
    const update = () => {
        const svg = area.querySelector("svg.board");
        const box = svg && svg.viewBox && svg.viewBox.baseVal;
        if (!box || !box.width || !area.clientHeight) {
            return; // paused (no board shown) or not laid out yet: keep the last width
        }

        // The drawn scale, and the layout's width without the room left around it.
        const margin = Number(svg.dataset.margin) || 0;
        const scale = Math.min(area.clientWidth / box.width, area.clientHeight / box.height);
        target.style.maxWidth = Math.round((box.width - 2 * margin) * scale) + "px";
    };

    const resized = new ResizeObserver(update);
    resized.observe(area);
    const changed = new MutationObserver(update);
    changed.observe(area, { subtree: true, childList: true, attributes: true, attributeFilter: ["viewBox"] });
    update();
    return { dispose: () => { resized.disconnect(); changed.disconnect(); } };
}

// The header's New game, Leaderboards and Settings are plain links. While the game page is
// showing, App.razor's script hands their clicks to it instead, so nothing reloads.
export function setPlayActions(dotnet) {
    window.mahjongPlayAction = dotnet ? action => dotnet.invokeMethodAsync("OnHeaderAction", action) : undefined;
}

// Background images are applied through a CSS variable; App.razor applies the saved one on load.
export function setBackground(url) {
    document.documentElement.style.setProperty("--page-background", `url("${url}")`);
    setSetting("backgroundUrl", url);
}
