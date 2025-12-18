using System;
using System.Globalization;

using UnityEngine;

public static class BoundsShape
{
	public struct Sphere : IEquatable<Sphere>, IFormattable
	{
		private Vector3 m_Center;
		private float m_Radius;

		public Vector3 center { get => m_Center; set => m_Center = value; }
		public float radius { get => m_Radius; set => m_Radius = value; }
		public float diameter { get => m_Radius * 2; set => m_Radius = value * 0.5f; }

		public Sphere(Vector3 center, float radius) { m_Center = center; m_Radius = radius; }

		public bool Equals(Sphere other) => m_Center.Equals(other.m_Center) && Mathf.Approximately(m_Radius, other.m_Radius);
		public override bool Equals(object obj) => obj is Sphere other && Equals(other);
		public override int GetHashCode() => HashCode.Combine(m_Center, m_Radius);
		public string ToString(string format) => ToString(format, null);
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (string.IsNullOrEmpty(format)) format = "F2";
			if (formatProvider == null) formatProvider = CultureInfo.InvariantCulture.NumberFormat;
			return $"Center: {m_Center.ToString(format, formatProvider)}, Radius: {m_Radius.ToString(format, formatProvider)}";
		}

		public void Encapsulate(Vector3 point, float radius) { 
			float dist = Vector3.Distance(m_Center, point) + radius; 
			if (dist > m_Radius) m_Radius = dist;
		}
		public void Encapsulate(Vector3 point) => Encapsulate(point, 0);
		public void Encapsulate(Sphere sphere) => Encapsulate(sphere.center, sphere.radius);
		public void Expand(float amount) => m_Radius += amount * 0.5f;
		public bool Intersects(Sphere bounds) => Vector3.Distance(m_Center, bounds.m_Center) <= (m_Radius + bounds.m_Radius);
		public bool Contains(Vector3 point) => Vector3.Distance(m_Center, point) <= m_Radius;
		public bool IntersectRay(Ray ray) => IntersectRay(ray, out _);
		public bool IntersectRay(Ray ray, out float distance)
		{
			distance = 0f; Vector3 l = m_Center - ray.origin; float tca = Vector3.Dot(l, ray.direction); if (tca < 0) return false;
			float d2 = Vector3.Dot(l, l) - tca * tca; float r2 = m_Radius * m_Radius; if (d2 > r2) return false;
			float thc = Mathf.Sqrt(r2 - d2); distance = tca - thc; return true;
		}
		public float SqrRadius() => m_Radius * m_Radius;
		public static bool operator ==(Sphere lhs, Sphere rhs) => lhs.Equals(rhs);
		public static bool operator !=(Sphere lhs, Sphere rhs) => !lhs.Equals(rhs);
	}

	public struct Circle : IEquatable<Circle>, IFormattable
	{
		public Vector3 m_Center;
		public float m_Radius;

		public Vector3 m_Normal;

		public Vector3 center { get => m_Center; set => m_Center = value; }
		public float radius { get => m_Radius; set => m_Radius = value; }
		public float diameter { get => m_Radius * 2; set => m_Radius = value * 0.5f; }
		public Vector3 normal { get => m_Normal; set => m_Normal = value; }

		public Circle(Vector3 center, float radius) { m_Center = center; m_Radius = radius; m_Normal = Vector3.up;}
		public Circle(Vector3 center, float radius, Vector3 normal) { m_Center = center; m_Radius = radius; m_Normal = normal; }

		public bool Equals(Circle other)
			=> m_Center.Equals(other.m_Center)
			&& Mathf.Approximately(m_Radius, other.m_Radius)
			&& m_Normal.Equals(other.m_Normal);
		public override bool Equals(object obj) => obj is Sphere other && Equals(other);
		public override int GetHashCode() => HashCode.Combine(m_Center, m_Radius);
		public string ToString(string format) => ToString(format, null);
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (string.IsNullOrEmpty(format)) format = "F2";
			if (formatProvider == null) formatProvider = CultureInfo.InvariantCulture.NumberFormat;
			return $"Center: {m_Center.ToString(format, formatProvider)}, Radius: {m_Radius.ToString(format, formatProvider)}, Normal: {m_Normal.ToString(format, formatProvider)}";
		}

		public void Encapsulate(Vector3 point) { Vector3 projected = point - Vector3.Project(point - m_Center, m_Normal); float dist = Vector3.Distance(m_Center, projected); if (dist > m_Radius) m_Radius = dist; }
		public void Encapsulate(Circle other) { Vector3 projectedOtherCenter = other.m_Center - Vector3.Project(other.m_Center - m_Center, m_Normal); float dist = Vector3.Distance(m_Center, projectedOtherCenter) + other.m_Radius; if (dist > m_Radius) m_Radius = dist; }
		public void Expand(float amount) => m_Radius += amount * 0.5f;
		public bool Intersects(Circle other) { if (Mathf.Abs(Vector3.Dot(m_Normal, other.m_Normal)) < 0.99f) return false; float dist = Vector3.Distance(m_Center, other.m_Center); return dist <= (m_Radius + other.m_Radius); }
		public bool Contains(Vector3 point) { if (Mathf.Abs(Vector3.Dot(point - m_Center, m_Normal)) > 0.001f) return false; return Vector3.Distance(m_Center, point) <= m_Radius; }
		public bool IntersectRay(Ray ray) => IntersectRay(ray, out _);
		public bool IntersectRay(Ray ray, out float distance)
		{
			distance = 0f; float denom = Vector3.Dot(m_Normal, ray.direction); if (Mathf.Abs(denom) < 0.0001f) return false;
			distance = Vector3.Dot(m_Center - ray.origin, m_Normal) / denom; if (distance < 0) return false;
			Vector3 hitPoint = ray.origin + ray.direction * distance; return Vector3.Distance(m_Center, hitPoint) <= m_Radius;
		}
		public float SqrRadius() => m_Radius * m_Radius;
		public static bool operator ==(Circle lhs, Circle rhs) => lhs.Equals(rhs);
		public static bool operator !=(Circle lhs, Circle rhs) => !lhs.Equals(rhs);
	}
}
