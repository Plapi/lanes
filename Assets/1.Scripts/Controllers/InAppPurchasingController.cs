using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class InAppPurchasingController : MonoBehaviourSingleton<InAppPurchasingController>, IDetailedStoreListener {

	private IStoreController storeController;
	private State state;
	private Action<ProductCollection> onInitComplete;
	private Action<bool> onPurchaseComplete;
	private InAppPurchaseProduct startedProduct;

	private async void Start() {
		try {
			InitializationOptions options = new InitializationOptions().SetEnvironmentName("production");
			await UnityServices.InitializeAsync(options);
		} catch (Exception exception) {
			Debug.LogException(exception);
		}
	}

	public void InitIfNeeded(Action<ProductCollection> onComplete) {
		if (state != State.None) {
			onComplete?.Invoke(null);
			return;
		}
		onInitComplete = onComplete;
		state = State.Initializing;
		ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		InAppPurchaseProduct[] products = Settings.Instance.inAppPurchaseProducts;
		for (int i = 0; i < products.Length; i++) {
			builder.AddProduct(products[i].id, products[i].productType);
		}
		UnityPurchasing.Initialize(this, builder);
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
		storeController = controller;
		state = State.Initialized;
		onInitComplete?.Invoke(storeController.products);
		onInitComplete = null;
	}

	public void OnInitializeFailed(InitializationFailureReason error) {
		state = State.None;
	}

	public void OnInitializeFailed(InitializationFailureReason error, string message) {
		state = State.None;
	}
	
	public void Purchase(InAppPurchaseProduct product, Action<bool> onComplete) {
		onPurchaseComplete = onComplete;
		storeController.InitiatePurchase(product.id);
		AnalyticsSystem.RecordBuyProductStartEvent(product.id, product.value.ToString());
		startedProduct = product;
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent) {
		onPurchaseComplete?.Invoke(true);
		onPurchaseComplete = null;
		AnalyticsSystem.RecordBuyProductCompleteEvent(startedProduct.id, startedProduct.value.ToString());
		TenjinSystem.PurchaseEvent(purchaseEvent);
		return PurchaseProcessingResult.Complete;
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
		onPurchaseComplete?.Invoke(false);
		onPurchaseComplete = null;
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription) {
		onPurchaseComplete?.Invoke(false);
		onPurchaseComplete = null;
	}

	private enum State {
		None,
		Initializing,
		Initialized
	}
}
