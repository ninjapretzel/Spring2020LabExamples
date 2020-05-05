using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SpriteAnimAsset", menuName = "Scriptable Objects/Create New Sprite Animation", order = 1000)]
public class SpriteAnimAsset : ScriptableObject {
	public SpriteAnim data;
	/// <summary> Implicitly coerce a <see cref="SpriteAnimAsset"/> into a <see cref="SpriteAnim"/>. </summary>
	/// <param name="asset"> Asset to convert </param>
	public static implicit operator SpriteAnim(SpriteAnimAsset asset) { return asset.data; }
}
