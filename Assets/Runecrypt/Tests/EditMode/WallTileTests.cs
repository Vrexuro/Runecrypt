using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Runecrypt.Tests
{
    // Regresi M1: collider dinding ruangan kosong sehingga pemain bisa menembus dinding.
    // Susunan sama dengan M1Builder.MakeSolidTilemap; tile yang dipasang di editor baru jadi collider
    // setelah ProcessTilemapChanges + GenerateGeometry (yang kini dipanggil M1Builder.BuildTilemaps).
    public class WallTileTests
    {
        [Test]
        public void SolidWallTilemap_BlocksOverlapAtWallCell()
        {
            var tile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Runecrypt/Data/Tiles/Wall.asset");
            var go = new GameObject("WallTileTest", typeof(Grid));
            go.transform.position = new Vector3(200f, 0f, 0f);
            try
            {
                var map = new GameObject("Walls");
                map.transform.SetParent(go.transform, false);
                map.layer = LayerMask.NameToLayer("Walls");
                var tm = map.AddComponent<Tilemap>();
                map.AddComponent<TilemapRenderer>();
                map.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                map.AddComponent<TilemapCollider2D>().compositeOperation = Collider2D.CompositeOperation.Merge;
                map.AddComponent<CompositeCollider2D>().geometryType = CompositeCollider2D.GeometryType.Polygons;
                for (int x = 0; x < 3; x++) tm.SetTile(new Vector3Int(x, 0, 0), tile);
                map.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
                map.GetComponent<CompositeCollider2D>().GenerateGeometry();
                Physics2D.SyncTransforms();
                var hit = Physics2D.OverlapCircle(new Vector2(201.5f, 0.5f), 0.2f, LayerMask.GetMask("Walls"));
                Assert.That(hit, Is.Not.Null);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
