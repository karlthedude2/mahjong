// Prototype: draws the Mahjong Cube game with Three.js. The game itself (which tiles exist, which
// are free, what matches) lives in CubeGame.cs; this only draws it, turns it, and reports taps.

import * as THREE from "../lib/three/three.module.min.js";
import { OrbitControls } from "../lib/three/OrbitControls.js";

const Spacing = 1.04;
const Colors = { red: "#c0141b", green: "#0d7a2e", blue: "#134ea8", wind: "#1b1b1b", flower: "#b0217a", season: "#b35a00" };
const Free = new THREE.Color(0xffffff);
const Blocked = new THREE.Color(0x8c8577);
const Selected = new THREE.Color(0xf2b632);
const Hinted = new THREE.Color(0x7fd3ff);

// A tile's face, drawn once per face name: the Simple tile set's symbol and caption on ivory.
function faceTexture(glyph, cache) {
    const key = glyph.main + "|" + glyph.caption;
    let texture = cache.get(key);
    if (texture) {
        return texture;
    }

    const size = 256;
    const canvas = document.createElement("canvas");
    canvas.width = canvas.height = size;
    const g = canvas.getContext("2d");
    g.fillStyle = "#fffdf6";
    g.fillRect(0, 0, size, size);
    g.strokeStyle = "#b9a57a";
    g.lineWidth = 14;
    g.strokeRect(7, 7, size - 14, size - 14);

    const font = '"Segoe UI", "Microsoft YaHei", "PingFang SC", "Noto Sans CJK SC", Arial, sans-serif';
    g.fillStyle = Colors[glyph.color] || Colors.wind;
    g.textAlign = "center";
    g.textBaseline = "middle";
    g.font = `800 120px ${font}`;
    g.fillText(glyph.main, size / 2, size * 0.42, size - 40);
    g.font = `700 38px ${font}`;
    g.fillText(glyph.caption.toUpperCase(), size / 2, size * 0.8, size - 40);

    texture = new THREE.CanvasTexture(canvas);
    texture.colorSpace = THREE.SRGBColorSpace;
    texture.anisotropy = 4;
    cache.set(key, texture);
    return texture;
}

export function create(host, dotnet) {
    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    host.appendChild(renderer.domElement);

    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(40, 1, 0.1, 100);
    scene.add(camera);
    scene.add(new THREE.AmbientLight(0xffffff, 1.6));
    // The light rides with the camera, so the side facing the player is always lit.
    const light = new THREE.DirectionalLight(0xffffff, 1.8);
    light.position.set(2, 3, 4);
    camera.add(light);

    const controls = new OrbitControls(camera, renderer.domElement);
    // A short glide after a drag: long enough to feel smooth, short enough that a tap right
    // afterwards lands on the tile under the finger.
    controls.enableDamping = true;
    controls.dampingFactor = 0.2;
    controls.enablePan = false;

    const box = new THREE.BoxGeometry(1, 1, 1);
    const edges = new THREE.EdgesGeometry(box);
    const edgeMaterial = new THREE.LineBasicMaterial({ color: 0x6b5a36 });
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
        // A new deal: tiles = [{ id, x, y, z, glyph: { main, caption, color } }].
        setBoard(size, tiles, freeIds) {
            clear();
            selectedId = null;
            hinted = new Set();
            free = new Set(freeIds);
            const middle = (size - 1) / 2;
            for (const tile of tiles) {
                const mesh = new THREE.Mesh(box, new THREE.MeshLambertMaterial({ map: faceTexture(tile.glyph, textures) }));
                mesh.position.set((tile.x - middle) * Spacing, (tile.y - middle) * Spacing, (tile.z - middle) * Spacing);
                mesh.userData.id = tile.id;
                mesh.add(new THREE.LineSegments(edges, edgeMaterial));
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
            edges.dispose();
            edgeMaterial.dispose();
            renderer.dispose();
            renderer.domElement.remove();
        },
    };
}
