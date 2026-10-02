// Prototype: draws the Mahjong Cube game with Three.js. The game itself (which tiles exist, which
// are free, what matches) lives in CubeGame.cs; this only draws it, turns it, and reports taps.

import * as THREE from "../lib/three/three.module.min.js";
import { OrbitControls } from "../lib/three/OrbitControls.js";
import { RoundedBoxGeometry } from "../lib/three/RoundedBoxGeometry.js";

const Spacing = 1.06;
const Colors = { red: "#c0141b", green: "#0d7a2e", blue: "#134ea8", wind: "#1b1b1b", flower: "#b0217a", season: "#b35a00" };
const Free = new THREE.Color(0xffffff);
const Blocked = new THREE.Color(0x8c8577);
const Selected = new THREE.Color(0xf2b632);
const Hinted = new THREE.Color(0x7fd3ff);

// What the cubes are made of. A stone with a texture is a flat, evenly lit, square picture of the
// material; each face of each cube shows its own slice of it, so no two faces look alike.
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
// from each side.

// A tile's face for a face set, made once per face name, set and picture: drawn as text at once,
// then replaced by the face's picture when it arrives.
function faceTexture(tile, cache, changed, set) {
    const url = tile.art?.[set] ?? tile.art?.any;
    const key = [tile.face, set, url ?? ""].join("|");
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
    // Pale with a dark outline on a dark stone, otherwise in the suit's colour.
    const write = (text, y, weight, px, outline) => {
        g.font = `${weight} ${px}px ${font}`;
        if (set === "dark") {
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

    if (url) {
        const art = new Image();
        art.onload = () => {
            g.clearRect(0, 0, size, size);
            const scale = Math.min(size / art.width, size / art.height);
            const w = art.width * scale, h = art.height * scale;
            g.drawImage(art, (size - w) / 2, (size - h) / 2, w, h);
            texture.needsUpdate = true;
            changed();
        };
        art.src = url;
    }
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
    // Soft light from every side gives the stone its sheen on the rounded edges (see softStudio).
    const environment = new THREE.PMREMGenerator(renderer);
    const studio = softStudio();
    scene.environment = environment.fromScene(studio, 0.04).texture;
    studio.traverse(o => { o.geometry?.dispose(); o.material?.dispose(); });
    scene.environmentIntensity = 1;
    const camera = new THREE.PerspectiveCamera(40, 1, 0.1, 100);
    scene.add(camera);
    // A light that rides with the camera, so the side facing the player is always the brightest. It
    // comes from above and to the left, so it doesn't bounce straight back into the player's eyes.
    const light = new THREE.DirectionalLight(0xffffff, 0.9);
    light.position.set(-4, 5, 2);
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
    // A stone by name, or a picture to try (url) under that name, with its configured face set
    // ("light" or "dark") if it has one. Without one, its faces are light until the picture arrives
    // and shows whether it's dark.
    function useStone(name, url, faces) {
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
                applyStone(chosen);
            });
            chosen.map.colorSpace = THREE.SRGBColorSpace;
            chosen.map.anisotropy = 4;
        }
        applyStone(stone);
    }

    // The face set the stone uses: as configured, else as its picture looks.
    function faceSet() {
        return stone.faces ?? stone.looks ?? "light";
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
            u.artMap.value = faceTexture(mesh.userData.tile, textures, changed, faceSet());
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
        // A new deal: tiles = [{ id, x, y, z, face, art: { light, dark, any }, glyph: { main, caption, color } }].
        setBoard(size, tiles, freeIds) {
            clear();
            selectedId = null;
            hinted = new Set();
            free = new Set(freeIds);
            const middle = (size - 1) / 2;
            for (const tile of tiles) {
                const mesh = new THREE.Mesh(box, cubeMaterial(faceTexture(tile, textures, changed, faceSet()), stone, blank));
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

        // "marble", "darkMarble", "onyx", "ivory", or any name with the url of a picture to try;
        // faces is the stone's configured face set, "light" or "dark", if any.
        setStone(name, url, faces) {
            useStone(name, url, faces);
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
