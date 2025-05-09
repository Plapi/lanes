using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public class SerializedDateTime {

	public long timestamp;

	public SerializedDateTime(DateTime date) {
		Date = date;
	}

	public DateTime Date {
		get => new(timestamp);
		set => timestamp = value.Ticks;
	}

	public override string ToString() {
		return Date.ToString(CultureInfo.InvariantCulture);
	}
}