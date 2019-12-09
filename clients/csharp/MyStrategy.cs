using System;
using System.Collections.Generic;
using AiCup2019.Model;
using System.Linq;
using static AiCup2019.Model.CustomData;

namespace AiCup2019
{

    public static class VectorsExtensions 
    {
        public static int Width = 24;
        public static int Height = 24;

        public static double EuclidianSqr(this Vec2Double a, Vec2Double b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
        }

        public static Vec2Double AimAt(this Vec2Double unit, Vec2Double nearestEnemy)
        {
            return new Vec2Double(nearestEnemy.X - unit.X, nearestEnemy.Y - unit.Y);
        }

        public static Vec2Float AsFloat(this Vec2Double a)
        {
            return new Vec2Float((float)a.X, (float)a.Y);
        }

        public static Vec2Double AsDouble(this Vec2Float a)
        {
            return new Vec2Double((double)a.X, (double)a.Y);
        }

        public static bool IsLeftTo(this Vec2Double a, Vec2Double b)
        {
            return a.X < b.X;
        }

        public static bool IsRightTo(this Vec2Double a, Vec2Double b)
        {
            return a.X > b.X;
        }

        public static Vec2Double AsHalf(this Vec2Double a, bool rightToLeft = false)
        {
            if (rightToLeft)
                return new Vec2Double(a.X*2, a.Y);

            return new Vec2Double(a.X/2, a.Y);
        }

        public static Line LineTo(this Vec2Double a, Vec2Double b)
        {
            return new Line(a.AsFloat(), b.AsFloat(), 0.4f , new ColorFloat(255,255,255,255));
        }

        public static Line LineToRight(this Vec2Double a, Vec2Double b) 
        {
            return a.LineTo(new Vec2Double(Width, b.Y));
        }

        public static Line LineToBottom(this Vec2Double a, Vec2Double b) 
        {
            return a.LineTo(new Vec2Double(b.X, -Height));
        }

        public static Line LineToTop(this Vec2Double a, Vec2Double b) 
        {
            return a.LineTo(new Vec2Double(b.X, Height));
        }

        public static Line LineToLeft(this Vec2Double a, Vec2Double b)
        {
            return a.LineTo(new Vec2Double(-Width, b.Y));
        }

        public static (Vec2Double,Vec2Double,Vec2Double,Vec2Double) AsRect(this Unit unit)
        {
            return (
                new Vec2Double(
                    Math.Abs(unit.Size.X-unit.Position.X),
                    Math.Abs(unit.Position.Y)
                ), // left-down
                new Vec2Double(
                    Math.Abs(unit.Size.X+unit.Position.X),
                    Math.Abs(unit.Position.Y)
                ), // right-down
                new Vec2Double(
                    Math.Abs(unit.Size.X-unit.Position.X),
                    Math.Abs(unit.Position.Y+unit.Size.Y)
                ), // left-top
                new Vec2Double(
                    Math.Abs(unit.Size.X+unit.Position.X),
                    Math.Abs(unit.Position.Y+unit.Size.Y)
                ) //right-top
            );
        }

        public static Rect AsDebugRect(this Unit unit)
        {
            return new Rect(
                    new Vec2Float(
                        (float)Math.Abs(unit.Size.X-unit.Position.X),
                        (float)Math.Abs(unit.Position.Y)
                    ),
                    new Vec2Float((float)unit.Size.X, (float)unit.Size.Y),
                    new ColorFloat(255,255,255,255)
                );
        }

        //https://stackoverflow.com/questions/32812265/fast-detecting-of-line-segment-and-rectangle-intersection-trough-cohen-sutherla
        public static bool DoLineRecIntersect(Vec2Double p1, Vec2Double p2, (Vec2Double r1, Vec2Double r2, Vec2Double r3, Vec2Double r4) tuple)
        {

            var (r1,r2,r3,r4) = tuple;

            if (p1.X > r1.X && p1.X > r2.X && p1.X > r3.X && p1.X > r4.X && p2.X > r1.X && p2.X > r2.X && p2.X > r3.X && p2.X > r4.X ) return false;
            if (p1.X < r1.X && p1.X < r2.X && p1.X < r3.X && p1.X < r4.X && p2.X < r1.X && p2.X < r2.X && p2.X < r3.X && p2.X < r4.X ) return false;
            if (p1.Y > r1.Y && p1.Y > r2.Y && p1.Y > r3.Y && p1.Y > r4.Y && p2.Y > r1.Y && p2.Y > r2.Y && p2.Y > r3.Y && p2.Y > r4.Y ) return false;
            if (p1.Y < r1.Y && p1.Y < r2.Y && p1.Y < r3.Y && p1.Y < r4.Y && p2.Y < r1.Y && p2.Y < r2.Y && p2.Y < r3.Y && p2.Y < r4.Y ) return false;


            double f1 = (p2.Y-p1.Y)*r1.X + (p1.X-p2.X)*r1.Y + (p2.X*p1.Y-p1.X*p2.Y);
            double f2 = (p2.Y-p1.Y)*r2.X + (p1.X-p2.X)*r2.Y + (p2.X*p1.Y-p1.X*p2.Y);
            double f3 = (p2.Y-p1.Y)*r3.X + (p1.X-p2.X)*r3.Y + (p2.X*p1.Y-p1.X*p2.Y);
            double f4 = (p2.Y-p1.Y)*r4.X + (p1.X-p2.X)*r4.Y + (p2.X*p1.Y-p1.X*p2.Y);

            if (f1<0 && f2<0 && f3<0 && f4<0) return false;
            if (f1>0 && f2>0 && f3>0 && f4>0) return false;

            return true;

        }
    }

