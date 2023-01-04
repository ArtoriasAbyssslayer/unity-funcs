/*
Copyright 2021-2023 Matti Hiltunen (https://www.mattihiltunen.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace mtti.Funcs.Types
{
    /// <summary>
    /// A quadratic bezier curve.
    /// </summary>
    public struct QuadraticBezier
    {
        /// <summary>
        /// Calculate position of a point along a quadratic bezier curve.
        /// </summary>
        /// <param name="start">Starting point</param>
        /// <param name="control">Control point</param>
        /// <param name="end">Ending point</param>
        public static Vector3 GetPoint(
            Vector3 start,
            Vector3 control,
            Vector3 end,
            float t
        )
        {
            var q0 = Vector3.Lerp(start, control, t);
            var q1 = Vector3.Lerp(control, end, t);
            return Vector3.Lerp(q0, q1, t);
        }

        public static float GetSegmentLength(int segmentCount)
        {
            return 1.0f / segmentCount;
        }

        /// <summary>
        /// Position of the curve starting point.
        /// </summary>
        public Vector3 Start
        {
            get { return _start; }
        }

        /// <summary>
        /// Position of the curve control point.
        /// </summary>
        public Vector3 Control
        {
            get { return _control; }
        }

        /// <summary>
        /// Position of the curve ending point.
        /// </summary>
        public Vector3 End
        {
            get { return _end; }
        }

        [SerializeField]
        private readonly Vector3 _start;

        [SerializeField]
        private readonly Vector3 _control;

        [SerializeField]
        private readonly Vector3 _end;

        public QuadraticBezier(
            Vector3 start,
            Vector3 control,
            Vector3 end
        )
        {
            _start = start;
            _control = control;
            _end = end;
        }

        /// <summary>
        /// Get a point along the curve.
        /// </summary>
        public Vector3 GetPoint(float t)
        {
            return GetPoint(
                _start,
                _control,
                _end,
                t
            );
        }

        public void GetPoints(Vector3[] result)
        {
            GetPoints(result, result.Length);
        }

        /// <summary>
        /// Get multiple points along the curve.
        /// </summary>
        public void GetPoints(
            Vector3[] result,
            int resolution
        )
        {
            if (resolution < 2)
            {
                throw new ArgumentException(
                    "Resolution must be at least 2",
                    "resolution"
                );
            }

            if (result.Length < resolution)
            {
                throw new ArgumentException(
                    "Result array is not large enough",
                    "result"
                );
            }

            var segmentLength = GetSegmentLength(resolution);
            for (var i = 0; i < resolution; i++)
            {
                result[i] = GetPoint(segmentLength * i);
            }
        }

        public void GetPoints(
            List<Vector3> result,
            int resolution
        )
        {
            var segmentLength = GetSegmentLength(resolution);
            for (var i = 0; i < resolution; i++)
            {
                result.Add(GetPoint(segmentLength * i));
            }
        }

        public void GetRays(Ray[] result)
        {
            GetRays(result, result.Length);
        }

        /// <summary>
        /// Get multiple points along the curve as rays which contain both
        /// the point and the direction to the next one.
        /// </summary>
        public void GetRays(
            Ray[] result,
            int resolution
        )
        {
            if (resolution < 2)
            {
                throw new ArgumentException(
                    "Resolution must be at least 2",
                    "resolution"
                );
            }

            if (result.Length < resolution)
            {
                throw new ArgumentException(
                    "Result array is not large enough",
                    "result"
                );
            }

            var segmentLength = GetSegmentLength(resolution);

            // Calculate points
            for (var i = 0; i < resolution; i++)
            {
                var t = segmentLength * i;
                result[i] = new Ray(GetPoint(t), Vector3.zero);
            }

            Vector3 current;
            Vector3 next;

            // Calculate directions, except for the last point
            for (var i = 0; i < resolution - 1; i++)
            {
                current = result[i].origin;
                next = result[i + 1].origin;

                var dirToNext = next - current;
                result[i] = new Ray(
                    current,
                    next - current
                );
            }

            // Direction for the last point isn't really definable, so we'll
            // use the same direction as for the one before it.
            var ilast = resolution - 1;
            result[ilast] = new Ray(
                result[ilast].origin,
                result[ilast - 1].direction
            );
        }

        /// <summary>
        /// Get multiple points along the curve as rays which contain both
        /// the point and the direction to the next one.
        /// </summary>
        public void GetRays(
            List<Ray> result,
            int resolution
        )
        {
            if (resolution < 2)
            {
                throw new ArgumentException(
                    "Resolution must be at least 2",
                    nameof(resolution)
                );
            }

            result.Clear();

            var segmentLength = GetSegmentLength(resolution);

            // Calculate points
            for (var i = 0; i < resolution; i++)
            {
                var t = segmentLength * i;
                result.Add(new Ray(GetPoint(t), Vector3.zero));
            }

            // Calculate directions, except for the last point
            for (var i = 0; i < resolution - 1; i++)
            {
                var current = result[i].origin;
                var next = result[i + 1].origin;

                result[i] = new Ray(
                    current,
                    next - current
                );
            }

            // Direction for the last point isn't really definable, so we'll
            // use the same direction as for the one before it.
            var ilast = resolution - 1;
            result[ilast] = new Ray(
                result[ilast].origin,
                result[ilast - 1].direction
            );
        }

        public float EstimateLength(int resolution = 3)
        {
            var length = 0.0f;
            var segmentLength = GetSegmentLength(resolution);
            var previousPoint = _start;

            for (var i = 0; i < resolution; i++)
            {
                var currentPoint = GetPoint(segmentLength * i);
                length += Vector3.Distance(previousPoint, currentPoint);

                previousPoint = currentPoint;
            }

            length += Vector3.Distance(previousPoint, _end);

            return length;
        }

        /// <summary>
        /// Split the curve into two at a <c>t</c> value.
        /// </summary>
        public void Split(
            float t,
            QuadraticBezier[] result
        )
        {
            var q0 = Vector3.Lerp(_start, _control, t);
            var q1 = Vector3.Lerp(_control, _end, t);
            var b = Vector3.Lerp(q0, q1, t);
            result[0] = new QuadraticBezier(_start, q0, b);
            result[1] = new QuadraticBezier(b, q1, _end);
        }
    }
}
