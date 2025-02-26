using System;
using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Other.Extensions;
using Com.ForbiddenByte.OSA.Core;
using Com.ForbiddenByte.OSA.CustomParams;
using Com.ForbiddenByte.OSA.DataHelpers;

public class CarListUI : OSA<BaseParamsWithPrefab, MyListItemViewsHolder> {

	[SerializeField] private Snapper8 snapper;
	[SerializeField] private Button goButton;
	
	private SimpleDataHelper<ClassListModel> data;
	private Action<int> onEnableItem;
	private Action<int> onDisableItem;

	public void Init(int count, Action<int> onEnableItem, Action<int> onDisableItem, Action<int> onSelect) {
		data = new SimpleDataHelper<ClassListModel>(this);
		for (int i = 0; i < count; i++) {
			data.List.Add(new ClassListModel());
		}
		this.onEnableItem = onEnableItem;
		this.onDisableItem = onDisableItem;
		goButton.onClick.AddListener(() => {
			onSelect(snapper.LastSnappedItemIndex);
		});
	}

	public void Release() {
		onEnableItem = null;
		onDisableItem = null;
	}

	public float GetItemPosX(int index) {
		var item = GetItemViewsHolderIfVisible(index);
		return item != null ? item.root.anchoredPosition.x : 0f;
	}
	
	protected override void Start() {
		base.Start();
		_Params.ItemPrefab.gameObject.SetActive(false);
		data.NotifyListChangedExternally();
	}

	protected override MyListItemViewsHolder CreateViewsHolder(int itemIndex) {
		var instance = new MyListItemViewsHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}

	protected override void UpdateViewsHolder(MyListItemViewsHolder newOrRecycled) {
		newOrRecycled.titleText.text = $"Car{newOrRecycled.ItemIndex}";
		onEnableItem?.Invoke(newOrRecycled.ItemIndex);
	}

	protected override void OnBeforeRecycleOrDisableViewsHolder(MyListItemViewsHolder inRecycleBinOrVisible, int newItemIndex) {
		base.OnBeforeRecycleOrDisableViewsHolder(inRecycleBinOrVisible, newItemIndex);
		onDisableItem?.Invoke(inRecycleBinOrVisible.ItemIndex);
	}
}

public class ClassListModel { }

public class MyListItemViewsHolder : BaseItemViewsHolder {
	public Text titleText;

	public override void CollectViews() {
		base.CollectViews();
		root.GetComponentAtPath("TitleText", out titleText);
	}
}