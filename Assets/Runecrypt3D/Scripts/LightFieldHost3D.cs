using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game3D
{
    // LightField dari Core apa adanya; dinding dicek dengan Linecast setinggi pinggang terhadap layer Walls.
    // Statis seperti LightFieldHost 2D, jadi tidak butuh objek di scene.
    public static class LightFieldHost3D
    {
        const float CheckHeight = 0.5f;
        static LightField field;
        static int wallMask = -1;

        public static LightField Field => field ??= new LightField(IsBlocked);

        static bool IsBlocked(Vector2 from, Vector2 to)
        {
            if (wallMask < 0) wallMask = LayerMask.GetMask("Walls");
            return Physics.Linecast(Plane3D.ToWorld(from, CheckHeight), Plane3D.ToWorld(to, CheckHeight), wallMask,
                QueryTriggerInteraction.Ignore);
        }
    }
}
