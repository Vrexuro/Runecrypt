using System.Collections.Generic;
using NUnit.Framework;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Tests
{
    public class FlowFieldTests
    {
        static readonly RectInt Area = new RectInt(-6, -6, 13, 13);

        static FlowField Build(Vector2Int target, params Vector2Int[] blocked)
        {
            var set = new HashSet<Vector2Int>(blocked);
            var f = new FlowField();
            f.Build(Area, target, c => set.Contains(c));
            return f;
        }

        static Vector2Int[] Column(int x, int fromY, int toY)
        {
            var cells = new List<Vector2Int>();
            for (int y = fromY; y <= toY; y++) cells.Add(new Vector2Int(x, y));
            return cells.ToArray();
        }

        // Mengikuti TryStep sampai target; gagal kalau melewati sel terhalang atau tidak sampai.
        static int Follow(FlowField f, Vector2Int from, Vector2Int target, ICollection<Vector2Int> blocked)
        {
            var cell = from;
            for (int i = 0; i < 200; i++)
            {
                if (cell == target) return i;
                Assert.IsTrue(f.TryStep(cell, out var next), $"tidak ada langkah dari {cell}");
                Assert.IsFalse(blocked.Contains(next), $"melangkah ke sel terhalang {next}");
                Assert.LessOrEqual(Mathf.Abs(next.x - cell.x) + Mathf.Abs(next.y - cell.y), 2, "langkah hanya ke tetangga");
                cell = next;
            }
            Assert.Fail("tidak sampai ke target");
            return -1;
        }

        [Test]
        public void OpenGrid_CostIsOctile()
        {
            var f = Build(Vector2Int.zero);
            Assert.AreEqual(0, f.Distance(Vector2Int.zero));
            Assert.AreEqual(30, f.Distance(new Vector2Int(3, 0)));
            Assert.AreEqual(28, f.Distance(new Vector2Int(2, 2)));
            Assert.AreEqual(34, f.Distance(new Vector2Int(3, 1)));
        }

        [Test]
        public void BlockedAndOutsideCells_AreUnreachable()
        {
            var f = Build(Vector2Int.zero, new Vector2Int(2, 0));
            Assert.AreEqual(-1, f.Distance(new Vector2Int(2, 0)));
            Assert.AreEqual(-1, f.Distance(new Vector2Int(50, 0)));
            Assert.IsFalse(f.TryStep(new Vector2Int(50, 0), out _));
        }

        [Test]
        public void TargetCell_HasNoStep()
        {
            var f = Build(Vector2Int.zero);
            Assert.IsFalse(f.TryStep(Vector2Int.zero, out _));
        }

        [Test]
        public void Path_GoesAroundWall()
        {
            var wall = Column(2, -2, 2);
            var f = Build(Vector2Int.zero, wall);
            int steps = Follow(f, new Vector2Int(4, 0), Vector2Int.zero, wall);
            Assert.Greater(steps, 4, "harus memutar, bukan menembus");
        }

        [Test]
        public void Path_LeavesPocketOpeningAwayFromTarget()
        {
            // ⊐ di kanan target, terbuka ke kanan.
            var blocked = new List<Vector2Int>(Column(2, -2, 2));
            for (int x = 3; x <= 5; x++) { blocked.Add(new Vector2Int(x, 2)); blocked.Add(new Vector2Int(x, -2)); }
            var f = Build(Vector2Int.zero, blocked.ToArray());
            Assert.IsTrue(f.TryStep(new Vector2Int(3, 0), out var first));
            Assert.Greater(first.x, 3, "langkah pertama keluar dari ceruk menjauhi target");
            Follow(f, new Vector2Int(3, 0), Vector2Int.zero, blocked);
        }

        [Test]
        public void Diagonal_DoesNotCutCorners()
        {
            // (1,1) ke (0,0) hanya lewat diagonal yang diapit dua sel terhalang: tidak boleh.
            var f = new FlowField();
            var blocked = new HashSet<Vector2Int> { new Vector2Int(1, 0), new Vector2Int(0, 1) };
            f.Build(new RectInt(0, 0, 2, 2), Vector2Int.zero, c => blocked.Contains(c));
            Assert.AreEqual(-1, f.Distance(new Vector2Int(1, 1)));
        }

        // Musuh yang badannya menempel ke penghalang bisa berada di sel terhalang; ia tetap harus dapat arah keluar.
        [Test]
        public void StepFromBlockedCell_GoesToReachableNeighbour()
        {
            var f = Build(Vector2Int.zero, new Vector2Int(3, 0));
            Assert.IsTrue(f.TryStep(new Vector2Int(3, 0), out var next));
            Assert.GreaterOrEqual(f.Distance(next), 0);
            Assert.Less(next.x, 3);
        }
    }
}
