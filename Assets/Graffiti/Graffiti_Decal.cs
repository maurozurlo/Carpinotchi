using UnityEngine;

public class Graffiti_Decal : MonoBehaviour
{
	public float eraseRadius = 0.1f;
	public float eraseSpeed = 0.5f;
	public Material graffitiMaterial;

	private Renderer rend;
	private Texture2D graffitiTexture;

	private void Start()
	{

		graffitiTexture = graffitiMaterial.mainTexture as Texture2D;
	}

	private void Update()
	{
		if (Input.GetMouseButton(0))
		{
			RaycastHit hit;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
			{
				if (hit.collider)
				{
					rend = hit.collider.GetComponent<Renderer>();
					Material material = rend.material;

					if (material.mainTexture.name == graffitiMaterial.mainTexture.name)
					{
						
						Vector2 uv = hit.textureCoord;
						Vector2 pixelUV = new Vector2(uv.x * material.mainTexture.width, uv.y * material.mainTexture.height);
						Texture2D texture = material.mainTexture as Texture2D;
						Color[] pixels = texture.GetPixels(Mathf.RoundToInt(pixelUV.x - eraseRadius), Mathf.RoundToInt(pixelUV.y - eraseRadius), Mathf.RoundToInt(eraseRadius * 2), Mathf.RoundToInt(eraseRadius * 2));
						
						for (int i = 0; i < pixels.Length; i++)
						{
							Vector2 currentPixel = new Vector2(i % Mathf.RoundToInt(eraseRadius * 2), i / Mathf.RoundToInt(eraseRadius * 2));
							if (Vector2.Distance(currentPixel, new Vector2(eraseRadius, eraseRadius)) <= eraseRadius)
							{
								pixels[i].a -= eraseSpeed * Time.deltaTime;
							}
						}
						texture.SetPixels(Mathf.RoundToInt(pixelUV.x - eraseRadius), Mathf.RoundToInt(pixelUV.y - eraseRadius), Mathf.RoundToInt(eraseRadius * 2), Mathf.RoundToInt(eraseRadius * 2), pixels);
						texture.Apply();
						if (texture.GetPixel(Mathf.RoundToInt(pixelUV.x), Mathf.RoundToInt(pixelUV.y)).a < 0.05f)
						{
							// Graffiti has been erased
						}
					}
				}
			}
		}
	}
}