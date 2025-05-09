using UnityEngine;

public class VaultTable : MonoBehaviour {

	private int[] levels;
	private Element table;
	private int currentLevel = -1;

	private void Awake() {
		levels = Settings.Instance.company.vaultRoom.tableMoneyLevels;
	}

	public void Init(int money) {
		int level = GetLevel(money);

		if (currentLevel == level) {
			return;
		}
		if (table != null) {
			ObjectPoolManager.Release(table);
			table = null;
		}
		table = Application.isPlaying
			? ObjectPoolManager.Get(Resources.Load<Element>($"Company/VaultRoom/VaultTable/Table{level}/Table{level}"))
			: Instantiate(Resources.Load<Element>($"Company/VaultRoom/VaultTable/Table{level}/Table{level}"));
		table.transform.parent = transform;
		table.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		table.gameObject.SetActive(true);
		table.name = table.name.Replace("(Clone)", "");
		currentLevel = level;
	}

	private int GetLevel(int money) {
		for (int i = levels.Length - 1; i >= 0; i--) {
			if (money >= levels[i]) {
				return i;
			}
		}
		return Mathf.Max(0, levels.Length - 1);
	}
}
