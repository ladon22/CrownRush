using System.Collections;
using UnityEngine;

public class KnightEnemy : EnemyBase
{
    private int XDirection = 1;

    public override void Initialize(EnemyManager EnemyManager, GridManager GridManager, int StartX, int StartY)
    {
        base.Initialize(EnemyManager, GridManager, StartX, StartY);
        XDirection = Random.value > 0.5 ? 1 : -1;
        if (CurrentX == 0)
        {
            XDirection = 1;
        }
        else if (CurrentX == GridManager.Columns - 1)
        {
            XDirection = -1;
        }
    }
    public override void CalculateNextPosition()
    {
        int NextY = CurrentY - 2;
        int NextX = CurrentX + XDirection;

        if (NextX <= 0) 
        { 
            NextX = 0; 
            XDirection = 1; 
        }
        else if (NextX >= GridManager.Columns - 1) 
        { 
            NextX = GridManager.Columns - 1; 
            XDirection = -1; 
        }

        if (!IsPositionOccupied(NextX, NextY))
        {
            CurrentX = NextX;
            CurrentY = NextY;
        }
        else
        {
            XDirection *= -1;
            NextX = CurrentX + XDirection;

            if (NextX <= 0) 
            { 
                NextX = 0; 
                XDirection = 1; 
            }
            else if (NextX >= GridManager.Columns - 1) 
            { 
                NextX = GridManager.Columns - 1; 
                XDirection = -1; 
            }

            if (!IsPositionOccupied(NextX, NextY))
            {
                CurrentX = NextX;
                CurrentY = NextY;
            }
        }

        //CurrentY -= 2;
        //if (CurrentX <= 0)
        //{
        //    CurrentX = 0;
        //    XDirection = 1;
        //}
        //else if (CurrentX >= GridManager.Columns - 1)
        //{
        //    CurrentX = GridManager.Columns - 1;
        //    XDirection = -1;
        //}
        //CurrentX += XDirection;
    }

    protected override IEnumerator AnimateMove(int StartX, int StartY, int TargetX, int TargetY)
    {
        Vector2 Wayporint = GridManager.GetWorldPosition(StartX, TargetY);
        while ((Vector2)transform.position != Wayporint)
        {
            transform.position = Vector2.MoveTowards(transform.position, Wayporint, MoveSpeed * Time.deltaTime);
            yield return null;
        }

        Vector2 FinalPosition = GridManager.GetWorldPosition(TargetX, TargetY);
        while ((Vector2)transform.position != FinalPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, FinalPosition, MoveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
