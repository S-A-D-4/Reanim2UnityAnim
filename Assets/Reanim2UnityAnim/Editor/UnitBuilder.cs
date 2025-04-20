﻿#nullable enable
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

			List<SpriteTrack> spriteTracks = new List<SpriteTrack>();
			List<Partition> partitions = new List<Partition>(config.customPartitions);
			List<RootTrack> rootTracks = ParseRoots(config, tracks);
			ClassifyTracks(config, tracks, partitions, spriteTracks, rootTracks);

			List<AnimationClip> clips = new List<AnimationClip>();
			foreach (Partition partition in partitions)
			{
				AnimationClip clip = new AnimationClip
				{ frameRate = 12, name = partition.name };

				CreateRootFrames(rootTracks, partition, clip, config.center);

				CreateSpriteFrames(spriteTracks, partition, clip, config.center);

				clips.Add(clip);
			}

			CreateAssets(config.name, spriteTracks, clips);
		}

		private static void CreateSpriteFrames(List<SpriteTrack> spriteTracks, Partition partition, AnimationClip clip, Vector2 center)
		{
			foreach (SpriteTrack spriteTrack in spriteTracks)
			{
				float? x, y, ax, ay, sx, sy, a;
				x = y = ax = ay = 0;
				sx = sy = a = 1;
				int? f = 0;

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
					if (frameIndex >= partition.startIndexInclude)
					{
						int currentFrameInPartition = frameIndex - partition.startIndexInclude;
						float currentTime = currentFrameInPartition / 12f;

						float dx, dy;
						if (spriteTrack.Parent != null)
						{
							dx = spriteTrack.Parent.startX;
							dy = spriteTrack.Parent.startY;
						}
						else
						{
							dx = center.x;
							dy = -center.y;
						}

						if (x != null) keyframesX.Add(new Keyframe(currentTime, x.Value - dx));
						if (y != null) keyframesY.Add(new Keyframe(currentTime, y.Value - dy));
						if (ax != null) keyframesAngleX.Add(new Keyframe(currentTime, ax.Value));
						if (ay != null) keyframesAngleY.Add(new Keyframe(currentTime, ay.Value));
						if (sx != null) keyframesSx.Add(new Keyframe(currentTime, sx.Value));
						if (sy != null) keyframesSy.Add(new Keyframe(currentTime, sy.Value));
						if (f != null) keyframesF.Add(new Keyframe(currentTime, f.Value));
						if (a != null) keyframesA.Add(new Keyframe(currentTime, a.Value));
					}
				}

				BindKeyframes(keyframesX, clip, spriteTrack.Path, typeof(Transform), "localPosition.x");
				BindKeyframes(keyframesY, clip, spriteTrack.Path, typeof(Transform), "localPosition.y");
				BindKeyframes(keyframesAngleX, clip, spriteTrack.Path, typeof(SpriteRenderer), "material._AngleX");
				BindKeyframes(keyframesAngleY, clip, spriteTrack.Path, typeof(SpriteRenderer), "material._AngleY");
				BindKeyframes(keyframesSx, clip, spriteTrack.Path, typeof(SpriteRenderer), "material._ScaleX");
				BindKeyframes(keyframesSy, clip, spriteTrack.Path, typeof(SpriteRenderer), "material._ScaleY");
				BindKeyframes(keyframesF, clip, spriteTrack.Path, typeof(SpriteRenderer), "material._IsVisible");
				BindKeyframes(keyframesA, clip, spriteTrack.Path, typeof(SpriteRenderer), "material._Alpha");
			}
		}

		private static void CreateRootFrames(List<RootTrack> rootTracks, Partition partition, AnimationClip clip, Vector2 center)
		{
			foreach (RootTrack rootTrack in rootTracks)
			{
				List<Keyframe> keyframesX = new List<Keyframe>();
				List<Keyframe> keyframesY = new List<Keyframe>();

				for (int frameIndex = 0; frameIndex < partition.endIndexExclude; frameIndex++)
				{
					Frame frame = rootTrack.Transforms[frameIndex];
					if (frameIndex <= partition.startIndexInclude)
					{
						if (frame.X != null) rootTrack.startX = frame.X.Value / 100;
						if (frame.Y != null) rootTrack.startY = -frame.Y.Value / 100;
					}
					float? x = frame.X / 100;
					float? y = -frame.Y / 100;
					if (frameIndex < partition.startIndexInclude) continue;
					if (frameIndex >= partition.startIndexInclude)
					{
						int currentFrameInPartition = frameIndex - partition.startIndexInclude;
						float currentTime = currentFrameInPartition / 12f;
						if (x != null) keyframesX.Add(new Keyframe(currentTime, x.Value - center.x));
						if (y != null) keyframesY.Add(new Keyframe(currentTime, y.Value + center.y));
					}
				}
				BindKeyframes(keyframesX, clip, rootTrack.Name, typeof(Transform), "localPosition.x");
				BindKeyframes(keyframesY, clip, rootTrack.Name, typeof(Transform), "localPosition.y");
			}
		}

		private static void ClassifyTracks(Reanim2UnityAnimConfig config, List<Track> tracks, List<Partition> partitions,
			List<SpriteTrack> spriteTracks, List<RootTrack> rootTracks)
		{
			for (int index = 0; index < tracks.Count; index++)
			{
				Track track = tracks[index];
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
				}

				foreach (string sprite in sprites)
				{
					if (spriteTracks.Find(spriteTrack => spriteTrack.ImageName == sprite && spriteTrack.Name == track.Name) == null)
					{
						SpriteTrack spriteTrack = new SpriteTrack(track.Name, track.Transforms, index, sprite);
						foreach (Root2Childs root2Child in config.root2Childs)
						{
							if (root2Child.childs.Contains(track.Name))
							{
								spriteTrack.Parent = rootTracks.Find(rootTrack => rootTrack.Name == root2Child.root);
							}
						}
						spriteTracks.Add(spriteTrack);
					}
				}
			}
			IEnumerable<IGrouping<string, SpriteTrack>> groupBy = spriteTracks.GroupBy(track => track.ImageName);

			foreach (IGrouping<string, SpriteTrack> group in groupBy)
			{
				if (group.Count() == 1)
				{
					foreach (SpriteTrack spriteTrack in group)
					{
						spriteTrack.Name = spriteTrack.ImageName;
					}
				}
				else
				{
					foreach (SpriteTrack spriteTrack in group)
					{
						spriteTrack.Name = $"{spriteTrack.ImageName}({spriteTrack.Name})";
					}
				}
			}
		}

		private static List<RootTrack> ParseRoots(Reanim2UnityAnimConfig config, List<Track> tracks)
		{
			List<RootTrack> rootTracks = new List<RootTrack>();
			foreach (Track track in tracks)
			{
				if (config.root2Childs.Any(root2Child => root2Child.root == track.Name))
				{
					RootTrack spriteTrack = new RootTrack(track.Name, track.Transforms);
					rootTracks.Add(spriteTrack);
				}
			}
			return rootTracks;
		}

		private static void BindKeyframes(List<Keyframe> keyframes, AnimationClip clip, string relativePath, Type componentType, string propertyName)
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

		private static void CreateAssets(string name, List<SpriteTrack> spriteTracks, List<AnimationClip> clips)
		{
			string targetFolder = $"Assets/Reanim2UnityAnim/Output/{name}Res/";
			Directory.CreateDirectory(targetFolder);

			GameObject gameObject = new GameObject(name);
			Animator animator = gameObject.AddComponent<Animator>();
			AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(targetFolder + name + "_Controller.controller");
			animator.runtimeAnimatorController = controller;
			UniqueMaterialController materialController = gameObject.AddComponent<UniqueMaterialController>();
			List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();

			foreach (AnimationClip clip in clips)
			{
				AnimatorState state = controller.layers[0].stateMachine.AddState(clip.name);
				state.motion = clip;
				AssetDatabase.CreateAsset(clip, targetFolder + clip.name + ".anim");
				AssetDatabase.SaveAssets();
			}
			AssetDatabase.Refresh();

			Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Reanim2UnityAnim/GameUnitShader_Mat.mat");

			foreach (SpriteTrack spriteTrack in spriteTracks)
			{
				GameObject child = new GameObject(spriteTrack.Name);
				Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Reanim2UnityAnim/reanim_all/" + spriteTrack.ImageName + ".png");
				SpriteRenderer spriteRenderer = child.AddComponent<SpriteRenderer>();
				spriteRenderer.sprite = sprite;
				spriteRenderer.material = mat;
				spriteRenderer.sortingOrder = spriteTrack.Order;
				if (spriteTrack.ParentPath != null)
				{
					Transform parent = gameObject.transform.Find(spriteTrack.ParentPath);
					if (parent == null)
					{
						parent = new GameObject(spriteTrack.ParentPath).transform;
						parent.SetParent(gameObject.transform);
					}
					child.transform.SetParent(parent);
				}
				else
				{
					child.transform.parent = gameObject.transform;
				}
				spriteRenderers.Add(spriteRenderer);
			}
			materialController.spriteRenderers = spriteRenderers.ToArray();

			PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, targetFolder + $"{name}.prefab", InteractionMode.AutomatedAction);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}