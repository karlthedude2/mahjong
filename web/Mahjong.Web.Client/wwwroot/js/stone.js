// The look shared by the 3D views (the Mahjong Cube and the 3D board): stone tiles in a soft studio
// light, with face art printed on them. Each view builds its own tiles and calls these.

import * as THREE from "../lib/three/three.module.min.js";

const Colors = { red: "#c0141b", green: "#0d7a2e", blue: "#134ea8", wind: "#1b1b1b", flower: "#b0217a", season: "#b35a00" };

// How a tile is tinted: as it is when free, dimmed when blocked, gold when selected, blue for a hint.
export const Tints = {
    free: new THREE.Color(0xffffff),
    blocked: new THREE.Color(0x8c8577),
    selected: new THREE.Color(0xf2b632),
    hinted: new THREE.Color(0x7fd3ff),
};

// What the tiles are made of. A stone with a texture is a flat, evenly lit, square picture of the
// material; each face of each tile shows its own slice of it, so no two faces look alike.
//
// Each stone uses one of two face sets: "light" (art made for pale stone) or "dark" (art made for
// dark stone; drawn symbols are pale with a dark outline). Which one is configuration, passed in by
// the page (Cube:StoneFaces in the server's settings); a stone that isn't configured is judged by
// how dark its picture is.
const Stones = {
    marble: { color: "#ffffff", texture: "textures/marble.jpg", roughness: 0.3 },
    darkMarble: { color: "#5a4a40", texture: "textures/dark-marble.jpg", roughness: 0.32 },
    ivory: { color: "#ecdfc2", roughness: 0.38 },
    onyx: { color: "#141416", generate: onyxPicture, roughness: 0.24 },
};

// The light the stone reflects: a soft studio rather than a room with bright lamps, so a polished
// face never mirrors a hot spot. A dome that is brightest overhead and dims toward the floor, with
// one broad, gentle panel above; looking up at a face shows only the dim lower dome.
function softStudio() {
    const scene = new THREE.Scene();
    const dome = new THREE.SphereGeometry(10, 48, 24);
    const position = dome.attributes.position;
    const colours = [];
    for (let i = 0; i < position.count; i++) {
        const up = position.getY(i) / 10; // 1 overhead, -1 underfoot
        const level = up >= 0 ? 0.62 + 0.3 * up : 0.62 + 0.32 * up;
        colours.push(level, level, level * 1.02);
    }
    dome.setAttribute("color", new THREE.Float32BufferAttribute(colours, 3));
    scene.add(new THREE.Mesh(dome, new THREE.MeshBasicMaterial({ side: THREE.BackSide, vertexColors: true })));

    const panel = new THREE.Mesh(new THREE.PlaneGeometry(9, 6), new THREE.MeshBasicMaterial({ color: new THREE.Color(1.5, 1.5, 1.5) }));
    panel.position.set(-1, 8, 2);
    panel.lookAt(0, 0, 0);
    scene.add(panel);
    return scene;
}

// Black onyx, drawn rather than photographed: near-black stone with soft, wavy charcoal bands and
// the odd faint pale vein, the way onyx is layered. Square and even, like a stone picture.
function onyxPicture() {
    const size = 512;
    const canvas = document.createElement("canvas");
    canvas.width = canvas.height = size;
    const g = canvas.getContext("2d");
    const image = g.createImageData(size, size);
    const px = image.data;
    for (let y = 0; y < size; y++) {
        for (let x = 0; x < size; x++) {
            const u = x / size, v = y / size;
            // Bands running across the stone at a slant, bent by a few slow waves.
            const warp = 0.06 * Math.sin(u * 7.1 + v * 2.3) + 0.035 * Math.sin(u * 15.7 - v * 5.9 + 1.3) + 0.015 * Math.sin(u * 37 + v * 23);
            const t = (v * 0.8 + u * 0.35 + warp) * 9;
            const band = Math.pow(0.5 + 0.5 * Math.sin(t * Math.PI * 2), 3);
            const fine = 0.5 + 0.5 * Math.sin(t * Math.PI * 11 + 2 * Math.sin(u * 9));
            const vein = Math.pow(Math.max(0, Math.sin(t * Math.PI * 2 + 0.6)), 60);
            const level = 14 + 22 * band + 6 * fine * band + 70 * vein;
            const i = (y * size + x) * 4;
            px[i] = level;
            px[i + 1] = level;
            px[i + 2] = level * 1.06;
            px[i + 3] = 255;
        }
    }
    g.putImageData(image, 0, 0);
    return canvas;
}

// Dark if the picture's average brightness is under 45%, measured on a small copy of it.
function isDark(image) {
    const canvas = document.createElement("canvas");
    canvas.width = canvas.height = 32;
    const g = canvas.getContext("2d", { willReadFrequently: true });
    g.drawImage(image, 0, 0, 32, 32);
    const px = g.getImageData(0, 0, 32, 32).data;
    let sum = 0;
    for (let i = 0; i < px.length; i += 4) {
        sum += 0.2126 * px[i] + 0.7152 * px[i + 1] + 0.0722 * px[i + 2];
    }
    return sum / (px.length / 4) / 255 < 0.45;
}

