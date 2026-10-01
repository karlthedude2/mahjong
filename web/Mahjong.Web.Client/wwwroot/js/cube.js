// Prototype: draws the Mahjong Cube game with Three.js. The game itself (which tiles exist, which
// are free, what matches) lives in CubeGame.cs; this only draws it, turns it, and reports taps.

import * as THREE from "../lib/three/three.module.min.js";
import { OrbitControls } from "../lib/three/OrbitControls.js";
import { RoundedBoxGeometry } from "../lib/three/RoundedBoxGeometry.js";
import { RoomEnvironment } from "../lib/three/RoomEnvironment.js";

const Spacing = 1.06;
const Colors = { red: "#c0141b", green: "#0d7a2e", blue: "#134ea8", wind: "#1b1b1b", flower: "#b0217a", season: "#b35a00" };
const Free = new THREE.Color(0xffffff);
const Blocked = new THREE.Color(0x8c8577);
const Selected = new THREE.Color(0xf2b632);
const Hinted = new THREE.Color(0x7fd3ff);

// What the cubes are made of. A stone with a texture is a flat, evenly lit, square picture of the
// material; each face of each cube shows its own slice of it, so no two faces look alike. On a dark
// stone ("ink: light", worked out from the picture), the drawn symbols are pale with a dark outline
// so they stand out.
const Stones = {
    marble: { color: "#ffffff", texture: "textures/marble.jpg", roughness: 0.16 },
    darkMarble: { color: "#5a4a40", texture: "textures/dark-marble.jpg", roughness: 0.18 },
    ivory: { color: "#ecdfc2", roughness: 0.28 },
};

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

// Square face art for the cube, one PNG per face name (2bams.png, Cdragon.png, ...), with a
// transparent background so the stone shows through. The picture runs over the rounded edges, so
// keep the symbol about 15% in from each side. Faces without a picture are drawn as text.
const ArtFolder = "tilesets/cube/";

// A tile's face art, made once per face name and ink: drawn as text at once, then replaced by the
// face's picture if there is one.
function faceTexture(tile, cache, changed, ink) {
    const key = tile.face + "|" + (ink ?? "colour");
    let texture = cache.get(key);
    if (texture) {
        return texture;
    }

    const size = 512;
    const canvas = document.createElement("canvas");
    canvas.width = canvas.height = size;
    const g = canvas.getContext("2d");

    const glyph = tile.glyph;
    const font = '"Segoe UI", "Microsoft YaHei", "PingFang SC", "Noto Sans CJK SC", Arial, sans-serif';
    g.textAlign = "center";
    g.textBaseline = "middle";
    g.lineJoin = "round";
    const write = (text, y, weight, px, outline) => {
        g.font = `${weight} ${px}px ${font}`;
        if (ink === "light") {
            g.strokeStyle = "rgba(20, 12, 6, 0.85)";
            g.lineWidth = outline;
            g.strokeText(text, size / 2, y, size * 0.7);
            g.fillStyle = "#f7f0e0";
        } else {
            g.fillStyle = Colors[glyph.color] || Colors.wind;
        }
        g.fillText(text, size / 2, y, size * 0.7);
    };
    write(glyph.main, size * 0.43, 800, 220, 22);
    write(glyph.caption.toUpperCase(), size * 0.76, 700, 64, 12);

    texture = new THREE.CanvasTexture(canvas);
    texture.colorSpace = THREE.SRGBColorSpace;
    texture.anisotropy = 4;
    cache.set(key, texture);

    const art = new Image();
    art.onload = () => {
        g.clearRect(0, 0, size, size);
        g.drawImage(art, 0, 0, size, size);
        texture.needsUpdate = true;
        changed();
    };
    art.src = ArtFolder + encodeURIComponent(tile.face) + ".png";
    return texture;
}

// The cube's material: the stone (a colour, or a slice of its texture), the face art on top, and
// the tile's tint (dimmed when blocked, gold when selected) over both. One shader for every cube.
function cubeMaterial(art, stone, blank) {
    const material = new THREE.MeshStandardMaterial({ roughness: stone.roughness, metalness: 0 });
    const uniforms = {
        artMap: { value: art },
        stoneMap: { value: stone.map ?? blank },
        useStoneMap: { value: stone.map ? 1 : 0 },
        stoneColor: { value: new THREE.Color(stone.color) },
        // Where this cube's slices are cut from the stone.
        seed: { value: new THREE.Vector2(Math.random(), Math.random()) },
    };
    material.userData.uniforms = uniforms;
    material.customProgramCacheKey = () => "cube-stone";
    material.onBeforeCompile = shader => {
        Object.assign(shader.uniforms, uniforms);
        shader.vertexShader = shader.vertexShader
            .replace("#include <common>", "#include <common>\nvarying vec2 vFaceUv;\nvarying vec3 vFaceNormal;")
            .replace("#include <uv_vertex>", "#include <uv_vertex>\nvFaceUv = uv;\nvFaceNormal = normal;");
        shader.fragmentShader = shader.fragmentShader
            .replace("#include <common>", `#include <common>
uniform sampler2D artMap;
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
diffuseColor.rgb *= mix(stone, art.rgb, art.a);`);
    };
    return material;
}

