using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Runecrypt.Tests3D
{
    // Docs/specs/2026-11-21-3d-animasi-jalan-design.md: model ber-rig Generic dengan klip Idle dan Move yang berulang.
    public class CharacterRigTests
    {
        [TestCase("Assets/Spike3D/Models/TulangPenjaga3D.fbx", 16 / 60f, "thigh.L")]
        [TestCase("Assets/Runecrypt3D/Models/Pyromancer3D.fbx", 32 / 60f, "robe_front.L_2")]
        public void Model_HasGenericLoopingClips(string path, float moveSeconds, string bone)
        {
            var imp = (ModelImporter)AssetImporter.GetAtPath(path);
            Assert.AreEqual(ModelImporterAnimationType.Generic, imp.animationType);
            var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview")).ToDictionary(c => c.name);
            Assert.That(clips.Keys, Is.SupersetOf(new[] { "Idle", "Move" }));
            Assert.IsTrue(clips["Idle"].isLooping && clips["Move"].isLooping);
            Assert.AreEqual(moveSeconds, clips["Move"].length, 0.02f);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(model.GetComponentInChildren<SkinnedMeshRenderer>());
            Assert.IsTrue(model.GetComponentsInChildren<Transform>().Any(t => t.name == bone));
        }
    }
}
