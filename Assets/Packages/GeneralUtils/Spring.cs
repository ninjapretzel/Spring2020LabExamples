using UnityEngine;
using System;
using System.Runtime.CompilerServices;
// Note: This was the minimal amount of code to bring over the 'spring' effect used in the ViewModel.
// It was divided into a few files (ISimpleAnim<> and helpers, other classes of animation, and other kinds of animations)
// But just the spring stuff is not that much code, so I left it as one file to have fewer files, as all the code here is related.

/// <summary> Generic animator interface for a given type. </summary>
/// <typeparam name="T">Type that is animated. </typeparam>
public interface ISimpleAnim<T> {
	/// <summary> Current value of animation </summary>
	T value { get; set; }
	/// <summary> Implementation should read the current <see cref="value"/> and produce the next value. </summary>
	void Update();

}

/// <summary> Some helper extension methods to use with <see cref="ISimpleAnim{T}"/>.</summary>
public static class SimpleAnimHelpers {

	/// <summary> Play an animation, using the reference's value as the previous frame's value,
	///  and set the reference to the next frame's value. </summary>
	/// <typeparam name="T"> Generic type of animation to play </typeparam>
	/// <param name="animator"> Animator logic </param>
	/// <param name="value"> Value to use and update. </param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RefAnim<T>(this ISimpleAnim<T> animator, ref T value) {
		animator.value = value;
		animator.Update();
		value = animator.value;
	}
	/// <summary> Play an animation, and return the next frames's value </summary>
	/// <typeparam name="T"> Generic type of animation to play </typeparam>
	/// <param name="animator"> Animator logic </param>
	/// <param name="value"> Value to use as the previous frame's value. </param>
	/// <returns> Next frame's value </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Animate<T>(this ISimpleAnim<T> animator, T value) {
		animator.value = value;
		animator.Update();
		return animator.value;
	}

}

/// <summary> Simple <see cref="float"/> spring </summary>
[Serializable] public class Spring : ISimpleAnim<float> {

	/// <summary> Current value, fulfils interface. </summary>
	public float value { get; set; }
	/// <summary> Target value the spring needs to reach. </summary>
	public float target;
	/// <summary> Current spring velocity </summary>
	public float velocity;
	/// <summary> Spring strength setting </summary>
	public float strength = 100;
	/// <summary> Spring dampening setting </summary>
	public float dampening = 1;
	/// <summary> Update method, fulfils interface. </summary>
	public void Update() {
		velocity += (target - value) * strength * Time.deltaTime;
		velocity *= Mathf.Pow(dampening * .0001f, Time.deltaTime);
		value += velocity * Time.deltaTime;
	}

}

/// <summary> 3-axis <see cref="Vector3"/> spring </summary>
[Serializable] public class SpringV3 : ISimpleAnim<Vector3> {

	/// <summary> Current value, fulfils interface. </summary>
	public Vector3 value { get; set; }
	/// <summary> Target value the spring needs to reach. </summary>
	public Vector3 target;
	/// <summary> Current spring velocity </summary>
	public Vector3 velocity;
	/// <summary> Spring strength setting </summary>
	public float strength = 100;
	/// <summary> Spring dampening setting </summary>
	public float dampening = 1;
	/// <summary> Update method, fulfils interface. </summary>
	public void Update() {
		velocity += (target - value) * strength * Time.deltaTime;
		velocity *= Mathf.Pow(dampening * .0001f, Time.deltaTime);
		value += velocity * Time.deltaTime;
	}

}

/// <summary> Complex <see cref="Quaternion"/> spring. </summary>
[Serializable] public class SpringQ : ISimpleAnim<Quaternion> {

	/// <summary> Current value, fulfils interface. </summary>
	public Quaternion value { get; set; }
	/// <summary> Target value the spring needs to reach. </summary>
	public Quaternion target = Quaternion.identity;
	/// <summary> Current spring velocity </summary>
	public Vector3 angularVelocity;

	/// <summary> Velocity axis </summary>
	public Vector3 velAxis;
	/// <summary> Velocity angle </summary>
	public float velAngle;

	/// <summary> Spring strength setting </summary>
	public float strength = 100;
	/// <summary> Spring dampening setting </summary>
	public float dampening = 1;
	/// <summary> Spring dampening for axis towards reducing the rotation </summary>
	public float axisDampening = 5;

	/// <summary> Update method, fulfils interface </summary>
	public void Update() {
		float angle;
		Vector3 axis;
		if (float.IsNaN(velAxis.x)) {
			velAxis = Vector3.zero;
			velAngle = 0;
			Debug.Log("vel axis triggered NAN check");
		}
		Quaternion inv = target * Quaternion.Inverse(value);
		inv.ToAngleAxis(out angle, out axis);
		//(target * Quaternion.Inverse(value)).ToAngleAxis(out angle, out axis);
		if (!float.IsNaN(axis.x)) {
			if (angle > 180) { angle -= 360; }
			if (angle < -180) { angle += 360; }
			velAxis = Vector3.Lerp(velAxis, axis, Time.deltaTime * axisDampening);
		} else {
			Debug.Log("new axis triggered NAN check");
		}
		velAngle += angle * strength * Time.deltaTime;
		velAngle *= Mathf.Pow(dampening * .0001f, Time.deltaTime);
		if (velAngle > 180) { velAngle = 180; }
		if (velAngle < -180) { velAngle = -180; }

		value *= Quaternion.AngleAxis(velAngle * Time.deltaTime, velAxis);
	}

}

