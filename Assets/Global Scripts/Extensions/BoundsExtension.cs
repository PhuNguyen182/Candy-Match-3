using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Comparers;

namespace GlobalScripts.Extensions
{
    public static class BoundsExtension
    {
        public static Vector3IntComparer Vector3IntComparer { get; } = new();

        public static BoundsInt GetBounds2D(this Vector3Int position, Vector3Int size)
        {
            return new BoundsInt(position - size / 2, size);
        }

        public static BoundsInt GetBounds2D(this Vector3Int position, int range = 0)
        {
            return new BoundsInt(position + new Vector3Int(-1, -1) * range, new(2 * range + 1, 2 * range + 1));
        }

        public static BoundsInt Expand2D(this BoundsInt boundsInt, int range)
        {
            BoundsInt bounds = new BoundsInt
            {
                xMin = boundsInt.xMin - range / 2,
                xMax = boundsInt.xMax + range / 2,
                yMin = boundsInt.yMin - range / 2,
                yMax = boundsInt.yMax + range / 2,
            };

            return bounds;
        }

        public static BoundsInt Encapsulate(List<Vector3Int> positions)
        {
            List<Vector3Int> sortedPosition = new(positions);
            sortedPosition.Sort(Vector3IntComparer);

            int count = sortedPosition.Count;
            Vector3Int firstPosition = sortedPosition[0];
            Vector3Int lastPosition = sortedPosition[count - 1];
            BoundsInt bounds = new BoundsInt
            {
                xMin = firstPosition.x,
                xMax = lastPosition.x + 1,
                yMin = firstPosition.y,
                yMax = lastPosition.y + 1
            };

            return bounds;
        }

        public static IEnumerable<Vector3Int> GetRow(this BoundsInt boundsInt, Vector3Int checkPosition)
        {
            for (int x = boundsInt.xMin; x < boundsInt.xMax; x++)
            {
                yield return new Vector3Int(x, checkPosition.y);
            }
        }

        public static IEnumerable<Vector3Int> GetColumn(this BoundsInt boundsInt, Vector3Int checkPosition)
        {
            for (int y = boundsInt.yMin; y < boundsInt.yMax; y++)
            {
                yield return new Vector3Int(checkPosition.x, y);
            }
        }

        public static void ForEach2D(this BoundsInt boundsInt, Action<Vector3Int> callback)
        {
            for (int x = boundsInt.xMin; x < boundsInt.xMax; x++)
            {
                for (int y = boundsInt.yMin; y < boundsInt.yMax; y++)
                {
                    callback.Invoke(new Vector3Int(x, y));
                }
            }
        }

        public static void ForEach3D(this BoundsInt boundsInt, Action<Vector3Int> callback)
        {
            for (int x = boundsInt.xMin; x < boundsInt.xMax; x++)
            {
                for (int y = boundsInt.yMin; y < boundsInt.yMax; y++)
                {
                    for (int z = boundsInt.zMin; z < boundsInt.zMax; z++)
                    {
                        callback.Invoke(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        public static IEnumerable<Vector3Int> Iterator2D(this BoundsInt boundsInt)
        {
            for (int x = boundsInt.xMin; x < boundsInt.xMax; x++)
            {
                for (int y = boundsInt.yMin; y < boundsInt.yMax; y++)
                {
                    yield return new Vector3Int(x, y);
                }
            }
        }

        public static IEnumerable<Vector3Int> Iterator3D(this BoundsInt boundsInt)
        {
            for (int x = boundsInt.xMin; x <= boundsInt.xMax; x++)
            {
                for (int y = boundsInt.yMin; y <= boundsInt.yMax; y++)
                {
                    for (int z = boundsInt.zMin; z <= boundsInt.zMax; z++)
                    {
                        yield return new Vector3Int(x, y, z);
                    }
                }
            }
        }

        public static IEnumerable<Vector3Int> IteratorIgnoreCorner2D(this BoundsInt boundsInt)
        {
            for (int x = boundsInt.xMin; x <= boundsInt.xMax; x++)
            {
                for (int y = boundsInt.yMin; y <= boundsInt.yMax; y++)
                {
                    if (x == boundsInt.xMin && y == boundsInt.yMin)
                        continue;

                    if (x == boundsInt.xMin && y == boundsInt.yMax)
                        continue;

                    if (x == boundsInt.xMax && y == boundsInt.yMin)
                        continue;

                    if (x == boundsInt.xMax && y == boundsInt.yMax)
                        continue;

                    yield return new Vector3Int(x, y);
                }
            }
        }

        public static IEnumerable<Vector3Int> GetBorder2D(this BoundsInt bounds)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    if (x > bounds.xMin && x < bounds.xMax - 2)
                        continue;

                    if (y > bounds.yMin && y < bounds.yMax - 2)
                        continue;

                    yield return new(x, y);
                }
            }
        }
    }
}
