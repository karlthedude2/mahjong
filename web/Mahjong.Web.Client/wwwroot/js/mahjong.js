// Browser helpers used by BrowserInterop.cs.

const sounds = new Map();

export function playSound(url) {
    let audio = sounds.get(url);
    if (!audio) {
        audio = new Audio(url);
        sounds.set(url, audio);
    }
    // Clone so quick clicks can overlap instead of cutting each other off.
    const instance = audio.cloneNode();
    instance.play().catch(() => { /* autoplay blocked until the first user gesture */ });
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