export function create(host, dotnet) {
    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    // Sharp enough on high-density screens without drawing four times the pixels.
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 1.5));
    renderer.toneMapping = THREE.NeutralToneMapping;
    host.appendChild(renderer.domElement);

    const scene = new THREE.Scene();
    // Soft light from every side, as in a room, gives the stone its sheen on the rounded edges.
    const environment = new THREE.PMREMGenerator(renderer);
    scene.environment = environment.fromScene(new RoomEnvironment(), 0.04).texture;
    scene.environmentIntensity = 0.75;
    const camera = new THREE.PerspectiveCamera(40, 1, 0.1, 100);
    scene.add(camera);
    // A light that rides with the camera, so the side facing the player is always the brightest.
    const light = new THREE.DirectionalLight(0xffffff, 1.2);
    light.position.set(2, 3, 4);
    camera.add(light);

    const controls = new OrbitControls(camera, renderer.domElement);
    // A short glide after a drag: long enough to feel smooth, short enough that a tap right
    // afterwards lands on the tile under the finger.
    controls.enableDamping = true;
    controls.dampingFactor = 0.2;
    controls.enablePan = false;

    // Slightly rounded edges: 3 steps round each one is smooth at this size, and keeps each cube to
    // under 600 triangles.
    const box = new RoundedBoxGeometry(1, 1, 1, 3, 0.06);
    const blank = new THREE.DataTexture(new Uint8Array([255, 255, 255, 255]), 1, 1);
    blank.needsUpdate = true;
    const textures = new Map();
    const meshes = new Map();
    const loader = new THREE.TextureLoader();
    let stone = Stones.marble;
    // The scene is drawn only when something changes: a turn, a tap, a texture arriving.
    let dirty = true;
    const changed = () => { dirty = true; };
    let selectedId = null;
    let hinted = new Set();
    let free = new Set();

    // Loads a stone's texture once; the cubes are drawn in its plain colour until it arrives.
    // A stone by name, or a picture to try (url) under that name. Its symbols are drawn in the
    // usual colours until the picture arrives and shows whether it's dark.
    function useStone(name, url) {
        if (url && !Stones[name]) {
            Stones[name] = { color: "#9a9a9a", texture: url, roughness: 0.18 };
        }
        stone = Stones[name] ?? Stones.ivory;
        if (stone.texture && !stone.map) {
            const chosen = stone;
            chosen.map = loader.load(chosen.texture, texture => {
                chosen.ink = isDark(texture.image) ? "light" : undefined;
                applyStone(chosen);
            });
            chosen.map.colorSpace = THREE.SRGBColorSpace;
            chosen.map.anisotropy = 4;
        }
        applyStone(stone);
    }

    function applyStone(chosen) {
        if (chosen !== stone) {
            return;
        }
        for (const mesh of meshes.values()) {
            const u = mesh.material.userData.uniforms;
            const ready = stone.map?.image != null;
            u.stoneMap.value = ready ? stone.map : blank;
            u.useStoneMap.value = ready ? 1 : 0;
            u.stoneColor.value.set(stone.color);
            u.artMap.value = faceTexture(mesh.userData.tile, textures, changed, stone.ink);
            mesh.material.roughness = stone.roughness;
        }
        changed();
    }

    function tint(mesh) {
        const id = mesh.userData.id;
        mesh.material.color.copy(id === selectedId ? Selected : hinted.has(id) ? Hinted : free.has(id) ? Free : Blocked);
    }

    // Cubes with a neighbour on all six sides can't be seen, so they aren't drawn.
    function hideBuried() {
        const at = new Set([...meshes.values()].map(m => m.userData.key));
        for (const mesh of meshes.values()) {
            const [x, y, z] = mesh.userData.at;
            mesh.visible = ![[1, 0, 0], [-1, 0, 0], [0, 1, 0], [0, -1, 0], [0, 0, 1], [0, 0, -1]]
                .every(([dx, dy, dz]) => at.has(`${x + dx},${y + dy},${z + dz}`));
        }
    }

    function clear() {
        for (const mesh of meshes.values()) {
            scene.remove(mesh);
            mesh.material.dispose();
        }
        meshes.clear();
    }

    function resize() {
        const width = host.clientWidth, height = host.clientHeight;
        if (width && height) {
            renderer.setSize(width, height, false);
            camera.aspect = width / height;
            camera.updateProjectionMatrix();
            changed();
        }
    }
    const resized = new ResizeObserver(resize);
    resized.observe(host);

    // A tap (not a drag that turns the cube) on a tile tells the game which one.
    const raycaster = new THREE.Raycaster();
    let down = null;
    renderer.domElement.addEventListener("pointerdown", e => { down = { x: e.clientX, y: e.clientY }; });
    renderer.domElement.addEventListener("pointerup", e => {
        if (!down || Math.hypot(e.clientX - down.x, e.clientY - down.y) > 6) {
            down = null;
            return;
        }
        down = null;
        const rect = renderer.domElement.getBoundingClientRect();
        const pointer = new THREE.Vector2(((e.clientX - rect.left) / rect.width) * 2 - 1, -((e.clientY - rect.top) / rect.height) * 2 + 1);
        raycaster.setFromCamera(pointer, camera);
        const hit = raycaster.intersectObjects([...meshes.values()].filter(m => m.visible), false)[0];
        if (hit) {
            dotnet.invokeMethodAsync("OnTileTapped", hit.object.userData.id);
        }
    });

    // Removed tiles shrink away instead of vanishing.
    const shrinking = [];
    renderer.setAnimationLoop(time => {
        for (let i = shrinking.length - 1; i >= 0; i--) {
            const item = shrinking[i];
            const t = Math.min(1, (time - (item.start ??= time)) / 220);
            item.mesh.scale.setScalar(1 - t);
            if (t >= 1) {
                dirty = true;
                scene.remove(item.mesh);
                item.mesh.material.dispose();
                shrinking.splice(i, 1);
            }
        }
        if (controls.update() || shrinking.length > 0 || dirty) {
            dirty = false;
            renderer.render(scene, camera);
        }
    });

    return {
        // A new deal: tiles = [{ id, x, y, z, face, glyph: { main, caption, color } }].
        setBoard(size, tiles, freeIds) {
            clear();
            selectedId = null;
            hinted = new Set();
            free = new Set(freeIds);
            const middle = (size - 1) / 2;
            for (const tile of tiles) {
                const mesh = new THREE.Mesh(box, cubeMaterial(faceTexture(tile, textures, changed, stone.ink), stone, blank));
                mesh.userData.tile = tile;
                mesh.position.set((tile.x - middle) * Spacing, (tile.y - middle) * Spacing, (tile.z - middle) * Spacing);
                mesh.userData.id = tile.id;
                mesh.userData.at = [tile.x, tile.y, tile.z];
                mesh.userData.key = `${tile.x},${tile.y},${tile.z}`;
                meshes.set(tile.id, mesh);
                scene.add(mesh);
                tint(mesh);
            }
            hideBuried();
            applyStone(stone);

            const distance = size * Spacing * 2.6;
            camera.position.set(distance * 0.6, distance * 0.5, distance * 0.62);
            controls.target.set(0, 0, 0);
            controls.minDistance = size * 1.2;
            controls.maxDistance = size * 5;
            resize();
            changed();
        },

        // "marble", "darkMarble", "ivory", or any name with the url of a picture to try.
        setStone(name, url) {
            useStone(name, url);
        },

        // After any change: the selected tile (or null), the free tiles, and a hint's tiles.
        update(selected, freeIds, hintIds) {
            selectedId = selected;
            free = new Set(freeIds);
            hinted = new Set(hintIds);
            meshes.forEach(tint);
            changed();
        },

        remove(ids) {
            for (const id of ids) {
                const mesh = meshes.get(id);
                if (mesh) {
                    meshes.delete(id);
                    shrinking.push({ mesh });
                }
            }
            hideBuried();
        },

        dispose() {
            renderer.setAnimationLoop(null);
            resized.disconnect();
            controls.dispose();
            clear();
            textures.forEach(t => t.dispose());
            Object.values(Stones).forEach(s => s.map?.dispose());
            blank.dispose();
            box.dispose();
            scene.environment.dispose();
            environment.dispose();
            renderer.dispose();
            renderer.domElement.remove();
        },
    };
}