    public abstract class BaseStrategy 
    {
        protected readonly Game Game;
        protected readonly Debug Debug;
        public BaseStrategy(Game game, Debug debug)
        {
            Debug = debug;
            Game = game;

            VectorsExtensions.Width = Game.Level.Tiles.Length;
        }

        protected virtual LootBox? NearestLoot<T>(Unit unit)
            where T : Item
        {
            LootBox? loot = null;
            foreach (var lootBox in Game.LootBoxes)
            {
                if (lootBox.Item is T)
                {
                    if (!loot.HasValue || unit.Position.EuclidianSqr(lootBox.Position) < unit.Position.EuclidianSqr(loot.Value.Position))
                    {
                        loot = lootBox;
                    }
                }
            }

            return loot;
        }

        protected virtual Unit? NearestEnemy(Unit unit)
        {
            Unit? nearestEnemy = null;
            foreach (var other in Game.Units)
            {
                if (other.PlayerId != unit.PlayerId)
                {
                    if (!nearestEnemy.HasValue || unit.Position.EuclidianSqr(other.Position) < unit.Position.EuclidianSqr(nearestEnemy.Value.Position))
                    {
                        nearestEnemy = other;
                    }
                }
            }

            return nearestEnemy;
        }

        protected bool Is(Vec2Double vector, Tile tile) => Game.Level.Tiles[(int)vector.X][(int)vector.Y] == tile;

        protected IEnumerable<Vec2Double> PointsBetween(Vec2Double point1, Vec2Double point2)
        {
            static double Y(double a, double b, double c, double x) => (a * x + c) * b;

            var a = point2.Y - point1.Y;
            var b = 1 / (point2.X - point1.X);
            var c = (point2.X * point1.Y) - (point1.X * point2.Y);

            for (var x = point1.X; x <= point2.X; x++)
                yield return new Vec2Double(x, Y(a,b,c,x));
        }


        protected bool HasWallBetween(Vec2Double point1, Vec2Double point2)
        {
            try
            {
                if (point1.IsLeftTo(point2))
                {
                    foreach (var point in PointsBetween(point1, point2))
                    {
                        if (Is(point, Tile.Wall))
                            return true;
                    }
                }
                else
                {
                    foreach (var point in PointsBetween(point2, point1))
                    {
                        if (Is(point, Tile.Wall))
                            return true;
                    }
                }
            } 
            catch
            {
                //
            }

            return false;
        }

    }

    public sealed class MyStrategy : BaseStrategy
    {
        public MyStrategy(Game game, Debug debug) : base(game, debug)
        {
        }

        public (bool X, bool Y) BulletsWillHit(Unit unit)
        {
            var list = new List<Line>(); 
            foreach (var bullet in Game.Bullets)
            {
                var unitFiredBullet = Game.Units.First(p => p.Id == bullet.UnitId);

                if (unitFiredBullet.Position.IsLeftTo(bullet.Position))
                {
                    if (unit.Position.IsRightTo(bullet.Position) && 
                        !HasWallBetween(unit.Position, bullet.Position))
                    {
                        return (Math.Abs(unit.Position.X - bullet.Position.X) < 10,
                            Math.Abs(unit.Position.Y - bullet.Position.Y) < 10);

                        var line = unitFiredBullet.Position.LineToRight(bullet.Position);
                        Debug.Draw(line);
                        var intersect = VectorsExtensions.DoLineRecIntersect(line.P1.AsDouble(), line.P2.AsDouble(), unit.AsRect());
                        Debug.Draw(new PlacedText("Right ->", unitFiredBullet.Position.AsFloat(), TextAlignment.Center, 100, new ColorFloat(255,255,255,255)));
                        Debug.Draw(new PlacedText($"INTERSECTS : {intersect}", unit.Position.AsFloat(), TextAlignment.Center, 100, new ColorFloat(255,255,255,255)));
                    }
                    
                }
                else
                {
                    if (unitFiredBullet.Position.IsRightTo(bullet.Position))
                    {
                        if (unit.Position.IsLeftTo(bullet.Position) && !HasWallBetween(unit.Position, bullet.Position))
                        {

                            return (Math.Abs(unit.Position.X - bullet.Position.X) < 10,
                                Math.Abs(unit.Position.Y - bullet.Position.Y) < 10);

                            var line = unitFiredBullet.Position.LineToLeft(bullet.Position);
                            Debug.Draw(line);
                            var intersect = VectorsExtensions.DoLineRecIntersect(line.P1.AsDouble(), line.P2.AsDouble(), unit.AsRect());

                            Debug.Draw(new PlacedText($"INTERSECTS : {intersect}", unit.Position.AsFloat(), TextAlignment.Center, 100, new ColorFloat(255,255,255,255)));
                            Debug.Draw(new PlacedText("<- Left", unitFiredBullet.Position.AsFloat(), TextAlignment.Center, 100, new ColorFloat(255,255,255,255)));
                        }
                    }
                }                
            }

            return (false, false);
        }

