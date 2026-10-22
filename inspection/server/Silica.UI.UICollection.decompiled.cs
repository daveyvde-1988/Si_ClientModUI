using UnityEngine;

namespace Silica.UI;

public class UICollection : MonoBehaviour
{
	[Tooltip("UI Mode - infantry, vehicle or alien")]
	public UIMode Mode;

	[Tooltip("Canvas to which this component belongs")]
	public Canvas Canvas;

	[Tooltip("All crosshairs present in this collection - for mode and subset switching")]
	public Crosshair[] AssociatedCrosshairs;

	[Tooltip("List of disable sets for various UI subset modes")]
	public UICollectionDisableSubset[] DisableSets;

	public void Awake()
	{
		if (Canvas == null)
		{
			Canvas = GetComponent<Canvas>();
		}
	}

	private void Start()
	{
		if (Canvas.worldCamera == null)
		{
			Canvas.worldCamera = Game.CurrentCamera;
		}
	}

	public void OnEnable()
	{
		Refresh();
	}

	public void Refresh()
	{
		for (int i = 0; i < AssociatedCrosshairs.Length; i++)
		{
			if (AssociatedCrosshairs[i].Subset == UIManager.Instance.CurrentSubset)
			{
				if (!AssociatedCrosshairs[i].gameObject.activeSelf)
				{
					AssociatedCrosshairs[i].gameObject.SetActive(value: true);
				}
			}
			else if (AssociatedCrosshairs[i].gameObject.activeSelf)
			{
				AssociatedCrosshairs[i].gameObject.SetActive(value: false);
			}
		}
		GameListCache.QueryGameObjects.Clear();
		GameListCache.QueryGameObjects2.Clear();
		for (int j = 0; j < DisableSets.Length; j++)
		{
			UICollectionDisableSubset uICollectionDisableSubset = DisableSets[j];
			if (uICollectionDisableSubset == null)
			{
				continue;
			}
			if (uICollectionDisableSubset.DisableSubset == UIManager.Instance.CurrentSubset)
			{
				for (int k = 0; k < uICollectionDisableSubset.DisableObjects.Length; k++)
				{
					GameObject item = uICollectionDisableSubset.DisableObjects[k];
					if (!GameListCache.QueryGameObjects.Contains(item))
					{
						GameListCache.QueryGameObjects.Add(item);
					}
				}
				continue;
			}
			for (int l = 0; l < uICollectionDisableSubset.DisableObjects.Length; l++)
			{
				GameObject item2 = uICollectionDisableSubset.DisableObjects[l];
				if (!GameListCache.QueryGameObjects2.Contains(item2))
				{
					GameListCache.QueryGameObjects2.Add(item2);
				}
			}
		}
		foreach (GameObject queryGameObject in GameListCache.QueryGameObjects)
		{
			queryGameObject.SetActive(value: false);
		}
		foreach (GameObject item3 in GameListCache.QueryGameObjects2)
		{
			item3.SetActive(value: true);
		}
	}
}
