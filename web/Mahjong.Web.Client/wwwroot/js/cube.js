// Prototype: draws the Mahjong Cube game with Three.js. The game itself (which tiles exist, which
// are free, what matches) lives in CubeGame.cs; this only draws it, turns it, and reports taps.

import * as THREE from "../lib/three/three.module.min.js";
import { OrbitControls } from "../lib/three/OrbitControls.js";
import { RoundedBoxGeometry } from "../lib/three/RoundedBoxGeometry.js";
import { RoomEnvironment } from "../lib/three/RoomEnvironment.js";

const Spacing = 1.06;
// The cubes and the background of their faces are one material: polished ivory.
const Ivory = "#ecdfc2";
const Colors = { red: "#c0141b", green: "#0d7a2e", blue: "#134ea8", wind: "#1b1b1b", flower: "#b0217a", season: "#b35a00" };
const Free = new THREE.Color(0xffffff);
const Blocked = new THREE.Color(0x8c8577);
const Selected = new THREE.Color(0xf2b632);
const Hinted = new THREE.Color(0x7fd3ff);

// Square face art for the cube, one PNG per face name (2bams.png, Cdragon.png, ...), drawn over the
// ivory so a transparent background shows the material. The picture runs over the rounded edges,
// so keep the symbol about 15% in from each side. Faces without a picture are drawn as text.
const ArtFolder = "tilesets/cube/";

// A tile's face, made once per face name: drawn as text at once, then replaced by the face's
// picture if there is one.
function faceTexture(tile, cache) {
    let texture = cache.get(tile.face);
    if (texture) {
        return texture;
    }

    const size = 512;
    const canvas = document.createElement("canvas");
    canvas.width = canvas.height = size;
    const g = canvas.getContext("2d");
    g.fillStyle = Ivory;
    g.fillRect(0, 0, size, size);

    const glyph = tile.glyph;
    const font = '"Segoe UI", "Microsoft YaHei", "PingFang SC", "Noto Sans CJK SC", Arial, sans-serif';
    g.fillStyle = Colors[glyph.color] || Colors.wind;
    g.textAlign = "center";
    g.textBaseline = "middle";
    g.font = `800 220px ${font}`;
    g.fillText(glyph.main, size / 2, size * 0.43, size * 0.7);
    g.font = `700 64px ${font}`;
    g.fillText(glyph.caption.toUpperCase(), size / 2, size * 0.76, size * 0.7);

    texture = new THREE.CanvasTexture(canvas);
    texture.colorSpace = THREE.SRGBColorSpace;
    texture.anisotropy = 8;
    cache.set(tile.face, texture);

    const art = new Image();
    art.onload = () => {
        g.fillStyle = Ivory;
        g.fillRect(0, 0, size, size);
        g.drawImage(art, 0, 0, size, size);
        texture.needsUpdate = true;
    };
    art.src = ArtFolder + encodeURIComponent(tile.face) + ".png";
    return texture;
}

export function create(host, dotnet) {
    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.toneMapping = THREE.NeutralToneMapping;
    host.appendChild(renderer.domElement);

    const scene = new THREE.Scene();
    // Soft light from every side, as in a room, gives the ivory its sheen on the rounded edges.
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

    const box = new RoundedBoxGeometry(1, 1, 1, 6, 0.16);
    const textures = new Map();
    const meshes = new Map();
    let selectedId = null;
    let hinted = new Set();
    let free = new Set();

    function tint(mesh) {
        const id = mesh.userData.id;
        mesh.material.color.copy(id === selectedId ? Selected : hinted.has(id) ? Hinted : free.has(id) ? Free : Blocked);
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
        const hit = raycaster.intersectObjects([...meshes.values()], false)[0];
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
                scene.remove(item.mesh);
                item.mesh.material.dispose();
                shrinking.splice(i, 1);
            }
        }
        controls.update();
        renderer.render(scene, camera);
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
                const material = new THREE.MeshStandardMaterial({ map: faceTexture(tile, textures), roughness: 0.28, metalness: 0 });
                const mesh = new THREE.Mesh(box, material);
                mesh.position.set((tile.x - middle) * Spacing, (tile.y - middle) * Spacing, (tile.z - middle) * Spacing);
                mesh.userData.id = tile.id;
                meshes.set(tile.id, mesh);
                scene.add(mesh);
                tint(mesh);
            }

            const distance = size * Spacing * 2.6;
            camera.position.set(distance * 0.6, distance * 0.5, distance * 0.62);
            controls.target.set(0, 0, 0);
            controls.minDistance = size * 1.2;
            controls.maxDistance = size * 5;
            resize();
        },

        // After any change: the selected tile (or null), the free tiles, and a hint's tiles.
        update(selected, freeIds, hintIds) {
            selectedId = selected;
            free = new Set(freeIds);
            hinted = new Set(hintIds);
            meshes.forEach(tint);
        },

        remove(ids) {
            for (const id of ids) {
                const mesh = meshes.get(id);
                if (mesh) {
                    meshes.delete(id);
                    shrinking.push({ mesh });
                }
            }
        },

        dispose() {
            renderer.setAnimationLoop(null);
            resized.disconnect();
            controls.dispose();
            clear();
            textures.forEach(t => t.dispose());
            box.dispose();
            scene.environment.dispose();
            environment.dispose();
            renderer.dispose();
            renderer.domElement.remove();
        },
    };
}
