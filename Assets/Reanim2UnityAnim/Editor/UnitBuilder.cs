#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reanim2UnityAnim.Editor.Data;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Reanim2UnityAnim.Editor
{
	public static class UnitBuilder
	{
		private const float ToRad = 3.14159265f / 180;

		public static void Create(Reanim2UnityAnimConfig config)
		{
			List<Track> tracks = TrackParser.ParseTracksFromFile(config.filePath);

			List<Partition> partitions = new List<Partition>();
			List<SpriteTrack> spriteTracks = new List<SpriteTrack>();
			foreach (Track track in tracks)
			{
				List<string> sprites = new List<string>();
				foreach (Frame frame in track.Transforms)
				{
					if (frame.Image != null)
					{
						sprites.Add(frame.Image);
					}
				}
				if (sprites.Count == 0 && track.Transforms.All(frame => frame.X == null))
				{
					Partition partition = new Partition(track);
					partitions.Add(partition);
					continue;
				}
				foreach (string sprite in sprites)
				{
					if (spriteTracks.Find(spriteTrack => spriteTrack.Name == sprite) == null)
					{
						SpriteTrack spriteTrack = new SpriteTrack(sprite, track.Transforms);
						spriteTracks.Add(spriteTrack);
					}
				}
			}
			
			List<AnimationClip> clips = new List<AnimationClip>();
			foreach (Partition partition in partitions)
			{
				AnimationClip clip = new AnimationClip()
				{ frameRate = 12, name = partition.name };

				foreach (SpriteTrack spriteTrack in spriteTracks)
				{
					float? x, y, ax, ay, sx, sy, a;
					x = y = ax = ay = 0;
					sx = sy = a = 1;
					int? f = 0;
					string? imgName = null;

					List<Keyframe> keyframesX = new List<Keyframe>();
					List<Keyframe> keyframesY = new List<Keyframe>();
					List<Keyframe> keyframesAngleX = new List<Keyframe>();
					List<Keyframe> keyframesAngleY = new List<Keyframe>();
					List<Keyframe> keyframesSx = new List<Keyframe>();
					List<Keyframe> keyframesSy = new List<Keyframe>();
					List<Keyframe> keyframesF = new List<Keyframe>();
					List<Keyframe> keyframesA = new List<Keyframe>();

					for (int frameIndex = 0; frameIndex < partition.endIndexExclude; frameIndex++)
					{
						Frame frame = spriteTrack.Transforms[frameIndex];
						if (frame.Image != null) imgName = frame.Image;
						if (frameIndex <= partition.startIndexInclude)
						{
							if (frame.X != null) x = frame.X.Value / 100;
							if (frame.Y != null) y = -frame.Y.Value / 100;
							if (frame.Ky != null) ax = frame.Ky.Value * ToRad;
							if (frame.Kx != null) ay = frame.Kx.Value * ToRad;
							if (frame.Sx != null) sx = frame.Sx.Value;
							if (frame.Sy != null) sy = frame.Sy.Value;
							if (frame.F != null) f = frame.F.Value;
							if (frame.A != null) a = frame.A.Value;
						}
						else
						{
							x = frame.X / 100;
							y = -frame.Y / 100;
							ax = frame.Ky * ToRad;
							ay = frame.Kx * ToRad;
							sx = frame.Sx;
							sy = frame.Sy;
							f = frame.F;
							a = frame.A;
						}

						if (frameIndex < partition.startIndexInclude) continue;
						if (frameIndex != partition.startIndexInclude && (imgName == null || imgName != spriteTrack.Name)) continue;
						int currentFrameInPartition = frameIndex - partition.startIndexInclude;
						float currentTime = currentFrameInPartition / 12f;
						if (x != null) keyframesX.Add(new Keyframe(currentTime, x.Value));
						if (y != null) keyframesY.Add(new Keyframe(currentTime, y.Value));
						if (ax != null) keyframesAngleX.Add(new Keyframe(currentTime, ax.Value));
						if (ay != null) keyframesAngleY.Add(new Keyframe(currentTime, ay.Value));
						if (sx != null) keyframesSx.Add(new Keyframe(currentTime, sx.Value));
						if (sy != null) keyframesSy.Add(new Keyframe(currentTime, sy.Value));
						if (f != null) keyframesF.Add(new Keyframe(currentTime, f.Value));
						if (a != null) keyframesA.Add(new Keyframe(currentTime, a.Value));
					}

					Frame firstFrame = spriteTrack.Transforms[partition.startIndexInclude];

					BindKeyframes(keyframesX, clip, spriteTrack.Name, typeof(Transform), "localPosition.x", firstFrame.X);
					BindKeyframes(keyframesY, clip, spriteTrack.Name, typeof(Transform), "localPosition.y", firstFrame.Y);
					BindKeyframes(keyframesAngleX, clip, spriteTrack.Name, typeof(SpriteRenderer), "material._AngleX", firstFrame.Ky * ToRad);
					BindKeyframes(keyframesAngleY, clip, spriteTrack.Name, typeof(SpriteRenderer), "material._AngleY", firstFrame.Kx * ToRad);
					BindKeyframes(keyframesSx, clip, spriteTrack.Name, typeof(SpriteRenderer), "material._ScaleX", firstFrame.Sx);
					BindKeyframes(keyframesSy, clip, spriteTrack.Name, typeof(SpriteRenderer), "material._ScaleY", firstFrame.Sy);
					BindKeyframes(keyframesF, clip, spriteTrack.Name, typeof(SpriteRenderer), "material._IsVisible", firstFrame.F);
					BindKeyframes(keyframesA, clip, spriteTrack.Name, typeof(SpriteRenderer), "material._Alpha", firstFrame.A);
				}
				clips.Add(clip);
			}

			CreateGameObject(config.name, spriteTracks.Select(track => track.Name).ToArray(), clips);
		}

		private static void BindKeyframes
			(List<Keyframe> keyframes, AnimationClip clip, string relativePath, Type componentType, string propertyName, float? firstValue)
		{
			float defaultValue = propertyName.StartsWith("material._Scale") || propertyName == "material._Alpha" ? 1f : 0f;

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			bool hasChanged = keyframes.Count > 1 || keyframes[0].value != defaultValue || firstValue != null;

			if (hasChanged)
			{
				AnimationCurve curve = new AnimationCurve(keyframes.ToArray());
				if (propertyName == "material._IsVisible")
				{
					for (int i = 0; i < curve.length; i++)
					{
						AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
						AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
					}
				}
				else
				{
					for (int i = 0; i < curve.length; i++)
					{
						curve.SmoothTangents(i, 0);
					}
				}
				clip.SetCurve(relativePath, componentType, propertyName, curve);
			}
		}

		private static void CreateGameObject(string name, string[] childNames, List<AnimationClip> clips)
		{
			string targetFolder = $"Assets/Reanim2UnityAnim/Output/{name}Res/";
			if (!AssetDatabase.IsValidFolder(targetFolder))
			{
				Directory.CreateDirectory(targetFolder);
			}
			GameObject gameObject = new GameObject(name);
			Animator animator = gameObject.AddComponent<Animator>();
			AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(targetFolder + name + "_Controller.controller");
			animator.runtimeAnimatorController = controller;

			foreach (AnimationClip clip in clips)
			{
				AnimatorState state = controller.layers[0].stateMachine.AddState(clip.name);
				state.motion = clip;
				AssetDatabase.CreateAsset(clip, targetFolder + clip.name + ".anim");
				AssetDatabase.SaveAssets();
			}
			AssetDatabase.Refresh();

			Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/GameUnitShader_Mat.mat");

			for (int index = 0; index < childNames.Length; index++)
			{
				string childName = childNames[index];
				GameObject child = new GameObject(childName);
				Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Reanim2UnityAnim/reanim_all/" + childName + ".png");
				SpriteRenderer spriteRenderer = child.AddComponent<SpriteRenderer>();
				spriteRenderer.sprite = sprite;
				spriteRenderer.material = mat;
				spriteRenderer.sortingOrder = index;
				child.transform.parent = gameObject.transform;
			}
		}
	}
}