// Face art: a picture for a face replaces its drawn symbol. tile.art holds URLs for the face sets,
// { light, dark, any }; "any" serves either set when there's no picture for it. A square picture
// with a transparent background suits best, so the stone shows through; other shapes are fitted
// without stretching. The picture runs over the rounded edges, so keep the symbol about 15% in
// from each side. aspect is the face's height over its width (1 for a cube, more for a tall tile).

// A tile's face for a face set, made once per face name, set and picture: drawn as text at once,
// then replaced by the face's picture when it arrives.
function faceTexture(tile, cache, changed, set, aspect) {
    const url = tile.art?.[set] ?? tile.art?.any;
    const key = [tile.face, set, url ?? ""].join("|");
    let texture = cache.get(key);
    if (texture) {
        return texture;
    }

    const width = 512, height = Math.round(512 * aspect);
    const canvas = document.createElement("canvas");
    canvas.width = width;
    canvas.height = height;
    const g = canvas.getContext("2d");

    const glyph = tile.glyph;
    const font = '"Segoe UI", "Microsoft YaHei", "PingFang SC", "Noto Sans CJK SC", Arial, sans-serif';
    g.textAlign = "center";
    g.textBaseline = "middle";
    g.lineJoin = "round";
    // Pale with a dark outline on a dark stone, otherwise in the suit's colour.
    const write = (text, y, weight, px, outline) => {
        g.font = `${weight} ${px}px ${font}`;
        if (set === "dark") {
            g.strokeStyle = "rgba(20, 12, 6, 0.85)";
            g.lineWidth = outline;
            g.strokeText(text, width / 2, y, width * 0.7);
            g.fillStyle = "#f7f0e0";
        } else {
            g.fillStyle = Colors[glyph.color] || Colors.wind;
        }
        g.fillText(text, width / 2, y, width * 0.7);
    };
    write(glyph.main, height / 2 - width * 0.07, 800, 220, 22);
    write(glyph.caption.toUpperCase(), height / 2 + width * 0.26, 700, 64, 12);

    texture = new THREE.CanvasTexture(canvas);
    texture.colorSpace = THREE.SRGBColorSpace;
    texture.anisotropy = 4;
    cache.set(key, texture);

    if (url) {
        const art = new Image();
        art.onload = () => {
            g.clearRect(0, 0, width, height);
            const scale = Math.min(width / art.width, height / art.height);
            const w = art.width * scale, h = art.height * scale;
            g.drawImage(art, (width - w) / 2, (height - h) / 2, w, h);
            texture.needsUpdate = true;
            changed();
        };
        art.src = url;
    }
    return texture;
}

// A tile's material: the stone (a colour, or a slice of its texture), the face art on it, and the
// tile's tint (dimmed when blocked, gold when selected) over both. One shader for every tile. With
// artOnTop, the art is printed only on the top face (a board tile); otherwise on every face (a cube).
function stoneMaterial(art, stone, blank, artOnTop) {
    const material = new THREE.MeshStandardMaterial({ roughness: stone.roughness, metalness: 0 });
    const uniforms = {
        artMap: { value: art },
        artOnTop: { value: artOnTop ? 1 : 0 },
        stoneMap: { value: stone.map ?? blank },
        useStoneMap: { value: stone.map ? 1 : 0 },
        stoneColor: { value: new THREE.Color(stone.color) },
        // Where this tile's slices are cut from the stone.
        seed: { value: new THREE.Vector2(Math.random(), Math.random()) },
    };
    material.userData.uniforms = uniforms;
    material.customProgramCacheKey = () => "tile-stone";
    material.onBeforeCompile = shader => {
        Object.assign(shader.uniforms, uniforms);
        shader.vertexShader = shader.vertexShader
            .replace("#include <common>", "#include <common>\nvarying vec2 vFaceUv;\nvarying vec3 vFaceNormal;")
            .replace("#include <uv_vertex>", "#include <uv_vertex>\nvFaceUv = uv;\nvFaceNormal = normal;");
        shader.fragmentShader = shader.fragmentShader
            .replace("#include <common>", `#include <common>
uniform sampler2D artMap;
uniform float artOnTop;
uniform sampler2D stoneMap;
uniform float useStoneMap;
uniform vec3 stoneColor;
uniform vec2 seed;
varying vec2 vFaceUv;
varying vec3 vFaceNormal;`)
            .replace("#include <map_fragment>", `
vec3 a = abs(vFaceNormal);
float face = a.x >= a.y && a.x >= a.z ? (vFaceNormal.x > 0.0 ? 0.0 : 1.0)
    : a.y >= a.z ? (vFaceNormal.y > 0.0 ? 2.0 : 3.0)
    : (vFaceNormal.z > 0.0 ? 4.0 : 5.0);
// A slice 45% of the picture across, turned by a quarter turn or more, kept inside the picture so
// no seam shows.
vec2 p = vFaceUv - 0.5;
float turn = mod(face + floor(seed.x * 4.0), 4.0);
p = turn < 0.5 ? p : turn < 1.5 ? vec2(-p.y, p.x) : turn < 2.5 ? -p : vec2(p.y, -p.x);
vec2 centre = 0.225 + 0.55 * fract(seed + face * vec2(0.37, 0.61));
vec3 stone = useStoneMap > 0.5 ? texture2D(stoneMap, centre + p * 0.45).rgb : stoneColor;
vec4 art = texture2D(artMap, vFaceUv);
float printed = artOnTop > 0.5 && face != 2.0 ? 0.0 : art.a;
diffuseColor.rgb *= mix(stone, art.rgb, printed);`);
    };
    return material;
}

