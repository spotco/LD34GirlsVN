#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using System;
using System.Text;

public class AssetExport {
	[MenuItem("SPEditorUtils/Write Streaming Assets To Cached Resources")]
	public static void write_streaming_assets_to_resources() {
		if (GameMain._context == null) {
			Debug.LogError("_context null");
			return;
		}
#if !UNITY_WEBPLAYER
		if (GameMain._context._file_cache != null) {
			FileUtil.DeleteFileOrDirectory(CachedStreamingAssets.FILECACHE_PATH);
			/*
			System.IO.File.WriteAllBytes(
				CachedStreamingAssets.FILECACHE_PATH,
				ProtoBufSerializer.serialize_to_bytes<FileCache>(FileCache.inst())
			);
			*/
			SPUtil.logf("Wrote to %s",CachedStreamingAssets.FILECACHE_PATH);
		} else {
			Debug.LogError("_file_cache null");
		}
		
		if (!System.IO.Directory.Exists(CachedStreamingAssets.TEXTURE_PATH_PREFIX)) {
			System.IO.Directory.CreateDirectory(CachedStreamingAssets.TEXTURE_PATH_PREFIX);
		}
		
		if (GameMain._context._tex_resc != null) {
			foreach(string key in GameMain._context._tex_resc._key_to_resourcevalue.Keys) {
				string texture_write_to_filepath = CachedStreamingAssets.texture_key_to_filepath(key);
				Texture2D texture = GameMain._context._tex_resc._key_to_resourcevalue[key]._tex as Texture2D;
				System.IO.File.WriteAllBytes(
					texture_write_to_filepath, texture.EncodeToPNG()
				);

				SPUtil.logf("Wrote texture %s",texture_write_to_filepath);
			}
		}
#endif
	}
	
	[MenuItem("SPEditorUtils/Delete Cached Resources")]
	public static void delete_cached_resources() {
#if !UNITY_WEBPLAYER
		FileUtil.DeleteFileOrDirectory(CachedStreamingAssets.FILECACHE_PATH);
		FileUtil.DeleteFileOrDirectory(CachedStreamingAssets.TEXTURE_PATH_PREFIX);
#endif
	}
	
}
#endif

public class CachedStreamingAssets {
	public const string FILECACHE_PATH = "Assets/Resources/CachedStreamingAssets/filecache.bytes";
	public const string TEXTURE_PATH_PREFIX = "Assets/Resources/CachedStreamingAssets/Textures/";

	public static string path_to_loadpath(string input) {
		return input.Replace("Assets/Resources/","").Replace(".bytes","");
	}

	public static string texture_key_to_filepath(string texkey) {
		return TEXTURE_PATH_PREFIX + texkey.Replace("/","_") + ".png";
	}

	public static string texture_key_to_resource_path(string texkey) {
		return "CachedStreamingAssets/Textures/" + texkey.Replace("/","_");
	}
}