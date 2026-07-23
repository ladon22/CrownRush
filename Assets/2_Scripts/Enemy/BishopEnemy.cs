using System.Collections.Generic;
using UnityEngine;

public class BishopEnemy : SlidingPieceEnemy
{
    private static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        new Vector2Int( 1,  1), new Vector2Int( 1, -1),
        new Vector2Int(-1,  1), new Vector2Int(-1, -1)
    };

    protected override Vector2Int[] MoveVectors => Directions;
}
