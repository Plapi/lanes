using System.Collections.Generic;
using UnityEngine;

public static class RandomTextsSystem {
	
	public static string Get(RandomTextsData data) {
		List<string> list = new(data.texts);
		if (data.lastRandomTextIndex != -1) {
			list.RemoveAt(data.lastRandomTextIndex);
		}
		data.lastRandomTextIndex = Random.Range(0, list.Count);
		return list[data.lastRandomTextIndex];
	}
	
	public static readonly RandomTextsData SuccessPerson = new() {
		texts = new[] {
			"Thanks!\nHere’s #coins# coins!",
			"Much appreciated!\n#coins# coins!",
			"You’re the best!\n#coins# coins!",
			"Smooth ride!\nTake #coins# coins!",
			"Great job!\nHere’s #coins# coins!",
			"Awesome! Take\nthese #coins# coins!",
			"Thanks a lot!\n#coins# coins!",
			"Fast & smooth!\n#coins# coins!"
		}
	};
	
	public static readonly RandomTextsData FailPerson = new() {
		texts = new[] {
			"Oh no!\nYou missed it!",
			"Oops!\nWrong spot!",
			"Hey!\nYou passed it!",
			"Wait!\nThat was my stop!",
			"Come on!\nToo far!",
			"You missed\nmy stop!",
			"That was\nmy drop-off!"
		}
	};
}

public class RandomTextsData {
	public string[] texts;
	public int lastRandomTextIndex = -1;
}