        public UnitAction GetAction(Unit unit)
        {
            var nearestEnemy = NearestEnemy(unit);
            var nearestWeapon = NearestLoot<Item.Weapon>(unit);
            var healthPack = NearestLoot<Item.HealthPack>(unit);

            Vec2Double targetPos = unit.Position;
            var jump = targetPos.Y > unit.Position.Y;
            if (healthPack.HasValue && unit.Health < Game.Properties.UnitMaxHealth*0.8)
            {
                targetPos = healthPack.Value.Position;
                if (targetPos.X > unit.Position.X && Game.Level.Tiles[(int)(unit.Position.X + 1)][(int)(unit.Position.Y)] == Tile.Wall)
                    jump = true;
                if (targetPos.X < unit.Position.X && Game.Level.Tiles[(int)(unit.Position.X - 1)][(int)(unit.Position.Y)] == Tile.Wall)
                    jump = true;
            }
            else 
            {
                if (!unit.Weapon.HasValue && nearestWeapon.HasValue)
                {
                    targetPos = nearestWeapon.Value.Position;

                    if (targetPos.X > unit.Position.X && Game.Level.Tiles[(int)(unit.Position.X + 1)][(int)(unit.Position.Y)] == Tile.Wall)
                        jump = true;
                    if (targetPos.X < unit.Position.X && Game.Level.Tiles[(int)(unit.Position.X - 1)][(int)(unit.Position.Y)] == Tile.Wall)
                        jump = true;
                }
                else if (nearestEnemy.HasValue)
                {
                    targetPos = nearestEnemy.Value.Position.AsHalf(
                        rightToLeft: nearestEnemy.Value.Position.IsLeftTo(unit.Position)
                    );
                    
                    if (nearestEnemy.Value.Position.X > unit.Position.X && Game.Level.Tiles[(int)(unit.Position.X + 1)][(int)(unit.Position.Y)] == Tile.Wall)
                        jump = true;
                    if (nearestEnemy.Value.Position.X < unit.Position.X && Game.Level.Tiles[(int)(unit.Position.X - 1)][(int)(unit.Position.Y)] == Tile.Wall)
                        jump = true;
                }
            }

            var velocity = 
                unit.Position.IsLeftTo(targetPos) 
                    ? Game.Properties.UnitMaxHorizontalSpeed 
                    : -Game.Properties.UnitMaxHorizontalSpeed;

            var aim = 
                nearestEnemy.HasValue 
                    ? unit.Position.AimAt(nearestEnemy.Value.Position) 
                    : new Vec2Double(0,0); 

            // Debug.Draw(
            //     new PlacedText(
            //         velocity.ToString(),
            //         unit.Position.AsFloat(),
            //         TextAlignment.Center,
            //         100,
            //         new ColorFloat(255,255,255,255)
            //     )
            // );


            var shoot = !HasWallBetween(unit.Position, nearestEnemy.Value.Position);
            var willHit = BulletsWillHit(unit);

            Debug.Draw(
                new PlacedText(
                    "Wall: " + (!shoot).ToString() + " | " + willHit.X.ToString() + " | " + jump.ToString(),
                    unit.Position.AsFloat(),
                    TextAlignment.Center,
                    100,
                    new ColorFloat(255,255,255,255)
                )
            );

            return new UnitAction
            {
                Velocity = willHit.X ? (velocity/2): velocity,
                Jump = willHit.X || jump,
                JumpDown = !jump,
                Aim = aim,
                Shoot = shoot,
                SwapWeapon = false,
                PlantMine = false
            };
        }
    }
}