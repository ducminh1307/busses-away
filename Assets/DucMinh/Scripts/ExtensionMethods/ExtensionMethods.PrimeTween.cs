// using System.Collections.Generic;
// using PrimeTween;
// using UnityEngine;
//
// namespace DucMinh
// {
//     public static partial class ExtensionMethods
//     {
//         public static Sequence MoveToPoints(this Transform target, List<Vector2> points, float duration, Ease ease = Ease.Linear)
//         {
//             if (points.IsNullOrEmpty())
//             {
//                 return Sequence.Create().Chain(Tween.Delay(0));
//             }
//
//             var totalDistance = 0f;
//             var currentPos = target.position;
//
//             totalDistance += Vector3.Distance(currentPos, points[0]);
//
//             for (int i = 0; i < points.Count - 1; i++)
//             {
//                 totalDistance += Vector3.Distance(points[i], points[i + 1]);
//             }
//
//             if (totalDistance <= Mathf.Epsilon)
//             {
//                 return Sequence.Create().Chain(Tween.Delay(0));
//             }
//
//             var sequence = Sequence.Create();
//             var previousPos = target.position;
//
//             foreach (var point in points)
//             {
//                 var segmentDistance = Vector3.Distance(previousPos, point);
//                 var segmentDuration = (segmentDistance / totalDistance) * duration;
//                 if (segmentDuration > 0)
//                 {
//                     sequence.Chain(Tween.Position(target, point, segmentDuration, ease));
//                 }
//                 previousPos = point;
//             }
//
//             return sequence;
//         }
//     }
// }