using UnityEngine;

namespace DTT.UI.ProceduralUI
{
	/// <summary>
	/// Handles the hit detection for <see cref="RoundedImage"/>.
	/// </summary>
	public class RoundedImageHitbox
	{
		/// <summary>
		/// The rectangle used for the hitbox of the image.
		/// </summary>
		public Rect HitboxRect => new Rect(_roundedImage.rectTransform.rect.position, _roundedImage.GetImageSize());
		
		/// <summary>
		/// The rounded image to test on.
		/// </summary>
		private RoundedImage _roundedImage; 
		
		/// <summary>
		/// Creates a new hitbox for a rounded image.
		/// </summary>
		/// <param name="roundedImage"></param>
		public RoundedImageHitbox(RoundedImage roundedImage) => this._roundedImage = roundedImage;
		
		/// <summary>
		/// Tests whether the rounded box was hit with the given screen position.
		/// </summary>
		/// <param name="screenPosition">The screen position to test for a hit.</param>
		/// <returns>Whether the box was hit.</returns>
		public bool HitTest(Vector2 screenPosition)
		{
			var radii = _roundedImage.GetCornerRounding();
			Vector4 rounding = new Vector4(
				radii[Corner.TOP_RIGHT],
				radii[Corner.BOTTOM_RIGHT],
				radii[Corner.TOP_LEFT],
				radii[Corner.BOTTOM_LEFT]
			);

			// Calculate the border radius that is useful for the SDF.
			float borderRadius = HitboxRect.GetShortLength() * 0.25f;
			// Apply the border thickness if we have the inside hitbox detection option turned on.
			if (_roundedImage.UseHitboxInside && _roundedImage.Mode == RoundingMode.BORDER)
				borderRadius *= _roundedImage.BorderThickness;

			// Transform screen position to local position of the rounded image.
			Vector2 position = _roundedImage.rectTransform.InverseTransformPoint(screenPosition);
			if (_roundedImage.UseHitboxOutside || _roundedImage.UseHitboxInside)
				return HitTest(position, HitboxRect.size, rounding, borderRadius, _roundedImage.DistanceFalloff);
			else
				return true;
		}
		
		/// <summary>
		/// Tests the rounded image using SDF data.
		/// </summary>
		/// <param name="samplePosition">Point in local space to hit test.</param>
		/// <param name="size">The size of the rect transform.</param>
		/// <param name="radii">The radii of all the corners.</param>
		/// <param name="borderDistance">The distance of the border.</param>
		/// <param name="falloff">The amount of falloff.</param>
		/// <returns>Whether the rounded box was hit.</returns>
		private bool HitTest(Vector2 samplePosition, 
							 Vector2 size, 
							 Vector4 radii, 
							 float borderDistance, 
							 float falloff)
		{
			// Transform radii to local space.
			Vector4 transformedRadii = radii * 0.5f * Mathf.Min(size.x, size.y);
			float distance = RoundedBoxSDF(samplePosition, size * 0.5f, transformedRadii);
			float distanceWithBorder = Mathf.Abs(distance + borderDistance) - borderDistance;
			float distanceWithBorderAndFalloff = distanceWithBorder - falloff / 2;
			return distanceWithBorderAndFalloff < 0;
		}

		/// <summary>
		/// Signed distance function to get the distance for a sample point.
		/// </summary>
		/// <param name="samplePoint">Point in local space to sample.</param>
		/// <param name="size">The size of the rounded box.</param>
		/// <param name="radii">The radii of the corners</param>
		/// <returns>The distance from the box.</returns>
		private float RoundedBoxSDF(Vector2 samplePoint, Vector2 size, Vector4 radii)
		{
			bool conditionA = samplePoint.x <= 0.0;
			bool conditionB = samplePoint.y <= 0.0;
			if (conditionA)
			{
				radii.x = radii.z;
				radii.y = radii.w;
			}
			if (conditionB)
			{
				radii.x = radii.y;
			}
			Vector2 absoluteSameplePosition = new Vector2(Mathf.Abs(samplePoint.x), Mathf.Abs(samplePoint.y));
			Vector2 q = absoluteSameplePosition - size;
			q.x += radii.x;
			q.y += radii.x;

			return Mathf.Min(Mathf.Max(q.x, q.y), 0.0f) + Vector2.Max(q, Vector2.zero).magnitude - radii.x;
		}
	}
}