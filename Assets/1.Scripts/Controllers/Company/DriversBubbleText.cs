using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DriversBubbleText", menuName = "Custom/DriversBubbleText")]
public class DriversBubbleText : ScriptableObject {
	
	[Multiline] [SerializeField] private string[] texts;

	private string text;

	public string GetText() {
		List<string> list = new(texts);
		if (!string.IsNullOrEmpty(text)) {
			list.Remove(text);
		}
		text = list[Random.Range(0, list.Count)];
		return text;
	}
}