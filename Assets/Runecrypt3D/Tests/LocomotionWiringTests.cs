using System.Collections;
using System.Linq;
using NUnit.Framework;
using Runecrypt.Game3D;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runecrypt.Tests3D
{
    // Prefab memakai Animator Idle <-> Move yang dikendalikan kecepatan (spec animasi jalan, Task 5 rencana).
    public class LocomotionWiringTests
    {
        const string Prefabs = "Assets/Runecrypt3D/Prefabs/";

        // Mengatur kecepatan di FixedUpdate seperti Enemy3D, supaya Update membaca kecepatan setelah simulasi fisika.
        class Driver : MonoBehaviour
        {
            public Vector3 velocity;
            void FixedUpdate() => GetComponent<Rigidbody>().linearVelocity = velocity;
        }

        [TestCase("Tulang3D", 2f, .5f, 1.3f, false)]
        [TestCase("Player3D", 5f, .6f, 1.2f, true)]
        public void Prefab_HasSpeedDrivenAnimator(string prefab, float v0, float min, float max, bool rev)
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefab + ".prefab");
            var anim = root.GetComponentInChildren<Animator>();
            Assert.IsNotNull(anim, "tidak ada Animator");
            var ctrl = anim.runtimeAnimatorController as AnimatorController;
            Assert.IsNotNull(ctrl, "Animator tanpa AnimatorController");
            var state = ctrl.layers[0].stateMachine.defaultState;
            var tree = state.motion as BlendTree;
            Assert.IsNotNull(tree, "state default bukan BlendTree");
            Assert.AreEqual("Speed", tree.blendParameter);
            Assert.AreEqual(new[] { "Idle", "Move" }, tree.children.Select(c => c.motion.name).ToArray());
            Assert.AreEqual(0f, tree.children[0].threshold, 1e-4f);
            Assert.AreEqual(min * v0, tree.children[1].threshold, 1e-4f);
            Assert.IsTrue(state.speedParameterActive);
            Assert.AreEqual("SpeedMul", state.speedParameter);

            var loco = new SerializedObject(root.GetComponent<Locomotion3D>());
            Assert.AreEqual(v0, loco.FindProperty("designSpeed").floatValue, 1e-4f);
            Assert.AreEqual(min, loco.FindProperty("minMul").floatValue, 1e-4f);
            Assert.AreEqual(max, loco.FindProperty("maxMul").floatValue, 1e-4f);
            Assert.AreEqual(rev, loco.FindProperty("reverseWhenBackward").boolValue);
            Assert.AreSame(anim, loco.FindProperty("animator").objectReferenceValue);

            var enemy = root.GetComponent<Enemy3D>();
            if (enemy == null) return;
            var flash = new SerializedObject(enemy).FindProperty("flashRenderers");
            bool skinned = Enumerable.Range(0, flash.arraySize).Any(i => flash.GetArrayElementAtIndex(i).objectReferenceValue is SkinnedMeshRenderer);
            Assert.IsTrue(skinned, "kedipan hit tidak mencakup SkinnedMeshRenderer");
        }

        static IEnumerator Steps(int n)
        {
            float target = Time.fixedTime + n * Time.fixedDeltaTime - 1e-4f;
            while (Time.fixedTime < target) yield return new WaitForFixedUpdate();
        }

        static (Driver drive, Animator anim) MakeTulang(Vector3 at)
        {
            var go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "Tulang3D.prefab"), at, Quaternion.identity);
            Object.DestroyImmediate(go.GetComponent<Enemy3D>());   // gerak dari Driver, bukan AI
            var anim = go.GetComponentInChildren<Animator>();
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;   // di x >= 100 tidak ada kamera; tanpa ini tulang tidak diperbarui
            return (go.AddComponent<Driver>(), anim);
        }

        static Transform Find(Transform t, string name) => t.GetComponentsInChildren<Transform>().First(c => c.name == name);

        [UnityTest]
        public IEnumerator Tulang_Moving_SwingsLegs()
        {
            yield return new EnterPlayMode();
            var (drive, anim) = MakeTulang(new Vector3(100f, 0f, 0f));
            var thigh = Find(drive.transform, "thigh.L");
            drive.velocity = new Vector3(2f, 0f, 0f);
            yield return Steps(30);
            Assert.Greater(anim.GetFloat("Speed"), 1.5f);
            var a = thigh.localRotation;
            yield return Steps(4);
            Assert.Greater(Quaternion.Angle(a, thigh.localRotation), 5f, "paha tidak mengayun");
            Object.Destroy(drive.gameObject);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator Tulang_Stopped_GoesIdle()
        {
            yield return new EnterPlayMode();
            var (drive, anim) = MakeTulang(new Vector3(110f, 0f, 0f));
            drive.velocity = new Vector3(2f, 0f, 0f);
            yield return Steps(30);
            Assert.Greater(anim.GetFloat("Speed"), 1.5f, "prasyarat: sempat berlari");
            drive.velocity = Vector3.zero;
            yield return Steps(50);
            Assert.Less(anim.GetFloat("Speed"), 0.1f);
            Object.Destroy(drive.gameObject);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator Tulang_PushingWall_GoesIdle()
        {
            yield return new EnterPlayMode();
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.layer = LayerMask.NameToLayer("Walls");
            wall.transform.position = new Vector3(120.8f, 0.5f, 0f);
            var (drive, anim) = MakeTulang(new Vector3(120f, 0f, 0f));
            Assert.IsNotNull(anim.runtimeAnimatorController, "prasyarat: Animator punya controller");
            drive.velocity = new Vector3(2f, 0f, 0f);
            yield return Steps(40);
            Assert.Less(anim.GetFloat("Speed"), 1.0f, "berlari di tempat saat terhalang dinding");
            Object.Destroy(drive.gameObject); Object.Destroy(wall);
            yield return new ExitPlayMode();
        }
    }
}