// The renderer, scene, soft studio light and camera for a 3D view in host.
export function createStage(host, fieldOfView) {
    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    // Sharp enough on high-density screens without drawing four times the pixels.
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 1.5));
    renderer.toneMapping = THREE.NeutralToneMapping;
    host.appendChild(renderer.domElement);

    const scene = new THREE.Scene();
    // Soft light from every side gives the stone its sheen on the rounded edges (see softStudio).
    const environment = new THREE.PMREMGenerator(renderer);
    const studio = softStudio();
    scene.environment = environment.fromScene(studio, 0.04).texture;
    studio.traverse(o => { o.geometry?.dispose(); o.material?.dispose(); });
    scene.environmentIntensity = 1;
    const camera = new THREE.PerspectiveCamera(fieldOfView, 1, 0.1, 200);
    scene.add(camera);
    // A light that rides with the camera, so the side facing the player is always the brightest. It
    // comes from above and to the left, so it doesn't bounce straight back into the player's eyes.
    const light = new THREE.DirectionalLight(0xffffff, 0.9);
    light.position.set(-4, 5, 2);
    camera.add(light);

    return {
        renderer, scene, camera, light,
        dispose() {
            scene.environment.dispose();
            environment.dispose();
            renderer.dispose();
            renderer.domElement.remove();
        },
    };
}

// The stone the tiles are made of, and their face art: makes each tile's material, and changes them
// all when the stone changes. meshes() lists the tiles, each with userData.tile (see faceTexture).
// onFaceSet, if given, hears which face set ("light" or "dark") the stone uses whenever it's applied.
export function createStoneLook({ meshes, changed, aspect = 1, artOnTop = false, onFaceSet }) {
    const blank = new THREE.DataTexture(new Uint8Array([255, 255, 255, 255]), 1, 1);
    blank.needsUpdate = true;
    const textures = new Map();
    const loader = new THREE.TextureLoader();
    let stone = Stones.marble;

    // The face set the stone uses: as configured, else as its picture looks.
    const faceSet = () => stone.faces ?? stone.looks ?? "light";
    const art = tile => faceTexture(tile, textures, changed, faceSet(), aspect);

    function apply(chosen) {
        if (chosen !== stone) {
            return;
        }
        const ready = stone.map?.image != null;
        for (const mesh of meshes()) {
            const u = mesh.material.userData.uniforms;
            u.stoneMap.value = ready ? stone.map : blank;
            u.useStoneMap.value = ready ? 1 : 0;
            u.stoneColor.value.set(stone.color);
            u.artMap.value = art(mesh.userData.tile);
            mesh.material.roughness = stone.roughness;
        }
        onFaceSet?.(faceSet());
        changed();
    }

    return {
        // A stone by name, or a picture to try (url) under that name, with its configured face set
        // ("light" or "dark") if it has one. Without one, its faces are light until the picture
        // arrives and shows whether it's dark. The tiles are drawn in the stone's plain colour until
        // its picture arrives.
        use(name, url, faces) {
            if (url && !Stones[name]) {
                Stones[name] = { color: "#9a9a9a", texture: url, roughness: 0.3 };
            }
            stone = Stones[name] ?? Stones.ivory;
            stone.faces = faces === "light" || faces === "dark" ? faces : undefined;
            if (stone.generate && !stone.map) {
                const picture = stone.generate();
                stone.map = new THREE.CanvasTexture(picture);
                stone.map.colorSpace = THREE.SRGBColorSpace;
                stone.map.anisotropy = 4;
                stone.looks = isDark(picture) ? "dark" : "light";
            }
            if (stone.texture && !stone.map) {
                const chosen = stone;
                chosen.map = loader.load(chosen.texture, texture => {
                    chosen.looks = isDark(texture.image) ? "dark" : "light";
                    apply(chosen);
                });
                chosen.map.colorSpace = THREE.SRGBColorSpace;
                chosen.map.anisotropy = 4;
            }
            apply(stone);
        },

        // A new tile's material, in the current stone.
        material(tile) {
            return stoneMaterial(art(tile), stone, blank, artOnTop);
        },

        // Puts every tile in the current stone again (after tiles were added).
        refresh() {
            apply(stone);
        },

        dispose() {
            textures.forEach(t => t.dispose());
            Object.values(Stones).forEach(s => { s.map?.dispose(); });
            blank.dispose();
        },
    };
}
