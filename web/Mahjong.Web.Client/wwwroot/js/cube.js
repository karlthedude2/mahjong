// Prototype: draws the Mahjong Cube game with Three.js. The game itself (which tiles exist, which
// are free, what matches) lives in CubeGame.cs; this only draws it, turns it, and reports taps.
// The stone, light and face art are shared with the 3D board (stone.js).

import * as THREE from "../lib/three/three.module.min.js";
import { OrbitControls } from "../lib/three/OrbitControls.js";
import { RoundedBoxGeometry } from "../lib/three/RoundedBoxGeometry.js";
import { Tints, createStage, createStoneLook } from "./stone.js";

const Spacing = 1.06;

export function create(host, dotnet) {
    const stage = createStage(host, 40);
    const { renderer, scene, camera } = stage;

    const controls = new OrbitControls(camera, renderer.domElement);
    // A short glide after a drag: long enough to feel smooth, short enough that a tap right
    // afterwards lands on the tile under the finger.
    controls.enableDamping = true;
    controls.dampingFactor = 0.2;
    controls.enablePan = false;

    // Slightly rounded edges: 3 steps round each one is smooth at this size, and keeps each cube to
    // under 600 triangles.
    const box = new RoundedBoxGeometry(1, 1, 1, 3, 0.06);
    const meshes = new Map();
    // The scene is drawn only when something changes: a turn, a tap, a texture arriving.
    let dirty = true;
    const changed = () => { dirty = true; };
    const look = createStoneLook({ meshes: () => meshes.values(), changed });
    let selectedId = null;
    let hinted = new Set();
    let free = new Set();

    function tint(mesh) {
        const id = mesh.userData.id;
        mesh.material.color.copy(id === selectedId ? Tints.selected : hinted.has(id) ? Tints.hinted : free.has(id) ? Tints.free : Tints.blocked);
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
                const mesh = new THREE.Mesh(box, look.material(tile));
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
            look.refresh();

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
            look.use(name, url, faces);
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
            look.dispose();
            box.dispose();
            stage.dispose();
        },
    };
}
