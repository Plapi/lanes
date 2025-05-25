using System.Collections.Generic;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.MiniJSON;

public static class TenjinSystem {

	private const string KEY = "ZGVCJ2NFUO4YWPBEXXR7B7ZYPHG35GBK";
	
	public static void Connect() {
		BaseTenjin instance = Tenjin.getInstance(KEY);
		instance.SetAppStoreType(AppStoreType.googleplay);
		instance.Connect();
	}

	public static void SendEvent(string name, string value) {
		BaseTenjin instance = Tenjin.getInstance(KEY);
		instance.SendEvent(name, value);
	}

	public static void PurchaseEvent(PurchaseEventArgs purchaseEventArgs) {
		var price = purchaseEventArgs.purchasedProduct.metadata.localizedPrice;
		double lPrice = decimal.ToDouble(price);
		var currencyCode = purchaseEventArgs.purchasedProduct.metadata.isoCurrencyCode;

		var wrapper = Json.Deserialize(purchaseEventArgs.purchasedProduct.receipt) as Dictionary<string, object>;  // https://gist.github.com/darktable/1411710
		if (null == wrapper) {
			return;
		}
		
		var store = (string)wrapper["Store"]; // GooglePlay, AmazonAppStore, AppleAppStore, etc.
		var payload = (string)wrapper["Payload"]; // For Apple this will be the base64 encoded ASN.1 receipt. For Android, it is the raw JSON receipt.
		var productId = purchaseEventArgs.purchasedProduct.definition.id;
		
		var googleDetails = Json.Deserialize(payload) as Dictionary<string, object>;
		var googleJson = (string)googleDetails["json"];
		var googleSig = (string)googleDetails["signature"];
		
		BaseTenjin instance = Tenjin.getInstance(KEY);
		instance.Transaction(productId, currencyCode, 1, lPrice, null, googleJson, googleSig);
	}
	
}
