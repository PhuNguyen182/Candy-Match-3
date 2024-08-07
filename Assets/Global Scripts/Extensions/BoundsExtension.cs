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
            sortedPosition.Sort(new Vector3IntComparer());

            Vector3Int firstPosition = sortedPosition.First();
            Vector3Int lastPosition = sortedPosition.Last();

            return new BoundsInt
            {
                xMin = firstPosition.x,
                xMax = lastPosition.x,
                yMin = firstPosition.y,
                yMax = lastPosition.y
            };
        }

        public static IEnumerable<Vector3Int> GetRow(this BoundsInt boundsInt, Vector3Int checkPosition)
        {
            for (int x = boundsInt.xMin; x <= boundsInt.xMax; x++)
            {
                yield return new Vector3Int(x, checkPosition.y);
            }
        }

        public static IEnumerable<Vector3Int> GetColumn(this BoundsInt boundsInt, Vector3Int checkPosition)
        {
            for (int y = boundsInt.yMin; y <= boundsInt.yMax; y++)
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
    }
}
