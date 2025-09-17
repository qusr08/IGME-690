using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

// NOTE: currently TextureUV isn't being saved outside of the program
// It would need to be saved to be used in a separate program.
public struct TextureUV
{
	public int nameID;
	public float pixelStartX;
	public float pixelStartY;
	public float pixelStartX2;
	public float pixelEndY;
	public float pixelEndX;
	public float pixelStartY2;
	public float pixelEndX2;
	public float pixelEndY2;
}

public class TextureAtlas
{
	public static TextureAtlas Instance = new TextureAtlas( );
	public static readonly int TextureSize = 16;
	public static readonly int TextureRows = 4;

	public void CreateAtlasComponentData (string directoryName, string outputFileName)
	{
		// Get all file names in this directory
		string[] names = Directory.GetFiles(directoryName, "*.png");

		// Make the list of uvs
		List<TextureUV> textureUVs = new List<TextureUV>(names.Length);

		// We're going to assume our images are a power of 2 so we just
		// Need to get the sqrt of the number of images and round up
		int squareRoot = Mathf.CeilToInt(Mathf.Sqrt(names.Length));
		int squareRootH = squareRoot;
		int atlasWidth = squareRoot * TextureSize;
		int atlasHeight = squareRootH * TextureSize;

		if (squareRoot * (squareRoot - 1) > names.Length)
		{
			squareRootH = squareRootH - 1;
			atlasHeight = squareRootH * TextureSize;
		}

		// Allocate space for the atlas and file data
		Texture2D atlas = new Texture2D(atlasWidth, atlasHeight);
		byte[ ][ ] fileData = new byte[names.Length][ ];

		// Read the file data in parallel
		Parallel.For(0, names.Length, index =>
		{
			fileData[index] = File.ReadAllBytes(names[index]);
		});

		// Put all the images into the image file and write
		// All the texture data to the texture uv map list.
		int x1 = 0;
		int y1 = 0;
		Texture2D temp = new Texture2D(TextureSize, TextureSize);
		float pWidth = (float) TextureSize;
		float pHeight = (float) TextureSize;
		float aWidth = (float) atlas.width;
		float aHeight = (float) atlas.height;

		for (int i = 0; i < names.Length; i++)
		{
			float pixelStartX = ((x1 * pWidth) + 1) / aWidth;
			float pixelStartY = ((y1 * pHeight) + 1) / aHeight;
			float pixelEndX = ((x1 + 1) * pWidth - 1) / aWidth;
			float pixelEndY = ((y1 + 1) * pHeight - 1) / aHeight;
			TextureUV currentUVInfo = new TextureUV
			{
				nameID = i,
				pixelStartX = pixelStartX,
				pixelStartY = pixelStartY,
				pixelStartX2 = pixelStartX,
				pixelEndY = pixelEndY,
				pixelEndX = pixelEndX,
				pixelStartY2 = pixelStartY,
				pixelEndX2 = pixelEndX,
				pixelEndY2 = pixelEndY
			};
			textureUVs.Add(currentUVInfo);

			temp.LoadImage(fileData[i]);
			atlas.SetPixels(x1 * TextureSize, y1 * TextureSize, TextureSize, TextureSize, temp.GetPixels( ));

			x1 = (x1 + 1) % squareRoot;
			if (x1 == 0)
			{
				y1++;
			}
		}

		atlas.Apply( );

		// Write the atlas out to a file
		File.WriteAllBytes(outputFileName, atlas.EncodeToPNG( ));
	}
}
