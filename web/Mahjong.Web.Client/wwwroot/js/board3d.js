// The solitaire board in 3D: each tile a stone block with its face on top, stacked in layers, seen
// from above at an angle like a real table. The game (which tiles are where, which are free, what a
// click does) stays in C#; this draws whatever the page says the board holds, and reports clicks.
// The stone, light and face art are shared with the Mahjong Cube (stone.js).

import * as THREE from "../lib/three/three.module.min.js";
import { OrbitControls } from "../lib/three/OrbitControls.js";
import { RoundedBoxGeometry } from "../lib/three/RoundedBoxGeometry.js";
import { Tints, createStage, createStoneLook } from "./stone.js";

// A tile's size: a board position is half a tile, so a tile is two positions wide and two deep.
const Width = 1, Depth = 1.33, Height = 0.62;
const Gap = 0.04;
// How far from straight down the board is first seen, and how far it may be tilted and turned.
const Tilt = 0.5, MaxTilt = 0.95, MaxTurn = 0.6;

export function create(host, dotnet) {
    const stage = createStage(host, 35);
    const { renderer, scene, camera } = stage;

    const controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;
    controls.dampingFactor = 0.2;
    controls.enablePan = false;
    controls.minPolarAngle = 0;
    controls.maxPolarAngle = MaxTilt;
    controls.minAzimuthAngle = -MaxTurn;
    controls.maxAzimuthAngle = MaxTurn;

    const block = new RoundedBoxGeometry(Width - Gap, Height, Depth - Gap, 3, 0.07);
    const meshes = new Map();
    let dirty = true;
    const changed = () => { dirty = true; };
    const look = createStoneLook({ meshes: () => meshes.values(), changed, aspect: Depth / Width, artOnTop: true });
    let faces = {};
    let selectedId = null, hinted = new Set();

    const place = (mesh, x, y, z) => mesh.position.set(x * Width / 2, z * Height + Height / 2, y * Depth / 2);

    // Gold when selected, blue for a hint, otherwise as it is. Unlike the cube, blocked tiles aren't
    // dimmed: on a board, which tiles are covered can be seen.
    function tint(mesh) {
        const id = mesh.userData.id;
        mesh.material.color.copy(id === selectedId ? Tints.selected : hinted.has(id) ? Tints.hinted : Tints.free);
    }

    // Frames the whole layout (not just the tiles left, so the view doesn't jump as tiles go). With
    // keepView, the player's tilt and turn are kept and only the distance changes (a resized window).
    let frame = null, framed = false;
    function fit(keepView) {
        if (!frame) {
            return;
        }
        const width = host.clientWidth, height = host.clientHeight;
        if (!width || !height) {
            return;
        }
        const { centre, across, deep } = frame;
        const vertical = THREE.MathUtils.degToRad(camera.fov) / 2;
        const horizontal = Math.atan(Math.tan(vertical) * camera.aspect);
        const distance = 1.08 * Math.max(across / 2 / Math.tan(horizontal), deep * Math.cos(Tilt) / 2 / Math.tan(vertical)) + Height * 4;
        controls.target.copy(centre);
        if (keepView && framed) {
            const offset = camera.position.clone().sub(centre).setLength(distance);
            camera.position.copy(centre).add(offset);
        } else {
            camera.position.set(centre.x, centre.y + distance * Math.cos(Tilt), centre.z + distance * Math.sin(Tilt));
            framed = true;
        }
        controls.minDistance = distance * 0.55;
        controls.maxDistance = distance * 1.6;
        controls.update();
        changed();
    }

    function resize() {
        const width = host.clientWidth, height = host.clientHeight;
        if (width && height) {
            renderer.setSize(width, height, false);
            camera.aspect = width / height;
            camera.updateProjectionMatrix();
            fit(true);
        }
    }
    const resized = new ResizeObserver(resize);
    resized.observe(host);

    // A click (not a drag that tilts the board) on a tile tells the game which one.
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
            dotnet.invokeMethodAsync("OnTileClicked", hit.object.userData.id);
        }
    });

    // Removed tiles shrink away; tiles that come back (undo) grow in.
    const shrinking = [], growing = [];
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
        for (let i = growing.length - 1; i >= 0; i--) {
            const item = growing[i];
            const t = Math.min(1, (time - (item.start ??= time)) / 180);
            item.mesh.scale.setScalar(Math.max(t, 0.001));
            if (t >= 1) {
                growing.splice(i, 1);
            }
        }
        if (controls.update() || shrinking.length > 0 || growing.length > 0 || dirty) {
            dirty = false;
            renderer.render(scene, camera);
        }
    });

    return {
        // The game's layout (every position, [x, y, z] in half-tile units), and its faces by name:
        // { name: { art: { light, dark, any }, glyph: { main, caption, color } } }.
        setLayout(positions, faceInfo) {
            faces = faceInfo;
            let x0 = Infinity, x1 = -Infinity, z0 = Infinity, z1 = -Infinity, top = 0;
            for (const [x, y, z] of positions) {
                x0 = Math.min(x0, x * Width / 2); x1 = Math.max(x1, x * Width / 2 + Width);
                z0 = Math.min(z0, y * Depth / 2); z1 = Math.max(z1, y * Depth / 2 + Depth);
                top = Math.max(top, z);
            }
            // Positions are a tile's corner; the meshes are centred on them, so shift by half a tile.
            frame = {
                centre: new THREE.Vector3((x0 + x1) / 2 - Width / 2, top * Height / 2, (z0 + z1) / 2 - Depth / 2),
                across: x1 - x0,
                deep: z1 - z0,
            };
            fit(false);
        },

        // "marble", "darkMarble", "onyx" or "ivory"; faces is its configured face set, if any.
        setStone(name, faces) {
            look.use(name, null, faces);
        },

        // The board now: tiles = [[id, x, y, z, faceName], ...], the selected tile, and
        // a hint's two. New tiles grow in, gone ones shrink away, and a shuffled face is reprinted.
        show(tiles, selected, hintIds) {
            const seen = new Set();
            let added = false;
            for (const [id, x, y, z, face] of tiles) {
                seen.add(id);
                let mesh = meshes.get(id);
                const tile = { face, ...faces[face] };
                if (!mesh) {
                    mesh = new THREE.Mesh(block, look.material(tile));
                    mesh.userData = { id, tile };
                    meshes.set(id, mesh);
                    scene.add(mesh);
                    mesh.scale.setScalar(0.001);
                    growing.push({ mesh });
                    added = true;
                } else if (mesh.userData.tile.face !== face) {
                    mesh.userData.tile = tile;
                    added = true;
                }
                place(mesh, x, y, z);
            }
            for (const [id, mesh] of meshes) {
                if (!seen.has(id)) {
                    meshes.delete(id);
                    shrinking.push({ mesh });
                }
            }
            if (added) {
                look.refresh();
            }
            selectedId = selected;
            hinted = new Set(hintIds);
            meshes.forEach(tint);
            changed();
        },

        dispose() {
            renderer.setAnimationLoop(null);
            resized.disconnect();
            controls.dispose();
            for (const mesh of meshes.values()) {
                mesh.material.dispose();
            }
            look.dispose();
            block.dispose();
            stage.dispose();
        },
    };
}
